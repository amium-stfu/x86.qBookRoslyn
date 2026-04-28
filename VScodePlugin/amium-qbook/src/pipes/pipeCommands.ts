import * as vscode from 'vscode';
import { PipeBridge, PipeCommandPayload, PipeConnectionState } from './namedPipeClients';

export type RuntimeButtonId = 'run' | 'stop' | 'rebuild';
export type RuntimeSignalKind = 'status' | 'alert';
export type RuntimeSignal = {
  button: RuntimeButtonId;
  kind: RuntimeSignalKind;
};

type RuntimeLogLevel = 'debug' | 'info' | 'error' | 'fatal';

export type PipeCommandReceiverContext = {
  notifyRuntimeSignal: (signal: RuntimeSignal, command?: PipeCommandPayload) => void;
  getRuntimeLogChannel: () => vscode.LogOutputChannel | undefined;
};

export type PipeCommandSenderContext = {
  getBridge: () => PipeBridge | undefined;
  showErrorMessage: (message: string) => void;
  broadcastStatus: (status: PipeConnectionState) => void;
};

export function handleIncomingPipeCommand(
  command: PipeCommandPayload,
  context: PipeCommandReceiverContext
): void {
  if (handleRuntimeLogCommand(command, context)) {
    return;
  }

  const signal = parseRuntimeSignal(command);
  if (!signal) {
    return;
  }

  context.notifyRuntimeSignal(signal, command);
}

function handleRuntimeLogCommand(command: PipeCommandPayload, context: PipeCommandReceiverContext): boolean {
  const level = normalizeRuntimeLogCommand(command?.Command);
  if (!level) {
    return false;
  }

  const message = extractRuntimeLogMessage(command.Args);
  logRuntimeMessage(level, message, context);
  return true;
}

function normalizeRuntimeLogCommand(value?: string): RuntimeLogLevel | undefined {
  const normalized = value?.trim().toLowerCase();
  if (!normalized) {
    return undefined;
  }

  if (normalized === 'loginfo') {
    return 'info';
  }
  if (normalized === 'logerror') {
    return 'error';
  }
  if (normalized === 'logfatal' || normalized === 'logifatal') {
    return 'fatal';
  }
  if (normalized === 'logdebug' || normalized === 'logidebug') {
    return 'debug';
  }

  return undefined;
}

function extractRuntimeLogMessage(args?: string[]): string {
  if (!Array.isArray(args) || args.length === 0) {
    return '';
  }
  const firstWithContent = args.find((entry) => typeof entry === 'string' && entry.trim().length > 0);
  return (firstWithContent ?? args[0] ?? '').toString();
}

function logRuntimeMessage(level: RuntimeLogLevel, rawMessage: string, context: PipeCommandReceiverContext): void {
  const channel = context.getRuntimeLogChannel();
  if (!channel) {
    return;
  }
  const message = rawMessage?.trim().length ? rawMessage : '(no message)';
  switch (level) {
    case 'debug':
      channel.debug(message);
      break;
    case 'info':
      channel.info(message);
      break;
    case 'error':
      channel.error(message);
      break;
    case 'fatal':
      channel.error(`[FATAL] ${message}`);
      break;
  }
}

export function extractRuntimeTimestamp(args?: string[]): string | undefined {
  if (!Array.isArray(args) || args.length === 0) {
    return undefined;
  }
  const first = args[0];
  return typeof first === 'string' && first.trim() ? first.trim() : undefined;
}

function parseRuntimeSignal(command: PipeCommandPayload): RuntimeSignal | undefined {
  const rawCommand = command?.Command?.trim();
  if (!rawCommand) {
    return undefined;
  }

  let kind: RuntimeSignalKind | undefined;
  let button: RuntimeButtonId | undefined;

  const colonIndex = rawCommand.indexOf(':');
  if (colonIndex >= 0) {
    const prefix = rawCommand.slice(0, colonIndex);
    const suffix = rawCommand.slice(colonIndex + 1);
    kind = normalizeRuntimeSignalKind(prefix);
    button = normalizeRuntimeButton(suffix) ?? extractRuntimeButtonFromArgs(command.Args);
  } else {
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

function extractRuntimeButtonFromArgs(args?: string[]): RuntimeButtonId | undefined {
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

function normalizeRuntimeSignalKind(value?: string): RuntimeSignalKind | undefined {
  const normalized = value?.trim().toLowerCase();
  if (normalized === 'status') {
    return 'status';
  }
  if (normalized === 'alert') {
    return 'alert';
  }
  return undefined;
}

function normalizeRuntimeButton(value?: string): RuntimeButtonId | undefined {
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

export async function sendRuntimeCommand(
  context: PipeCommandSenderContext,
  command: string,
  args?: string[]
): Promise<void> {
  const bridge = context.getBridge();
  if (!bridge) {
    context.showErrorMessage('Pipe is not connected.');
    return;
  }

  const payload: PipeCommandPayload = {
    Command: command,
    ...(Array.isArray(args) && args.length > 0 ? { Args: args } : {}),
  };

  try {
    await bridge.send(payload);
  } catch (error: unknown) {
    const details = error instanceof Error ? error.message : String(error);
    context.showErrorMessage(`Pipe command '${command}' failed: ${details}`);
    context.broadcastStatus('disconnected');
  }
}
