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
exports.PipeBridge = exports.PipeEventClient = exports.PipeCommandClient = void 0;
const net = __importStar(require("node:net"));
const readline = __importStar(require("node:readline"));
const node_events_1 = require("node:events");
const vscode = __importStar(require("vscode"));
function ensurePipeName(value) {
    const trimmed = value?.trim();
    if (!trimmed) {
        throw new Error('Named pipe identifier is required.');
    }
    return trimmed;
}
function resolvePipePath(pipeName) {
    if (process.platform === 'win32') {
        return `\\\\.\\pipe\\${pipeName}`;
    }
    // Fallback for non-Windows host (mainly for testing)
    return pipeName.startsWith('/') ? pipeName : `/tmp/${pipeName}`;
}
class PipeCommandClient {
    pipeName;
    logger;
    pipePath;
    constructor(pipeName, logger) {
        this.pipeName = pipeName;
        this.logger = logger;
        this.pipeName = ensurePipeName(pipeName);
        this.pipePath = resolvePipePath(this.pipeName);
    }
    async send(command, signal) {
        if (!command || typeof command.Command !== 'string' || !command.Command.trim()) {
            throw new Error('PipeCommandPayload.Command must be a non-empty string.');
        }
        const payload = JSON.stringify(command);
        return new Promise((resolve, reject) => {
            let settled = false;
            let writeCompleted = false;
            let abortHandler;
            const socket = net.createConnection(this.pipePath);
            const cleanup = (error) => {
                if (settled) {
                    return;
                }
                settled = true;
                socket.removeAllListeners();
                if (!socket.destroyed) {
                    socket.destroy();
                }
                if (signal && abortHandler) {
                    signal.removeEventListener('abort', abortHandler);
                }
                if (error) {
                    this.logger?.error?.(`PipeCommandClient send failed (${this.pipeName})`, error);
                    reject(error);
                }
                else {
                    resolve();
                }
            };
            const onClose = () => {
                if (!writeCompleted) {
                    cleanup(new Error(`Pipe '${this.pipeName}' closed before payload was sent.`));
                }
                else {
                    cleanup();
                }
            };
            const onError = (error) => {
                cleanup(error);
            };
            abortHandler = () => {
                cleanup(new Error('PipeCommandClient send aborted.'));
            };
            socket.once('connect', () => {
                socket.write(payload + '\n', (error) => {
                    if (error) {
                        cleanup(error);
                        return;
                    }
                    writeCompleted = true;
                    socket.end();
                });
            });
            socket.once('close', onClose);
            socket.once('error', onError);
            if (signal) {
                if (signal.aborted) {
                    abortHandler();
                }
                else if (abortHandler) {
                    signal.addEventListener('abort', abortHandler, { once: true });
                }
            }
        });
    }
}
exports.PipeCommandClient = PipeCommandClient;
class PipeEventClient extends node_events_1.EventEmitter {
    pipeName;
    reconnectDelay;
    logger;
    pipePath;
    socket;
    reader;
    reconnectTimer;
    disposed = false;
    connectionState = 'disconnected';
    constructor(pipeName, reconnectDelay = 500, logger) {
        super();
        this.pipeName = pipeName;
        this.reconnectDelay = reconnectDelay;
        this.logger = logger;
        this.pipeName = ensurePipeName(pipeName);
        this.pipePath = resolvePipePath(this.pipeName);
        this.connectWithRetry();
    }
    dispose() {
        this.disposed = true;
        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
            this.reconnectTimer = undefined;
        }
        this.cleanupConnections();
        this.removeAllListeners();
        this.setState('disconnected');
    }
    connectWithRetry() {
        if (this.disposed) {
            return;
        }
        this.logger?.info?.(`Connecting to pipe '${this.pipeName}'...`);
        const socket = net.createConnection(this.pipePath);
        this.socket = socket;
        socket.once('connect', () => {
            this.logger?.info?.(`Connected to pipe '${this.pipeName}'.`);
            this.setState('connected');
        });
        socket.on('error', (error) => {
            this.logger?.error?.(`Pipe '${this.pipeName}' reported an error.`, error);
            this.setState('disconnected');
            this.scheduleReconnect();
            this.emit('error', error);
        });
        socket.on('close', () => {
            this.logger?.info?.(`Pipe '${this.pipeName}' connection closed.`);
            this.setState('disconnected');
            this.scheduleReconnect();
        });
        const reader = readline.createInterface({
            input: socket,
            crlfDelay: Infinity,
        });
        reader.on('line', (line) => this.handleLine(line));
        reader.on('close', () => {
            this.setState('disconnected');
            this.scheduleReconnect();
        });
        this.reader = reader;
    }
    scheduleReconnect() {
        if (this.disposed || this.reconnectTimer) {
            return;
        }
        this.cleanupConnections();
        this.reconnectTimer = setTimeout(() => {
            this.reconnectTimer = undefined;
            this.connectWithRetry();
        }, Math.max(100, this.reconnectDelay));
        this.reconnectTimer.unref?.();
    }
    cleanupConnections() {
        try {
            this.reader?.removeAllListeners();
            this.reader?.close();
        }
        catch {
            // ignore
        }
        finally {
            this.reader = undefined;
        }
        if (this.socket) {
            this.socket.removeAllListeners();
            try {
                this.socket.destroy();
            }
            catch {
                // ignore
            }
            this.socket = undefined;
        }
    }
    handleLine(rawLine) {
        const trimmed = rawLine?.trim();
        if (!trimmed) {
            return;
        }
        try {
            const payload = JSON.parse(trimmed);
            if (!payload || typeof payload.Command !== 'string') {
                this.logger?.error?.(`Pipe '${this.pipeName}' payload missing Command.`, payload);
                return;
            }
            this.emit('message', payload);
        }
        catch (error) {
            this.logger?.error?.(`Failed to parse payload from pipe '${this.pipeName}'.`, error);
            this.emit('error', error);
        }
    }
    setState(state) {
        if (this.connectionState === state) {
            return;
        }
        this.connectionState = state;
        this.emit(state);
    }
}
exports.PipeEventClient = PipeEventClient;
class PipeBridge {
    commandClient;
    eventClient;
    messageEmitter = new vscode.EventEmitter();
    statusEmitter = new vscode.EventEmitter();
    disposables = [];
    onMessage = this.messageEmitter.event;
    onStatus = this.statusEmitter.event;
    constructor(options) {
        this.commandClient = new PipeCommandClient(options.serverPipe, options.logger);
        this.eventClient = new PipeEventClient(options.clientPipe, options.reconnectDelay, options.logger);
        const messageListener = (command) => {
            this.messageEmitter.fire(command);
        };
        const connectedListener = () => this.statusEmitter.fire('connected');
        const disconnectedListener = () => this.statusEmitter.fire('disconnected');
        this.eventClient.on('message', messageListener);
        this.eventClient.on('connected', connectedListener);
        this.eventClient.on('disconnected', disconnectedListener);
        this.disposables.push(new vscode.Disposable(() => this.eventClient.off('message', messageListener)));
        this.disposables.push(new vscode.Disposable(() => this.eventClient.off('connected', connectedListener)));
        this.disposables.push(new vscode.Disposable(() => this.eventClient.off('disconnected', disconnectedListener)));
    }
    send(command, signal) {
        return this.commandClient.send(command, signal);
    }
    dispose() {
        this.eventClient.dispose();
        this.messageEmitter.dispose();
        this.statusEmitter.dispose();
        this.disposables.forEach((disposable) => {
            try {
                disposable.dispose();
            }
            catch {
                // ignore
            }
        });
        this.disposables.length = 0;
    }
}
exports.PipeBridge = PipeBridge;
//# sourceMappingURL=namedPipeClients.js.map