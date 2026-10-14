import * as vscode from 'vscode';
import * as path from 'node:path';
import { TextDecoder } from 'util';
import { PipeBridge, PipeCommandPayload, PipeConnectionState } from './pipes/namedPipeClients';
import {
  extractRuntimeTimestamp,
  handleIncomingPipeCommand,
  PipeCommandReceiverContext,
  PipeCommandSenderContext,
  RuntimeButtonId,
  RuntimeSignal,
  RuntimeSignalKind,
  sendRuntimeCommand,
} from './pipes/pipeCommands.js';
import {
  ActivePipeConfiguration,
  BookTreePayload,
  BookTreeNode,
  BridgeMessage,
  PageMetadata,
  RebuildAutomation,
} from './types/extensionTypes.js';
import {
  waitForNextDiagnosticsUpdate,
} from './services/diagnostics/csharpDiagnostics.js';
import { areStringSetsEqual } from './services/common/setUtils.js';
import { openRelativeFile as openRelativeBookFile } from './services/book/fileOpenService.js';
import { createClearedProjectViewState } from './services/book/projectStateService.js';
import { verifyAllTreeCsFiles as verifyTreeCsFilesWorkflow } from './services/book/verifyWorkflowService.js';
import {
  discoverPageFolders,
  findBookFileInWorkspace,
  getDirectoryUri,
  readPageFolder,
} from './services/book/treeService.js';
import {
  relativePathFromUri,
  normalizeRelativePath,
} from './services/book/verificationService.js';
import {
  isManagedQbookDebugSession,
} from './services/debug/helpers.js';
import {
  registerDebugSessionLifecycle,
} from './services/debug/sessionLifecycleService.js';
import {
  postBookStatus,
} from './services/ui/bookStatusService.js';
import {
  postDebugState,
  postPipeStatus,
  postRuntimeInfo,
  postRuntimeState,
  postStatusText,
} from './services/ui/webviewStatusService.js';
import { dispatchWebviewMessage } from './services/ui/webviewMessageDispatcher.js';
import { getWebviewHtml } from './webview/panelHtml.js';

const COMMAND_ID = 'amium-qbook.bridge';
const VIEW_ID = 'amium-qbook.panel';
const textDecoder = new TextDecoder('utf-8');

let activePipeBridge: PipeBridge | undefined;
let activePipeSubscription: vscode.Disposable | undefined;
let pipeOutputChannel: vscode.OutputChannel | undefined;
let runtimeLogChannel: vscode.LogOutputChannel | undefined;
let currentPipeConfig: ActivePipeConfiguration | undefined;
let lastProjectRoot: vscode.Uri | undefined;
let pipeConnectionState: PipeConnectionState = 'disconnected';
let activePipeStatusSubscription: vscode.Disposable | undefined;
let viewProviderRef: QBookViewProvider | undefined;
let extensionContextRef: vscode.ExtensionContext | undefined;
let pipeRefresh: Promise<void> = Promise.resolve();
let deactivated = false;

const pipeCommandReceiverContext: PipeCommandReceiverContext = {
  notifyRuntimeSignal: (signal: RuntimeSignal, command?: PipeCommandPayload) =>
    viewProviderRef?.notifyRuntimeSignal(signal, command),
  getRuntimeLogChannel,
};

const pipeCommandSenderContext: PipeCommandSenderContext = {
  getBridge: () => activePipeBridge,
  showErrorMessage: (message: string) => vscode.window.showErrorMessage(message),
  broadcastStatus: broadcastPipeStatus,
};

type RuntimeButtonState = Record<RuntimeButtonId, RuntimeSignalKind | null>;

