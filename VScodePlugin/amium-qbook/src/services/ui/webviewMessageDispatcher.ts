import * as vscode from 'vscode';
import {
  PipeCommandSenderContext,
  RuntimeButtonId,
  RuntimeSignalKind,
  sendRuntimeCommand,
} from '../../pipes/pipeCommands.js';
import { BridgeMessage, BookTreePayload, PageMetadata, RebuildAutomation } from '../../types/extensionTypes.js';
import { ensureNoCSharpErrorsBeforeRebuild } from '../diagnostics/csharpDiagnostics.js';
import { createTimestampedBackup } from '../book/backupService.js';
import {
  handleCreatePageCommand,
  handleDeletePageCommand,
  handleImportPageCommand,
  handleRenamePageCommand,
  handleReorderPagesCommand,
} from '../book/pageCommandService.js';
import {
  handleToggleHiddenCommand,
  handleUpdateFormatCommand,
  handleUpdateTitleCommand,
} from '../book/pageMetadataCommandService.js';
import {
  handleAddSubcodeCommand,
  handleDeleteSubnodeCommand,
  handleRenameSubnodeCommand,
} from '../book/subnodeCommandService.js';
import { handleStartDebuggingCommand, handleStopDebuggingCommand } from '../debug/debugCommandService.js';
import { postStatusText } from './webviewStatusService.js';

export type WebviewMessageDispatchContext = {
  message: BridgeMessage;
  syncAutomationOptions: (message?: BridgeMessage) => void;
  beginRebuildAutomation: () => RebuildAutomation | undefined;
  isAutomationActive: (operation: RebuildAutomation) => boolean;
  sendPreparedRebuild: (operation: RebuildAutomation) => Promise<void>;
  failRebuildAutomation: (operation: RebuildAutomation, detail: string) => void;
  pipeCommandSenderContext: PipeCommandSenderContext;
  loadAndSendTreeData: () => Promise<void>;
  openRelativeFile: (relativePath?: string) => Promise<void>;
  verifyAllTreeCsFiles: () => Promise<void>;
  applyRuntimeHighlight: (button: RuntimeButtonId, kind: RuntimeSignalKind) => void;
  notifyRuntimeRequested: (button: RuntimeButtonId) => boolean;
  notifyRuntimeFailed: (button: RuntimeButtonId, detail: string) => void;
  currentView: vscode.WebviewView | undefined;
  managedDebugSession: vscode.DebugSession | undefined;
  setManagedDebugSession: (session: vscode.DebugSession | undefined) => void;
  knownDebugSessions: Map<string, vscode.DebugSession>;
  isManagedDebugSession: (session: vscode.DebugSession) => boolean;
  lastBookRoot: vscode.Uri | undefined;
  nextDebugRunId: () => number;
  getRuntimeLogChannel: () => vscode.LogOutputChannel | undefined;
  applyMetadataPatch: (pageName: string, patch: Partial<PageMetadata>) => void;
  isRenamingPage: boolean;
  setRenamingPage: (value: boolean) => void;
  reloadTreeView: () => Promise<void>;
  lastPayload: BookTreePayload | undefined;
  selectedPath: string | undefined;
  errorPaths: Set<string>;
  reorderPayloadNodes: (nextOrder: string[]) => void;
};

