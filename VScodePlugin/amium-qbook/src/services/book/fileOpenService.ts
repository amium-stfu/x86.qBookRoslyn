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
    vscode.window.showWarningMessage('No file selected.');
    return undefined;
  }

  if (!baseUri) {
    vscode.window.showWarningMessage('No qBook project is open.');
    return undefined;
  }

  const targetUri = resolveRelativeFileUri(relativePath, baseUri);
  if (!targetUri) {
    vscode.window.showErrorMessage('File is outside the qBook directory.');
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
    vscode.window.showErrorMessage(`File could not be opened: ${relativePath}\n${details}`);
    return undefined;
  }
}