export function activate(context: vscode.ExtensionContext): void {
  extensionContextRef = context;
  deactivated = false;
  getRuntimeLogChannel()?.info('Book workspace activated; initializing runtime connection.');
  void refreshWorkspacePipeBridge(context);
  const viewProvider = new QBookViewProvider(context);
  viewProviderRef = viewProvider;
  viewProviderRef.updatePipeStatus(pipeConnectionState);

  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(VIEW_ID, viewProvider, {
      webviewOptions: { retainContextWhenHidden: true },
    })
  );

  context.subscriptions.push(
    vscode.window.onDidChangeActiveTextEditor((editor: vscode.TextEditor | undefined) => {
      viewProvider.handleActiveEditorChange(editor);
    })
  );

  context.subscriptions.push(
    vscode.languages.onDidChangeDiagnostics(() => {
      viewProvider.refreshDiagnostics();
    })
  );

  const openPanelCommand = vscode.commands.registerCommand(COMMAND_ID, async () => {
    await vscode.commands.executeCommand(`${VIEW_ID}.focus`);
  });

  context.subscriptions.push(openPanelCommand);

  viewProvider.handleActiveEditorChange(vscode.window.activeTextEditor);
  viewProvider.refreshDiagnostics();

  const pipeWatcher = vscode.workspace.createFileSystemWatcher('**/{Book.json,pipes.json}');
  context.subscriptions.push(
    pipeWatcher,
    pipeWatcher.onDidCreate(() => void refreshWorkspacePipeBridge(context)),
    pipeWatcher.onDidChange(() => void refreshWorkspacePipeBridge(context)),
    pipeWatcher.onDidDelete(() => void refreshWorkspacePipeBridge(context)),
    vscode.workspace.onDidChangeWorkspaceFolders(() => void refreshWorkspacePipeBridge(context))
  );

  context.subscriptions.push(
    vscode.workspace.onDidChangeConfiguration((event: vscode.ConfigurationChangeEvent) => {
      if (
        event.affectsConfiguration('amium-qbook.pipe.serverName') ||
        event.affectsConfiguration('amium-qbook.pipe.clientName') ||
        event.affectsConfiguration('amium-qbook.pipe.reconnectDelay')
      ) {
        void refreshWorkspacePipeBridge(context);
      }
    })
  );
}

export function deactivate(): void {
  deactivated = true;
  disposeActivePipeBridge();
}

function refreshWorkspacePipeBridge(context: vscode.ExtensionContext): Promise<void> {
  pipeRefresh = pipeRefresh.then(async () => {
    if (deactivated) return;
    const bookUri = await findBookFileInWorkspace();
    const bookRoot = bookUri ? getDirectoryUri(bookUri) : undefined;
    const pipes = bookRoot ? await readProjectPipeConfig(bookRoot) : undefined;
    if (deactivated) return;
    lastProjectRoot = bookRoot;
    if (pipes) {
      updatePipeBridge(context, { ...pipes, reconnectDelay: getReconnectDelaySetting(), source: 'project' });
    } else {
      if (currentPipeConfig?.source === 'project') disposeActivePipeBridge();
      applySettingsPipeBridge(context);
    }
  }).catch((error: unknown) => {
    getRuntimeLogChannel()?.error('Runtime connection initialization failed', error);
  });
  return pipeRefresh;
}

function applySettingsPipeBridge(context: vscode.ExtensionContext): void {
  const config = vscode.workspace.getConfiguration('amium-qbook');
  const serverPipe = (config.get<string>('pipe.serverName') ?? '').trim();
  const clientPipe = (config.get<string>('pipe.clientName') ?? '').trim();
  const reconnectDelay = getReconnectDelaySetting();

  if (!serverPipe || !clientPipe) {
    if (currentPipeConfig?.source !== 'project') {
      disposeActivePipeBridge();
    }
    return;
  }

  updatePipeBridge(context, {
    serverPipe,
    clientPipe,
    reconnectDelay,
    source: 'settings',
  });
}

function getReconnectDelaySetting(): number {
  return Math.max(100, vscode.workspace.getConfiguration('amium-qbook').get<number>('pipe.reconnectDelay') ?? 500);
}

async function readProjectPipeConfig(bookRoot: vscode.Uri): Promise<{ serverPipe: string; clientPipe: string } | undefined> {
  try {
    const fileUri = vscode.Uri.joinPath(bookRoot, 'pipes.json');
    const raw = await vscode.workspace.fs.readFile(fileUri);
    const text = textDecoder.decode(raw);
    const data = JSON.parse(text) as { ServerPipe?: string; ClientPipe?: string };
    const serverPipe = (data.ServerPipe ?? '').trim();
    const clientPipe = (data.ClientPipe ?? '').trim();
    if (!serverPipe || !clientPipe) {
      getRuntimeLogChannel()?.error('Project pipes.json requires non-empty ServerPipe and ClientPipe values.');
      return undefined;
    }

    return { serverPipe, clientPipe };
  } catch (error) {
    if (!(error instanceof vscode.FileSystemError && error.code === 'FileNotFound')) {
      getRuntimeLogChannel()?.error('Could not read project pipes.json', error);
    }
    return undefined;
  }
}

