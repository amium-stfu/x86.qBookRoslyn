import * as vscode from 'vscode';
import { PipeConnectionState } from '../../pipes/namedPipeClients.js';
import { RuntimeButtonId, RuntimeSignalKind } from '../../pipes/pipeCommands.js';

type RuntimeButtonState = Record<RuntimeButtonId, RuntimeSignalKind | null>;

export function postStatusText(view: vscode.WebviewView | undefined, text: string): void {
  if (!view || !text) {
    return;
  }
  view.webview.postMessage({ type: 'statusText', text });
}

export function postPipeStatus(view: vscode.WebviewView | undefined, status: PipeConnectionState): void {
  if (!view) {
    return;
  }
  view.webview.postMessage({ type: 'pipeStatus', status });
}

export function postRuntimeState(view: vscode.WebviewView | undefined, runtimeButtonState: RuntimeButtonState): void {
  if (!view) {
    return;
  }
  view.webview.postMessage({ type: 'runtimeState', payload: { ...runtimeButtonState } });
}

export function postDebugState(view: vscode.WebviewView | undefined, active: boolean): void {
  if (!view) {
    return;
  }
  view.webview.postMessage({ type: 'debugState', payload: { active } });
}

export function postRuntimeInfo(view: vscode.WebviewView | undefined, text: string): void {
  if (!view || !text) {
    return;
  }
  view.webview.postMessage({ type: 'runtimeInfo', text });
}