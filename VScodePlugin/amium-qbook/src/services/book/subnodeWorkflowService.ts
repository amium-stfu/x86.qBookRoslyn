import * as vscode from 'vscode';
import { TextEncoder } from 'util';
import { generateSubcodeTemplate } from './templates.js';
import { getPageDirectory, updatePageMetadataFile } from './pageMetadataFileService.js';
import { pathExists } from './fileSystemService.js';
import { extractSubnodeCodeName } from './validation.js';
import { removeSubnodeReferences, renameSubnodeReferences } from './references.js';

const textEncoder = new TextEncoder();

export type AddSubcodeResult =
  | { status: 'exists'; fileName: string }
  | { status: 'created'; fileName: string };

export type RenameSubnodeResult =
  | { status: 'source-missing'; fileName: string; newFileName: string }
  | { status: 'target-exists'; fileName: string; newFileName: string }
  | { status: 'renamed'; fileName: string; newFileName: string };

export type DeleteSubnodeResult =
  | { status: 'missing'; fileName: string }
  | { status: 'deleted'; fileName: string };

export async function addSubcodeToPage(
  bookRoot: vscode.Uri | undefined,
  targetPage: string,
  codeName: string
): Promise<AddSubcodeResult> {
  const pageDir = getPageDirectory(bookRoot, targetPage);
  const fileName = `${targetPage}.${codeName}.cs`;
  const fileUri = vscode.Uri.joinPath(pageDir, fileName);

  if (await pathExists(fileUri)) {
    return { status: 'exists', fileName };
  }

  await vscode.workspace.fs.createDirectory(pageDir);
  await vscode.workspace.fs.writeFile(fileUri, textEncoder.encode(generateSubcodeTemplate(targetPage, codeName)));

  await updatePageMetadataFile(bookRoot, targetPage, (doc) => {
    const includes = Array.isArray(doc.Includes)
      ? doc.Includes.filter((entry): entry is string => typeof entry === 'string')
      : [];
    if (!includes.includes(fileName)) {
      includes.push(fileName);
    }
    doc.Includes = includes;

    const codeOrder = Array.isArray(doc.CodeOrder)
      ? doc.CodeOrder.filter((entry): entry is string => typeof entry === 'string')
      : [];
    if (!codeOrder.includes(fileName)) {
      codeOrder.push(fileName);
    }
    doc.CodeOrder = codeOrder;
  });

  return { status: 'created', fileName };
}

export async function renameSubnodeInPage(
  bookRoot: vscode.Uri | undefined,
  pageName: string,
  fileName: string,
  nextCodeName: string
): Promise<RenameSubnodeResult> {
  const newFileName = `${pageName}.${nextCodeName}.cs`;
  const pageDir = getPageDirectory(bookRoot, pageName);
  const sourceUri = vscode.Uri.joinPath(pageDir, fileName);
  const targetUri = vscode.Uri.joinPath(pageDir, newFileName);

  const exists = await pathExists(sourceUri);
  if (!exists) {
    return { status: 'source-missing', fileName, newFileName };
  }

  if (await pathExists(targetUri)) {
    return { status: 'target-exists', fileName, newFileName };
  }

  await vscode.workspace.fs.rename(sourceUri, targetUri, { overwrite: false });
  await updatePageMetadataFile(bookRoot, pageName, (doc) => {
    renameSubnodeReferences(doc, pageName, fileName, newFileName, (page, nextFileName) =>
      extractSubnodeCodeName(page, nextFileName)
    );
  });

  return { status: 'renamed', fileName, newFileName };
}

export async function deleteSubnodeFromPage(
  bookRoot: vscode.Uri | undefined,
  pageName: string,
  fileName: string
): Promise<DeleteSubnodeResult> {
  const pageDir = getPageDirectory(bookRoot, pageName);
  const targetUri = vscode.Uri.joinPath(pageDir, fileName);
  const exists = await pathExists(targetUri);
  if (!exists) {
    return { status: 'missing', fileName };
  }

  await vscode.workspace.fs.delete(targetUri, { recursive: false, useTrash: false });
  await updatePageMetadataFile(bookRoot, pageName, (doc) => {
    removeSubnodeReferences(doc, pageName, fileName);
  });

  return { status: 'deleted', fileName };
}

function getErrorDetails(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

export function getCreateSubcodeErrorMessage(error: unknown): string {
  return `Subcode konnte nicht erstellt werden: ${getErrorDetails(error)}`;
}

export function getRenameSubnodeErrorMessage(error: unknown): string {
  return `Code-Datei konnte nicht umbenannt werden: ${getErrorDetails(error)}`;
}

export function getDeleteSubnodeErrorMessage(error: unknown): string {
  return `Code-Datei konnte nicht gelöscht werden: ${getErrorDetails(error)}`;
}