export async function dispatchWebviewMessage(context: WebviewMessageDispatchContext): Promise<void> {
  const message = context.message;

  switch (message.type) {
    case 'requestAutomationState':
      context.syncAutomationOptions();
      break;
    case 'updateAutomationOptions':
      context.syncAutomationOptions(message);
      break;
    case 'requestTree':
      await context.loadAndSendTreeData();
      break;
    case 'openFile':
      await context.openRelativeFile(message.relativePath);
      break;
    case 'initialize':
    case 'run':
    case 'stop': {
      const button = message.type;
      if (!context.notifyRuntimeRequested(button)) break;
      const command = button.charAt(0).toUpperCase() + button.slice(1);
      if (!await sendRuntimeCommand(context.pipeCommandSenderContext, command)) {
        context.notifyRuntimeFailed(button, 'Runtime command could not be sent; host completion is unknown.');
      }
      break;
    }
    case 'checkCode': {
      await context.verifyAllTreeCsFiles();
      const hasNoCSharpErrors = await ensureNoCSharpErrorsBeforeRebuild();
      postStatusText(context.currentView, hasNoCSharpErrors
        ? 'Check Code completed: no C# errors found'
        : 'Check Code completed: C# errors found');
      break;
    }
    case 'fastRebuild': {
      const operation = context.beginRebuildAutomation();
      if (!operation) break;
      const saveStarted = Date.now();
      context.getRuntimeLogChannel()?.info('Rebuild save BEGIN');
      try {
        if (!await vscode.workspace.saveAll(false)) {
          throw new Error('Not all files could be saved.');
        }
      } catch (error: unknown) {
        const details = error instanceof Error ? error.message : String(error);
        postStatusText(context.currentView, `Rebuild canceled: Save failed (${details})`);
        context.failRebuildAutomation(operation, 'Save failed: ' + details);
        vscode.window.showErrorMessage(`Save failed: ${details}`);
        break;
      }
      context.getRuntimeLogChannel()?.info(`Rebuild save END duration=${Date.now() - saveStarted}ms`);
      if (!context.isAutomationActive(operation)) break;
      if (operation.autoBackup) {
        try {
          postStatusText(context.currentView, 'Rebuild backup running');
          await createTimestampedBackup(context.lastBookRoot);
        } catch (error: unknown) {
          const details = error instanceof Error ? error.message : String(error);
          context.failRebuildAutomation(operation, 'Backup failed: ' + details);
          vscode.window.showErrorMessage('Backup failed: ' + details);
          break;
        }
      }
      if (context.isAutomationActive(operation)) {
        await context.sendPreparedRebuild(operation);
      }
      break;
    }
    case 'backup':
      try {
        postStatusText(context.currentView, 'Backup running');
        const snapshotUri = await createTimestampedBackup(context.lastBookRoot);
        const snapshotName = snapshotUri.path.split('/').filter(Boolean).pop() ?? snapshotUri.fsPath;
        postStatusText(context.currentView, `Backup created: ${snapshotName}`);
      } catch (error: unknown) {
        const details = error instanceof Error ? error.message : String(error);
        postStatusText(context.currentView, `Backup failed: ${details}`);
        vscode.window.showErrorMessage(`Backup failed: ${details}`);
      }
      break;
    case 'debugStart':
      await handleStartDebuggingCommand({
        managedDebugSession: context.managedDebugSession,
        setManagedDebugSession: context.setManagedDebugSession,
        knownDebugSessions: context.knownDebugSessions,
        isManagedDebugSession: context.isManagedDebugSession,
        lastBookRoot: context.lastBookRoot,
        currentView: context.currentView,
        nextRunId: context.nextDebugRunId,
        getRuntimeLogChannel: context.getRuntimeLogChannel,
      });
      break;
    case 'debugStop':
      await handleStopDebuggingCommand(context.managedDebugSession, context.currentView);
      break;
    case 'toggleHidden':
      await handleToggleHiddenCommand(message, context.pipeCommandSenderContext, context.lastBookRoot, context.applyMetadataPatch);
      break;
    case 'updateTitle':
      await handleUpdateTitleCommand(message, context.pipeCommandSenderContext, context.lastBookRoot, context.applyMetadataPatch);
      break;
    case 'updateFormat':
      await handleUpdateFormatCommand(message, context.pipeCommandSenderContext, context.lastBookRoot, context.applyMetadataPatch);
      break;
    case 'renamePage':
      await handleRenamePageCommand({
        message,
        bookRoot: context.lastBookRoot,
        isRenamingPage: context.isRenamingPage,
        setRenamingPage: context.setRenamingPage,
        reloadTreeView: context.reloadTreeView,
      });
      break;
    case 'reorderPages':
      await handleReorderPagesCommand({
        message,
        bookRoot: context.lastBookRoot,
        payload: context.lastPayload,
        view: context.currentView,
        selectedPath: context.selectedPath,
        errorPaths: context.errorPaths,
        sendRuntimePageOrder: async (nextOrder: string[]) => {
          await sendRuntimeCommand(context.pipeCommandSenderContext, 'PageOrder', nextOrder);
        },
        applyPayloadOrder: context.reorderPayloadNodes,
      });
      break;
    case 'renameSubnode':
      await handleRenameSubnodeCommand(message, context.lastBookRoot, context.reloadTreeView);
      break;
    case 'deleteSubnode':
      await handleDeleteSubnodeCommand(message, context.lastBookRoot, context.reloadTreeView);
      break;
    case 'createPage':
      await handleCreatePageCommand(context.lastBookRoot, context.reloadTreeView);
      break;
    case 'importPage':
      await handleImportPageCommand(context.lastBookRoot, context.reloadTreeView);
      break;
    case 'save':
      vscode.window.showInformationMessage(`Save clicked | Page=${message.page ?? ''}`);
      console.log('[Webview->Extension] save', message);
      break;
    case 'addSubcode':
      await handleAddSubcodeCommand(message, context.lastBookRoot, context.reloadTreeView);
      break;
    case 'deletePage':
      await handleDeletePageCommand(message, context.lastBookRoot, context.reloadTreeView);
      break;
    default:
      console.log('[Webview->Extension] unknown message', message);
      break;
  }
}
