import * as net from 'node:net';
import * as readline from 'node:readline';
import { EventEmitter } from 'node:events';
import * as vscode from 'vscode';

export type PipeLogger = {
  info?: (message: string) => void;
  error?: (message: string, error?: unknown) => void;
};

export type PipeCommandPayload = {
  Command: string;
  Args?: string[];
};

export type PipeConnectionState = 'connected' | 'disconnected';

function ensurePipeName(value: string): string {
  const trimmed = value?.trim();
  if (!trimmed) {
    throw new Error('Named pipe identifier is required.');
  }
  return trimmed;
}

function resolvePipePath(pipeName: string): string {
  if (process.platform === 'win32') {
    return `\\\\.\\pipe\\${pipeName}`;
  }

  // Fallback for non-Windows host (mainly for testing)
  return pipeName.startsWith('/') ? pipeName : `/tmp/${pipeName}`;
}

export class PipeCommandClient {
  private readonly pipePath: string;

  constructor(private readonly pipeName: string, private readonly logger?: PipeLogger) {
    this.pipeName = ensurePipeName(pipeName);
    this.pipePath = resolvePipePath(this.pipeName);
  }

  async send(command: PipeCommandPayload, signal?: AbortSignal): Promise<void> {
    if (!command || typeof command.Command !== 'string' || !command.Command.trim()) {
      throw new Error('PipeCommandPayload.Command must be a non-empty string.');
    }

    const payload = JSON.stringify(command);

    return new Promise<void>((resolve, reject) => {
      let settled = false;
      let writeCompleted = false;
      let abortHandler: (() => void) | undefined;
      const socket = net.createConnection(this.pipePath);

      const cleanup = (error?: Error) => {
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
        } else {
          resolve();
        }
      };

      const onClose = () => {
        if (!writeCompleted) {
          cleanup(new Error(`Pipe '${this.pipeName}' closed before payload was sent.`));
        } else {
          cleanup();
        }
      };

      const onError = (error: Error) => {
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
        } else if (abortHandler) {
          signal.addEventListener('abort', abortHandler, { once: true });
        }
      }
    });
  }
}

export class PipeEventClient extends EventEmitter {
  private readonly pipePath: string;
  private socket?: net.Socket;
  private reader?: readline.Interface;
  private reconnectTimer?: NodeJS.Timeout;
  private disposed = false;
  private connectionState: PipeConnectionState = 'disconnected';

  constructor(private readonly pipeName: string, private readonly reconnectDelay = 500, private readonly logger?: PipeLogger) {
    super();
    this.pipeName = ensurePipeName(pipeName);
    this.pipePath = resolvePipePath(this.pipeName);
    this.connectWithRetry();
  }

  public dispose(): void {
    this.disposed = true;
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = undefined;
    }
    this.cleanupConnections();
    this.removeAllListeners();
    this.setState('disconnected');
  }

  private connectWithRetry(): void {
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

  private scheduleReconnect(): void {
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

  private cleanupConnections(): void {
    try {
      this.reader?.removeAllListeners();
      this.reader?.close();
    } catch {
      // ignore
    } finally {
      this.reader = undefined;
    }

    if (this.socket) {
      this.socket.removeAllListeners();
      try {
        this.socket.destroy();
      } catch {
        // ignore
      }
      this.socket = undefined;
    }
  }

  private handleLine(rawLine: string): void {
    const trimmed = rawLine?.trim();
    if (!trimmed) {
      return;
    }

    try {
      const payload = JSON.parse(trimmed) as PipeCommandPayload;
      if (!payload || typeof payload.Command !== 'string') {
        this.logger?.error?.(`Pipe '${this.pipeName}' payload missing Command.`, payload);
        return;
      }
      this.emit('message', payload);
    } catch (error) {
      this.logger?.error?.(`Failed to parse payload from pipe '${this.pipeName}'.`, error);
      this.emit('error', error);
    }
  }

  private setState(state: PipeConnectionState): void {
    if (this.connectionState === state) {
      return;
    }
    this.connectionState = state;
    this.emit(state);
  }
}

export type PipeBridgeOptions = {
  serverPipe: string;
  clientPipe: string;
  reconnectDelay?: number;
  logger?: PipeLogger;
};

export class PipeBridge implements vscode.Disposable {
  private readonly commandClient: PipeCommandClient;
  private readonly eventClient: PipeEventClient;
  private readonly messageEmitter = new vscode.EventEmitter<PipeCommandPayload>();
  private readonly statusEmitter = new vscode.EventEmitter<PipeConnectionState>();
  private readonly disposables: vscode.Disposable[] = [];

  public readonly onMessage = this.messageEmitter.event;
  public readonly onStatus = this.statusEmitter.event;

  constructor(options: PipeBridgeOptions) {
    this.commandClient = new PipeCommandClient(options.serverPipe, options.logger);
    this.eventClient = new PipeEventClient(options.clientPipe, options.reconnectDelay, options.logger);

    const messageListener = (command: PipeCommandPayload) => {
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

  public send(command: PipeCommandPayload, signal?: AbortSignal): Promise<void> {
    return this.commandClient.send(command, signal);
  }

  public dispose(): void {
    this.eventClient.dispose();
    this.messageEmitter.dispose();
    this.statusEmitter.dispose();
    this.disposables.forEach((disposable) => {
      try {
        disposable.dispose();
      } catch {
        // ignore
      }
    });
    this.disposables.length = 0;
  }
}
