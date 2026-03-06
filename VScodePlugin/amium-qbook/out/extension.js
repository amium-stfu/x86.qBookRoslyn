"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const vscode = __importStar(require("vscode"));
const path = __importStar(require("node:path"));
const util_1 = require("util");
const namedPipeClients_1 = require("./pipes/namedPipeClients");
const pipeCommands_js_1 = require("./pipes/pipeCommands.js");
const setUtils_js_1 = require("./services/common/setUtils.js");
const fileOpenService_js_1 = require("./services/book/fileOpenService.js");
const projectStateService_js_1 = require("./services/book/projectStateService.js");
const verifyWorkflowService_js_1 = require("./services/book/verifyWorkflowService.js");
const treeService_js_1 = require("./services/book/treeService.js");
const verificationService_js_1 = require("./services/book/verificationService.js");
const helpers_js_1 = require("./services/debug/helpers.js");
const sessionLifecycleService_js_1 = require("./services/debug/sessionLifecycleService.js");
const bookStatusService_js_1 = require("./services/ui/bookStatusService.js");
const webviewStatusService_js_1 = require("./services/ui/webviewStatusService.js");
const webviewMessageDispatcher_js_1 = require("./services/ui/webviewMessageDispatcher.js");
const panelHtml_js_1 = require("./webview/panelHtml.js");
const COMMAND_ID = 'amium-qbook.bridge';
const VIEW_ID = 'amium-qbook.panel';
const textDecoder = new util_1.TextDecoder('utf-8');
let activePipeBridge;
let activePipeSubscription;
let pipeOutputChannel;
let runtimeLogChannel;
let currentPipeConfig;
let lastProjectRoot;
let pipeConnectionState = 'disconnected';
let activePipeStatusSubscription;
let viewProviderRef;
let extensionContextRef;
const pipeCommandReceiverContext = {
    notifyRuntimeSignal: (signal, command) => viewProviderRef?.notifyRuntimeSignal(signal, command),
    getRuntimeLogChannel,
};
const pipeCommandSenderContext = {
    getBridge: () => activePipeBridge,
    showErrorMessage: (message) => vscode.window.showErrorMessage(message),
    broadcastStatus: broadcastPipeStatus,
};
function activate(context) {
    extensionContextRef = context;
    const viewProvider = new QBookViewProvider(context);
    viewProviderRef = viewProvider;
    viewProviderRef.updatePipeStatus(pipeConnectionState);
    context.subscriptions.push(vscode.window.registerWebviewViewProvider(VIEW_ID, viewProvider, {
        webviewOptions: { retainContextWhenHidden: true },
    }));
    context.subscriptions.push(vscode.window.onDidChangeActiveTextEditor((editor) => {
        viewProvider.handleActiveEditorChange(editor);
    }));
    context.subscriptions.push(vscode.languages.onDidChangeDiagnostics(() => {
        viewProvider.refreshDiagnostics();
    }));
    const openPanelCommand = vscode.commands.registerCommand(COMMAND_ID, async () => {
        await vscode.commands.executeCommand(`${VIEW_ID}.focus`);
    });
    context.subscriptions.push(openPanelCommand);
    viewProvider.handleActiveEditorChange(vscode.window.activeTextEditor);
    viewProvider.refreshDiagnostics();
    applySettingsPipeBridge(context);
    context.subscriptions.push(vscode.workspace.onDidChangeConfiguration((event) => {
        if (event.affectsConfiguration('amium-qbook.pipe.serverName') ||
            event.affectsConfiguration('amium-qbook.pipe.clientName') ||
            event.affectsConfiguration('amium-qbook.pipe.reconnectDelay')) {
            if (currentPipeConfig?.source === 'project' && lastProjectRoot) {
                void applyProjectPipeBridge(context, lastProjectRoot);
            }
            else {
                applySettingsPipeBridge(context);
            }
        }
    }));
}
function deactivate() {
    disposeActivePipeBridge();
}
function applySettingsPipeBridge(context) {
    const config = vscode.workspace.getConfiguration('amium-qbook');
    const serverPipe = (config.get('pipe.serverName') ?? '').trim();
    const clientPipe = (config.get('pipe.clientName') ?? '').trim();
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
async function applyProjectPipeBridge(context, bookRoot) {
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
function getReconnectDelaySetting() {
    return Math.max(100, vscode.workspace.getConfiguration('amium-qbook').get('pipe.reconnectDelay') ?? 500);
}
async function readProjectPipeConfig(bookRoot) {
    try {
        const fileUri = vscode.Uri.joinPath(bookRoot, 'pipes.json');
        const raw = await vscode.workspace.fs.readFile(fileUri);
        const text = textDecoder.decode(raw);
        const data = JSON.parse(text);
        const serverPipe = (data.ServerPipe ?? '').trim();
        const clientPipe = (data.ClientPipe ?? '').trim();
        if (!serverPipe || !clientPipe) {
            return undefined;
        }
        return { serverPipe, clientPipe };
    }
    catch {
        return undefined;
    }
}
function updatePipeBridge(context, config) {
    if (!config) {
        disposeActivePipeBridge();
        return;
    }
    const normalized = {
        serverPipe: config.serverPipe.trim(),
        clientPipe: config.clientPipe.trim(),
        reconnectDelay: Math.max(100, config.reconnectDelay),
        source: config.source,
    };
    if (!normalized.serverPipe || !normalized.clientPipe) {
        disposeActivePipeBridge();
        return;
    }
    const matches = currentPipeConfig &&
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
function createPipeBridgeInstance(context, config) {
    const logger = {
        info: (message) => getPipeOutputChannel(context).appendLine(`[info] ${message}`),
        error: (message, error) => {
            const channel = getPipeOutputChannel(context);
            channel.appendLine(`[error] ${message}`);
            if (error) {
                if (error instanceof Error) {
                    channel.appendLine(error.stack ?? error.message);
                }
                else {
                    channel.appendLine(String(error));
                }
            }
        },
    };
    const bridge = new namedPipeClients_1.PipeBridge({
        serverPipe: config.serverPipe,
        clientPipe: config.clientPipe,
        reconnectDelay: config.reconnectDelay,
        logger,
    });
    const channel = getPipeOutputChannel(context);
    channel.appendLine(`[pipe] Listening on ${config.clientPipe} â† Runtime, sending via ${config.serverPipe} â†’ Runtime (${config.source})`);
    activePipeSubscription?.dispose();
    activePipeSubscription = bridge.onMessage((command) => {
        const suffix = Array.isArray(command.Args) && command.Args.length > 0 ? ` :: ${JSON.stringify(command.Args)}` : '';
        channel.appendLine(`[recv] ${command.Command}${suffix}`);
        (0, pipeCommands_js_1.handleIncomingPipeCommand)(command, pipeCommandReceiverContext);
    });
    activePipeStatusSubscription?.dispose();
    activePipeStatusSubscription = bridge.onStatus((status) => {
        broadcastPipeStatus(status);
    });
    return bridge;
}
function disposeActivePipeBridge() {
    activePipeSubscription?.dispose();
    activePipeSubscription = undefined;
    activePipeStatusSubscription?.dispose();
    activePipeStatusSubscription = undefined;
    if (activePipeBridge) {
        try {
            activePipeBridge.dispose();
        }
        catch {
            // ignore
        }
        activePipeBridge = undefined;
    }
    currentPipeConfig = undefined;
    broadcastPipeStatus('disconnected');
}
function broadcastPipeStatus(status) {
    pipeConnectionState = status;
    viewProviderRef?.updatePipeStatus(status);
    if (status === 'connected') {
        const runtimeChannel = getRuntimeLogChannel();
        runtimeChannel?.show(true);
    }
}
function getPipeOutputChannel(context) {
    if (!pipeOutputChannel) {
        pipeOutputChannel = vscode.window.createOutputChannel('qBook Pipes');
        context.subscriptions.push(pipeOutputChannel);
    }
    return pipeOutputChannel;
}
function getRuntimeLogChannel() {
    if (!extensionContextRef) {
        return undefined;
    }
    if (!runtimeLogChannel) {
        runtimeLogChannel = vscode.window.createOutputChannel('qBook Runtime', { log: true });
        extensionContextRef.subscriptions.push(runtimeLogChannel);
    }
    return runtimeLogChannel;
}
class QBookViewProvider {
    context;
    currentView;
    lastBookRoot;
    selectedPath;
    errorPaths = new Set();
    lastPayload;
    pipeStatus = 'disconnected';
    runtimeButtonState = {
        run: null,
        stop: null,
        rebuild: null,
    };
    runtimeHighlightTimers = new Map();
    managedDebugSession;
    isRenamingPage = false;
    debugAttachRunCounter = 0;
    knownDebugSessions = new Map();
    constructor(context) {
        this.context = context;
        this.context.subscriptions.push(...(0, sessionLifecycleService_js_1.registerDebugSessionLifecycle)({
            knownDebugSessions: this.knownDebugSessions,
            isManagedDebugSession: (session) => (0, helpers_js_1.isManagedQbookDebugSession)(session),
            getManagedDebugSession: () => this.managedDebugSession,
            setManagedDebugSession: (session) => {
                this.managedDebugSession = session;
            },
            getCurrentView: () => this.currentView,
            getRuntimeLogChannel: () => getRuntimeLogChannel(),
        }));
    }
    resolveWebviewView(webviewView) {
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
        webviewView.webview.onDidReceiveMessage(async (message) => {
            await (0, webviewMessageDispatcher_js_1.dispatchWebviewMessage)({
                message,
                pipeCommandSenderContext,
                loadAndSendTreeData: () => this.loadAndSendTreeData(webviewView),
                openRelativeFile: (relativePath) => this.openRelativeFile(relativePath),
                verifyAllTreeCsFiles: () => this.verifyAllTreeCsFiles(),
                applyRuntimeHighlight: (button, kind) => {
                    this.applyRuntimeHighlight(button, kind);
                },
                currentView: this.currentView,
                managedDebugSession: this.managedDebugSession,
                setManagedDebugSession: (session) => {
                    this.managedDebugSession = session;
                },
                knownDebugSessions: this.knownDebugSessions,
                isManagedDebugSession: (session) => (0, helpers_js_1.isManagedQbookDebugSession)(session),
                lastBookRoot: this.lastBookRoot,
                nextDebugRunId: () => ++this.debugAttachRunCounter,
                getRuntimeLogChannel: () => getRuntimeLogChannel(),
                applyMetadataPatch: (pageName, patch) => {
                    this.applyMetadataPatch(pageName, patch);
                },
                isRenamingPage: this.isRenamingPage,
                setRenamingPage: (value) => {
                    this.isRenamingPage = value;
                },
                reloadTreeView: () => this.reloadTreeView(),
                lastPayload: this.lastPayload,
                selectedPath: this.selectedPath,
                errorPaths: this.errorPaths,
                reorderPayloadNodes: (nextOrder) => {
                    this.reorderPayloadNodes(nextOrder);
                },
            });
        });
        const logoFile = vscode.Uri.joinPath(this.context.extensionUri, 'media', 'amiumlogo2gray.png');
        const logoWebviewUri = webviewView.webview.asWebviewUri(logoFile);
        webviewView.webview.html = (0, panelHtml_js_1.getWebviewHtml)(webviewView.webview, logoWebviewUri);
        this.loadAndSendTreeData(webviewView).catch((error) => {
            const details = error instanceof Error ? error.message : String(error);
            webviewView.webview.postMessage({ type: 'bookError', message: details });
        });
        (0, webviewStatusService_js_1.postPipeStatus)(this.currentView, this.pipeStatus);
        (0, webviewStatusService_js_1.postRuntimeState)(this.currentView, this.runtimeButtonState);
        (0, webviewStatusService_js_1.postDebugState)(this.currentView, Boolean(this.managedDebugSession));
    }
    async loadAndSendTreeData(webviewView) {
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
            (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            webviewView.webview.postMessage({
                type: 'bookError',
                message: `Fehler beim Laden der Book.json: ${details}`,
            });
        }
    }
    async buildTreePayload() {
        const bookUri = await (0, treeService_js_1.findBookFileInWorkspace)();
        if (!bookUri) {
            this.clearProjectState();
            return undefined;
        }
        const bookRoot = (0, treeService_js_1.getDirectoryUri)(bookUri);
        this.lastBookRoot = bookRoot;
        lastProjectRoot = bookRoot;
        await applyProjectPipeBridge(this.context, bookRoot);
        this.refreshDiagnostics();
        this.handleActiveEditorChange(vscode.window.activeTextEditor);
        const fileContents = await vscode.workspace.fs.readFile(bookUri);
        const bookText = textDecoder.decode(fileContents);
        const bookData = JSON.parse(bookText);
        const orderedPages = Array.isArray(bookData.PageOrder)
            ? bookData.PageOrder
            : await (0, treeService_js_1.discoverPageFolders)(bookRoot);
        const nodes = [];
        for (const pageName of orderedPages) {
            if (typeof pageName !== 'string' || !pageName) {
                continue;
            }
            const node = await (0, treeService_js_1.readPageFolder)(bookRoot, pageName);
            nodes.push(node);
        }
        return {
            projectName: bookData.ProjectName,
            rootPath: bookRoot.fsPath,
            nodes,
        };
    }
    async openRelativeFile(relativePath, options) {
        const baseUri = this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri;
        const normalizedRelative = await (0, fileOpenService_js_1.openRelativeFile)(relativePath, baseUri, {
            preview: options?.preview,
            preserveFocus: options?.preserveFocus,
        });
        if (normalizedRelative) {
            this.selectedPath = normalizedRelative;
            (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
        }
    }
    async verifyAllTreeCsFiles() {
        await (0, verifyWorkflowService_js_1.verifyAllTreeCsFiles)(this.lastPayload, {
            baseUri: this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri,
            postStatusText: (text) => (0, webviewStatusService_js_1.postStatusText)(this.currentView, text),
            openRelativeFile: (relativePath, options) => this.openRelativeFile(relativePath, options),
            refreshDiagnostics: () => this.refreshDiagnostics(),
            getErrorPaths: () => this.errorPaths,
        });
    }
    async reloadTreeView() {
        if (!this.currentView) {
            return;
        }
        await this.loadAndSendTreeData(this.currentView);
    }
    applyMetadataPatch(pageName, patch) {
        if (!this.lastPayload || !this.currentView) {
            return;
        }
        const target = this.lastPayload.nodes.find((node) => node.page === pageName);
        if (!target) {
            return;
        }
        const base = target.metadata ?? { name: pageName, title: pageName, format: 'A4', hidden: false };
        target.metadata = { ...base, ...patch };
        this.currentView.webview.postMessage({ type: 'bookData', payload: this.lastPayload });
        (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
    }
    reorderPayloadNodes(order) {
        if (!this.lastPayload || !Array.isArray(order) || !order.length) {
            return;
        }
        const lookup = new Map(this.lastPayload.nodes.map((node) => [node.page, node]));
        const nextNodes = [];
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
    handleActiveEditorChange(editor) {
        const relative = (0, verificationService_js_1.relativePathFromUri)(this.lastBookRoot, editor?.document?.uri);
        if (relative !== this.selectedPath) {
            this.selectedPath = relative;
            (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
        }
    }
    refreshDiagnostics() {
        if (!this.lastBookRoot) {
            if (this.errorPaths.size > 0) {
                this.errorPaths.clear();
                (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
            }
            return;
        }
        const basePath = path.resolve(this.lastBookRoot.fsPath);
        const diagnostics = vscode.languages.getDiagnostics();
        const next = new Set();
        for (const [uri, diagList] of diagnostics) {
            if (!diagList.some((diag) => diag.severity === vscode.DiagnosticSeverity.Error)) {
                continue;
            }
            const absolutePath = path.resolve(uri.fsPath);
            if (!absolutePath.toLowerCase().startsWith(basePath.toLowerCase())) {
                continue;
            }
            const relative = (0, verificationService_js_1.normalizeRelativePath)(path.relative(basePath, absolutePath));
            next.add(relative);
        }
        if (!(0, setUtils_js_1.areStringSetsEqual)(this.errorPaths, next)) {
            this.errorPaths = next;
            (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
        }
        else {
            this.errorPaths = next;
        }
    }
    updatePipeStatus(status) {
        this.pipeStatus = status;
        (0, webviewStatusService_js_1.postPipeStatus)(this.currentView, this.pipeStatus);
    }
    notifyRuntimeSignal(signal, command) {
        this.applyRuntimeHighlight(signal.button, signal.kind);
        const timestamp = (0, pipeCommands_js_1.extractRuntimeTimestamp)(command?.Args);
        const label = signal.button === 'run' ? 'Run' : signal.button === 'rebuild' ? 'Rebuild' : 'Stop';
        const nextStatus = timestamp
            ? `Runtime ${signal.kind}: ${label} @ ${timestamp}`
            : `Runtime ${signal.kind}: ${label}`;
        if (signal.kind === 'status') {
            if (signal.button === 'rebuild') {
                (0, webviewStatusService_js_1.postStatusText)(this.currentView, 'Rebuild done');
            }
            else if (signal.button === 'run') {
                (0, webviewStatusService_js_1.postStatusText)(this.currentView, 'Running...');
            }
            else if (signal.button === 'stop') {
                (0, webviewStatusService_js_1.postStatusText)(this.currentView, 'Stopped');
            }
        }
        (0, webviewStatusService_js_1.postRuntimeInfo)(this.currentView, nextStatus);
    }
    applyRuntimeHighlight(button, kind) {
        this.runtimeButtonState[button] = kind;
        (0, webviewStatusService_js_1.postRuntimeState)(this.currentView, this.runtimeButtonState);
        const existingTimer = this.runtimeHighlightTimers.get(button);
        if (existingTimer) {
            clearTimeout(existingTimer);
        }
        const timer = setTimeout(() => {
            this.runtimeHighlightTimers.delete(button);
            if (this.runtimeButtonState[button] === kind) {
                this.runtimeButtonState[button] = null;
                (0, webviewStatusService_js_1.postRuntimeState)(this.currentView, this.runtimeButtonState);
            }
        }, 4000);
        this.runtimeHighlightTimers.set(button, timer);
    }
    clearProjectState() {
        const cleared = (0, projectStateService_js_1.createClearedProjectViewState)(this.lastBookRoot, this.selectedPath, this.errorPaths, this.lastPayload);
        this.lastBookRoot = cleared.lastBookRoot;
        lastProjectRoot = undefined;
        this.selectedPath = cleared.selectedPath;
        this.errorPaths = cleared.errorPaths;
        this.lastPayload = cleared.lastPayload;
        if (cleared.hadState) {
            (0, bookStatusService_js_1.postBookStatus)(this.currentView, this.selectedPath, this.errorPaths, this.lastPayload);
        }
        if (currentPipeConfig?.source === 'project') {
            applySettingsPipeBridge(this.context);
        }
    }
}
//# sourceMappingURL=extension.js.map