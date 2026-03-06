function getErrorDetails(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

export function getReorderPagesErrorMessage(error: unknown): string {
  return `Neue Page-Reihenfolge konnte nicht gespeichert werden: ${getErrorDetails(error)}`;
}

export function getCreatePageErrorMessage(error: unknown): string {
  return `Page konnte nicht erstellt werden: ${getErrorDetails(error)}`;
}

export function getImportPageErrorMessage(error: unknown): string {
  return `Page konnte nicht importiert werden: ${getErrorDetails(error)}`;
}