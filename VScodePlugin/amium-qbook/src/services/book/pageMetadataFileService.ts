import * as vscode from 'vscode';
import { TextDecoder, TextEncoder } from 'util';
import { OPageDocument } from '../../types/extensionTypes.js';
import { rewriteFileReferences } from './references.js';

const textDecoder = new TextDecoder('utf-8');
const textEncoder = new TextEncoder();

export function getPageDirectory(bookRoot: vscode.Uri | undefined, pageName: string): vscode.Uri {
  if (!bookRoot) {
    throw new Error('Kein qBook-Projekt geladen.');
  }
  return vscode.Uri.joinPath(bookRoot, 'Pages', pageName);
}

export async function updatePageMetadataFile(
  bookRoot: vscode.Uri | undefined,
  pageName: string,
  mutator: (doc: OPageDocument) => void,
  renameContext?: { oldPage: string; newPage: string }
): Promise<void> {
  const pageDir = getPageDirectory(bookRoot, pageName);
  const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
  await vscode.workspace.fs.createDirectory(pageDir);
  let document: OPageDocument = {};

  try {
    const raw = await vscode.workspace.fs.readFile(metaUri);
    if (raw?.length) {
      const text = textDecoder.decode(raw);
      if (text.trim().length > 0) {
        document = JSON.parse(text) as OPageDocument;
      }
    }
  } catch (error: unknown) {
    if (error instanceof vscode.FileSystemError) {
      const fsError = error as vscode.FileSystemError;
      if (fsError.code === 'FileNotFound') {
        document = {};
      } else {
        throw fsError;
      }
    } else {
      throw error;
    }
  }

  mutator(document);
  if (renameContext) {
    rewriteFileReferences(document, renameContext);
  }

  const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
  await vscode.workspace.fs.writeFile(metaUri, serialized);
}