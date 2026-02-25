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
const COMMAND_ID = 'amium-qbook.bridge';
const VIEW_ID = 'amium-qbook.panel';
const textDecoder = new util_1.TextDecoder('utf-8');
const textEncoder = new util_1.TextEncoder();
let activePipeBridge;
let activePipeSubscription;
let pipeOutputChannel;
let currentPipeConfig;
let lastProjectRoot;
let pipeConnectionState = 'disconnected';
let activePipeStatusSubscription;
let viewProviderRef;
function activate(context) {
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
    channel.appendLine(`[pipe] Listening on ${config.clientPipe} ← Runtime, sending via ${config.serverPipe} → Runtime (${config.source})`);
    activePipeSubscription?.dispose();
    activePipeSubscription = bridge.onMessage((command) => {
        const suffix = Array.isArray(command.Args) && command.Args.length > 0 ? ` :: ${JSON.stringify(command.Args)}` : '';
        channel.appendLine(`[recv] ${command.Command}${suffix}`);
        handleIncomingPipeCommand(command);
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
}
function handleIncomingPipeCommand(command) {
    const signal = parseRuntimeSignal(command);
    if (!signal) {
        return;
    }
    viewProviderRef?.notifyRuntimeSignal(signal, command);
}
function extractRuntimeTimestamp(args) {
    if (!Array.isArray(args) || args.length === 0) {
        return undefined;
    }
    const first = args[0];
    return typeof first === 'string' && first.trim() ? first.trim() : undefined;
}
function parseRuntimeSignal(command) {
    const rawCommand = command?.Command?.trim();
    if (!rawCommand) {
        return undefined;
    }
    let kind;
    let button;
    const colonIndex = rawCommand.indexOf(':');
    if (colonIndex >= 0) {
        const prefix = rawCommand.slice(0, colonIndex);
        const suffix = rawCommand.slice(colonIndex + 1);
        kind = normalizeRuntimeSignalKind(prefix);
        button = normalizeRuntimeButton(suffix) ?? extractRuntimeButtonFromArgs(command.Args);
    }
    else {
        kind = normalizeRuntimeSignalKind(rawCommand);
        if (kind) {
            button = extractRuntimeButtonFromArgs(command.Args);
        }
    }
    if (kind && !button && colonIndex === -1) {
        button = normalizeRuntimeButton(rawCommand) ?? extractRuntimeButtonFromArgs(command.Args);
    }
    if (kind && button) {
        return { kind, button };
    }
    return undefined;
}
function extractRuntimeButtonFromArgs(args) {
    if (!Array.isArray(args)) {
        return undefined;
    }
    for (const entry of args) {
        const button = typeof entry === 'string' ? normalizeRuntimeButton(entry) : undefined;
        if (button) {
            return button;
        }
    }
    return undefined;
}
function normalizeRuntimeSignalKind(value) {
    const normalized = value?.trim().toLowerCase();
    if (normalized === 'status') {
        return 'status';
    }
    if (normalized === 'alert') {
        return 'alert';
    }
    return undefined;
}
function normalizeRuntimeButton(value) {
    const normalized = value?.trim().toLowerCase();
    if (!normalized) {
        return undefined;
    }
    if (normalized === 'run' || normalized === 'start') {
        return 'run';
    }
    if (normalized === 'rebuild' || normalized === 'build' || normalized === 'recompile') {
        return 'rebuild';
    }
    if (normalized === 'stop' || normalized === 'destroy') {
        return 'stop';
    }
    return undefined;
}
async function sendRuntimeCommand(command, args) {
    if (!activePipeBridge) {
        vscode.window.showErrorMessage('Pipe ist nicht verbunden.');
        return;
    }
    const payload = {
        Command: command,
        ...(Array.isArray(args) && args.length > 0 ? { Args: args } : {}),
    };
    try {
        await activePipeBridge.send(payload);
    }
    catch (error) {
        const details = error instanceof Error ? error.message : String(error);
        vscode.window.showErrorMessage(`PipeCommand '${command}' fehlgeschlagen: ${details}`);
        broadcastPipeStatus('disconnected');
    }
}
function withTimeout(promise, timeoutMs, errorMessage) {
    return new Promise((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error(errorMessage)), timeoutMs);
        promise.then((value) => {
            clearTimeout(timer);
            resolve(value);
        }, (error) => {
            clearTimeout(timer);
            reject(error);
        });
    });
}
function collectWorkspaceCSharpErrors() {
    const entries = [];
    const openCSharpDocs = vscode.workspace.textDocuments.filter((doc) => doc.languageId === 'csharp');
    const openCSharpDocUris = new Set(openCSharpDocs.map((doc) => doc.uri.toString()));
    const considered = new Map();
    const diagnostics = vscode.languages.getDiagnostics();
    for (const [uri] of diagnostics) {
        const uriKey = uri.toString();
        if (uri.scheme === 'file') {
            const filePath = uri.fsPath.toLowerCase();
            if (!filePath.endsWith('.cs')) {
                continue;
            }
            // Limit to workspace folders to avoid pulling in random external files.
            if (!vscode.workspace.getWorkspaceFolder(uri)) {
                continue;
            }
            considered.set(uriKey, uri);
            continue;
        }
        // Unsaved/virtual docs: only consider if the document is actually a C# doc.
        if (openCSharpDocUris.has(uriKey)) {
            considered.set(uriKey, uri);
        }
    }
    // Also consider any open C# docs even if they currently don't show up in the global diagnostics list.
    for (const doc of openCSharpDocs) {
        considered.set(doc.uri.toString(), doc.uri);
    }
    for (const uri of considered.values()) {
        const diagList = vscode.languages.getDiagnostics(uri);
        const errors = diagList.filter((diag) => diag.severity === vscode.DiagnosticSeverity.Error);
        if (errors.length > 0) {
            entries.push({ uri, errors });
        }
    }
    return entries;
}
async function waitForNextDiagnosticsUpdate(timeoutMs) {
    await new Promise((resolve) => {
        let settled = false;
        let timeoutHandle;
        const sub = vscode.languages.onDidChangeDiagnostics(() => finish());
        const finish = () => {
            if (settled) {
                return;
            }
            settled = true;
            sub.dispose();
            if (timeoutHandle !== undefined) {
                clearTimeout(timeoutHandle);
            }
            resolve();
        };
        timeoutHandle = setTimeout(() => finish(), timeoutMs);
    });
}
async function ensureNoCSharpErrorsBeforeRebuild() {
    // Make sure edits are persisted so language server diagnostics are up-to-date.
    await vscode.workspace.saveAll(false);
    await waitForNextDiagnosticsUpdate(800);
    const entries = collectWorkspaceCSharpErrors();
    if (entries.length === 0) {
        return true;
    }
    return false;
}
function getPipeOutputChannel(context) {
    if (!pipeOutputChannel) {
        pipeOutputChannel = vscode.window.createOutputChannel('qBook Pipes');
        context.subscriptions.push(pipeOutputChannel);
    }
    return pipeOutputChannel;
}
function getWebviewHtml(webview, logoUri) {
    const nonce = createNonce();
    const cspSource = webview.cspSource;
    const logoSrc = logoUri.toString();
    return `<!DOCTYPE html>
<html lang="de">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta http-equiv="Content-Security-Policy" content="default-src 'none'; img-src ${cspSource} https: data:; style-src 'unsafe-inline'; script-src 'nonce-${nonce}';" />
  <title>qBook Calibration</title>
  <style>
    body {
      font-family: var(--vscode-font-family);
      color: var(--vscode-foreground);
      background: var(--vscode-editor-background);
      margin: 0;
      padding: 12px;
    }

    .brand {
      display: flex;
      justify-content: center;
      margin-bottom: 8px;
    }

    .brand img {
      max-width: 140px;
      height: auto;
      opacity: 0.95;
      image-rendering: -webkit-optimize-contrast;
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      margin-bottom: 12px;
    }

    .pipe-status {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 10px;
      border-radius: 4px;
      font-size: 12px;
      margin-bottom: 12px;
      border: 1px solid transparent;
    }

    .pipe-status .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
      display: inline-block;
    }

    .pipe-status--connected {
      background: rgba(96, 175, 91, 0.16);
      border-color: rgba(96, 175, 91, 0.6);
      color: var(--vscode-testing-iconPassed, #60af5b);
    }

    .pipe-status--broken {
      background: rgba(241, 76, 76, 0.16);
      border-color: rgba(241, 76, 76, 0.6);
      color: var(--vscode-errorForeground, #f14c4c);
    }

    button {
      flex: 1 1 48%;
      min-width: 110px;
      padding: 6px 10px;
      border: 1px solid var(--vscode-button-border, transparent);
      background: var(--vscode-button-background);
      color: var(--vscode-button-foreground);
      cursor: pointer;
      border-radius: 4px;
      transition: background 0.2s ease, box-shadow 0.2s ease, color 0.2s ease;
    }

    button.secondary {
      background: var(--vscode-input-background);
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-input-border, #666);
    }

    button.runtime-status {
      background: linear-gradient(135deg, rgba(255, 210, 141, 0.95), rgba(255, 162, 51, 0.95));
      color: #2b1500;
      border-color: rgba(255, 162, 51, 0.9);
      box-shadow: 0 0 10px rgba(255, 162, 51, 0.45);
    }

    button.runtime-alert {
      background: linear-gradient(135deg, rgba(255, 182, 160, 0.98), rgba(255, 99, 71, 0.95));
      color: #2b0900;
      border-color: rgba(255, 99, 71, 0.9);
      box-shadow: 0 0 12px rgba(255, 99, 71, 0.45);
    }

    .form {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    label {
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      opacity: 0.85;
    }

    input,
    textarea,
    select {
      width: 100%;
      box-sizing: border-box;
      background: var(--vscode-input-background);
      color: var(--vscode-input-foreground);
      border: 1px solid var(--vscode-input-border, #666);
      border-radius: 4px;
      padding: 6px;
      font-family: inherit;
    }

    textarea {
      min-height: 90px;
      resize: vertical;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .status {
      margin-top: 12px;
      padding: 8px;
      border: 1px solid var(--vscode-panel-border, #444);
      border-radius: 4px;
      font-size: 12px;
      opacity: 0.9;
    }

    .tree {
      margin-top: 16px;
      padding-top: 12px;
      border-top: 1px solid var(--vscode-panel-border, #444);
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .tree-header-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
    }

    .tree-header {
      font-weight: 600;
      font-size: 13px;
      letter-spacing: 0.03em;
      text-transform: uppercase;
      color: var(--vscode-foreground);
      opacity: 0.8;
    }

    .icon-button {
      width: 28px;
      height: 28px;
      border-radius: 4px;
      border: 1px solid var(--vscode-input-border, #666);
      background: var(--vscode-input-background);
      color: var(--vscode-foreground);
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
      line-height: 1;
    }

    .icon-button:hover {
      border-color: var(--vscode-focusBorder, #3794ff);
      color: var(--vscode-focusBorder, #3794ff);
    }

    .tree details {
      border: 1px solid var(--vscode-panel-border, #444);
      border-radius: 4px;
      padding: 6px 8px;
      background: var(--vscode-sideBar-background, rgba(255, 255, 255, 0.02));
    }

    .tree .page-entry {
      position: relative;
      margin-bottom: 6px;
    }

    .tree .page-entry.drop-before::before,
    .tree .page-entry.drop-after::after {
      content: '';
      position: absolute;
      left: 4px;
      right: 4px;
      height: 2px;
      background: var(--vscode-focusBorder, #3794ff);
    }

    .tree .page-entry.drop-before::before {
      top: -3px;
    }

    .tree .page-entry.drop-after::after {
      bottom: -3px;
    }

    .tree summary {
      cursor: pointer;
      font-weight: 600;
      outline: none;
      display: flex;
      align-items: center;
      gap: 6px;
    }

    .tree .drag-handle {
      width: 14px;
      height: 14px;
      border-radius: 2px;
      border: 1px solid transparent;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      cursor: grab;
      color: var(--vscode-foreground);
      font-size: 10px;
      user-select: none;
    }

    .tree .drag-handle:hover {
      border-color: var(--vscode-focusBorder, #3794ff);
    }

    .tree ul {
      list-style: none;
      padding-left: 12px;
      margin: 6px 0 0 0;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .tree li {
      font-size: 12px;
      padding: 2px 4px;
      border-radius: 3px;
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid transparent;
      cursor: pointer;
      transition: background 0.15s ease;
    }

    .tree li:hover {
      background: rgba(255, 255, 255, 0.08);
    }

    .tree li.selected {
      background: rgba(255, 168, 0, 0.25);
      border-color: rgba(255, 168, 0, 0.7);
    }

    .tree li.error {
      border-color: tomato;
      background: rgba(255, 99, 71, 0.2);
    }

    .tree li.error.selected {
      box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.2);
    }

    .tree-empty,
    .tree-error {
      font-size: 12px;
      opacity: 0.8;
    }

    .tree-error {
      color: var(--vscode-editorError-foreground, #f14c4c);
    }

    .badge {
      font-size: 11px;
      padding: 1px 6px;
      border-radius: 999px;
      border: 1px solid transparent;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .badge.hidden {
      border-color: rgba(255, 255, 255, 0.2);
      background: rgba(255, 255, 255, 0.07);
      color: rgba(255, 255, 255, 0.8);
    }

    .menu-backdrop {
      position: fixed;
      inset: 0;
      background: transparent;
      display: none;
      z-index: 900;
    }

    .menu-backdrop.visible {
      display: block;
    }

    .context-menu {
      position: fixed;
      min-width: 240px;
      max-width: 280px;
      padding: 12px;
      background: var(--vscode-editorWidget-background, var(--vscode-sideBar-background));
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-widget-border, #555);
      border-radius: 8px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.35);
      display: none;
      z-index: 1000;
      gap: 10px;
    }

    .context-menu.visible {
      display: flex;
      flex-direction: column;
    }

    .context-menu .menu-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 600;
    }

    .context-menu .menu-section {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .context-menu input[type="text"] {
      width: 100%;
      box-sizing: border-box;
      background: var(--vscode-input-background);
      color: var(--vscode-input-foreground);
      border: 1px solid var(--vscode-input-border, #666);
      border-radius: 4px;
      padding: 6px;
      font-family: inherit;
    }

    .context-menu .format-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    .context-menu .format-buttons .format {
      flex: 0 1 auto;
      min-width: 64px;
      padding: 4px 10px;
      background: var(--vscode-button-secondaryBackground, var(--vscode-input-background));
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-input-border, #666);
    }

    .context-menu .format-buttons .format.active {
      background: var(--vscode-button-background);
      color: var(--vscode-button-foreground);
      border-color: var(--vscode-button-border, transparent);
    }

    .context-menu .radio-group {
      display: flex;
      gap: 12px;
      font-size: 12px;
    }

    .context-menu .radio-group label {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .context-menu .menu-actions {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 8px;
      margin-top: 4px;
    }

    .context-menu .menu-actions button {
      width: 100%;
    }

    .context-menu .menu-actions button.full-width {
      grid-column: 1 / -1;
    }

    .context-menu button.danger {
      background: rgba(241, 76, 76, 0.18);
      color: var(--vscode-errorForeground, #f14c4c);
      border-color: rgba(241, 76, 76, 0.6);
    }

    .context-menu button.danger:hover {
      background: rgba(241, 76, 76, 0.28);
    }

    .context-menu .menu-footer {
      margin-top: 10px;
      padding-top: 10px;
      border-top: 1px solid var(--vscode-widget-border, #555);
    }

    .context-menu .menu-footer button {
      width: 100%;
    }
  </style>
</head>
<body>
  <div class="brand">
    <img src="${logoSrc}" alt="amium qBook" />
  </div>
  <div id="pipeStatus" class="pipe-status pipe-status--broken">
    <span class="dot" aria-hidden="true"></span>
    <span id="pipeStatusText">Pipe broken</span>
  </div>
  <div class="toolbar">
    <button id="btnStop" class="secondary">Stop</button>
    <button id="btnRun" class="secondary">Run</button>
    <button id="btnRebuild" class="secondary">Rebuild</button>
  </div>

  <div class="status" id="status">Ready</div>

  <div class="tree">
    <div class="tree-header-row">
      <div id="treeHeaderTitle" class="tree-header">Pages</div>
      <button id="btnAddPage" class="icon-button" title="Add Page" aria-label="Add Page">+</button>
    </div>
    <div id="bookTree">
      <div class="tree-empty">Lade Book.json ...</div>
    </div>
  </div>

  <div id="menuBackdrop" class="menu-backdrop" aria-hidden="true"></div>
  <div id="contextMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span id="menuTitle">Page Optionen</span>
    </div>
    <div class="menu-section">
      <label for="pageInput">Page</label>
      <input id="pageInput" type="text" value="" autocomplete="off" />
    </div>
    <div class="menu-section">
      <label for="titleInput">Titel</label>
      <input id="titleInput" type="text" placeholder="Titel" autocomplete="off" />
    </div>
    <div class="menu-section">
      <label>Format</label>
      <div class="format-buttons">
        <button type="button" class="format" data-value="A4">A4</button>
        <button type="button" class="format" data-value="16/9">16/9</button>
        <button type="button" class="format" data-value="16/10">16/10</button>
      </div>
    </div>
    <div class="menu-section">
      <label>Visibility</label>
      <div class="radio-group">
        <label>
          <input type="radio" name="visibility" value="visible" checked /> Visible
        </label>
        <label>
          <input type="radio" name="visibility" value="hidden" /> Hidden
        </label>
      </div>
    </div>
    <div class="menu-actions">
      <button id="addSubcodeBtn" type="button" class="secondary">Add Subcode</button>
      <button id="deletePageBtn" type="button" class="danger full-width">Delete Page</button>
    </div>
    <div class="menu-footer">
      <button id="menuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <div id="subnodeMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span id="subnodeMenuTitle">Code Optionen</span>
    </div>
    <div class="menu-section">
      <label for="subnodeNameInput">Name</label>
      <input id="subnodeNameInput" type="text" autocomplete="off" />
    </div>
    <div class="menu-actions">
      <button id="subnodeDeleteBtn" type="button" class="danger">Delete</button>
    </div>
    <div class="menu-footer">
      <button id="subnodeMenuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <div id="newPageMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span>Neue Page</span>
    </div>
    <div class="menu-actions">
      <button id="createPageBtn" type="button">New Page</button>
      <button id="importPageBtn" type="button" class="secondary">Import Page</button>
    </div>
    <div class="menu-footer">
      <button id="newMenuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <script nonce="${nonce}">
    const vscode = acquireVsCodeApi();

    const btnRun = document.getElementById('btnRun');
    const btnStop = document.getElementById('btnStop');
    const btnRebuild = document.getElementById('btnRebuild');
    const treeHeaderTitle = document.getElementById('treeHeaderTitle');
    const pageInput = document.getElementById('pageInput');
    const titleInput = document.getElementById('titleInput');
    const formatButtons = Array.from(document.querySelectorAll('button.format'));
    const status = document.getElementById('status');
    const bookTree = document.getElementById('bookTree');
    const pipeStatusElement = document.getElementById('pipeStatus');
    const pipeStatusText = document.getElementById('pipeStatusText');
    const contextMenu = document.getElementById('contextMenu');
    const menuBackdrop = document.getElementById('menuBackdrop');
    const menuTitle = document.getElementById('menuTitle');
    const menuClose = document.getElementById('menuClose');
    const addSubcodeBtn = document.getElementById('addSubcodeBtn');
    const deletePageBtn = document.getElementById('deletePageBtn');
    const btnAddPage = document.getElementById('btnAddPage');
    const newPageMenu = document.getElementById('newPageMenu');
    const createPageBtn = document.getElementById('createPageBtn');
    const importPageBtn = document.getElementById('importPageBtn');
    const newMenuClose = document.getElementById('newMenuClose');
    const subnodeMenu = document.getElementById('subnodeMenu');
    const subnodeMenuTitle = document.getElementById('subnodeMenuTitle');
    const subnodeNameInput = document.getElementById('subnodeNameInput');
    const subnodeDeleteBtn = document.getElementById('subnodeDeleteBtn');
    const subnodeMenuClose = document.getElementById('subnodeMenuClose');
    const visibilityRadios = Array.from(document.querySelectorAll('input[name="visibility"]'));

    const treeState = {
      selectedPath: null,
      errorPaths: [],
      selectedFormat: 'A4',
      hiddenMode: 'visible'
    };

    const menuState = {
      folder: null,
      page: null,
    };

    const subnodeState = {
      page: null,
      fileName: null,
    };

    const dragState = {
      sourcePage: null,
      targetPage: null,
      position: 'before'
    };

    const runtimeButtons = {
      run: btnRun,
      stop: btnStop,
      rebuild: btnRebuild,
    };

    const runtimeState = {
      run: null,
      stop: null,
      rebuild: null,
    };

    const runtimeButtonKeys = ['run', 'stop', 'rebuild'];

    function updateRuntimeButtonsState(nextState) {
      const snapshot = nextState && typeof nextState === 'object' ? nextState : {};
      runtimeButtonKeys.forEach((key) => {
        const incoming = snapshot[key];
        const normalized = incoming === 'status' || incoming === 'alert' ? incoming : null;
        runtimeState[key] = normalized;
        const target = runtimeButtons[key];
        if (target) {
          target.classList.toggle('runtime-status', normalized === 'status');
          target.classList.toggle('runtime-alert', normalized === 'alert');
        }
      });
    }

    function encodeHtml(value) {
      if (typeof value !== 'string') {
        return '';
      }
      return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function encodeAttr(value) {
      return encodeHtml(value).replace(/"/g, '&quot;');
    }

    function applyPipeStatus(state) {
      if (!pipeStatusElement || !pipeStatusText) {
        return;
      }

      const normalized = state === 'connected' ? 'connected' : 'broken';
      pipeStatusElement.classList.toggle('pipe-status--connected', normalized === 'connected');
      pipeStatusElement.classList.toggle('pipe-status--broken', normalized !== 'connected');
      pipeStatusText.textContent = normalized === 'connected' ? 'Pipe connected' : 'Pipe broken';
    }

    function updateTreeState(partial) {
      if (Object.prototype.hasOwnProperty.call(partial, 'selectedPath')) {
        treeState.selectedPath = partial.selectedPath ?? null;
      }

      if (Object.prototype.hasOwnProperty.call(partial, 'errorPaths')) {
        treeState.errorPaths = Array.isArray(partial.errorPaths) ? partial.errorPaths : [];
      }

      updateTreeHighlights();
    }

    function updateTreeHighlights() {
      if (!bookTree) {
        return;
      }

      const selectedLower = typeof treeState.selectedPath === 'string' ? treeState.selectedPath.toLowerCase() : '';
      const errorSet = new Set(
        Array.isArray(treeState.errorPaths)
          ? treeState.errorPaths
              .map((entry) => (typeof entry === 'string' ? entry.toLowerCase() : ''))
              .filter(Boolean)
          : []
      );

      const nodes = bookTree.querySelectorAll('li[data-file]');
      nodes.forEach((li) => {
        const rel = (li.getAttribute('data-file') || '').toLowerCase();
        li.classList.toggle('selected', Boolean(selectedLower) && rel === selectedLower);
        li.classList.toggle('error', errorSet.has(rel));
      });
    }

    function updateHiddenRadios() {
      const currentMode = treeState.hiddenMode === 'hidden' ? 'hidden' : 'visible';
      visibilityRadios.forEach((radio) => {
        if (radio instanceof HTMLInputElement) {
          radio.checked = radio.value === currentMode;
        }
      });
    }

    function setHiddenMode(isHidden) {
      treeState.hiddenMode = isHidden ? 'hidden' : 'visible';
      updateHiddenRadios();
    }

    function resolveNodePageValue() {
      if (typeof menuState.folder === 'string' && menuState.folder.length > 0) {
        return menuState.folder;
      }
      if (pageInput && 'value' in pageInput && pageInput.value) {
        return pageInput.value;
      }
      return '';
    }

    function sendHiddenCommand(isHidden) {
      vscode.postMessage({
        type: 'toggleHidden',
        hidden: Boolean(isHidden),
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function sendTitleCommand() {
      vscode.postMessage({
        type: 'updateTitle',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function sendFormatCommand(formatValue) {
      const normalized = typeof formatValue === 'string' ? formatValue : treeState.selectedFormat;
      vscode.postMessage({
        type: 'updateFormat',
        format: normalized,
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function collectPageOrderFromDom() {
      if (!bookTree) {
        return [];
      }
      const entries = Array.from(bookTree.querySelectorAll('.page-entry'));
      return entries
        .map((entry) => (entry instanceof HTMLElement ? entry.getAttribute('data-page') : null))
        .filter((value) => typeof value === 'string' && value.length > 0);
    }

    function findPageEntry(page) {
      if (!bookTree || !page) {
        return null;
      }
      const entries = bookTree.querySelectorAll('.page-entry');
      for (const entry of entries) {
        if (entry instanceof HTMLElement && entry.getAttribute('data-page') === page) {
          return entry;
        }
      }
      return null;
    }

    function clearDropIndicators() {
      if (!bookTree) {
        return;
      }
      const entries = bookTree.querySelectorAll('.page-entry');
      entries.forEach((entry) => {
        entry.classList.remove('drop-before', 'drop-after');
      });
    }

    function applyDropIndicator(entry, isAfter) {
      if (!entry) {
        return;
      }
      entry.classList.toggle('drop-before', !isAfter);
      entry.classList.toggle('drop-after', isAfter);
    }

    function reorderDomEntries(sourcePage, targetEntry, position) {
      if (!bookTree) {
        return false;
      }
      const sourceEntry = findPageEntry(sourcePage);
      if (!sourceEntry || !targetEntry || sourceEntry === targetEntry) {
        return false;
      }

      if (position === 'after') {
        const next = targetEntry.nextSibling;
        bookTree.insertBefore(sourceEntry, next);
      } else {
        bookTree.insertBefore(sourceEntry, targetEntry);
      }
      return true;
    }

    function publishPageOrder() {
      const order = collectPageOrderFromDom();
      if (!order.length) {
        return;
      }
      vscode.postMessage({ type: 'reorderPages', order });
    }

    function resetDragState() {
      dragState.sourcePage = null;
      dragState.targetPage = null;
      dragState.position = 'before';
      clearDropIndicators();
    }

    function payload(type) {
      return {
        type,
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        pageText: '',
        timestamp: new Date().toISOString()
      };
    }

    function setStatus(text) {
      if (status) status.textContent = text;
    }

    btnRun?.addEventListener('click', () => {
      setStatus('Run gestartet');
      vscode.postMessage(payload('run'));
    });

    btnStop?.addEventListener('click', () => {
      setStatus('Stop angefordert');
      vscode.postMessage(payload('stop'));
    });

    btnRebuild?.addEventListener('click', () => {
      setStatus('Rebuild läuft');
      vscode.postMessage(payload('rebuild'));
    });

    formatButtons.forEach((btn) => {
      btn.addEventListener('click', () => {
        const value = btn.getAttribute('data-value');
        treeState.selectedFormat = value;
        updateFormatButtons();
        sendFormatCommand(value);
      });
    });

    visibilityRadios.forEach((radio) => {
      radio.addEventListener('change', () => {
        if (!(radio instanceof HTMLInputElement) || !radio.checked) {
          return;
        }
        const selectedHidden = radio.value === 'hidden';
        setHiddenMode(selectedHidden);
        sendHiddenCommand(selectedHidden);
      });
    });

    if (titleInput) {
      titleInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          sendTitleCommand();
          closeContextMenu();
        }
      });
      titleInput.addEventListener('change', () => {
        sendTitleCommand();
      });
    }

    if (pageInput) {
      const sendRename = () => {
        vscode.postMessage({
          type: 'renamePage',
          page: pageInput && 'value' in pageInput ? pageInput.value : '',
          title: titleInput && 'value' in titleInput ? titleInput.value : '',
          format: treeState.selectedFormat ?? 'A4',
          hidden: treeState.hiddenMode === 'hidden',
          folder: menuState.folder ?? undefined,
          nodePage: resolveNodePageValue() || undefined,
        });
      };

      pageInput.addEventListener('change', sendRename);
      pageInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          sendRename();
          closeContextMenu();
        }
      });
    }

    function updateFormatButtons() {
      formatButtons.forEach((btn) => {
        const value = btn.getAttribute('data-value');
        btn.classList.toggle('active', value === treeState.selectedFormat);
      });
    }

    function positionMenuElement(element, x, y) {
      if (!element) {
        return;
      }
      const menuWidth = element.offsetWidth || 260;
      const menuHeight = element.offsetHeight || 240;
      const posX = Math.max(8, Math.min(x, window.innerWidth - menuWidth - 8));
      const posY = Math.max(8, Math.min(y, window.innerHeight - menuHeight - 8));
      element.style.left = posX + 'px';
      element.style.top = posY + 'px';
    }

    function updateBackdropVisibility() {
      const pageMenuVisible = Boolean(contextMenu && contextMenu.classList.contains('visible'));
      const subnodeMenuVisible = Boolean(subnodeMenu && subnodeMenu.classList.contains('visible'));
      const newMenuVisible = Boolean(newPageMenu && newPageMenu.classList.contains('visible'));
      if (pageMenuVisible || subnodeMenuVisible || newMenuVisible) {
        menuBackdrop?.classList.add('visible');
      } else {
        menuBackdrop?.classList.remove('visible');
      }
    }

    function closeContextMenu() {
      contextMenu?.classList.remove('visible');
      menuState.folder = null;
      menuState.page = null;
      updateBackdropVisibility();
    }

    function closeSubnodeMenu() {
      subnodeMenu?.classList.remove('visible');
      if (subnodeNameInput && 'value' in subnodeNameInput) {
        subnodeNameInput.value = '';
      }
      subnodeState.page = null;
      subnodeState.fileName = null;
      updateBackdropVisibility();
    }

    function closeNewMenu() {
      newPageMenu?.classList.remove('visible');
      updateBackdropVisibility();
    }

    function closeAllMenus() {
      closeContextMenu();
      closeNewMenu();
      closeSubnodeMenu();
      closeNewMenu();
    }

    function openContextMenu(event, pageData) {
      if (!contextMenu || !menuBackdrop) {
        return;
      }

      closeSubnodeMenu();
      closeNewMenu();

      const pageValue = pageData?.page ?? '';
      const titleValue = pageData?.title ?? '';
      const formatValue = pageData?.format ?? 'A4';
      const isHidden = Boolean(pageData?.hidden);

      if (pageInput && 'value' in pageInput) {
        pageInput.value = pageValue;
      }

      if (titleInput && 'value' in titleInput) {
        titleInput.value = titleValue;
      }

      treeState.selectedFormat = formatValue;
      setHiddenMode(isHidden);
      updateFormatButtons();

      if (menuTitle) {
        menuTitle.textContent = titleValue || pageValue || 'Page Optionen';
      }

      menuState.folder = pageData?.folder ?? null;
      menuState.page = pageValue;

      contextMenu.style.visibility = 'hidden';
      contextMenu.classList.add('visible');
      updateBackdropVisibility();

      requestAnimationFrame(() => {
        positionMenuElement(contextMenu, event.clientX, event.clientY);
        if (contextMenu) {
          contextMenu.style.visibility = 'visible';
        }
      });
    }

    function extractCodeName(page, fileName) {
      if (!page || !fileName) {
        return '';
      }
      const prefix = page + '.';
      const suffix = '.cs';
      if (!fileName.startsWith(prefix) || !fileName.endsWith(suffix)) {
        return '';
      }
      return fileName.slice(prefix.length, fileName.length - suffix.length);
    }

    function openSubnodeMenu(event, fileData) {
      if (!subnodeMenu || !subnodeNameInput) {
        return;
      }

      const page = fileData?.page ?? '';
      const fileName = fileData?.fileName ?? '';
      const locked = Boolean(fileData?.locked);
      if (!page || !fileName) {
        return;
      }

      const codeName = extractCodeName(page, fileName);
      if (!codeName) {
        setStatus('Dieses File kann nicht editiert werden.');
        return;
      }

      if (locked || codeName.toLowerCase() === 'qpage') {
        setStatus('qPage Dateien dürfen nicht verändert werden.');
        return;
      }

      closeContextMenu();

      subnodeState.page = page;
      subnodeState.fileName = fileName;

      subnodeNameInput.value = codeName;
      if (subnodeMenuTitle) {
        subnodeMenuTitle.textContent = codeName;
      }

      subnodeMenu.style.visibility = 'hidden';
      subnodeMenu.classList.add('visible');
      updateBackdropVisibility();

      requestAnimationFrame(() => {
        positionMenuElement(subnodeMenu, event.clientX, event.clientY);
        if (subnodeMenu) {
          subnodeMenu.style.visibility = 'visible';
        }
        if (subnodeNameInput) {
          subnodeNameInput.focus();
          subnodeNameInput.select();
        }
      });
    }

    menuBackdrop?.addEventListener('click', () => closeAllMenus());
    menuClose?.addEventListener('click', () => closeContextMenu());
    subnodeMenuClose?.addEventListener('click', () => closeSubnodeMenu());
    newMenuClose?.addEventListener('click', () => closeNewMenu());
    window.addEventListener('keydown', (event) => {
      if (event.key === 'Escape') {
        closeAllMenus();
      }
    });
    window.addEventListener('resize', () => closeAllMenus());

    btnAddPage?.addEventListener('click', (event) => {
      event.preventDefault();
      event.stopPropagation();
      if (newPageMenu?.classList.contains('visible')) {
        closeNewMenu();
        return;
      }
      closeContextMenu();
      closeSubnodeMenu();
      if (!newPageMenu) {
        return;
      }
      newPageMenu.style.visibility = 'hidden';
      newPageMenu.classList.add('visible');
      updateBackdropVisibility();
      requestAnimationFrame(() => {
        if (newPageMenu) {
          const anchorRect = btnAddPage.getBoundingClientRect();
          const anchorX = anchorRect.left + anchorRect.width / 2;
          const anchorY = anchorRect.bottom + 4;
          positionMenuElement(newPageMenu, anchorX, anchorY);
          newPageMenu.style.visibility = 'visible';
        }
      });
    });

    createPageBtn?.addEventListener('click', () => {
      vscode.postMessage({ type: 'createPage' });
      closeNewMenu();
    });

    importPageBtn?.addEventListener('click', () => {
      vscode.postMessage({ type: 'importPage' });
      closeNewMenu();
    });

    function submitSubnodeRename() {
      if (!subnodeNameInput || !subnodeState.page || !subnodeState.fileName) {
        return;
      }
      const nextValue = subnodeNameInput.value.trim();
      if (!nextValue) {
        setStatus('Name darf nicht leer sein.');
        return;
      }
      vscode.postMessage({
        type: 'renameSubnode',
        page: subnodeState.page,
        fileName: subnodeState.fileName,
        codeName: nextValue,
      });
      closeSubnodeMenu();
    }

    subnodeNameInput?.addEventListener('keydown', (event) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        submitSubnodeRename();
      }
    });

    subnodeDeleteBtn?.addEventListener('click', () => {
      if (!subnodeState.page || !subnodeState.fileName) {
        setStatus('Keine Datei ausgewählt.');
        return;
      }
      vscode.postMessage({
        type: 'deleteSubnode',
        page: subnodeState.page,
        fileName: subnodeState.fileName,
      });
      closeSubnodeMenu();
    });

    addSubcodeBtn?.addEventListener('click', () => {
      vscode.postMessage({
        type: 'addSubcode',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        folder: menuState.folder ?? undefined,
      });
      closeAllMenus();
    });

    deletePageBtn?.addEventListener('click', () => {
      vscode.postMessage({
        type: 'deletePage',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        folder: menuState.folder ?? undefined,
      });
      closeAllMenus();
    });

    function renderTree(payload) {
      if (!bookTree) {
        return;
      }

      const nodes = Array.isArray(payload?.nodes) ? payload.nodes : [];
      const projectLabel = payload?.projectName ?? 'qBook Projekt';

      if (treeHeaderTitle) {
        treeHeaderTitle.textContent = projectLabel || 'Pages';
      }

      if (!nodes.length) {
        bookTree.innerHTML = '<p class="tree-empty">Keine Pages gefunden.</p>';
        return;
      }

      const treeMarkup = nodes
        .map((node) => {
          const children = Array.isArray(node.files)
            ? node.files
                .map((file) => {
                  const rel = typeof file.relativePath === 'string' ? file.relativePath : '';
                  const encodedRel = encodeAttr(rel);
                  const displayName = encodeHtml(file.displayName ?? file.name ?? '');
                  const rawFileName = typeof file.name === 'string' ? file.name : '';
                  const encodedFileName = encodeAttr(rawFileName);
                  const parentPage = encodeAttr(node.page ?? '');
                  const isLocked = rawFileName.toLowerCase().endsWith('.qpage.cs');
                  const lockedAttr = isLocked ? ' data-locked="true"' : '';
                  if (!rel) {
                    return '<li>' + displayName + '</li>';
                  }

                  const attrs =
                    '<li data-file="' +
                    encodedRel +
                    '" data-page="' +
                    parentPage +
                    '" data-filename="' +
                    encodedFileName +
                    '"' +
                    lockedAttr +
                    ' title="' +
                    encodedRel +
                    '">';
                  return attrs + displayName + '</li>';
                })
                .join('')
            : '';
          const pageLabel = encodeHtml(node.page ?? '');
          const hiddenIcon = node.metadata?.hidden
            ? '<span class="badge hidden" title="Hidden">Hidden</span>'
            : '';
          const normalizedPage = encodeAttr(node.page ?? '');
          const dragHandle =
            '<span class="drag-handle" draggable="true" data-page="' +
            normalizedPage +
            '" title="Seite verschieben" aria-label="Seite verschieben">⋮⋮</span>';
          const summaryAttributes =
            ' data-page="' +
            encodeAttr(node.metadata?.name ?? node.page ?? '') +
            '" data-title="' +
            encodeAttr(node.metadata?.title ?? node.metadata?.name ?? node.page ?? '') +
            '" data-format="' +
            encodeAttr(node.metadata?.format ?? 'A4') +
            '" data-hidden="' +
            (node.metadata?.hidden ? 'true' : 'false') +
            '" data-folder="' +
            encodeAttr(node.page ?? '') +
            '"';
          return (
            '<details class="page-entry" data-page="' +
            normalizedPage +
            '" open><summary' +
            summaryAttributes +
            '>' +
            dragHandle +
            '<span>' +
            pageLabel +
            '</span>' +
            hiddenIcon +
            '</summary><ul>' +
            children +
            '</ul></details>'
          );
        })
        .join('');

      bookTree.innerHTML = treeMarkup;
      updateTreeHighlights();
    }

    function renderTreeError(message) {
      if (!bookTree) {
        return;
      }

      const text = message ?? 'Tree konnte nicht geladen werden.';
      const safeText = encodeHtml(text);
      if (treeHeaderTitle) {
        treeHeaderTitle.textContent = 'Book';
      }
      bookTree.innerHTML = '<p class="tree-error">' + safeText + '</p>';
      updateTreeHighlights();
    }

    bookTree?.addEventListener('click', (event) => {
      closeAllMenus();
      const target = event.target;
      if (!target) {
        return;
      }

      const element = target instanceof HTMLElement ? target : target.parentElement;
      if (!element || typeof element.closest !== 'function') {
        return;
      }

      const li = element.closest('li[data-file]');
      if (!li) {
        return;
      }

      const relativePath = li.getAttribute('data-file');
      if (relativePath) {
        vscode.postMessage({ type: 'openFile', relativePath });
      }
    });

    bookTree?.addEventListener('contextmenu', (event) => {
      const target = event.target instanceof HTMLElement ? event.target : null;
      if (!target) {
        return;
      }

      const fileElement = target.closest('li[data-file]');
      if (fileElement) {
        event.preventDefault();
        const fileData = {
          page: fileElement.getAttribute('data-page') ?? '',
          fileName: fileElement.getAttribute('data-filename') ?? '',
          locked: fileElement.getAttribute('data-locked') === 'true',
        };
        openSubnodeMenu(event, fileData);
        return;
      }

      const element = target.closest('summary[data-page]');
      if (!element) {
        return;
      }

      event.preventDefault();
      const data = {
        page: element.getAttribute('data-page') ?? '',
        title: element.getAttribute('data-title') ?? '',
        format: element.getAttribute('data-format') ?? 'A4',
        hidden: element.getAttribute('data-hidden') === 'true',
        folder: element.getAttribute('data-folder') ?? '',
      };

      openContextMenu(event, data);
    });

    bookTree?.addEventListener('dragstart', (event) => {
      const handle = event.target instanceof HTMLElement ? event.target.closest('.drag-handle') : null;
      if (!handle) {
        return;
      }

      const page = handle.getAttribute('data-page');
      if (!page) {
        return;
      }

      dragState.sourcePage = page;
      dragState.targetPage = null;
      dragState.position = 'before';
      if (event.dataTransfer) {
        event.dataTransfer.setData('text/plain', page);
        event.dataTransfer.effectAllowed = 'move';
      }
    });

    bookTree?.addEventListener('dragover', (event) => {
      if (!dragState.sourcePage) {
        return;
      }
      const entry = event.target instanceof HTMLElement ? event.target.closest('.page-entry') : null;
      if (!entry) {
        return;
      }
      const page = entry.getAttribute('data-page');
      if (!page || page === dragState.sourcePage) {
        dragState.targetPage = null;
        clearDropIndicators();
        return;
      }
      event.preventDefault();
      const rect = entry.getBoundingClientRect();
      const isAfter = event.clientY > rect.top + rect.height / 2;
      dragState.targetPage = page;
      dragState.position = isAfter ? 'after' : 'before';
      clearDropIndicators();
      applyDropIndicator(entry, isAfter);
      if (event.dataTransfer) {
        event.dataTransfer.dropEffect = 'move';
      }
    });

    bookTree?.addEventListener('drop', (event) => {
      if (!dragState.sourcePage) {
        return;
      }
      event.preventDefault();
      const { sourcePage, targetPage, position } = dragState;
      resetDragState();
      if (!sourcePage || !targetPage) {
        return;
      }
      const targetEntry = findPageEntry(targetPage);
      if (!targetEntry) {
        return;
      }
      const moved = reorderDomEntries(sourcePage, targetEntry, position);
      if (moved) {
        publishPageOrder();
      }
    });

    document.addEventListener('dragend', () => {
      resetDragState();
    });

    document.addEventListener('drop', (event) => {
      if (!bookTree) {
        return;
      }
      const node = event.target instanceof Node ? event.target : null;
      if (node && bookTree.contains(node)) {
        return;
      }
      resetDragState();
    });

    window.addEventListener('message', (event) => {
      const { type, payload, message, status: incomingPipeStatus, text } = event.data ?? {};
      if (type === 'bookData') {
        renderTree(payload);
      } else if (type === 'bookError') {
        renderTreeError(message);
      } else if (type === 'bookStatus') {
        const update = {};
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'selectedPath')) {
          update.selectedPath = payload.selectedPath;
        }
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'errorPaths')) {
          update.errorPaths = Array.isArray(payload.errorPaths) ? payload.errorPaths : [];
        }
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'form')) {
          applyFormValues(payload.form);
        }
        updateTreeState(update);
      } else if (type === 'pipeStatus') {
        applyPipeStatus(typeof incomingPipeStatus === 'string' ? incomingPipeStatus : 'disconnected');
      } else if (type === 'runtimeState') {
        updateRuntimeButtonsState(payload);
      } else if (type === 'statusText' && typeof text === 'string') {
        setStatus(text);
      }
    });
    function applyFormValues(formPayload) {
      if (!formPayload) {
        return;
      }

      if (pageInput && 'value' in pageInput && typeof formPayload.page === 'string') {
        pageInput.value = formPayload.page;
      }

      if (titleInput && 'value' in titleInput && typeof formPayload.title === 'string') {
        titleInput.value = formPayload.title;
      }

      const formatValue = typeof formPayload.format === 'string' ? formPayload.format : 'A4';
      treeState.selectedFormat = formatValue;
      updateFormatButtons();

      const hiddenValue = typeof formPayload.hidden === 'boolean' ? formPayload.hidden : false;
      setHiddenMode(hiddenValue);
    }

    updateFormatButtons();
    updateHiddenRadios();
    applyPipeStatus('disconnected');
    updateRuntimeButtonsState(runtimeState);

    vscode.postMessage({ type: 'requestTree' });
  </script>
</body>
</html>`;
}
function createNonce() {
    const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let value = '';
    for (let i = 0; i < 16; i++) {
        value += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return value;
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
    isRenamingPage = false;
    constructor(context) {
        this.context = context;
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
            switch (message.type) {
                case 'requestTree':
                    await this.loadAndSendTreeData(webviewView);
                    break;
                case 'openFile':
                    await this.openRelativeFile(message.relativePath);
                    break;
                case 'run':
                    await sendRuntimeCommand('Run');
                    break;
                case 'stop':
                    await sendRuntimeCommand('Destroy');
                    break;
                case 'rebuild':
                    await this.verifyAllTreeCsFiles();
                    const canRebuild = await ensureNoCSharpErrorsBeforeRebuild();
                    if (!canRebuild) {
                        this.postStatusText('Rebuild abgebrochen: C#-Fehler gefunden');
                        this.applyRuntimeHighlight('rebuild', 'alert');
                        break;
                    }
                    this.postStatusText('Rebuild läuft');
                    await sendRuntimeCommand('Rebuild');
                    break;
                case 'toggleHidden':
                    await this.handleToggleHidden(message);
                    break;
                case 'updateTitle':
                    await this.handleUpdateTitle(message);
                    break;
                case 'updateFormat':
                    await this.handleUpdateFormat(message);
                    break;
                case 'renamePage':
                    await this.handleRenamePage(message);
                    break;
                case 'reorderPages':
                    await this.handleReorderPages(message);
                    break;
                case 'renameSubnode':
                    await this.handleRenameSubnode(message);
                    break;
                case 'deleteSubnode':
                    await this.handleDeleteSubnode(message);
                    break;
                case 'createPage':
                    await this.handleCreatePage();
                    break;
                case 'importPage':
                    await this.handleImportPage();
                    break;
                case 'save':
                    vscode.window.showInformationMessage(`Save clicked | Page=${message.page ?? ''}`);
                    console.log('[Webview->Extension] save', message);
                    break;
                case 'renamePage':
                    vscode.window.showInformationMessage(`Rename Page (Preview) | Page=${message.page ?? ''}`);
                    console.log('[Webview->Extension] renamePage', message);
                    break;
                case 'addSubcode':
                    await this.handleAddSubcode(message);
                    break;
                case 'deletePage':
                    await this.handleDeletePage(message);
                    break;
                default:
                    console.log('[Webview->Extension] unknown message', message);
                    break;
            }
        });
        const logoFile = vscode.Uri.joinPath(this.context.extensionUri, 'media', 'amiumlogo2gray.png');
        const logoWebviewUri = webviewView.webview.asWebviewUri(logoFile);
        webviewView.webview.html = getWebviewHtml(webviewView.webview, logoWebviewUri);
        this.loadAndSendTreeData(webviewView).catch((error) => {
            const details = error instanceof Error ? error.message : String(error);
            webviewView.webview.postMessage({ type: 'bookError', message: details });
        });
        this.postPipeStatus();
        this.postRuntimeState();
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
            this.postBookStatus();
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
        const bookUri = await this.findBookFile();
        if (!bookUri) {
            this.clearProjectState();
            return undefined;
        }
        const bookRoot = this.getDirectoryUri(bookUri);
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
            : await this.discoverPageFolders(bookRoot);
        const nodes = [];
        for (const pageName of orderedPages) {
            if (typeof pageName !== 'string' || !pageName) {
                continue;
            }
            const node = await this.readPageFolder(bookRoot, pageName);
            nodes.push(node);
        }
        return {
            projectName: bookData.ProjectName,
            rootPath: bookRoot.fsPath,
            nodes,
        };
    }
    async openRelativeFile(relativePath, options) {
        if (!relativePath) {
            vscode.window.showWarningMessage('Keine Datei ausgewählt.');
            return;
        }
        const baseUri = this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri;
        if (!baseUri) {
            vscode.window.showWarningMessage('Es ist kein qBook-Projekt geöffnet.');
            return;
        }
        const normalizedRelative = this.normalizeRelative(relativePath);
        const absolutePath = path.resolve(baseUri.fsPath, normalizedRelative);
        const basePath = path.resolve(baseUri.fsPath);
        if (!absolutePath.toLowerCase().startsWith(basePath.toLowerCase())) {
            vscode.window.showErrorMessage('Datei liegt außerhalb des qBook-Verzeichnisses.');
            return;
        }
        const targetUri = vscode.Uri.file(absolutePath);
        try {
            const document = await vscode.workspace.openTextDocument(targetUri);
            await vscode.window.showTextDocument(document, {
                preview: options?.preview ?? false,
                preserveFocus: options?.preserveFocus ?? false,
            });
            this.selectedPath = normalizedRelative;
            this.postBookStatus();
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Datei konnte nicht geöffnet werden: ${relativePath}\n${details}`);
        }
    }
    async verifyAllTreeCsFiles() {
        if (!this.lastPayload) {
            return;
        }
        const initiallyVisible = this.collectOpenTextTabUris();
        const visited = new Map();
        const csFiles = this.lastPayload.nodes
            .flatMap((node) => node.files)
            .filter((file) => typeof file.relativePath === 'string' && file.relativePath.toLowerCase().endsWith('.cs'));
        for (const file of csFiles) {
            const targetUri = this.resolveRelativeFileUri(file.relativePath);
            if (!targetUri) {
                continue;
            }
            const uriKey = this.uriKey(targetUri);
            const normalizedRelativePath = this.normalizeRelative(file.relativePath);
            const relativeKey = normalizedRelativePath.toLowerCase();
            visited.set(uriKey, { relativePath: normalizedRelativePath, relativeKey });
            const label = file.displayName?.trim() || file.name?.trim() || file.relativePath;
            this.postStatusText(`Verifying code in ${label}`);
            await this.openRelativeFile(file.relativePath, { preview: false, preserveFocus: true });
            await waitForNextDiagnosticsUpdate(350);
            await new Promise((resolve) => setTimeout(resolve, 120));
        }
        this.postStatusText('Prüfe C# Diagnostics ...');
        await waitForNextDiagnosticsUpdate(800);
        await new Promise((resolve) => setTimeout(resolve, 120));
        this.refreshDiagnostics();
        const errorRelativeSet = new Set(Array.from(this.errorPaths, (entry) => entry.toLowerCase()));
        for (const [uriKey, info] of visited) {
            if (errorRelativeSet.has(info.relativeKey) && !this.isTextTabVisible(uriKey)) {
                await this.openRelativeFile(info.relativePath, { preview: false, preserveFocus: true });
            }
        }
        const closable = Array.from(visited.entries())
            .filter(([uriKey, info]) => !initiallyVisible.has(uriKey) && !errorRelativeSet.has(info.relativeKey))
            .map(([uriKey]) => uriKey);
        await this.closeVerificationOnlyTabs(closable);
    }
    collectOpenTextTabUris() {
        const result = new Set();
        for (const group of vscode.window.tabGroups.all) {
            for (const tab of group.tabs) {
                if (tab.input instanceof vscode.TabInputText) {
                    result.add(this.uriKey(tab.input.uri));
                }
            }
        }
        return result;
    }
    isTextTabVisible(uriKey) {
        for (const group of vscode.window.tabGroups.all) {
            for (const tab of group.tabs) {
                if (tab.input instanceof vscode.TabInputText && this.uriKey(tab.input.uri) === uriKey) {
                    return true;
                }
            }
        }
        return false;
    }
    uriKey(uri) {
        return path.resolve(uri.fsPath).toLowerCase();
    }
    resolveRelativeFileUri(relativePath) {
        const baseUri = this.lastBookRoot ?? vscode.workspace.workspaceFolders?.[0]?.uri;
        if (!baseUri) {
            return undefined;
        }
        const normalizedRelative = this.normalizeRelative(relativePath);
        const absolutePath = path.resolve(baseUri.fsPath, normalizedRelative);
        const basePath = path.resolve(baseUri.fsPath);
        if (!absolutePath.toLowerCase().startsWith(basePath.toLowerCase())) {
            return undefined;
        }
        return vscode.Uri.file(absolutePath);
    }
    async closeVerificationOnlyTabs(uriKeys) {
        if (!uriKeys.length) {
            return;
        }
        const uriSet = new Set(uriKeys);
        const tabsToClose = [];
        for (const group of vscode.window.tabGroups.all) {
            for (const tab of group.tabs) {
                if (!(tab.input instanceof vscode.TabInputText)) {
                    continue;
                }
                const tabUriKey = this.uriKey(tab.input.uri);
                if (!uriSet.has(tabUriKey)) {
                    continue;
                }
                tabsToClose.push(tab);
            }
        }
        if (tabsToClose.length === 0) {
            return;
        }
        try {
            await vscode.window.tabGroups.close(tabsToClose, true);
        }
        catch {
            // ignore close issues to avoid blocking rebuild
        }
    }
    async handleToggleHidden(message) {
        if (typeof message.hidden !== 'boolean') {
            return;
        }
        const targetPage = this.resolveNodePageIdentifier(message);
        if (!targetPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        try {
            const state = message.hidden ? 'true' : 'false';
            await sendRuntimeCommand('HidePage', [targetPage, state]);
            await this.updatePageMetadataFile(targetPage, (doc) => {
                doc.Hidden = message.hidden;
            });
            this.applyMetadataPatch(targetPage, { hidden: message.hidden });
        }
        catch (error) {
            this.reportMetadataError('Hidden', error);
        }
    }
    async handleUpdateTitle(message) {
        const targetPage = this.resolveNodePageIdentifier(message);
        if (!targetPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        const nextTitle = typeof message.title === 'string' ? message.title : '';
        try {
            await sendRuntimeCommand('PageText', [targetPage, nextTitle]);
            await this.updatePageMetadataFile(targetPage, (doc) => {
                doc.Text = nextTitle;
            });
            this.applyMetadataPatch(targetPage, { title: nextTitle });
        }
        catch (error) {
            this.reportMetadataError('Titel', error);
        }
    }
    async handleUpdateFormat(message) {
        const targetPage = this.resolveNodePageIdentifier(message);
        if (!targetPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        const nextFormat = this.normalizeFormatValue(message.format);
        if (!nextFormat) {
            vscode.window.showWarningMessage('Ungültiges Format.');
            return;
        }
        try {
            await sendRuntimeCommand('PageFormat', [targetPage, nextFormat]);
            await this.updatePageMetadataFile(targetPage, (doc) => {
                doc.Format = nextFormat;
            });
            this.applyMetadataPatch(targetPage, { format: nextFormat });
        }
        catch (error) {
            this.reportMetadataError('Format', error);
        }
    }
    async handleRenamePage(message) {
        if (this.isRenamingPage) {
            vscode.window.showWarningMessage('Bitte warten Sie, bis der laufende Page-Rename abgeschlossen ist.');
            return;
        }
        const oldPage = this.normalizePageName(this.resolveNodePageIdentifier(message));
        const newPage = this.normalizePageName(message.page);
        if (!oldPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        if (!newPage) {
            vscode.window.showWarningMessage('Neuer Page-Name ist ungültig.');
            return;
        }
        if (oldPage === newPage) {
            vscode.window.showInformationMessage('Page-Name unverändert.');
            return;
        }
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geöffnet.');
            return;
        }
        const newDir = this.getPageDirectory(newPage);
        if (await this.pathExists(newDir)) {
            vscode.window.showErrorMessage(`Page '${newPage}' existiert bereits.`);
            return;
        }
        this.isRenamingPage = true;
        try {
            await vscode.window.withProgress({
                location: vscode.ProgressLocation.Notification,
                title: `Benenne Page '${oldPage}' in '${newPage}' um...`,
            }, async () => {
                await this.renamePageFolder(oldPage, newPage);
                await this.renamePageFiles(oldPage, newPage);
                await this.renamePageInBookFile(oldPage, newPage);
                await this.updatePageMetadataFile(newPage, (doc) => {
                    doc.Name = newPage;
                    if (typeof message.title === 'string' && message.title.trim()) {
                        doc.Text = message.title.trim();
                    }
                }, { oldPage, newPage });
                await this.renameNamespaceReferences(oldPage, newPage);
            });
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Page '${oldPage}' wurde in '${newPage}' umbenannt.`);
        }
        catch (error) {
            this.reportMetadataError('Rename', error);
        }
        finally {
            this.isRenamingPage = false;
        }
    }
    async handleDeletePage(message) {
        const targetPage = this.normalizePageName(this.resolveNodePageIdentifier(message));
        if (!targetPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const confirmation = await vscode.window.showWarningMessage(`Möchten Sie die Page '${targetPage}' inklusive aller Dateien löschen?`, { modal: true }, 'Löschen');
        if (confirmation !== 'Löschen') {
            return;
        }
        const pageDir = this.getPageDirectory(targetPage);
        try {
            if (await this.pathExists(pageDir)) {
                await vscode.workspace.fs.delete(pageDir, { recursive: true, useTrash: false });
            }
            await this.removePageOrderEntry(targetPage);
            await this.updateProgramClass();
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Page '${targetPage}' wurde gelöscht.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Page '${targetPage}' konnte nicht gelöscht werden: ${details}`);
        }
    }
    async handleReorderPages(message) {
        if (!Array.isArray(message.order) || message.order.length === 0) {
            return;
        }
        if (!this.lastPayload || !this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const knownPages = this.lastPayload.nodes.map((node) => node.page);
        if (!knownPages.length) {
            return;
        }
        const knownSet = new Set(knownPages);
        const normalized = [];
        for (const entry of message.order) {
            const page = this.normalizePageName(entry);
            if (!page || !knownSet.has(page) || normalized.includes(page)) {
                continue;
            }
            normalized.push(page);
        }
        if (!normalized.length) {
            return;
        }
        const remaining = knownPages.filter((page) => !normalized.includes(page));
        const nextOrder = [...normalized, ...remaining];
        const hasChange = nextOrder.length !== knownPages.length || knownPages.some((page, index) => nextOrder[index] !== page);
        if (!hasChange) {
            return;
        }
        try {
            await this.writeBookPageOrder(nextOrder);
            await sendRuntimeCommand('PageOrder', nextOrder);
            this.reorderPayloadNodes(nextOrder);
            if (this.lastPayload && this.currentView) {
                this.currentView.webview.postMessage({ type: 'bookData', payload: this.lastPayload });
                this.postBookStatus();
            }
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Neue Page-Reihenfolge konnte nicht gespeichert werden: ${details}`);
        }
    }
    async handleCreatePage() {
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const rawName = await vscode.window.showInputBox({
            prompt: 'Enter Page Name',
            placeHolder: 'MyPage',
            ignoreFocusOut: true,
            validateInput: (value) => {
                if (!value || !value.trim()) {
                    return 'Page-Name ist erforderlich.';
                }
                if (/[\\/]/.test(value)) {
                    return 'Der Name darf keine / oder \\einhalten.';
                }
                return undefined;
            },
        });
        if (rawName === undefined) {
            return;
        }
        const pageName = this.normalizePageName(rawName);
        if (!pageName) {
            vscode.window.showWarningMessage('Page-Name ist ungültig.');
            return;
        }
        const pageDir = this.getPageDirectory(pageName);
        if (await this.pathExists(pageDir)) {
            vscode.window.showErrorMessage(`Page '${pageName}' existiert bereits.`);
            return;
        }
        try {
            await vscode.workspace.fs.createDirectory(pageDir);
            const codeUri = vscode.Uri.joinPath(pageDir, `${pageName}.qPage.cs`);
            const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
            await vscode.workspace.fs.writeFile(codeUri, textEncoder.encode(this.generateQPageTemplate(pageName)));
            await vscode.workspace.fs.writeFile(metaUri, textEncoder.encode(this.generateOPageTemplate(pageName)));
            await this.ensurePageOrderEntry(pageName);
            await this.updateProgramClass();
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Page '${pageName}' wurde erstellt.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Page konnte nicht erstellt werden: ${details}`);
        }
    }
    async handleImportPage() {
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const selection = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: false,
            canSelectMany: false,
            title: 'Select oPage.json for import',
            openLabel: 'Import',
            filters: { JSON: ['json'] },
        });
        if (!selection || selection.length === 0) {
            return;
        }
        const metaFile = selection[0];
        const fileName = path.basename(metaFile.fsPath);
        if (fileName.toLowerCase() !== 'opage.json') {
            vscode.window.showErrorMessage('Bitte wählen Sie eine Datei namens oPage.json aus.');
            return;
        }
        const sourceDirPath = path.dirname(metaFile.fsPath);
        const pageName = this.normalizePageName(path.basename(sourceDirPath));
        if (!pageName) {
            vscode.window.showErrorMessage('Der Zielordnername ist kein gültiger Page-Name.');
            return;
        }
        const targetDir = this.getPageDirectory(pageName);
        if (await this.pathExists(targetDir)) {
            vscode.window.showErrorMessage(`Page '${pageName}' existiert bereits im aktuellen Projekt.`);
            return;
        }
        const sourceDir = vscode.Uri.file(sourceDirPath);
        try {
            await this.copyDirectoryRecursive(sourceDir, targetDir);
            await this.ensurePageOrderEntry(pageName);
            await this.updateProgramClass();
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Page '${pageName}' wurde importiert.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Page konnte nicht importiert werden: ${details}`);
        }
    }
    async handleAddSubcode(message) {
        const targetPage = this.normalizePageName(this.resolveNodePageIdentifier(message));
        if (!targetPage) {
            vscode.window.showWarningMessage('Keine Page ausgewählt.');
            return;
        }
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const rawName = await vscode.window.showInputBox({
            prompt: `Neuer Subcode-Name für ${targetPage}`,
            placeHolder: 'MySubcode',
            ignoreFocusOut: true,
            validateInput: (value) => {
                if (!value || !value.trim()) {
                    return 'Name ist erforderlich.';
                }
                if (!this.normalizeCodeName(value)) {
                    return 'Nur Buchstaben, Zahlen, Punkt, Unterstrich und Bindestrich sind erlaubt.';
                }
                if (value.trim().toLowerCase() === 'qpage') {
                    return 'Subcode darf nicht qPage heißen.';
                }
                return undefined;
            },
        });
        if (rawName === undefined) {
            return;
        }
        const codeName = this.normalizeCodeName(rawName);
        if (!codeName) {
            vscode.window.showWarningMessage('Subcode-Name ist ungültig.');
            return;
        }
        if (codeName.toLowerCase() === 'qpage') {
            vscode.window.showWarningMessage('Subcode darf nicht qPage heißen.');
            return;
        }
        const pageDir = this.getPageDirectory(targetPage);
        const fileName = `${targetPage}.${codeName}.cs`;
        const fileUri = vscode.Uri.joinPath(pageDir, fileName);
        try {
            if (await this.pathExists(fileUri)) {
                vscode.window.showErrorMessage(`Datei '${fileName}' existiert bereits.`);
                return;
            }
            await vscode.workspace.fs.createDirectory(pageDir);
            await vscode.workspace.fs.writeFile(fileUri, textEncoder.encode(this.generateSubcodeTemplate(targetPage, codeName)));
            await this.updatePageMetadataFile(targetPage, (doc) => {
                const includes = Array.isArray(doc.Includes)
                    ? doc.Includes.filter((entry) => typeof entry === 'string')
                    : [];
                if (!includes.includes(fileName)) {
                    includes.push(fileName);
                }
                doc.Includes = includes;
                const codeOrder = Array.isArray(doc.CodeOrder)
                    ? doc.CodeOrder.filter((entry) => typeof entry === 'string')
                    : [];
                if (!codeOrder.includes(fileName)) {
                    codeOrder.push(fileName);
                }
                doc.CodeOrder = codeOrder;
            });
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Subcode '${fileName}' wurde erstellt.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Subcode konnte nicht erstellt werden: ${details}`);
        }
    }
    async handleRenameSubnode(message) {
        const pageName = this.normalizePageName(message.page);
        if (!pageName) {
            vscode.window.showWarningMessage('Keine Page für die Datei ausgewählt.');
            return;
        }
        const fileName = this.normalizeSubnodeFileName(pageName, message.fileName);
        if (!fileName) {
            vscode.window.showWarningMessage('Ungültiger Dateiname.');
            return;
        }
        const currentCode = this.extractSubnodeCodeName(pageName, fileName);
        if (!currentCode) {
            vscode.window.showWarningMessage('Codename konnte nicht ermittelt werden.');
            return;
        }
        if (this.isProtectedSubnodeFile(pageName, fileName)) {
            vscode.window.showWarningMessage('qPage Dateien dürfen nicht umbenannt werden.');
            return;
        }
        const nextCodeName = this.normalizeCodeName(message.codeName);
        if (!nextCodeName) {
            vscode.window.showWarningMessage('Ungültiger Codename. Erlaubt sind Buchstaben, Zahlen, Punkt, Unterstrich und Bindestrich.');
            return;
        }
        if (currentCode === nextCodeName) {
            vscode.window.showInformationMessage('Codename unverändert.');
            return;
        }
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const newFileName = `${pageName}.${nextCodeName}.cs`;
        const pageDir = this.getPageDirectory(pageName);
        const sourceUri = vscode.Uri.joinPath(pageDir, fileName);
        const targetUri = vscode.Uri.joinPath(pageDir, newFileName);
        try {
            const exists = await this.pathExists(sourceUri);
            if (!exists) {
                vscode.window.showErrorMessage(`Datei '${fileName}' wurde nicht gefunden.`);
                return;
            }
            if (await this.pathExists(targetUri)) {
                vscode.window.showErrorMessage(`Datei '${newFileName}' existiert bereits.`);
                return;
            }
            await vscode.workspace.fs.rename(sourceUri, targetUri, { overwrite: false });
            await this.updatePageMetadataFile(pageName, (doc) => {
                this.renameSubnodeReferences(doc, pageName, fileName, newFileName);
            });
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Datei '${fileName}' wurde in '${newFileName}' umbenannt.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Code-Datei konnte nicht umbenannt werden: ${details}`);
        }
    }
    async handleDeleteSubnode(message) {
        const pageName = this.normalizePageName(message.page);
        if (!pageName) {
            vscode.window.showWarningMessage('Keine Page für die Datei ausgewählt.');
            return;
        }
        const fileName = this.normalizeSubnodeFileName(pageName, message.fileName);
        if (!fileName) {
            vscode.window.showWarningMessage('Ungültiger Dateiname.');
            return;
        }
        if (this.isProtectedSubnodeFile(pageName, fileName)) {
            vscode.window.showWarningMessage('qPage Dateien dürfen nicht gelöscht werden.');
            return;
        }
        if (!this.lastBookRoot) {
            vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
            return;
        }
        const pageDir = this.getPageDirectory(pageName);
        const targetUri = vscode.Uri.joinPath(pageDir, fileName);
        try {
            const confirmation = await vscode.window.showWarningMessage(`Are you sure you want to delete '${fileName}'?`, { modal: true }, 'Delete');
            if (confirmation !== 'Delete') {
                return;
            }
            const exists = await this.pathExists(targetUri);
            if (!exists) {
                vscode.window.showErrorMessage(`Datei '${fileName}' wurde nicht gefunden.`);
                return;
            }
            await vscode.workspace.fs.delete(targetUri, { recursive: false, useTrash: false });
            await this.updatePageMetadataFile(pageName, (doc) => {
                this.removeSubnodeReferences(doc, pageName, fileName);
            });
            await this.reloadTreeView();
            vscode.window.showInformationMessage(`Datei '${fileName}' wurde gelöscht.`);
        }
        catch (error) {
            const details = error instanceof Error ? error.message : String(error);
            vscode.window.showErrorMessage(`Code-Datei konnte nicht gelöscht werden: ${details}`);
        }
    }
    normalizeFormatValue(value) {
        const normalized = value?.trim();
        if (!normalized) {
            return undefined;
        }
        const upper = normalized.toUpperCase();
        if (upper === 'A4' || upper === '16/9' || upper === '16/10') {
            return upper;
        }
        return undefined;
    }
    normalizePageName(value) {
        const trimmed = value?.trim();
        if (!trimmed || /[\\/]/.test(trimmed)) {
            return undefined;
        }
        return trimmed;
    }
    normalizeCodeName(value) {
        const trimmed = value?.trim();
        if (!trimmed) {
            return undefined;
        }
        if (!/^[A-Za-z0-9._-]+$/.test(trimmed)) {
            return undefined;
        }
        return trimmed;
    }
    normalizeSubnodeFileName(pageName, fileName) {
        const trimmed = fileName?.trim();
        if (!trimmed || /[\\/]/.test(trimmed)) {
            return undefined;
        }
        const lower = trimmed.toLowerCase();
        if (!lower.endsWith('.cs')) {
            return undefined;
        }
        if (!trimmed.startsWith(`${pageName}.`)) {
            return undefined;
        }
        return trimmed;
    }
    extractSubnodeCodeName(pageName, fileName) {
        const prefix = `${pageName}.`;
        const suffix = '.cs';
        if (!fileName.startsWith(prefix) || !fileName.endsWith(suffix)) {
            return undefined;
        }
        return fileName.slice(prefix.length, fileName.length - suffix.length);
    }
    isProtectedSubnodeFile(pageName, fileName) {
        const codeName = this.extractSubnodeCodeName(pageName, fileName);
        return Boolean(codeName && codeName.toLowerCase() === 'qpage');
    }
    async renamePageFolder(oldPage, newPage) {
        const oldDir = this.getPageDirectory(oldPage);
        const newDir = this.getPageDirectory(newPage);
        const exists = await this.pathExists(oldDir);
        if (!exists) {
            throw new Error(`Page-Ordner '${oldPage}' wurde nicht gefunden.`);
        }
        await vscode.workspace.fs.rename(oldDir, newDir, { overwrite: false });
    }
    async renamePageFiles(oldPage, newPage) {
        const pageDir = this.getPageDirectory(newPage);
        const entries = await vscode.workspace.fs.readDirectory(pageDir);
        for (const [name, type] of entries) {
            if (type !== vscode.FileType.File) {
                continue;
            }
            if (!name.toLowerCase().endsWith('.cs')) {
                continue;
            }
            if (!name.startsWith(`${oldPage}.`)) {
                continue;
            }
            const suffix = name.substring(oldPage.length);
            const targetName = `${newPage}${suffix}`;
            const sourceUri = vscode.Uri.joinPath(pageDir, name);
            const targetUri = vscode.Uri.joinPath(pageDir, targetName);
            await vscode.workspace.fs.rename(sourceUri, targetUri, { overwrite: false });
        }
    }
    async renameNamespaceReferences(oldPage, newPage) {
        if (!this.lastBookRoot) {
            return;
        }
        const oldToken = `Definition${oldPage}`;
        const newToken = `Definition${newPage}`;
        const pattern = new vscode.RelativePattern(this.lastBookRoot, '**/*.cs');
        const files = await vscode.workspace.findFiles(pattern);
        for (const file of files) {
            const raw = await vscode.workspace.fs.readFile(file);
            const text = textDecoder.decode(raw);
            if (!text.includes(oldToken)) {
                continue;
            }
            const updated = text.split(oldToken).join(newToken);
            if (updated !== text) {
                await vscode.workspace.fs.writeFile(file, textEncoder.encode(updated));
            }
        }
    }
    async renamePageInBookFile(oldPage, newPage) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        const bookUri = this.getBookFileUri();
        const raw = await vscode.workspace.fs.readFile(bookUri);
        const text = textDecoder.decode(raw);
        const data = JSON.parse(text);
        let mutated = false;
        if (Array.isArray(data.PageOrder)) {
            data.PageOrder = data.PageOrder.map((entry) => (entry === oldPage ? newPage : entry));
            mutated = true;
        }
        if (!mutated) {
            return;
        }
        const serialized = textEncoder.encode(JSON.stringify(data, null, 2) + '\n');
        await vscode.workspace.fs.writeFile(bookUri, serialized);
    }
    async writeBookPageOrder(order) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        const bookUri = this.getBookFileUri();
        let document = {};
        try {
            const raw = await vscode.workspace.fs.readFile(bookUri);
            if (raw?.length) {
                const text = textDecoder.decode(raw);
                if (text.trim().length > 0) {
                    document = JSON.parse(text);
                }
            }
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code === 'FileNotFound') {
                    document = {};
                }
                else {
                    throw fsError;
                }
            }
            else {
                throw error;
            }
        }
        document.PageOrder = order;
        const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
        await vscode.workspace.fs.writeFile(bookUri, serialized);
    }
    async ensurePageOrderEntry(pageName) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        const bookUri = this.getBookFileUri();
        let document = {};
        let mutated = false;
        try {
            const raw = await vscode.workspace.fs.readFile(bookUri);
            if (raw?.length) {
                const text = textDecoder.decode(raw);
                if (text.trim().length > 0) {
                    document = JSON.parse(text);
                }
            }
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code === 'FileNotFound') {
                    document = {};
                }
                else {
                    throw fsError;
                }
            }
            else {
                throw error;
            }
        }
        if (!Array.isArray(document.PageOrder)) {
            document.PageOrder = [];
            mutated = true;
        }
        if (!document.PageOrder.includes(pageName)) {
            document.PageOrder.push(pageName);
            mutated = true;
        }
        if (mutated) {
            const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
            await vscode.workspace.fs.writeFile(bookUri, serialized);
        }
    }
    async removePageOrderEntry(pageName) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        const bookUri = this.getBookFileUri();
        let document = {};
        let mutated = false;
        try {
            const raw = await vscode.workspace.fs.readFile(bookUri);
            if (raw?.length) {
                const text = textDecoder.decode(raw);
                if (text.trim().length > 0) {
                    document = JSON.parse(text);
                }
            }
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code === 'FileNotFound') {
                    document = {};
                }
                else {
                    throw fsError;
                }
            }
            else {
                throw error;
            }
        }
        if (Array.isArray(document.PageOrder)) {
            const originalLength = document.PageOrder.length;
            document.PageOrder = document.PageOrder.filter((entry) => entry !== pageName);
            mutated = mutated || document.PageOrder.length !== originalLength;
        }
        if (mutated) {
            const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
            await vscode.workspace.fs.writeFile(bookUri, serialized);
        }
    }
    async updateProgramClass() {
        if (!this.lastBookRoot) {
            return;
        }
        const pageNames = await this.getOrderedPageNames();
        const programUri = vscode.Uri.joinPath(this.lastBookRoot, 'Program.cs');
        const content = this.generateProgramClassContent(pageNames);
        await vscode.workspace.fs.writeFile(programUri, textEncoder.encode(content));
    }
    async getOrderedPageNames() {
        if (!this.lastBookRoot) {
            return [];
        }
        const bookUri = this.getBookFileUri();
        let document = {};
        try {
            const raw = await vscode.workspace.fs.readFile(bookUri);
            if (raw?.length) {
                const text = textDecoder.decode(raw);
                if (text.trim().length > 0) {
                    document = JSON.parse(text);
                }
            }
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code !== 'FileNotFound') {
                    throw fsError;
                }
            }
            else {
                throw error;
            }
        }
        const normalized = [];
        if (Array.isArray(document.PageOrder)) {
            for (const entry of document.PageOrder) {
                const value = typeof entry === 'string' ? entry : '';
                const page = this.normalizePageName(value);
                if (page && !normalized.includes(page)) {
                    normalized.push(page);
                }
            }
        }
        if (normalized.length) {
            return normalized;
        }
        return await this.discoverPageFolders(this.lastBookRoot);
    }
    generateProgramClassContent(pageNames) {
        const properties = pageNames
            .map((page) => `\t\tpublic static Definition${page}.qPage ${page} { get; } = new Definition${page}.qPage();`)
            .join('\n');
        const methodBody = (method) => {
            if (!pageNames.length) {
                return '\t\t\t// Keine Pages registriert';
            }
            return pageNames.map((page) => `\t\t\t${page}.${method}();`).join('\n');
        };
        const propertyBlock = properties ? `${properties}\n\n` : '';
        return `namespace QB
{
	public static class Program
	{
${propertyBlock}\t\tpublic static void Initialize()
		{
${methodBody('Initialize')}
		}

		public static void Run()
		{
${methodBody('Run')}
		}

		public static void Destroy()
		{
${methodBody('Destroy')}
		}
	}
}
`;
    }
    async copyDirectoryRecursive(source, target) {
        await vscode.workspace.fs.createDirectory(target);
        const entries = await vscode.workspace.fs.readDirectory(source);
        for (const [name, type] of entries) {
            const sourceChild = vscode.Uri.joinPath(source, name);
            const targetChild = vscode.Uri.joinPath(target, name);
            if (type === vscode.FileType.Directory) {
                await this.copyDirectoryRecursive(sourceChild, targetChild);
            }
            else if (type === vscode.FileType.File) {
                const data = await vscode.workspace.fs.readFile(sourceChild);
                await vscode.workspace.fs.writeFile(targetChild, data);
            }
            else if (type === vscode.FileType.SymbolicLink) {
                try {
                    const stats = await vscode.workspace.fs.stat(sourceChild);
                    if (stats.type === vscode.FileType.Directory) {
                        await this.copyDirectoryRecursive(sourceChild, targetChild);
                    }
                    else if (stats.type === vscode.FileType.File) {
                        const data = await vscode.workspace.fs.readFile(sourceChild);
                        await vscode.workspace.fs.writeFile(targetChild, data);
                    }
                }
                catch {
                    // Ignore broken symlinks
                }
            }
        }
    }
    getBookFileUri() {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        return vscode.Uri.joinPath(this.lastBookRoot, 'Book.json');
    }
    async updatePageMetadataFile(pageName, mutator, renameContext) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        const pageDir = this.getPageDirectory(pageName);
        const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
        await vscode.workspace.fs.createDirectory(pageDir);
        let document = {};
        try {
            const raw = await vscode.workspace.fs.readFile(metaUri);
            if (raw?.length) {
                const text = textDecoder.decode(raw);
                if (text.trim().length > 0) {
                    document = JSON.parse(text);
                }
            }
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code === 'FileNotFound') {
                    document = {};
                }
                else {
                    throw fsError;
                }
            }
            else {
                throw error;
            }
        }
        mutator(document);
        if (renameContext) {
            this.rewriteFileReferences(document, renameContext);
        }
        const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
        await vscode.workspace.fs.writeFile(metaUri, serialized);
    }
    generateQPageTemplate(pageName) {
        return `namespace Definition${pageName} { //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class qPage
    {
        //common fields/properties/methods/classes/types go here

        public void Initialize()
        {
            //initialization code goes here

        }

        public void Run()
        {
            //run/work code goes here

        }

        public void Destroy()
        {
            //destroy/cleanup code goes here
        }
    }
    //<CodeEnd>
}
`;
    }
    generateOPageTemplate(pageName) {
        const document = {
            Name: pageName,
            Text: pageName,
            OrderIndex: 0,
            Hidden: false,
            Format: 'A4',
            Includes: [],
            CodeOrder: [],
            Section: '',
            Url: null,
        };
        return JSON.stringify(document, null, 2) + '\n';
    }
    generateSubcodeTemplate(pageName, codeName) {
        return `namespace Definition${pageName}
{
    //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class ${codeName}
    {

    }
    //<CodeEnd>
}
`;
    }
    rewriteFileReferences(document, context) {
        const refFields = ['Includes', 'CodeOrder'];
        for (const field of refFields) {
            const value = document[field];
            if (!Array.isArray(value)) {
                continue;
            }
            document[field] = value.map((entry) => {
                if (typeof entry !== 'string') {
                    return entry;
                }
                if (!entry.startsWith(`${context.oldPage}.`)) {
                    return entry;
                }
                return `${context.newPage}${entry.substring(context.oldPage.length)}`;
            });
        }
    }
    renameSubnodeReferences(document, pageName, oldFileName, newFileName) {
        const refFields = ['Includes', 'CodeOrder'];
        for (const field of refFields) {
            const value = document[field];
            if (!Array.isArray(value)) {
                continue;
            }
            document[field] = value.map((entry) => {
                if (!this.matchesSubnodeReference(entry, pageName, oldFileName)) {
                    return entry;
                }
                const original = typeof entry === 'string' ? entry.trim() : '';
                const hasExtension = Boolean(original && original.toLowerCase().endsWith('.cs'));
                const hasPrefix = Boolean(original && original.startsWith(`${pageName}.`));
                if (!hasExtension) {
                    const codeName = this.extractSubnodeCodeName(pageName, newFileName) ?? newFileName;
                    return hasPrefix ? `${pageName}.${codeName}` : codeName;
                }
                return newFileName;
            });
        }
    }
    removeSubnodeReferences(document, pageName, fileName) {
        const refFields = ['Includes', 'CodeOrder'];
        for (const field of refFields) {
            const value = document[field];
            if (!Array.isArray(value)) {
                continue;
            }
            document[field] = value.filter((entry) => !this.matchesSubnodeReference(entry, pageName, fileName));
        }
    }
    matchesSubnodeReference(entry, pageName, fileName) {
        if (typeof entry !== 'string') {
            return false;
        }
        const entryToken = this.canonicalizeSubnodeReference(pageName, entry);
        const fileToken = this.canonicalizeSubnodeReference(pageName, fileName);
        if (!entryToken || !fileToken) {
            return false;
        }
        return entryToken === fileToken;
    }
    canonicalizeSubnodeReference(pageName, entry) {
        const trimmed = entry?.trim();
        if (!trimmed) {
            return undefined;
        }
        let payload = trimmed;
        if (payload.toLowerCase().endsWith('.cs')) {
            payload = payload.slice(0, -3);
        }
        const prefix = `${pageName}.`;
        if (payload.length > prefix.length && payload.slice(0, prefix.length).toLowerCase() === prefix.toLowerCase()) {
            payload = payload.slice(prefix.length);
        }
        if (!payload) {
            return undefined;
        }
        return `${pageName}.${payload}.cs`.toLowerCase();
    }
    getPageDirectory(pageName) {
        if (!this.lastBookRoot) {
            throw new Error('Kein qBook-Projekt geladen.');
        }
        return vscode.Uri.joinPath(this.lastBookRoot, 'Pages', pageName);
    }
    async pathExists(uri) {
        try {
            await vscode.workspace.fs.stat(uri);
            return true;
        }
        catch (error) {
            if (error instanceof vscode.FileSystemError) {
                const fsError = error;
                if (fsError.code === 'FileNotFound') {
                    return false;
                }
                throw fsError;
            }
            throw error;
        }
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
        this.postBookStatus();
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
    reportMetadataError(action, error) {
        const details = error instanceof Error ? error.message : String(error);
        vscode.window.showErrorMessage(`Page-Metadaten (${action}) konnten nicht gespeichert werden: ${details}`);
    }
    resolveNodePageIdentifier(message) {
        if (typeof message.nodePage === 'string' && message.nodePage.trim()) {
            return message.nodePage.trim();
        }
        if (typeof message.folder === 'string' && message.folder.trim()) {
            return message.folder.trim();
        }
        if (typeof message.page === 'string' && message.page.trim()) {
            return message.page.trim();
        }
        return undefined;
    }
    handleActiveEditorChange(editor) {
        const relative = this.relativePathFromUri(editor?.document?.uri);
        if (relative !== this.selectedPath) {
            this.selectedPath = relative;
            this.postBookStatus();
        }
    }
    refreshDiagnostics() {
        if (!this.lastBookRoot) {
            if (this.errorPaths.size > 0) {
                this.errorPaths.clear();
                this.postBookStatus();
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
            const relative = this.normalizeRelative(path.relative(basePath, absolutePath));
            next.add(relative);
        }
        if (!this.areSetsEqual(this.errorPaths, next)) {
            this.errorPaths = next;
            this.postBookStatus();
        }
        else {
            this.errorPaths = next;
        }
    }
    async readPageFolder(bookRoot, pageName) {
        const pagesDir = vscode.Uri.joinPath(bookRoot, 'Pages');
        const pageDir = vscode.Uri.joinPath(pagesDir, pageName);
        let codeOrder = [];
        let metadata = { name: pageName, title: pageName, format: 'A4', hidden: false };
        try {
            const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
            const metaRaw = await vscode.workspace.fs.readFile(metaUri);
            const metaText = textDecoder.decode(metaRaw);
            const metaData = JSON.parse(metaText);
            if (Array.isArray(metaData.CodeOrder)) {
                codeOrder = metaData.CodeOrder;
            }
            metadata = {
                name: metaData.Name ?? pageName,
                title: metaData.Text ?? metaData.Name ?? pageName,
                format: metaData.Format ?? 'A4',
                hidden: Boolean(metaData.Hidden),
            };
        }
        catch {
            // Meta file optional; ignore errors.
        }
        const filesInFolder = await this.safeReadDirectory(pageDir);
        const visibleFiles = filesInFolder
            .filter(([name, type]) => type === vscode.FileType.File && name !== 'oPage.json')
            .map(([name]) => name);
        const orderedFiles = codeOrder.filter((entry) => typeof entry === 'string' && entry.length > 0);
        for (const fileName of visibleFiles) {
            if (!orderedFiles.includes(fileName)) {
                orderedFiles.push(fileName);
            }
        }
        const primaryIndex = orderedFiles.findIndex((fileName) => this.isPrimaryCodeFile(fileName));
        if (primaryIndex > 0) {
            const [primaryFile] = orderedFiles.splice(primaryIndex, 1);
            orderedFiles.unshift(primaryFile);
        }
        const files = orderedFiles.map((fileName) => {
            const fileUri = vscode.Uri.joinPath(pageDir, fileName);
            const displayName = this.getDisplayName(pageName, fileName);
            return {
                name: fileName,
                relativePath: this.normalizeRelative(path.relative(bookRoot.fsPath, fileUri.fsPath)),
                displayName,
            };
        });
        return { page: pageName, files, metadata };
    }
    relativePathFromUri(uri) {
        if (!uri || !this.lastBookRoot) {
            return undefined;
        }
        const basePath = path.resolve(this.lastBookRoot.fsPath);
        const targetPath = path.resolve(uri.fsPath);
        if (!targetPath.toLowerCase().startsWith(basePath.toLowerCase())) {
            return undefined;
        }
        return this.normalizeRelative(path.relative(basePath, targetPath));
    }
    normalizeRelative(value) {
        return value.replace(/\\/g, '/');
    }
    postBookStatus() {
        if (!this.currentView) {
            return;
        }
        const form = this.getFormStateForSelection();
        this.currentView.webview.postMessage({
            type: 'bookStatus',
            payload: {
                selectedPath: this.selectedPath ?? null,
                errorPaths: Array.from(this.errorPaths),
                form,
            },
        });
    }
    updatePipeStatus(status) {
        this.pipeStatus = status;
        this.postPipeStatus();
    }
    notifyRuntimeSignal(signal, command) {
        this.applyRuntimeHighlight(signal.button, signal.kind);
        const timestamp = extractRuntimeTimestamp(command?.Args);
        const label = signal.button === 'run' ? 'Run' : signal.button === 'rebuild' ? 'Rebuild' : 'Stop';
        const nextStatus = timestamp
            ? `Runtime ${signal.kind}: ${label} @ ${timestamp}`
            : `Runtime ${signal.kind}: ${label}`;
        if (signal.kind === 'status') {
            if (signal.button === 'rebuild') {
                this.postStatusText('Rebuild done');
            }
            else if (signal.button === 'run') {
                this.postStatusText('Running...');
            }
            else if (signal.button === 'stop') {
                this.postStatusText('Stopped');
            }
        }
        if (this.currentView) {
            this.currentView.webview.postMessage({ type: 'runtimeInfo', text: nextStatus });
        }
    }
    postStatusText(text) {
        if (!this.currentView || !text) {
            return;
        }
        this.currentView.webview.postMessage({ type: 'statusText', text });
    }
    postPipeStatus() {
        if (!this.currentView) {
            return;
        }
        this.currentView.webview.postMessage({ type: 'pipeStatus', status: this.pipeStatus });
    }
    postRuntimeState() {
        if (!this.currentView) {
            return;
        }
        this.currentView.webview.postMessage({ type: 'runtimeState', payload: { ...this.runtimeButtonState } });
    }
    applyRuntimeHighlight(button, kind) {
        this.runtimeButtonState[button] = kind;
        this.postRuntimeState();
        const existingTimer = this.runtimeHighlightTimers.get(button);
        if (existingTimer) {
            clearTimeout(existingTimer);
        }
        const timer = setTimeout(() => {
            this.runtimeHighlightTimers.delete(button);
            if (this.runtimeButtonState[button] === kind) {
                this.runtimeButtonState[button] = null;
                this.postRuntimeState();
            }
        }, 4000);
        this.runtimeHighlightTimers.set(button, timer);
    }
    getFormStateForSelection() {
        if (!this.selectedPath || !this.lastPayload) {
            return undefined;
        }
        for (const node of this.lastPayload.nodes) {
            for (const file of node.files) {
                if (file.relativePath === this.selectedPath) {
                    return {
                        page: node.metadata?.name ?? node.page,
                        title: node.metadata?.title ?? node.page,
                        format: node.metadata?.format ?? 'A4',
                        hidden: node.metadata?.hidden ?? false,
                    };
                }
            }
        }
        return undefined;
    }
    clearProjectState() {
        const hadState = Boolean(this.lastBookRoot || this.selectedPath || this.errorPaths.size > 0);
        this.lastBookRoot = undefined;
        lastProjectRoot = undefined;
        this.selectedPath = undefined;
        this.errorPaths.clear();
        this.lastPayload = undefined;
        if (hadState) {
            this.postBookStatus();
        }
        if (currentPipeConfig?.source === 'project') {
            applySettingsPipeBridge(this.context);
        }
    }
    areSetsEqual(a, b) {
        if (a.size !== b.size) {
            return false;
        }
        for (const value of a) {
            if (!b.has(value)) {
                return false;
            }
        }
        return true;
    }
    async discoverPageFolders(bookRoot) {
        const pagesDir = vscode.Uri.joinPath(bookRoot, 'Pages');
        const entries = await this.safeReadDirectory(pagesDir);
        return entries
            .filter(([, type]) => type === vscode.FileType.Directory)
            .map(([name]) => name)
            .sort((a, b) => a.localeCompare(b));
    }
    async findBookFile() {
        const folders = vscode.workspace.workspaceFolders ?? [];
        for (const folder of folders) {
            const candidate = vscode.Uri.joinPath(folder.uri, 'Book.json');
            try {
                const stat = await vscode.workspace.fs.stat(candidate);
                if (stat.type === vscode.FileType.File) {
                    return candidate;
                }
            }
            catch {
                // ignore
            }
        }
        return undefined;
    }
    getDirectoryUri(fileUri) {
        const segments = fileUri.path.split('/');
        segments.pop();
        const directoryPath = segments.join('/') || '/';
        return fileUri.with({ path: directoryPath });
    }
    isPrimaryCodeFile(fileName) {
        const lower = fileName.toLowerCase();
        return lower.endsWith('.qpage.cs') || lower.endsWith('.opage.cs');
    }
    getDisplayName(pageName, fileName) {
        if (!fileName) {
            return '';
        }
        const normalizedPage = pageName.toLowerCase();
        const normalizedFile = fileName.toLowerCase();
        if (normalizedFile.startsWith(normalizedPage.toLowerCase())) {
            const suffix = fileName.substring(pageName.length);
            if (suffix.toLowerCase().startsWith('.qpage') || suffix.toLowerCase().startsWith('.opage')) {
                return 'qPage';
            }
            if (suffix.toLowerCase().startsWith('.procedures')) {
                return 'Procedures';
            }
            if (suffix.toLowerCase().startsWith('.test')) {
                return 'Test';
            }
            if (suffix.toLowerCase().startsWith('.view')) {
                return 'View';
            }
            if (suffix.toLowerCase().startsWith('.customcode')) {
                return 'Custom Code';
            }
            if (suffix.toLowerCase().startsWith('.sequencer')) {
                return 'Sequencer';
            }
            if (suffix.toLowerCase().startsWith('.testdevice')) {
                return 'Test Device';
            }
            if (suffix.toLowerCase().startsWith('.clientudl')) {
                return 'Client UDL';
            }
        }
        if (fileName.toLowerCase().endsWith('.cs')) {
            const withoutExtension = fileName.slice(0, -3);
            if (withoutExtension.startsWith(`${pageName}.`)) {
                return withoutExtension.slice(pageName.length + 1);
            }
            return withoutExtension;
        }
        if (fileName.startsWith(`${pageName}.`)) {
            return fileName.slice(pageName.length + 1);
        }
        return fileName;
    }
    async safeReadDirectory(uri) {
        try {
            return await vscode.workspace.fs.readDirectory(uri);
        }
        catch {
            return [];
        }
    }
}
//# sourceMappingURL=extension.js.map