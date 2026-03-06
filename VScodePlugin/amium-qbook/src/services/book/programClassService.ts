import * as vscode from 'vscode';
import { TextDecoder, TextEncoder } from 'util';
import { generateProgramClassContent } from './templates.js';
import { getBookFileUri } from './bookFileService.js';
import { discoverPageFolders } from './treeService.js';
import { normalizePageName } from './validation.js';

const textDecoder = new TextDecoder('utf-8');
const textEncoder = new TextEncoder();

export async function updateProgramClass(bookRoot: vscode.Uri | undefined): Promise<void> {
  if (!bookRoot) {
    return;
  }

  const pageNames = await getOrderedPageNames(bookRoot);
  const programUri = vscode.Uri.joinPath(bookRoot, 'Program.cs');
  const content = generateProgramClassContent(pageNames);
  await vscode.workspace.fs.writeFile(programUri, textEncoder.encode(content));
}

async function getOrderedPageNames(bookRoot: vscode.Uri): Promise<string[]> {
  const bookUri = getBookFileUri(bookRoot);
  let document: { PageOrder?: unknown } = {};

  try {
    const raw = await vscode.workspace.fs.readFile(bookUri);
    if (raw?.length) {
      const text = textDecoder.decode(raw);
      if (text.trim().length > 0) {
        document = JSON.parse(text) as { PageOrder?: unknown };
      }
    }
  } catch (error: unknown) {
    if (error instanceof vscode.FileSystemError) {
      const fsError = error as vscode.FileSystemError;
      if (fsError.code !== 'FileNotFound') {
        throw fsError;
      }
    } else {
      throw error;
    }
  }

  const normalized: string[] = [];
  if (Array.isArray(document.PageOrder)) {
    for (const entry of document.PageOrder) {
      const value = typeof entry === 'string' ? entry : '';
      const page = normalizePageName(value);
      if (page && !normalized.includes(page)) {
        normalized.push(page);
      }
    }
  }

  if (normalized.length) {
    return normalized;
  }

  return await discoverPageFolders(bookRoot);
}