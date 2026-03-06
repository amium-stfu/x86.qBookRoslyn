import * as vscode from 'vscode';
import { BridgeMessage, PageMetadata } from '../../types/extensionTypes.js';
import { PipeCommandSenderContext, sendRuntimeCommand } from '../../pipes/pipeCommands.js';
import { resolveNodePageIdentifier } from './messageResolverService.js';
import { reportMetadataError } from './metadataService.js';
import { updatePageMetadataFile } from './pageMetadataFileService.js';
import { normalizeFormatValue } from './validation.js';

type ApplyMetadataPatch = (pageName: string, patch: Partial<PageMetadata>) => void;

export async function handleToggleHiddenCommand(
  message: BridgeMessage,
  senderContext: PipeCommandSenderContext,
  bookRoot: vscode.Uri | undefined,
  applyMetadataPatch: ApplyMetadataPatch
): Promise<void> {
  if (typeof message.hidden !== 'boolean') {
    return;
  }

  const targetPage = resolveNodePageIdentifier(message);
  if (!targetPage) {
    vscode.window.showWarningMessage('Keine Page ausgewÃ¤hlt.');
    return;
  }

  try {
    const state = message.hidden ? 'true' : 'false';
    await sendRuntimeCommand(senderContext, 'HidePage', [targetPage, state]);
    await updatePageMetadataFile(bookRoot, targetPage, (doc) => {
      doc.Hidden = message.hidden;
    });
    applyMetadataPatch(targetPage, { hidden: message.hidden });
  } catch (error: unknown) {
    reportMetadataError('Hidden', error);
  }
}

export async function handleUpdateTitleCommand(
  message: BridgeMessage,
  senderContext: PipeCommandSenderContext,
  bookRoot: vscode.Uri | undefined,
  applyMetadataPatch: ApplyMetadataPatch
): Promise<void> {
  const targetPage = resolveNodePageIdentifier(message);
  if (!targetPage) {
    vscode.window.showWarningMessage('Keine Page ausgewÃ¤hlt.');
    return;
  }

  const nextTitle = typeof message.title === 'string' ? message.title : '';
  try {
    await sendRuntimeCommand(senderContext, 'PageText', [targetPage, nextTitle]);
    await updatePageMetadataFile(bookRoot, targetPage, (doc) => {
      doc.Text = nextTitle;
    });
    applyMetadataPatch(targetPage, { title: nextTitle });
  } catch (error: unknown) {
    reportMetadataError('Titel', error);
  }
}

export async function handleUpdateFormatCommand(
  message: BridgeMessage,
  senderContext: PipeCommandSenderContext,
  bookRoot: vscode.Uri | undefined,
  applyMetadataPatch: ApplyMetadataPatch
): Promise<void> {
  const targetPage = resolveNodePageIdentifier(message);
  if (!targetPage) {
    vscode.window.showWarningMessage('Keine Page ausgewÃ¤hlt.');
    return;
  }

  const nextFormat = normalizeFormatValue(message.format);
  if (!nextFormat) {
    vscode.window.showWarningMessage('UngÃ¼ltiges Format.');
    return;
  }

  try {
    await sendRuntimeCommand(senderContext, 'PageFormat', [targetPage, nextFormat]);
    await updatePageMetadataFile(bookRoot, targetPage, (doc) => {
      doc.Format = nextFormat;
    });
    applyMetadataPatch(targetPage, { format: nextFormat });
  } catch (error: unknown) {
    reportMetadataError('Format', error);
  }
}