import * as vscode from 'vscode';
import { removePageOrderEntry } from './bookFileService.js';
import { pathExists } from './fileSystemService.js';
import { getPageDirectory } from './pageMetadataFileService.js';

export async function deletePageFromProject(
  bookRoot: vscode.Uri | undefined,
  pageName: string
): Promise<void> {
  if (!bookRoot) {
    throw new Error('Kein qBook-Projekt geladen.');
  }

  const pageDir = getPageDirectory(bookRoot, pageName);
  if (await pathExists(pageDir)) {
    await vscode.workspace.fs.delete(pageDir, { recursive: true, useTrash: false });
  }

  await removePageOrderEntry(bookRoot, pageName);
}

export function getDeletePageErrorMessage(pageName: string, error: unknown): string {
  const details = error instanceof Error ? error.message : String(error);
  return `Page '${pageName}' konnte nicht gelöscht werden: ${details}`;
}