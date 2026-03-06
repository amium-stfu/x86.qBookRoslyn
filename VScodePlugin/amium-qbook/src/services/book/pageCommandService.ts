import * as path from 'node:path';
import * as vscode from 'vscode';
import { BookTreePayload, BridgeMessage } from '../../types/extensionTypes.js';
import { postBookStatus } from '../ui/bookStatusService.js';
import { writeBookPageOrder, ensurePageOrderEntry, renamePageInBookFile } from './bookFileService.js';
import {
  getCreatePageErrorMessage,
  getImportPageErrorMessage,
  getReorderPagesErrorMessage,
} from './bookOperationErrors.js';
import { deletePageFromProject, getDeletePageErrorMessage } from './pageDeleteService.js';
import { createPageScaffold, importPageDirectory, renamePageFiles, renamePageFolder } from './pageFileWorkflowService.js';
import { getPageDirectory, updatePageMetadataFile } from './pageMetadataFileService.js';
import { resolveNextPageOrder } from './pageReorderService.js';
import { updateProgramClass } from './programClassService.js';
import { pathExists, renameNamespaceReferences } from './fileSystemService.js';
import { resolveNodePageIdentifier } from './messageResolverService.js';
import { normalizePageName } from './validation.js';
import { reportMetadataError } from './metadataService.js';

type ReloadTreeView = () => Promise<void>;

export type RenamePageContext = {
  message: BridgeMessage;
  bookRoot: vscode.Uri | undefined;
  isRenamingPage: boolean;
  setRenamingPage: (value: boolean) => void;
  reloadTreeView: ReloadTreeView;
};

export async function handleRenamePageCommand(context: RenamePageContext): Promise<void> {
  if (context.isRenamingPage) {
    vscode.window.showWarningMessage('Bitte warten Sie, bis der laufende Page-Rename abgeschlossen ist.');
    return;
  }

  const oldPage = normalizePageName(resolveNodePageIdentifier(context.message));
  const newPage = normalizePageName(context.message.page);

  if (!oldPage) {
    vscode.window.showWarningMessage('Keine Page ausgewÃ¤hlt.');
    return;
  }

  if (!newPage) {
    vscode.window.showWarningMessage('Neuer Page-Name ist ungÃ¼ltig.');
    return;
  }

  if (oldPage === newPage) {
    vscode.window.showInformationMessage('Page-Name unverÃ¤ndert.');
    return;
  }

  if (!context.bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geÃ¶ffnet.');
    return;
  }

  const newDir = getPageDirectory(context.bookRoot, newPage);
  if (await pathExists(newDir)) {
    vscode.window.showErrorMessage(`Page '${newPage}' existiert bereits.`);
    return;
  }

  context.setRenamingPage(true);
  try {
    await vscode.window.withProgress(
      {
        location: vscode.ProgressLocation.Notification,
        title: `Benenne Page '${oldPage}' in '${newPage}' um...`,
      },
      async () => {
        await renamePageFolder(context.bookRoot, oldPage, newPage);
        await renamePageFiles(context.bookRoot, oldPage, newPage);
        await renamePageInBookFile(context.bookRoot!, oldPage, newPage);
        await updatePageMetadataFile(
          context.bookRoot,
          newPage,
          (doc) => {
            doc.Name = newPage;
            if (typeof context.message.title === 'string' && context.message.title.trim()) {
              doc.Text = context.message.title.trim();
            }
          },
          { oldPage, newPage }
        );
        await renameNamespaceReferences(context.bookRoot!, oldPage, newPage);
      }
    );

    await context.reloadTreeView();
    vscode.window.showInformationMessage(`Page '${oldPage}' wurde in '${newPage}' umbenannt.`);
  } catch (error: unknown) {
    reportMetadataError('Rename', error);
  } finally {
    context.setRenamingPage(false);
  }
}

