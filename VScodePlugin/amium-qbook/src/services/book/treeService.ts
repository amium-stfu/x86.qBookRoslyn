import * as vscode from 'vscode';
import * as path from 'node:path';
import { TextDecoder } from 'util';
import { BookTreeLeaf, BookTreeNode, PageMetadata } from '../../types/extensionTypes.js';

const textDecoder = new TextDecoder('utf-8');

export async function findBookFileInWorkspace(): Promise<vscode.Uri | undefined> {
  const folders = vscode.workspace.workspaceFolders ?? [];

  for (const folder of folders) {
    const candidate = vscode.Uri.joinPath(folder.uri, 'Book.json');
    try {
      const stat = await vscode.workspace.fs.stat(candidate);
      if (stat.type === vscode.FileType.File) {
        return candidate;
      }
    } catch {
      // ignore
    }
  }

  return undefined;
}

export function getDirectoryUri(fileUri: vscode.Uri): vscode.Uri {
  const segments = fileUri.path.split('/');
  segments.pop();
  const directoryPath = segments.join('/') || '/';
  return fileUri.with({ path: directoryPath });
}

export async function discoverPageFolders(bookRoot: vscode.Uri): Promise<string[]> {
  const pagesDir = vscode.Uri.joinPath(bookRoot, 'Pages');
  const entries = await safeReadDirectory(pagesDir);
  return entries
    .filter(([, type]) => type === vscode.FileType.Directory)
    .map(([name]) => name)
    .sort((a, b) => a.localeCompare(b));
}

export async function readPageFolder(bookRoot: vscode.Uri, pageName: string): Promise<BookTreeNode> {
  const pagesDir = vscode.Uri.joinPath(bookRoot, 'Pages');
  const pageDir = vscode.Uri.joinPath(pagesDir, pageName);
  let codeOrder: string[] = [];
  let metadata: PageMetadata = { name: pageName, title: pageName, format: 'A4', hidden: false };

  try {
    const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
    const metaRaw = await vscode.workspace.fs.readFile(metaUri);
    const metaText = textDecoder.decode(metaRaw);
    const metaData = JSON.parse(metaText) as {
      CodeOrder?: string[];
      Name?: string;
      Text?: string;
      Format?: string;
      Hidden?: boolean;
    };
    if (Array.isArray(metaData.CodeOrder)) {
      codeOrder = metaData.CodeOrder;
    }
    metadata = {
      name: metaData.Name ?? pageName,
      title: metaData.Text ?? metaData.Name ?? pageName,
      format: metaData.Format ?? 'A4',
      hidden: Boolean(metaData.Hidden),
    };
  } catch {
    // Meta file optional; ignore errors.
  }

  const filesInFolder = await safeReadDirectory(pageDir);
  const visibleFiles = filesInFolder
    .filter(([name, type]) => type === vscode.FileType.File && name !== 'oPage.json')
    .map(([name]) => name);

  const orderedFiles = codeOrder.filter((entry): entry is string => typeof entry === 'string' && entry.length > 0);
  for (const fileName of visibleFiles) {
    if (!orderedFiles.includes(fileName)) {
      orderedFiles.push(fileName);
    }
  }

  const primaryIndex = orderedFiles.findIndex((fileName) => isPrimaryCodeFile(fileName));
  if (primaryIndex > 0) {
    const [primaryFile] = orderedFiles.splice(primaryIndex, 1);
    orderedFiles.unshift(primaryFile);
  }

  const files: BookTreeLeaf[] = orderedFiles.map((fileName) => {
    const fileUri = vscode.Uri.joinPath(pageDir, fileName);
    const displayName = getDisplayName(pageName, fileName);
    return {
      name: fileName,
      relativePath: normalizeRelative(path.relative(bookRoot.fsPath, fileUri.fsPath)),
      displayName,
    };
  });

  return { page: pageName, files, metadata };
}

function isPrimaryCodeFile(fileName: string): boolean {
  const lower = fileName.toLowerCase();
  return lower.endsWith('.qpage.cs') || lower.endsWith('.opage.cs');
}

function getDisplayName(pageName: string, fileName: string): string {
  if (!fileName) {
    return '';
  }

  const normalizedPage = pageName.toLowerCase();
  const normalizedFile = fileName.toLowerCase();

  if (normalizedFile.startsWith(normalizedPage.toLowerCase())) {
    const suffix = fileName.substring(pageName.length);
    if (suffix.toLowerCase().startsWith('.qpage') || suffix.toLowerCase().startsWith('.opage')) {
      return 'qPage';
    }
    if (suffix.toLowerCase().startsWith('.procedures')) {
      return 'Procedures';
    }
    if (suffix.toLowerCase().startsWith('.test')) {
      return 'Test';
    }
    if (suffix.toLowerCase().startsWith('.view')) {
      return 'View';
    }
    if (suffix.toLowerCase().startsWith('.customcode')) {
      return 'Custom Code';
    }
    if (suffix.toLowerCase().startsWith('.sequencer')) {
      return 'Sequencer';
    }
    if (suffix.toLowerCase().startsWith('.testdevice')) {
      return 'Test Device';
    }
    if (suffix.toLowerCase().startsWith('.clientudl')) {
      return 'Client UDL';
    }
  }

  if (fileName.toLowerCase().endsWith('.cs')) {
    const withoutExtension = fileName.slice(0, -3);
    if (withoutExtension.startsWith(`${pageName}.`)) {
      return withoutExtension.slice(pageName.length + 1);
    }
    return withoutExtension;
  }

  if (fileName.startsWith(`${pageName}.`)) {
    return fileName.slice(pageName.length + 1);
  }

  return fileName;
}

function normalizeRelative(value: string): string {
  return value.replace(/\\/g, '/');
}

async function safeReadDirectory(uri: vscode.Uri): Promise<[string, vscode.FileType][]> {
  try {
    return await vscode.workspace.fs.readDirectory(uri);
  } catch {
    return [];
  }
}