import * as vscode from 'vscode';
import { TextDecoder, TextEncoder } from 'util';

const textDecoder = new TextDecoder('utf-8');
const textEncoder = new TextEncoder();

type BookDocument = Record<string, unknown> & { PageOrder?: string[] };

export function getBookFileUri(bookRoot: vscode.Uri): vscode.Uri {
  return vscode.Uri.joinPath(bookRoot, 'Book.json');
}

export async function renamePageInBookFile(bookRoot: vscode.Uri, oldPage: string, newPage: string): Promise<void> {
  const bookUri = getBookFileUri(bookRoot);
  const raw = await vscode.workspace.fs.readFile(bookUri);
  const text = textDecoder.decode(raw);
  const data = JSON.parse(text) as { PageOrder?: unknown } & Record<string, unknown>;

  let mutated = false;
  if (Array.isArray(data.PageOrder)) {
    data.PageOrder = data.PageOrder.map((entry) => (entry === oldPage ? newPage : entry));
    mutated = true;
  }

  if (!mutated) {
    return;
  }

  const serialized = textEncoder.encode(JSON.stringify(data, null, 2) + '\n');
  await vscode.workspace.fs.writeFile(bookUri, serialized);
}

export async function writeBookPageOrder(bookRoot: vscode.Uri, order: string[]): Promise<void> {
  const bookUri = getBookFileUri(bookRoot);
  const document = await readBookDocument(bookUri);
  document.PageOrder = order;
  await writeBookDocument(bookUri, document);
}

export async function ensurePageOrderEntry(bookRoot: vscode.Uri, pageName: string): Promise<void> {
  const bookUri = getBookFileUri(bookRoot);
  const document = await readBookDocument(bookUri);
  let mutated = false;

  if (!Array.isArray(document.PageOrder)) {
    document.PageOrder = [];
    mutated = true;
  }

  if (!document.PageOrder.includes(pageName)) {
    document.PageOrder.push(pageName);
    mutated = true;
  }

  if (mutated) {
    await writeBookDocument(bookUri, document);
  }
}

export async function removePageOrderEntry(bookRoot: vscode.Uri, pageName: string): Promise<void> {
  const bookUri = getBookFileUri(bookRoot);
  const document = await readBookDocument(bookUri);
  let mutated = false;

  if (Array.isArray(document.PageOrder)) {
    const originalLength = document.PageOrder.length;
    document.PageOrder = document.PageOrder.filter((entry) => entry !== pageName);
    mutated = mutated || document.PageOrder.length !== originalLength;
  }

  if (mutated) {
    await writeBookDocument(bookUri, document);
  }
}

async function readBookDocument(bookUri: vscode.Uri): Promise<BookDocument> {
  let document: BookDocument = {};

  try {
    const raw = await vscode.workspace.fs.readFile(bookUri);
    if (raw?.length) {
      const text = textDecoder.decode(raw);
      if (text.trim().length > 0) {
        document = JSON.parse(text) as BookDocument;
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

  return document;
}

async function writeBookDocument(bookUri: vscode.Uri, document: BookDocument): Promise<void> {
  const serialized = textEncoder.encode(JSON.stringify(document, null, 2) + '\n');
  await vscode.workspace.fs.writeFile(bookUri, serialized);
}
