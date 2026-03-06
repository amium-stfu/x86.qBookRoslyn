import * as vscode from 'vscode';
import { TextEncoder } from 'util';
import { getPageDirectory } from './pageMetadataFileService.js';
import { copyDirectoryRecursive, pathExists } from './fileSystemService.js';
import { generateOPageTemplate, generateQPageTemplate } from './templates.js';

const textEncoder = new TextEncoder();

export async function renamePageFolder(
  bookRoot: vscode.Uri | undefined,
  oldPage: string,
  newPage: string
): Promise<void> {
  const oldDir = getPageDirectory(bookRoot, oldPage);
  const newDir = getPageDirectory(bookRoot, newPage);
  const exists = await pathExists(oldDir);
  if (!exists) {
    throw new Error(`Page-Ordner '${oldPage}' wurde nicht gefunden.`);
  }
  await vscode.workspace.fs.rename(oldDir, newDir, { overwrite: false });
}

export async function renamePageFiles(
  bookRoot: vscode.Uri | undefined,
  oldPage: string,
  newPage: string
): Promise<void> {
  const pageDir = getPageDirectory(bookRoot, newPage);
  const entries = await vscode.workspace.fs.readDirectory(pageDir);
  for (const [name, type] of entries) {
    if (type !== vscode.FileType.File) {
      continue;
    }
    if (!name.toLowerCase().endsWith('.cs')) {
      continue;
    }
    if (!name.startsWith(`${oldPage}.`)) {
      continue;
    }
    const suffix = name.substring(oldPage.length);
    const targetName = `${newPage}${suffix}`;
    const sourceUri = vscode.Uri.joinPath(pageDir, name);
    const targetUri = vscode.Uri.joinPath(pageDir, targetName);
    await vscode.workspace.fs.rename(sourceUri, targetUri, { overwrite: false });
  }
}

export async function createPageScaffold(bookRoot: vscode.Uri | undefined, pageName: string): Promise<void> {
  const pageDir = getPageDirectory(bookRoot, pageName);
  await vscode.workspace.fs.createDirectory(pageDir);
  const codeUri = vscode.Uri.joinPath(pageDir, `${pageName}.qPage.cs`);
  const metaUri = vscode.Uri.joinPath(pageDir, 'oPage.json');
  await vscode.workspace.fs.writeFile(codeUri, textEncoder.encode(generateQPageTemplate(pageName)));
  await vscode.workspace.fs.writeFile(metaUri, textEncoder.encode(generateOPageTemplate(pageName)));
}

export async function importPageDirectory(sourceDir: vscode.Uri, targetDir: vscode.Uri): Promise<void> {
  await copyDirectoryRecursive(sourceDir, targetDir);
}