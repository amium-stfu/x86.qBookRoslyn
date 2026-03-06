import * as vscode from 'vscode';
import { BridgeMessage } from '../../types/extensionTypes.js';
import { resolveNodePageIdentifier } from './messageResolverService.js';
import {
  extractSubnodeCodeName,
  isProtectedSubnodeFile,
  normalizeCodeName,
  normalizePageName,
  normalizeSubnodeFileName,
} from './validation.js';
import {
  addSubcodeToPage,
  deleteSubnodeFromPage,
  getCreateSubcodeErrorMessage,
  getDeleteSubnodeErrorMessage,
  getRenameSubnodeErrorMessage,
  renameSubnodeInPage,
} from './subnodeWorkflowService.js';

type ReloadTreeView = () => Promise<void>;

export async function handleAddSubcodeCommand(
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

  const rawName = await vscode.window.showInputBox({
    prompt: `Neuer Subcode-Name fÃ¼r ${targetPage}`,
    placeHolder: 'MySubcode',
    ignoreFocusOut: true,
    validateInput: (value: string) => {
      if (!value || !value.trim()) {
        return 'Name ist erforderlich.';
      }
      if (!normalizeCodeName(value)) {
        return 'Nur Buchstaben, Zahlen, Punkt, Unterstrich und Bindestrich sind erlaubt.';
      }
      if (value.trim().toLowerCase() === 'qpage') {
        return 'Subcode darf nicht qPage heiÃŸen.';
      }
      return undefined;
    },
  });

  if (rawName === undefined) {
    return;
  }

  const codeName = normalizeCodeName(rawName);
  if (!codeName) {
    vscode.window.showWarningMessage('Subcode-Name ist ungÃ¼ltig.');
    return;
  }

  if (codeName.toLowerCase() === 'qpage') {
    vscode.window.showWarningMessage('Subcode darf nicht qPage heiÃŸen.');
    return;
  }

  try {
    const result = await addSubcodeToPage(bookRoot, targetPage, codeName);
    if (result.status === 'exists') {
      vscode.window.showErrorMessage(`Datei '${result.fileName}' existiert bereits.`);
      return;
    }

    await reloadTreeView();
    vscode.window.showInformationMessage(`Subcode '${result.fileName}' wurde erstellt.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getCreateSubcodeErrorMessage(error));
  }
}

export async function handleRenameSubnodeCommand(
  message: BridgeMessage,
  bookRoot: vscode.Uri | undefined,
  reloadTreeView: ReloadTreeView
): Promise<void> {
  const pageName = normalizePageName(message.page);
  if (!pageName) {
    vscode.window.showWarningMessage('Keine Page fÃ¼r die Datei ausgewÃ¤hlt.');
    return;
  }

  const fileName = normalizeSubnodeFileName(pageName, message.fileName);
  if (!fileName) {
    vscode.window.showWarningMessage('UngÃ¼ltiger Dateiname.');
    return;
  }

  const currentCode = extractSubnodeCodeName(pageName, fileName);
  if (!currentCode) {
    vscode.window.showWarningMessage('Codename konnte nicht ermittelt werden.');
    return;
  }

  if (isProtectedSubnodeFile(pageName, fileName)) {
    vscode.window.showWarningMessage('qPage Dateien dÃ¼rfen nicht umbenannt werden.');
    return;
  }

  const nextCodeName = normalizeCodeName(message.codeName);
  if (!nextCodeName) {
    vscode.window.showWarningMessage('UngÃ¼ltiger Codename. Erlaubt sind Buchstaben, Zahlen, Punkt, Unterstrich und Bindestrich.');
    return;
  }

  if (currentCode === nextCodeName) {
    vscode.window.showInformationMessage('Codename unverÃ¤ndert.');
    return;
  }

  if (!bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  try {
    const result = await renameSubnodeInPage(bookRoot, pageName, fileName, nextCodeName);
    if (result.status === 'source-missing') {
      vscode.window.showErrorMessage(`Datei '${fileName}' wurde nicht gefunden.`);
      return;
    }

    if (result.status === 'target-exists') {
      vscode.window.showErrorMessage(`Datei '${result.newFileName}' existiert bereits.`);
      return;
    }

    await reloadTreeView();
    vscode.window.showInformationMessage(`Datei '${result.fileName}' wurde in '${result.newFileName}' umbenannt.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getRenameSubnodeErrorMessage(error));
  }
}

export async function handleDeleteSubnodeCommand(
  message: BridgeMessage,
  bookRoot: vscode.Uri | undefined,
  reloadTreeView: ReloadTreeView
): Promise<void> {
  const pageName = normalizePageName(message.page);
  if (!pageName) {
    vscode.window.showWarningMessage('Keine Page fÃ¼r die Datei ausgewÃ¤hlt.');
    return;
  }

  const fileName = normalizeSubnodeFileName(pageName, message.fileName);
  if (!fileName) {
    vscode.window.showWarningMessage('UngÃ¼ltiger Dateiname.');
    return;
  }

  if (isProtectedSubnodeFile(pageName, fileName)) {
    vscode.window.showWarningMessage('qPage Dateien dÃ¼rfen nicht gelÃ¶scht werden.');
    return;
  }

  if (!bookRoot) {
    vscode.window.showWarningMessage('Kein qBook-Projekt geladen.');
    return;
  }

  try {
    const confirmation = await vscode.window.showWarningMessage(
      `Are you sure you want to delete '${fileName}'?`,
      { modal: true },
      'Delete'
    );
    if (confirmation !== 'Delete') {
      return;
    }

    const result = await deleteSubnodeFromPage(bookRoot, pageName, fileName);
    if (result.status === 'missing') {
      vscode.window.showErrorMessage(`Datei '${fileName}' wurde nicht gefunden.`);
      return;
    }

    await reloadTreeView();
    vscode.window.showInformationMessage(`Datei '${result.fileName}' wurde gelÃ¶scht.`);
  } catch (error: unknown) {
    vscode.window.showErrorMessage(getDeleteSubnodeErrorMessage(error));
  }
}