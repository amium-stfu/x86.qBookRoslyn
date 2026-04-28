import * as vscode from 'vscode';
import {
  resolveDebugWorkspaceFolder,
  resolveProjectAttachConfiguration,
  startDebuggingFromProjectLaunchConfig,
  tryAdoptExistingQbookDebugSession,
} from './attachService.js';
import { logDebugLandscape, logDebugProviderState } from './logging.js';
import { postDebugState, postStatusText } from '../ui/webviewStatusService.js';

export type StartDebuggingContext = {
  managedDebugSession: vscode.DebugSession | undefined;
  setManagedDebugSession: (session: vscode.DebugSession | undefined) => void;
  knownDebugSessions: Map<string, vscode.DebugSession>;
  isManagedDebugSession: (session: vscode.DebugSession) => boolean;
  lastBookRoot: vscode.Uri | undefined;
  currentView: vscode.WebviewView | undefined;
  nextRunId: () => number;
  getRuntimeLogChannel: () => vscode.LogOutputChannel | undefined;
};

export async function handleStartDebuggingCommand(context: StartDebuggingContext): Promise<void> {
  if (context.managedDebugSession) {
    vscode.window.showInformationMessage('Debugger is already connected.');
    return;
  }

  const runId = context.nextRunId();
  const startedAt = Date.now();
  const runtimeChannel = context.getRuntimeLogChannel();
  runtimeChannel?.info(`==== qBook Attach Run #${runId} START ====`);
  runtimeChannel?.info(
    `Attach run #${runId} context: lastBookRoot=${context.lastBookRoot?.fsPath ?? 'n/a'} workspaceFolders=${(vscode.workspace.workspaceFolders ?? []).map((folder) => folder.uri.fsPath).join(' | ') || 'none'}`
  );
  logDebugProviderState(runtimeChannel);
  logDebugLandscape(
    `run=${runId} before-start`,
    context.knownDebugSessions,
    (candidate) => context.isManagedDebugSession(candidate),
    runtimeChannel
  );

  const adoptedSession = tryAdoptExistingQbookDebugSession(
    context.knownDebugSessions,
    (candidate) => context.isManagedDebugSession(candidate)
  );
  if (adoptedSession) {
    context.setManagedDebugSession(adoptedSession);
    postDebugState(context.currentView, Boolean(adoptedSession));
    postStatusText(context.currentView, 'Debugger already connected');
    runtimeChannel?.info(`Attach run #${runId} reused existing session id=${adoptedSession.id} name=${adoptedSession.name}`);
    runtimeChannel?.info(`==== qBook Attach Run #${runId} ATTACHED-EXISTING ====`);
    return;
  }

  const debugWorkspace = resolveDebugWorkspaceFolder(context.lastBookRoot);
  const launchConfigSelection = await resolveProjectAttachConfiguration(debugWorkspace?.uri);
  runtimeChannel?.info(`Attach run #${runId} workspace=${debugWorkspace?.uri.fsPath ?? 'undefined'} mode=launch-config-only`);
  if (launchConfigSelection) {
    const selectedConfig = launchConfigSelection.configuration as Record<string, unknown>;
    runtimeChannel?.info(
      `Attach run #${runId} launch-config-selected=${launchConfigSelection.configurationName} file=${launchConfigSelection.sourcePath}`
    );
    runtimeChannel?.info(
      `Attach run #${runId} launch-defaults=${JSON.stringify({
        justMyCode: selectedConfig.justMyCode ?? null,
        requireExactSource: selectedConfig.requireExactSource ?? null,
        hasSourceFileMap: Boolean(selectedConfig.sourceFileMap),
        hasSymbolOptions: Boolean(selectedConfig.symbolOptions),
        hasLogging: Boolean(selectedConfig.logging),
      })}`
    );
  } else {
    runtimeChannel?.error(`Attach run #${runId} launch-config-selected=none (searched only .vscode/launch.json)`);
  }

  runtimeChannel?.info('Using only resolved project attach configuration (no dynamic attach payloads).');

  postStatusText(context.currentView, 'Debugger connecting ...');
  const startedFromLaunch = await startDebuggingFromProjectLaunchConfig(
    debugWorkspace,
    launchConfigSelection,
    runId,
    startedAt,
    runtimeChannel
  );
  logDebugLandscape(
    `run=${runId} after-start`,
    context.knownDebugSessions,
    (candidate) => context.isManagedDebugSession(candidate),
    runtimeChannel
  );
  if (!startedFromLaunch) {
    vscode.window.showErrorMessage('Failed to start debugger.');
    postStatusText(context.currentView, 'Debugger start failed');
    runtimeChannel?.error(`==== qBook Attach Run #${runId} FAILED totalElapsed=${Date.now() - startedAt}ms ====`);
  }
}

export async function handleStopDebuggingCommand(
  managedDebugSession: vscode.DebugSession | undefined,
  currentView: vscode.WebviewView | undefined
): Promise<void> {
  if (!managedDebugSession) {
    vscode.window.showInformationMessage('No active qBook debugger.');
    return;
  }

  postStatusText(currentView, 'Debugger disconnecting ...');
  try {
    await vscode.debug.stopDebugging(managedDebugSession);
  } catch (error: unknown) {
    const details = error instanceof Error ? error.message : String(error);
    vscode.window.showErrorMessage(`Failed to stop debugger: ${details}`);
  }
}