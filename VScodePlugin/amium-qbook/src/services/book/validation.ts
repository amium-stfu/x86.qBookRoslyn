export function normalizeFormatValue(value?: string): string | undefined {
  const normalized = value?.trim();
  if (!normalized) {
    return undefined;
  }

  const upper = normalized.toUpperCase();
  if (upper === 'A4' || upper === '16/9' || upper === '16/10') {
    return upper;
  }

  return undefined;
}

export function normalizePageName(value?: string | null): string | undefined {
  const trimmed = value?.trim();
  if (!trimmed || /[\\/]/.test(trimmed)) {
    return undefined;
  }
  return trimmed;
}

export function normalizeCodeName(value?: string | null): string | undefined {
  const trimmed = value?.trim();
  if (!trimmed) {
    return undefined;
  }
  if (!/^[A-Za-z0-9._-]+$/.test(trimmed)) {
    return undefined;
  }
  return trimmed;
}

export function normalizeSubnodeFileName(pageName: string, fileName?: string | null): string | undefined {
  const trimmed = fileName?.trim();
  if (!trimmed || /[\\/]/.test(trimmed)) {
    return undefined;
  }
  const lower = trimmed.toLowerCase();
  if (!lower.endsWith('.cs')) {
    return undefined;
  }
  if (!trimmed.startsWith(`${pageName}.`)) {
    return undefined;
  }
  return trimmed;
}

export function extractSubnodeCodeName(pageName: string, fileName: string): string | undefined {
  const prefix = `${pageName}.`;
  const suffix = '.cs';
  if (!fileName.startsWith(prefix) || !fileName.endsWith(suffix)) {
    return undefined;
  }
  return fileName.slice(prefix.length, fileName.length - suffix.length);
}

export function isProtectedSubnodeFile(pageName: string, fileName: string): boolean {
  const codeName = extractSubnodeCodeName(pageName, fileName);
  return Boolean(codeName && codeName.toLowerCase() === 'qpage');
}
