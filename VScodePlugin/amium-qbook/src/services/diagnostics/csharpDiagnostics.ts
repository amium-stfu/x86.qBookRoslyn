import * as vscode from 'vscode';

export type CSharpErrorEntry = {
  uri: vscode.Uri;
  errors: vscode.Diagnostic[];
};

export function collectWorkspaceCSharpErrors(): CSharpErrorEntry[] {
  const entries: CSharpErrorEntry[] = [];

  const openCSharpDocs = vscode.workspace.textDocuments.filter((doc: vscode.TextDocument) => doc.languageId === 'csharp');
  const openCSharpDocUris = new Set(openCSharpDocs.map((doc: vscode.TextDocument) => doc.uri.toString()));

  const considered = new Map<string, vscode.Uri>();
  const diagnostics = vscode.languages.getDiagnostics();

  for (const [uri] of diagnostics) {
    const uriKey = uri.toString();

    if (uri.scheme === 'file') {
      const filePath = uri.fsPath.toLowerCase();
      if (!filePath.endsWith('.cs')) {
        continue;
      }

      if (!vscode.workspace.getWorkspaceFolder(uri)) {
        continue;
      }

      considered.set(uriKey, uri);
      continue;
    }

    if (openCSharpDocUris.has(uriKey)) {
      considered.set(uriKey, uri);
    }
  }

  for (const doc of openCSharpDocs) {
    considered.set(doc.uri.toString(), doc.uri);
  }

  for (const uri of considered.values()) {
    const diagList = vscode.languages.getDiagnostics(uri);
    const errors = diagList.filter((diag: vscode.Diagnostic) => diag.severity === vscode.DiagnosticSeverity.Error);
    if (errors.length > 0) {
      entries.push({ uri, errors });
    }
  }

  return entries;
}

export async function waitForNextDiagnosticsUpdate(timeoutMs: number): Promise<void> {
  await new Promise<void>((resolve) => {
    let settled = false;

    let timeoutHandle: ReturnType<typeof setTimeout> | undefined;
    const sub = vscode.languages.onDidChangeDiagnostics(() => finish());

    const finish = (): void => {
      if (settled) {
        return;
      }
      settled = true;
      sub.dispose();
      if (timeoutHandle !== undefined) {
        clearTimeout(timeoutHandle);
      }
      resolve();
    };

    timeoutHandle = setTimeout(() => finish(), timeoutMs);
  });
}

export async function ensureNoCSharpErrorsBeforeRebuild(): Promise<boolean> {
  await vscode.workspace.saveAll(false);
  await waitForNextDiagnosticsUpdate(800);

  const entries = collectWorkspaceCSharpErrors();
  return entries.length === 0;
}
