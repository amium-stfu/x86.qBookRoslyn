import * as vscode from 'vscode';
import { normalizeRelativePath, resolveRelativeFileUri } from './verificationService.js';

export type OpenRelativeFileOptions = {
  preview?: boolean;
  preserveFocus?: boolean;
};

export async function openRelativeFile(
  relativePath: string | undefined,
  baseUri: vscode.Uri | undefined,
  options?: OpenRelativeFileOptions
): Promise<string | undefined> {
  if (!relativePath) {
    vscode.window.showWarningMessage('Keine Datei ausgewÃ¤hlt.');
    return undefined;
  }

  if (!baseUri) {
    vscode.window.showWarningMessage('Es ist kein qBook-Projekt geÃ¶ffnet.');
    return undefined;
  }

  const targetUri = resolveRelativeFileUri(relativePath, baseUri);
  if (!targetUri) {
    vscode.window.showErrorMessage('Datei liegt auÃŸerhalb des qBook-Verzeichnisses.');
    return undefined;
  }

  const normalizedRelative = normalizeRelativePath(relativePath);

  try {
    const document = await vscode.workspace.openTextDocument(targetUri);
    await vscode.window.showTextDocument(document, {
      preview: options?.preview ?? false,
      preserveFocus: options?.preserveFocus ?? false,
    });
    return normalizedRelative;
  } catch (error: unknown) {
    const details = error instanceof Error ? error.message : String(error);
    vscode.window.showErrorMessage(`Datei konnte nicht geÃ¶ffnet werden: ${relativePath}\n${details}`);
    return undefined;
  }
}