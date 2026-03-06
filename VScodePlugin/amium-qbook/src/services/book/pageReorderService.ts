import { normalizePageName } from './validation.js';

export function resolveNextPageOrder(knownPages: string[], requestedOrder: unknown[]): string[] | undefined {
  if (!knownPages.length || !Array.isArray(requestedOrder) || !requestedOrder.length) {
    return undefined;
  }

  const knownSet = new Set(knownPages);
  const normalized: string[] = [];
  for (const entry of requestedOrder) {
    const value = typeof entry === 'string' ? entry : '';
    const page = normalizePageName(value);
    if (!page || !knownSet.has(page) || normalized.includes(page)) {
      continue;
    }
    normalized.push(page);
  }

  if (!normalized.length) {
    return undefined;
  }

  const remaining = knownPages.filter((page) => !normalized.includes(page));
  const nextOrder = [...normalized, ...remaining];
  const hasChange =
    nextOrder.length !== knownPages.length || knownPages.some((page, index) => nextOrder[index] !== page);

  return hasChange ? nextOrder : undefined;
}