function updatePipeBridge(context: vscode.ExtensionContext, config?: ActivePipeConfiguration): void {
  if (!config) {
    disposeActivePipeBridge();
    return;
  }

  const normalized: ActivePipeConfiguration = {
    serverPipe: config.serverPipe.trim(),
    clientPipe: config.clientPipe.trim(),
    reconnectDelay: Math.max(100, config.reconnectDelay),
    source: config.source,
  };

  if (!normalized.serverPipe || !normalized.clientPipe) {
    disposeActivePipeBridge();
    return;
  }

  const matches =
    currentPipeConfig &&
    currentPipeConfig.serverPipe === normalized.serverPipe &&
    currentPipeConfig.clientPipe === normalized.clientPipe &&
    currentPipeConfig.reconnectDelay === normalized.reconnectDelay;

  if (matches) {
    currentPipeConfig = normalized;
    return;
  }

  disposeActivePipeBridge();
  activePipeBridge = createPipeBridgeInstance(context, normalized);
  currentPipeConfig = normalized;
}

function createPipeBridgeInstance(context: vscode.ExtensionContext, config: ActivePipeConfiguration): PipeBridge {
  const logger = {
    info: (message: string) => getPipeOutputChannel(context).appendLine(`[info] ${message}`),
    error: (message: string, error?: unknown) => {
      const channel = getPipeOutputChannel(context);
      channel.appendLine(`[error] ${message}`);
      if (error) {
        if (error instanceof Error) {
          channel.appendLine(error.stack ?? error.message);
        } else {
          channel.appendLine(String(error));
        }
      }
    },
  };

  const bridge = new PipeBridge({
    serverPipe: config.serverPipe,
    clientPipe: config.clientPipe,
    reconnectDelay: config.reconnectDelay,
    logger,
  });

  const channel = getPipeOutputChannel(context);
  channel.appendLine(`[pipe] Listening on ${config.clientPipe} â† Runtime, sending via ${config.serverPipe} â†’ Runtime (${config.source})`);

  activePipeSubscription?.dispose();
  activePipeSubscription = bridge.onMessage((command: PipeCommandPayload) => {
    const suffix = Array.isArray(command.Args) && command.Args.length > 0 ? ` :: ${JSON.stringify(command.Args)}` : '';
    channel.appendLine(`[recv] ${command.Command}${suffix}`);
    handleIncomingPipeCommand(command, pipeCommandReceiverContext);
  });

  activePipeStatusSubscription?.dispose();
  activePipeStatusSubscription = bridge.onStatus((status: PipeConnectionState) => {
    getRuntimeLogChannel()?.info(`Runtime pipe ${status}: ${config.clientPipe}`);
    broadcastPipeStatus(status);
  });

  return bridge;
}

function disposeActivePipeBridge(): void {
  activePipeSubscription?.dispose();
  activePipeSubscription = undefined;
  activePipeStatusSubscription?.dispose();
  activePipeStatusSubscription = undefined;
  if (activePipeBridge) {
    try {
      activePipeBridge.dispose();
    } catch {
      // ignore
    }
    activePipeBridge = undefined;
  }
  currentPipeConfig = undefined;
  broadcastPipeStatus('disconnected');
}

function broadcastPipeStatus(status: PipeConnectionState): void {
  pipeConnectionState = status;
  viewProviderRef?.updatePipeStatus(status);
  if (status === 'connected') {
    const runtimeChannel = getRuntimeLogChannel();
    runtimeChannel?.show(true);
  }
}


function getPipeOutputChannel(context: vscode.ExtensionContext): vscode.OutputChannel {
  if (!pipeOutputChannel) {
    pipeOutputChannel = vscode.window.createOutputChannel('qBook Pipes');
    context.subscriptions.push(pipeOutputChannel);
  }

  return pipeOutputChannel;
}

