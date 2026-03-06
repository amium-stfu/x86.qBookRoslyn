import { OPageDocument } from '../../types/extensionTypes.js';

export function rewriteFileReferences(document: OPageDocument, context: { oldPage: string; newPage: string }): void {
  const refFields: Array<keyof OPageDocument> = ['Includes', 'CodeOrder'];
  for (const field of refFields) {
    const value = document[field];
    if (!Array.isArray(value)) {
      continue;
    }
    document[field] = value.map((entry) => {
      if (typeof entry !== 'string') {
        return entry;
      }
      if (!entry.startsWith(`${context.oldPage}.`)) {
        return entry;
      }
      return `${context.newPage}${entry.substring(context.oldPage.length)}`;
    });
  }
}

export function renameSubnodeReferences(
  document: OPageDocument,
  pageName: string,
  oldFileName: string,
  newFileName: string,
  resolveSubnodeCodeName: (pageName: string, fileName: string) => string | undefined
): void {
  const refFields: Array<keyof OPageDocument> = ['Includes', 'CodeOrder'];
  for (const field of refFields) {
    const value = document[field];
    if (!Array.isArray(value)) {
      continue;
    }
    document[field] = value.map((entry) => {
      if (!matchesSubnodeReference(entry, pageName, oldFileName)) {
        return entry;
      }
      const original = typeof entry === 'string' ? entry.trim() : '';
      const hasExtension = Boolean(original && original.toLowerCase().endsWith('.cs'));
      const hasPrefix = Boolean(original && original.startsWith(`${pageName}.`));
      if (!hasExtension) {
        const codeName = resolveSubnodeCodeName(pageName, newFileName) ?? newFileName;
        return hasPrefix ? `${pageName}.${codeName}` : codeName;
      }
      return newFileName;
    });
  }
}

export function removeSubnodeReferences(document: OPageDocument, pageName: string, fileName: string): void {
  const refFields: Array<keyof OPageDocument> = ['Includes', 'CodeOrder'];
  for (const field of refFields) {
    const value = document[field];
    if (!Array.isArray(value)) {
      continue;
    }
    document[field] = value.filter((entry) => !matchesSubnodeReference(entry, pageName, fileName));
  }
}

function matchesSubnodeReference(entry: unknown, pageName: string, fileName: string): boolean {
  if (typeof entry !== 'string') {
    return false;
  }
  const entryToken = canonicalizeSubnodeReference(pageName, entry);
  const fileToken = canonicalizeSubnodeReference(pageName, fileName);
  if (!entryToken || !fileToken) {
    return false;
  }
  return entryToken === fileToken;
}

function canonicalizeSubnodeReference(pageName: string, entry: string): string | undefined {
  const trimmed = entry?.trim();
  if (!trimmed) {
    return undefined;
  }
  let payload = trimmed;
  if (payload.toLowerCase().endsWith('.cs')) {
    payload = payload.slice(0, -3);
  }
  const prefix = `${pageName}.`;
  if (payload.length > prefix.length && payload.slice(0, prefix.length).toLowerCase() === prefix.toLowerCase()) {
    payload = payload.slice(prefix.length);
  }
  if (!payload) {
    return undefined;
  }
  return `${pageName}.${payload}.cs`.toLowerCase();
}
