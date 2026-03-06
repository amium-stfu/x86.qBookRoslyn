import * as vscode from 'vscode';
import * as path from 'node:path';

export function uriKey(uri: vscode.Uri): string {
  return path.resolve(uri.fsPath).toLowerCase();
}

export function resolveRelativeFileUri(relativePath: string, baseUri?: vscode.Uri): vscode.Uri | undefined {
  if (!baseUri) {
    return undefined;
  }

  const normalizedRelative = normalizeRelativePath(relativePath);
  const absolutePath = path.resolve(baseUri.fsPath, normalizedRelative);
  const basePath = path.resolve(baseUri.fsPath);

  if (!absolutePath.toLowerCase().startsWith(basePath.toLowerCase())) {
    return undefined;
  }

  return vscode.Uri.file(absolutePath);
}

export function relativePathFromUri(baseUri: vscode.Uri | undefined, targetUri: vscode.Uri | undefined): string | undefined {
  if (!baseUri || !targetUri) {
    return undefined;
  }

  const basePath = path.resolve(baseUri.fsPath);
  const targetPath = path.resolve(targetUri.fsPath);

  if (!targetPath.toLowerCase().startsWith(basePath.toLowerCase())) {
    return undefined;
  }

  return normalizeRelativePath(path.relative(basePath, targetPath));
}

export function collectOpenTextTabUris(): Set<string> {
  const result = new Set<string>();
  for (const group of vscode.window.tabGroups.all) {
    for (const tab of group.tabs) {
      if (tab.input instanceof vscode.TabInputText) {
        result.add(uriKey(tab.input.uri));
      }
    }
  }
  return result;
}

export function isTextTabVisible(targetUriKey: string): boolean {
  for (const group of vscode.window.tabGroups.all) {
    for (const tab of group.tabs) {
      if (tab.input instanceof vscode.TabInputText && uriKey(tab.input.uri) === targetUriKey) {
        return true;
      }
    }
  }
  return false;
}

export async function closeVerificationOnlyTabs(uriKeys: string[]): Promise<void> {
  if (!uriKeys.length) {
    return;
  }

  const uriSet = new Set(uriKeys);
  const tabsToClose: vscode.Tab[] = [];

  for (const group of vscode.window.tabGroups.all) {
    for (const tab of group.tabs) {
      if (!(tab.input instanceof vscode.TabInputText)) {
        continue;
      }

      const tabUriKey = uriKey(tab.input.uri);
      if (!uriSet.has(tabUriKey)) {
        continue;
      }

      tabsToClose.push(tab);
    }
  }

  if (!tabsToClose.length) {
    return;
  }

  try {
    await vscode.window.tabGroups.close(tabsToClose, true);
  } catch {
    // ignore close issues to avoid blocking rebuild
  }
}

export function normalizeRelativePath(value: string): string {
  return value.replace(/\\/g, '/');
}