function getRuntimeLogChannel(): vscode.LogOutputChannel | undefined {
  if (!extensionContextRef) {
    return undefined;
  }
  if (!runtimeLogChannel) {
    runtimeLogChannel = vscode.window.createOutputChannel('qBook Runtime', { log: true });
    extensionContextRef.subscriptions.push(runtimeLogChannel);
  }
  return runtimeLogChannel;
}


class QBookViewProvider implements vscode.WebviewViewProvider {
  private currentView?: vscode.WebviewView;
  private lastBookRoot?: vscode.Uri;
  private selectedPath?: string;
  private errorPaths = new Set<string>();
  private lastPayload?: BookTreePayload;
  private pipeStatus: PipeConnectionState = 'disconnected';
  private runtimeButtonState: RuntimeButtonState = {
    initialize: null,
    run: null,
    stop: null,
    rebuild: null,
  };
  private managedDebugSession?: vscode.DebugSession;
  private isRenamingPage = false;
  private debugAttachRunCounter = 0;
  private knownDebugSessions = new Map<string, vscode.DebugSession>();
  private runtimeInFlight?: RuntimeButtonId;
  private autoRun = true;
  private autoBackup = true;
  private rebuildAutomation?: RebuildAutomation;

  public constructor(private readonly context: vscode.ExtensionContext) {
    this.context.subscriptions.push(
      ...registerDebugSessionLifecycle({
        knownDebugSessions: this.knownDebugSessions,
        isManagedDebugSession: (session: vscode.DebugSession) => isManagedQbookDebugSession(session),
        getManagedDebugSession: () => this.managedDebugSession,
        setManagedDebugSession: (session: vscode.DebugSession | undefined) => {
          this.managedDebugSession = session;
        },
        getCurrentView: () => this.currentView,
        getRuntimeLogChannel: () => getRuntimeLogChannel(),
      })
    );
  }

  public resolveWebviewView(webviewView: vscode.WebviewView): void {
    this.currentView = webviewView;
    webviewView.onDidDispose(() => {
      if (this.currentView === webviewView) {
        this.currentView = undefined;
      }
    });

    webviewView.webview.options = {
      enableScripts: true,
      localResourceRoots: [vscode.Uri.joinPath(this.context.extensionUri, 'media')],
    };

    webviewView.webview.onDidReceiveMessage(async (message: BridgeMessage) => {
      await dispatchWebviewMessage({
        message,
        syncAutomationOptions: (options) => this.syncAutomationOptions(options),
        beginRebuildAutomation: () => this.beginRebuildAutomation(),
        isAutomationActive: (operation) => this.rebuildAutomation === operation,
        sendPreparedRebuild: (operation) => this.sendAutomationStage(operation, 'rebuild'),
        failRebuildAutomation: (operation, detail) => {
          if (this.rebuildAutomation === operation) this.finishRuntimeFailure('rebuild', detail);
        },
        pipeCommandSenderContext,
        notifyRuntimeRequested: (button) => this.notifyRuntimeRequested(button),
        notifyRuntimeFailed: (button, detail) => this.finishRuntimeFailure(button, detail),
        loadAndSendTreeData: () => this.loadAndSendTreeData(webviewView),
        openRelativeFile: (relativePath?: string) => this.openRelativeFile(relativePath),
        verifyAllTreeCsFiles: () => this.verifyAllTreeCsFiles(),
        applyRuntimeHighlight: (button: RuntimeButtonId, kind: RuntimeSignalKind) => {
          this.applyRuntimeHighlight(button, kind);
        },
        currentView: this.currentView,
        managedDebugSession: this.managedDebugSession,
        setManagedDebugSession: (session: vscode.DebugSession | undefined) => {
          this.managedDebugSession = session;
        },
        knownDebugSessions: this.knownDebugSessions,
        isManagedDebugSession: (session: vscode.DebugSession) => isManagedQbookDebugSession(session),
        lastBookRoot: this.lastBookRoot,
        nextDebugRunId: () => ++this.debugAttachRunCounter,
        getRuntimeLogChannel: () => getRuntimeLogChannel(),
        applyMetadataPatch: (pageName: string, patch: Partial<PageMetadata>) => {
          this.applyMetadataPatch(pageName, patch);
        },
        isRenamingPage: this.isRenamingPage,
        setRenamingPage: (value: boolean) => {
          this.isRenamingPage = value;
        },
        reloadTreeView: () => this.reloadTreeView(),
        lastPayload: this.lastPayload,
        selectedPath: this.selectedPath,
        errorPaths: this.errorPaths,
        reorderPayloadNodes: (nextOrder: string[]) => {
          this.reorderPayloadNodes(nextOrder);
        },
      });
    });

    const logoFile = vscode.Uri.joinPath(this.context.extensionUri, 'media', 'amiumlogo2gray.png');
    const logoWebviewUri = webviewView.webview.asWebviewUri(logoFile);
    webviewView.webview.html = getWebviewHtml(webviewView.webview, logoWebviewUri);

    this.loadAndSendTreeData(webviewView).catch((error: unknown) => {
      const details = error instanceof Error ? error.message : String(error);
      webviewView.webview.postMessage({ type: 'bookError', message: details });
    });

    postPipeStatus(this.currentView, this.pipeStatus);
    postRuntimeState(this.currentView, this.runtimeButtonState);
    postDebugState(this.currentView, Boolean(this.managedDebugSession));
  }

