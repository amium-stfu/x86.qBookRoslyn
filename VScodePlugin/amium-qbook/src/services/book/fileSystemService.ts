import * as vscode from 'vscode';
import { TextDecoder, TextEncoder } from 'util';

const textDecoder = new TextDecoder('utf-8');
const textEncoder = new TextEncoder();

export async function pathExists(uri: vscode.Uri): Promise<boolean> {
  try {
    await vscode.workspace.fs.stat(uri);
    return true;
  } catch (error: unknown) {
    if (error instanceof vscode.FileSystemError) {
      const fsError = error as vscode.FileSystemError;
      if (fsError.code === 'FileNotFound') {
        return false;
      }
      throw fsError;
    }
    throw error;
  }
}

export async function copyDirectoryRecursive(source: vscode.Uri, target: vscode.Uri): Promise<void> {
  await vscode.workspace.fs.createDirectory(target);
  const entries = await vscode.workspace.fs.readDirectory(source);
  for (const [name, type] of entries) {
    const sourceChild = vscode.Uri.joinPath(source, name);
    const targetChild = vscode.Uri.joinPath(target, name);
    if (type === vscode.FileType.Directory) {
      await copyDirectoryRecursive(sourceChild, targetChild);
    } else if (type === vscode.FileType.File) {
      const data = await vscode.workspace.fs.readFile(sourceChild);
      await vscode.workspace.fs.writeFile(targetChild, data);
    } else if (type === vscode.FileType.SymbolicLink) {
      try {
        const stats = await vscode.workspace.fs.stat(sourceChild);
        if (stats.type === vscode.FileType.Directory) {
          await copyDirectoryRecursive(sourceChild, targetChild);
        } else if (stats.type === vscode.FileType.File) {
          const data = await vscode.workspace.fs.readFile(sourceChild);
          await vscode.workspace.fs.writeFile(targetChild, data);
        }
      } catch {
        // Ignore broken symlinks
      }
    }
  }
}

export async function renameNamespaceReferences(bookRoot: vscode.Uri, oldPage: string, newPage: string): Promise<void> {
  const oldToken = `Definition${oldPage}`;
  const newToken = `Definition${newPage}`;
  const pattern = new vscode.RelativePattern(bookRoot, '**/*.cs');
  const files = await vscode.workspace.findFiles(pattern);

  for (const file of files) {
    const raw = await vscode.workspace.fs.readFile(file);
    const text = textDecoder.decode(raw);
    if (!text.includes(oldToken)) {
      continue;
    }
    const updated = text.split(oldToken).join(newToken);
    if (updated !== text) {
      await vscode.workspace.fs.writeFile(file, textEncoder.encode(updated));
    }
  }
}
