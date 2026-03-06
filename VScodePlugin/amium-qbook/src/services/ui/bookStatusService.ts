import * as vscode from 'vscode';
import { BookTreePayload, FormState } from '../../types/extensionTypes.js';

export function getFormStateForSelection(
  selectedPath: string | undefined,
  payload: BookTreePayload | undefined
): FormState | undefined {
  if (!selectedPath || !payload) {
    return undefined;
  }

  for (const node of payload.nodes) {
    for (const file of node.files) {
      if (file.relativePath === selectedPath) {
        return {
          page: node.metadata?.name ?? node.page,
          title: node.metadata?.title ?? node.page,
          format: node.metadata?.format ?? 'A4',
          hidden: node.metadata?.hidden ?? false,
        };
      }
    }
  }

  return undefined;
}

export function postBookStatus(
  view: vscode.WebviewView | undefined,
  selectedPath: string | undefined,
  errorPaths: Set<string>,
  payload: BookTreePayload | undefined
): void {
  if (!view) {
    return;
  }

  const form = getFormStateForSelection(selectedPath, payload);
  view.webview.postMessage({
    type: 'bookStatus',
    payload: {
      selectedPath: selectedPath ?? null,
      errorPaths: Array.from(errorPaths),
      form,
    },
  });
}