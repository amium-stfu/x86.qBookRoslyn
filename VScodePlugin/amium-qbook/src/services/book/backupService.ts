import * as vscode from 'vscode';
import { copyDirectoryRecursive, pathExists } from './fileSystemService.js';

function getParentDirectoryUri(uri: vscode.Uri): vscode.Uri {
  const segments = uri.path.split('/').filter((segment) => segment.length > 0);
  if (segments.length === 0) {
    return uri;
  }
  segments.pop();
  const parentPath = '/' + segments.join('/');
  return uri.with({ path: parentPath || '/' });
}

function getDirectoryName(uri: vscode.Uri): string {
  const segments = uri.path.split('/').filter((segment) => segment.length > 0);
  return segments.length > 0 ? segments[segments.length - 1] : '';
}

function formatTimestamp(now: Date): string {
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  const hours = String(now.getHours()).padStart(2, '0');
  const minutes = String(now.getMinutes()).padStart(2, '0');
  const seconds = String(now.getSeconds()).padStart(2, '0');
  return `${year}-${month}-${day}_${hours}${minutes}${seconds}`;
}

function resolveBackupDirectoryName(sourceDirectoryName: string): string {
  if (/\.code$/i.test(sourceDirectoryName)) {
    return sourceDirectoryName.replace(/\.code$/i, '.backup');
  }
  return `${sourceDirectoryName}.backup`;
}

async function resolveUniqueSnapshotDirectory(backupRoot: vscode.Uri, baseName: string): Promise<vscode.Uri> {
  let candidate = vscode.Uri.joinPath(backupRoot, baseName);
  let counter = 1;

  while (await pathExists(candidate)) {
    candidate = vscode.Uri.joinPath(backupRoot, `${baseName}_${counter}`);
    counter += 1;
  }

  return candidate;
}

export async function createTimestampedBackup(bookRoot: vscode.Uri | undefined): Promise<vscode.Uri> {
  if (!bookRoot) {
    throw new Error('Book-Verzeichnis konnte nicht ermittelt werden.');
  }

  const sourceStat = await vscode.workspace.fs.stat(bookRoot);
  if (sourceStat.type !== vscode.FileType.Directory) {
    throw new Error('Book-Verzeichnis ist kein Ordner.');
  }

  const sourceDirectoryName = getDirectoryName(bookRoot);
  if (!sourceDirectoryName) {
    throw new Error('Book-Ordnername fehlt.');
  }

  const parentDirectory = getParentDirectoryUri(bookRoot);
  const backupDirectoryName = resolveBackupDirectoryName(sourceDirectoryName);
  const backupRoot = vscode.Uri.joinPath(parentDirectory, backupDirectoryName);
  await vscode.workspace.fs.createDirectory(backupRoot);

  const timestamp = formatTimestamp(new Date());
  const snapshotName = `${timestamp}_${sourceDirectoryName}`;
  const snapshotDirectory = await resolveUniqueSnapshotDirectory(backupRoot, snapshotName);
  await copyDirectoryRecursive(bookRoot, snapshotDirectory);

  return snapshotDirectory;
}