  private async loadAndSendTreeData(webviewView: vscode.WebviewView): Promise<void> {
    try {
      const payload = await this.buildTreePayload();

      if (!payload) {
        this.clearProjectState();
        webviewView.webview.postMessage({
          type: 'bookError',
          message: 'No Book.json found in the current workspace.',
        });
        return;
      }

      webviewView.webview.postMessage({ type: 'bookData', payload });
      this.lastPayload = payload;
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    } catch (error) {
      const details = error instanceof Error ? error.message : String(error);
      webviewView.webview.postMessage({
        type: 'bookError',
        message: `Error loading Book.json: ${details}`,
      });
    }
  }

  private async buildTreePayload(): Promise<BookTreePayload | undefined> {
    const bookUri = await findBookFileInWorkspace();

    if (!bookUri) {
      this.clearProjectState();
      return undefined;
    }

    const bookRoot = getDirectoryUri(bookUri);
    this.lastBookRoot = bookRoot;
    await refreshWorkspacePipeBridge(this.context);
    this.refreshDiagnostics();
    this.handleActiveEditorChange(vscode.window.activeTextEditor);
    const fileContents = await vscode.workspace.fs.readFile(bookUri);
    const bookText = textDecoder.decode(fileContents);
    const bookData = JSON.parse(bookText) as {
      ProjectName?: string;
      PageOrder?: string[];
    };

    const orderedPages = Array.isArray(bookData.PageOrder)
      ? bookData.PageOrder
      : await discoverPageFolders(bookRoot);

    const nodes: BookTreeNode[] = [];

    for (const pageName of orderedPages) {
      if (typeof pageName !== 'string' || !pageName) {
        continue;
      }
      const node = await readPageFolder(bookRoot, pageName);
      nodes.push(node);
    }

    return {
      projectName: bookData.ProjectName,
      rootPath: bookRoot.fsPath,
      nodes,
    };
  }

  private async openRelativeFile(relativePath?: string, options?: { preview?: boolean; preserveFocus?: boolean }): Promise<void> {
    const baseUri = this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri;
    const normalizedRelative = await openRelativeBookFile(relativePath, baseUri, {
      preview: options?.preview,
      preserveFocus: options?.preserveFocus,
    });
    if (normalizedRelative) {
      this.selectedPath = normalizedRelative;
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    }
  }

  private async verifyAllTreeCsFiles(): Promise<void> {
    await verifyTreeCsFilesWorkflow(this.lastPayload, {
      baseUri: this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri,
      postStatusText: (text: string) => postStatusText(this.currentView, text),
      openRelativeFile: (relativePath: string, options?: { preview?: boolean; preserveFocus?: boolean }) =>
        this.openRelativeFile(relativePath, options),
      refreshDiagnostics: () => this.refreshDiagnostics(),
      getErrorPaths: () => this.errorPaths,
    });
  }

