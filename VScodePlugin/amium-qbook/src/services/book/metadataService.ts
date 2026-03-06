import * as vscode from 'vscode';

export function reportMetadataError(action: string, error: unknown): void {
  const details = error instanceof Error ? error.message : String(error);
  vscode.window.showErrorMessage(`Page-Metadaten (${action}) konnten nicht gespeichert werden: ${details}`);
}