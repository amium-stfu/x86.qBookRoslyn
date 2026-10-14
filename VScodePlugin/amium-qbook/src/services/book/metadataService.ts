import * as vscode from 'vscode';

export function reportMetadataError(action: string, error: unknown): void {
  const details = error instanceof Error ? error.message : String(error);
  vscode.window.showErrorMessage(`Page-Metadata (${action}) could not be saved: ${details}`);
}