  private async reloadTreeView(): Promise<void> {
    if (!this.currentView) {
      return;
    }
    await this.loadAndSendTreeData(this.currentView);
  }

  private applyMetadataPatch(pageName: string, patch: Partial<PageMetadata>): void {
    if (!this.lastPayload || !this.currentView) {
      return;
    }

    const target = this.lastPayload.nodes.find((node) => node.page === pageName);
    if (!target) {
      return;
    }

    const base: PageMetadata =
      target.metadata ?? { name: pageName, title: pageName, format: 'A4', hidden: false };
    target.metadata = { ...base, ...patch } as PageMetadata;

    this.currentView.webview.postMessage({ type: 'bookData', payload: this.lastPayload });
    postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
  }

  private reorderPayloadNodes(order: string[]): void {
    if (!this.lastPayload || !Array.isArray(order) || !order.length) {
      return;
    }

    const lookup = new Map(this.lastPayload.nodes.map((node) => [node.page, node] as const));
    const nextNodes: BookTreeNode[] = [];

    for (const page of order) {
      const node = lookup.get(page);
      if (node) {
        nextNodes.push(node);
        lookup.delete(page);
      }
    }

    for (const node of lookup.values()) {
      nextNodes.push(node);
    }

    this.lastPayload.nodes = nextNodes;
  }

  public handleActiveEditorChange(editor: vscode.TextEditor | undefined): void {
    const relative = relativePathFromUri(this.lastBookRoot, editor?.document?.uri);
    if (relative !== this.selectedPath) {
      this.selectedPath = relative;
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    }
  }

  public refreshDiagnostics(): void {
    if (!this.lastBookRoot) {
      if (this.errorPaths.size > 0) {
        this.errorPaths.clear();
        postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
      }
      return;
    }

    const basePath = path.resolve(this.lastBookRoot.fsPath);
    const diagnostics = vscode.languages.getDiagnostics();
    const next = new Set<string>();

    for (const [uri, diagList] of diagnostics) {
      if (!diagList.some((diag: vscode.Diagnostic) => diag.severity === vscode.DiagnosticSeverity.Error)) {
        continue;
      }

      const absolutePath = path.resolve(uri.fsPath);
      if (!absolutePath.toLowerCase().startsWith(basePath.toLowerCase())) {
        continue;
      }

      const relative = normalizeRelativePath(path.relative(basePath, absolutePath));
      next.add(relative);
    }

    if (!areStringSetsEqual(this.errorPaths, next)) {
      this.errorPaths = next;
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    } else {
      this.errorPaths = next;
    }
  }

  public updatePipeStatus(status: PipeConnectionState): void {
    this.pipeStatus = status;
    postPipeStatus(this.currentView, this.pipeStatus);
    if (status === 'disconnected') {
      if (this.runtimeInFlight) {
        this.finishRuntimeFailure(this.runtimeInFlight, 'Pipe disconnected; host completion is unknown.');
      } else {
        postRuntimeInfo(this.currentView, 'Runtime offline');
      }
    }
  }

  public notifyRuntimeSignal(signal: RuntimeSignal, command?: PipeCommandPayload): void {
    const lifecycle = command?.Args?.includes('Lifecycle');
    const operation = this.rebuildAutomation;
    if (operation) {
      if (signal.kind === 'alert') {
        this.finishRuntimeFailure(this.runtimeInFlight ?? 'rebuild', command?.Args?.[1] ?? 'Host action failed.');
        return;
      }
      if (!lifecycle || operation.stage === 'preparing' || signal.button !== operation.stage) return;
    }
    if (!lifecycle && (this.runtimeInFlight || Object.values(this.runtimeButtonState).some(state => state === 'alert'))) return;
    if (signal.kind === 'progress') {
      if (!this.runtimeInFlight) this.notifyRuntimeRequested(signal.button);
      return;
    }
    if (this.runtimeInFlight && signal.button !== this.runtimeInFlight) return;
    if (lifecycle) this.runtimeInFlight = undefined;
    if (signal.kind === 'alert') {
      this.finishRuntimeFailure(signal.button, command?.Args?.[1] ?? 'Host action failed.');
      return;
    }
    this.applyRuntimeHighlight(signal.button, signal.kind);
    const label = signal.button.charAt(0).toUpperCase() + signal.button.slice(1);
    const timestamp = extractRuntimeTimestamp(command?.Args);
    postStatusText(this.currentView, label + ' completed');
    postRuntimeInfo(this.currentView, 'Runtime status: ' + label + (timestamp ? ' @ ' + timestamp : ''));
    if (operation && lifecycle && signal.kind === 'status') {
      if (operation.stage === 'rebuild' && operation.autoRun) {
        void this.sendAutomationStage(operation, 'initialize');
      } else if (operation.stage === 'initialize') {
        void this.sendAutomationStage(operation, 'run');
      } else {
        this.rebuildAutomation = undefined;
      }
    }
  }