export async function handleDeletePageCommand(
  message: BridgeMessage,
  bookRoot: vscode.Uri | undefined,
  reloadTreeView: ReloadTreeView
): Promise<void> {
  const targetPage = normalizePageName(resolveNodePageIdentifier(message));
  if (!targetPage) {
    vscode.window.showWarningMessage('Keine Page ausgewÃ¤hlt.');
    return;
  }

  if (!bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  const confirmation = await vscode.window.showWarningMessage(
    `MÃ¶chten Sie die Page '${targetPage}' inklusive aller Dateien lÃ¶schen?`,
    { modal: true },
    'LÃ¶schen'
  );
  if (confirmation !== 'LÃ¶schen') {
    return;
  }

  try {
    await deletePageFromProject(bookRoot, targetPage);
    await updateProgramClass(bookRoot);
    await reloadTreeView();
    vscode.window.showInformationMessage(`Page '${targetPage}' wurde gelÃ¶scht.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getDeletePageErrorMessage(targetPage, error));
  }
}

export type ReorderPagesContext = {
  message: BridgeMessage;
  bookRoot: vscode.Uri | undefined;
  payload: BookTreePayload | undefined;
  view: vscode.WebviewView | undefined;
  selectedPath: string | undefined;
  errorPaths: Set<string>;
  sendRuntimePageOrder: (nextOrder: string[]) => Promise<void>;
  applyPayloadOrder: (nextOrder: string[]) => void;
};

export async function handleReorderPagesCommand(context: ReorderPagesContext): Promise<void> {
  if (!Array.isArray(context.message.order) || context.message.order.length === 0) {
    return;
  }

  if (!context.payload || !context.bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  const knownPages = context.payload.nodes.map((node) => node.page);
  if (!knownPages.length) {
    return;
  }

  const nextOrder = resolveNextPageOrder(knownPages, context.message.order);
  if (!nextOrder) {
    return;
  }

  try {
    await writeBookPageOrder(context.bookRoot, nextOrder);
    await context.sendRuntimePageOrder(nextOrder);
    context.applyPayloadOrder(nextOrder);
    if (context.payload && context.view) {
      context.view.webview.postMessage({ type: 'bookData', payload: context.payload });
      postBookStatus(context.view, context.selectedPath, context.errorPaths, context.payload);
    }
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getReorderPagesErrorMessage(error));
  }
}

export async function handleCreatePageCommand(
  bookRoot: vscode.Uri | undefined,
  reloadTreeView: ReloadTreeView
): Promise<void> {
  if (!bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  const rawName = await vscode.window.showInputBox({
    prompt: 'Enter Page Name',
    placeHolder: 'MyPage',
    ignoreFocusOut: true,
    validateInput: (value: string) => {
      if (!value || !value.trim()) {
        return 'Page-Name ist erforderlich.';
      }
      if (/[\\/]/.test(value)) {
        return 'Der Name darf keine / oder \\ beinhalten.';
      }
      return undefined;
    },
  });

  if (rawName === undefined) {
    return;
  }

  const pageName = normalizePageName(rawName);
  if (!pageName) {
    vscode.window.showWarningMessage('Page-Name ist ungÃ¼ltig.');
    return;
  }

  const pageDir = getPageDirectory(bookRoot, pageName);
  if (await pathExists(pageDir)) {
    vscode.window.showErrorMessage(`Page '${pageName}' existiert bereits.`);
    return;
  }

  try {
    await createPageScaffold(bookRoot, pageName);
    await ensurePageOrderEntry(bookRoot, pageName);
    await updateProgramClass(bookRoot);
    await reloadTreeView();
    vscode.window.showInformationMessage(`Page '${pageName}' wurde erstellt.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getCreatePageErrorMessage(error));
  }
}

export async function handleImportPageCommand(
  bookRoot: vscode.Uri | undefined,
  reloadTreeView: ReloadTreeView
): Promise<void> {
  if (!bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  const selection = await vscode.window.showOpenDialog({
    canSelectFiles: true,
    canSelectFolders: false,
    canSelectMany: false,
    title: 'Select oPage.json for import',
    openLabel: 'Import',
    filters: { JSON: ['json'] },
  });

  if (!selection || selection.length === 0) {
    return;
  }

  const metaFile = selection[0];
  const fileName = path.basename(metaFile.fsPath);
  if (fileName.toLowerCase() !== 'opage.json') {
    vscode.window.showErrorMessage('Bitte wÃ¤hlen Sie eine Datei namens oPage.json aus.');
    return;
  }

  const sourceDirPath = path.dirname(metaFile.fsPath);
  const pageName = normalizePageName(path.basename(sourceDirPath));
  if (!pageName) {
    vscode.window.showErrorMessage('Der Zielordnername ist kein gÃ¼ltiger Page-Name.');
    return;
  }

  const targetDir = getPageDirectory(bookRoot, pageName);
  if (await pathExists(targetDir)) {
    vscode.window.showErrorMessage(`Page '${pageName}' existiert bereits im aktuellen Projekt.`);
    return;
  }

  const sourceDir = vscode.Uri.file(sourceDirPath);

  try {
    await importPageDirectory(sourceDir, targetDir);
    await ensurePageOrderEntry(bookRoot, pageName);
    await updateProgramClass(bookRoot);
    await reloadTreeView();
    vscode.window.showInformationMessage(`Page '${pageName}' wurde importiert.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getImportPageErrorMessage(error));
  }
}