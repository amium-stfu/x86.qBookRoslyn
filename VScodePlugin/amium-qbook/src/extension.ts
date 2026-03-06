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
} from './pipes/pipeCommands.js';
import {
  ActivePipeConfiguration,
  BookTreePayload,
  BookTreeNode,
  BridgeMessage,
  PageMetadata,
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

  applySettingsPipeBridge(context);

  context.subscriptions.push(
    vscode.workspace.onDidChangeConfiguration((event: vscode.ConfigurationChangeEvent) => {
      if (
        event.affectsConfiguration('amium-qbook.pipe.serverName') ||
        event.affectsConfiguration('amium-qbook.pipe.clientName') ||
        event.affectsConfiguration('amium-qbook.pipe.reconnectDelay')
      ) {
        if (currentPipeConfig?.source === 'project' && lastProjectRoot) {
          void applyProjectPipeBridge(context, lastProjectRoot);
        } else {
          applySettingsPipeBridge(context);
        }
      }
    })
  );
}

export function deactivate(): void {
  disposeActivePipeBridge();
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

async function applyProjectPipeBridge(context: vscode.ExtensionContext, bookRoot: vscode.Uri): Promise<void> {
  const pipes = await readProjectPipeConfig(bookRoot);
  if (!pipes) {
    if (currentPipeConfig?.source === 'project') {
      applySettingsPipeBridge(context);
    }
    return;
  }

  updatePipeBridge(context, {
    ...pipes,
    reconnectDelay: getReconnectDelaySetting(),
    source: 'project',
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
      return undefined;
    }

    return { serverPipe, clientPipe };
  } catch {
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
    run: null,
    stop: null,
    rebuild: null,
  };
  private runtimeHighlightTimers = new Map<RuntimeButtonId, ReturnType<typeof setTimeout>>();
  private managedDebugSession?: vscode.DebugSession;
  private isRenamingPage = false;
  private debugAttachRunCounter = 0;
  private knownDebugSessions = new Map<string, vscode.DebugSession>();

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
        pipeCommandSenderContext,
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
          message: 'Keine Book.json im aktuellen Workspace gefunden.',
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
        message: `Fehler beim Laden der Book.json: ${details}`,
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
    lastProjectRoot = bookRoot;
    await applyProjectPipeBridge(this.context, bookRoot);
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
  }

  public notifyRuntimeSignal(signal: RuntimeSignal, command?: PipeCommandPayload): void {
    this.applyRuntimeHighlight(signal.button, signal.kind);
    const timestamp = extractRuntimeTimestamp(command?.Args);
    const label = signal.button === 'run' ? 'Run' : signal.button === 'rebuild' ? 'Rebuild' : 'Stop';
    const nextStatus = timestamp
      ? `Runtime ${signal.kind}: ${label} @ ${timestamp}`
      : `Runtime ${signal.kind}: ${label}`;

    if (signal.kind === 'status') {
      if (signal.button === 'rebuild') {
        postStatusText(this.currentView, 'Rebuild done');
      } else if (signal.button === 'run') {
        postStatusText(this.currentView, 'Running...');
      } else if (signal.button === 'stop') {
        postStatusText(this.currentView, 'Stopped');
      }
    }

    postRuntimeInfo(this.currentView, nextStatus);
  }

  private applyRuntimeHighlight(button: RuntimeButtonId, kind: RuntimeSignalKind): void {
    this.runtimeButtonState[button] = kind;
    postRuntimeState(this.currentView, this.runtimeButtonState);

    const existingTimer = this.runtimeHighlightTimers.get(button);
    if (existingTimer) {
      clearTimeout(existingTimer);
    }

    const timer = setTimeout(() => {
      this.runtimeHighlightTimers.delete(button);
      if (this.runtimeButtonState[button] === kind) {
        this.runtimeButtonState[button] = null;
        postRuntimeState(this.currentView, this.runtimeButtonState);
      }
    }, 4000);
    this.runtimeHighlightTimers.set(button, timer);
  }

  private clearProjectState(): void {
    const cleared = createClearedProjectViewState(
      this.lastBookRoot,
      this.selectedPath,
      this.errorPaths,
      this.lastPayload
    );

    this.lastBookRoot = cleared.lastBookRoot;
    lastProjectRoot = undefined;
    this.selectedPath = cleared.selectedPath;
    this.errorPaths = cleared.errorPaths;
    this.lastPayload = cleared.lastPayload;
    if (cleared.hadState) {
      postBookStatus(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    }
    if (currentPipeConfig?.source === 'project') {
      applySettingsPipeBridge(this.context);
    }
  }

}

