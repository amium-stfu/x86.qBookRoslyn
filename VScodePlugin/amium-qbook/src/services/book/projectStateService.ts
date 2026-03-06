import * as vscode from 'vscode';
import { BookTreePayload } from '../../types/extensionTypes.js';

export type ClearedProjectViewState = {
  hadState: boolean;
  lastBookRoot: undefined;
  selectedPath: undefined;
  errorPaths: Set<string>;
  lastPayload: undefined;
};

export function createClearedProjectViewState(
  lastBookRoot: vscode.Uri | undefined,
  selectedPath: string | undefined,
  errorPaths: Set<string>,
  lastPayload: BookTreePayload | undefined
): ClearedProjectViewState {
  const hadState = Boolean(lastBookRoot || selectedPath || errorPaths.size > 0 || lastPayload);
  return {
    hadState,
    lastBookRoot: undefined,
    selectedPath: undefined,
    errorPaths: new Set<string>(),
    lastPayload: undefined,
  };
}