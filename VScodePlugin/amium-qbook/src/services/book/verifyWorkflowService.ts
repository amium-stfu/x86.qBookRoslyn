import * as vscode from 'vscode';
import { waitForNextDiagnosticsUpdate } from '../diagnostics/csharpDiagnostics.js';
import { BookTreePayload } from '../../types/extensionTypes.js';
import {
  closeVerificationOnlyTabs,
  collectOpenTextTabUris,
  isTextTabVisible,
  normalizeRelativePath,
  resolveRelativeFileUri,
  uriKey,
} from './verificationService.js';

type OpenRelativeFileOptions = {
  preview?: boolean;
  preserveFocus?: boolean;
};

export type VerifyTreeCsFilesContext = {
  baseUri: vscode.Uri | undefined;
  postStatusText: (text: string) => void;
  openRelativeFile: (relativePath: string, options?: OpenRelativeFileOptions) => Promise<void>;
  refreshDiagnostics: () => void;
  getErrorPaths: () => Iterable<string>;
};

export async function verifyAllTreeCsFiles(
  payload: BookTreePayload | undefined,
  context: VerifyTreeCsFilesContext
): Promise<void> {
  if (!payload) {
    return;
  }

  const initiallyVisible = collectOpenTextTabUris();
  const visited = new Map<string, { relativePath: string; relativeKey: string }>();

  const csFiles = payload.nodes
    .flatMap((node) => node.files)
    .filter((file) => typeof file.relativePath === 'string' && file.relativePath.toLowerCase().endsWith('.cs'));

  for (const file of csFiles) {
    const targetUri = resolveRelativeFileUri(file.relativePath, context.baseUri);
    if (!targetUri) {
      continue;
    }

    const uriKeyValue = uriKey(targetUri);
    const normalizedPath = normalizeRelativePath(file.relativePath);
    const relativeKey = normalizedPath.toLowerCase();
    visited.set(uriKeyValue, { relativePath: normalizedPath, relativeKey });

    const label = file.displayName?.trim() || file.name?.trim() || file.relativePath;
    context.postStatusText(`Verifying code in ${label}`);
    await context.openRelativeFile(file.relativePath, { preview: false, preserveFocus: true });
    await waitForNextDiagnosticsUpdate(350);
    await sleep(120);
  }

  context.postStatusText('PrÃ¼fe C# Diagnostics ...');
  await waitForNextDiagnosticsUpdate(800);
  await sleep(120);
  context.refreshDiagnostics();

  const errorRelativeSet = new Set(Array.from(context.getErrorPaths(), (entry) => entry.toLowerCase()));

  for (const [uriKeyValue, info] of visited) {
    if (errorRelativeSet.has(info.relativeKey) && !isTextTabVisible(uriKeyValue)) {
      await context.openRelativeFile(info.relativePath, { preview: false, preserveFocus: true });
    }
  }

  const closable = Array.from(visited.entries())
    .filter(([uriKeyValue, info]) => !initiallyVisible.has(uriKeyValue) && !errorRelativeSet.has(info.relativeKey))
    .map(([uriKeyValue]) => uriKeyValue);

  await closeVerificationOnlyTabs(closable);
}

async function sleep(ms: number): Promise<void> {
  await new Promise<void>((resolve) => setTimeout(resolve, ms));
}