  private syncAutomationOptions(message?: BridgeMessage): void {
    if (typeof message?.autoRun === 'boolean') this.autoRun = message.autoRun;
    if (typeof message?.autoBackup === 'boolean') this.autoBackup = message.autoBackup;
    this.currentView?.webview.postMessage({
      type: 'automationState',
      payload: { autoRun: this.autoRun, autoBackup: this.autoBackup },
    });
  }

  private beginRebuildAutomation(): RebuildAutomation | undefined {
    if (!this.notifyRuntimeRequested('rebuild')) return undefined;
    const operation: RebuildAutomation = {
      autoRun: this.autoRun,
      autoBackup: this.autoBackup,
      stage: 'preparing',
    };
    this.rebuildAutomation = operation;
    return operation;
  }

  private async sendAutomationStage(
    operation: RebuildAutomation,
    stage: 'rebuild' | 'initialize' | 'run'
  ): Promise<void> {
    if (this.rebuildAutomation !== operation) return;
    operation.stage = stage;
    this.runtimeInFlight = stage;
    this.applyRuntimeHighlight(stage, 'progress');
    postStatusText(this.currentView, stage + ' running');
    getRuntimeLogChannel()?.info(stage + ' requested');
    const command = stage.charAt(0).toUpperCase() + stage.slice(1);
    const sent = await sendRuntimeCommand(pipeCommandSenderContext, command);
    if (!sent && this.rebuildAutomation === operation && operation.stage === stage) {
      this.finishRuntimeFailure(stage, 'Runtime command could not be sent; host completion is unknown.');
    }
  }

  private notifyRuntimeRequested(button: RuntimeButtonId): boolean {
    if (this.runtimeInFlight) {
      vscode.window.showErrorMessage('A runtime action is still pending.');
      return false;
    }
    this.runtimeInFlight = button;
    this.applyRuntimeHighlight(button, 'progress');
    postStatusText(this.currentView, button + ' running');
    getRuntimeLogChannel()?.info(button + ' requested');
    return true;
  }

  private finishRuntimeFailure(button: RuntimeButtonId, detail: string): void {
    this.rebuildAutomation = undefined;
    this.runtimeInFlight = undefined;
    this.applyRuntimeHighlight(button, 'alert');
    const message = button + ' failed: ' + detail;
    postStatusText(this.currentView, message);
    postRuntimeInfo(this.currentView, message);
    getRuntimeLogChannel()?.error(message);
  }

  private applyRuntimeHighlight(button: RuntimeButtonId, kind: RuntimeSignalKind): void {
    if (this.runtimeButtonState[button] === kind) {
      return;
    }

    this.runtimeButtonState.initialize = null;
    this.runtimeButtonState.run = null;
    this.runtimeButtonState.stop = null;
    this.runtimeButtonState.rebuild = null;
    this.runtimeButtonState[button] = kind;
    postRuntimeState(this.currentView, this.runtimeButtonState);
  }

  private clearProjectState(): void {
    const cleared = createClearedProjectViewState(
      this.lastBookRoot,
      this.selectedPath,
      this.errorPaths,
      this.lastPayload
    );

    this.lastBookRoot = cleared.lastBookRoot;
    this.selectedPath = cleared.selectedPath;
    this.errorPaths = cleared.errorPaths;
    this.lastPayload = cleared.lastPayload;
    if (cleared.hadState) {
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    }

  }

}

