function getErrorDetails(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

export function getReorderPagesErrorMessage(error: unknown): string {
  return `New page order could not be saved: ${getErrorDetails(error)}`;
}

export function getCreatePageErrorMessage(error: unknown): string {
  return `Page could not be created: ${getErrorDetails(error)}`;
}

export function getImportPageErrorMessage(error: unknown): string {
  return `Page could not be imported: ${getErrorDetails(error)}`;
}