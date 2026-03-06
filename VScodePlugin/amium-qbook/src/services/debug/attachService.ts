import * as vscode from 'vscode';
import { cloneJsonValue, isDotnetAttachSession, pickAttachConfiguration } from './helpers.js';
import { readLaunchConfigurationSources } from './launchConfigService.js';

export type LaunchConfigSelection = {
  configuration: vscode.DebugConfiguration;
  configurationName: string;
  sourcePath: string;
};

export function tryAdoptExistingQbookDebugSession(
  knownDebugSessions: Map<string, vscode.DebugSession>,
  isManagedDebugSession: (session: vscode.DebugSession) => boolean
): vscode.DebugSession | undefined {
  const active = vscode.debug.activeDebugSession;

  if (active && isDotnetAttachSession(active)) {
    return active;
  }

  for (const session of knownDebugSessions.values()) {
    if (isManagedDebugSession(session)) {
      return session;
    }
  }

  return undefined;
}

export function resolveDebugWorkspaceFolder(lastBookRoot?: vscode.Uri): vscode.WorkspaceFolder | undefined {
  if (lastBookRoot) {
    const fromBookRoot = vscode.workspace.getWorkspaceFolder(lastBookRoot);
    if (fromBookRoot) {
      return fromBookRoot;
    }
  }

  return vscode.workspace.workspaceFolders?.[0];
}

export async function resolveProjectAttachConfiguration(
  rootUri: vscode.Uri | undefined
): Promise<LaunchConfigSelection | undefined> {
  const launchSources = await readLaunchConfigurationSources(rootUri);
  const preferredNames = ['qbook: Attach to host process', 'qbook: Attach (pick process)', 'qBook Attach'];

  for (const source of launchSources) {
    const selected = pickAttachConfiguration(source.configurations, preferredNames);
    if (!selected) {
      continue;
    }

    const normalized = {
      ...(cloneJsonValue(selected) as Record<string, unknown>),
      name: (selected.name as string) ?? 'qBook Attach',
      request: 'attach',
      type: (selected.type as string) ?? 'coreclr',
      __amiumQbookDebug: true,
    } as vscode.DebugConfiguration;

    if (!Object.prototype.hasOwnProperty.call(normalized, 'justMyCode')) {
      normalized.justMyCode = false;
    }
    if (!Object.prototype.hasOwnProperty.call(normalized, 'requireExactSource')) {
      normalized.requireExactSource = false;
    }
    if (!Object.prototype.hasOwnProperty.call(normalized, 'logging')) {
      normalized.logging = {
        moduleLoad: true,
        exceptions: true,
        programOutput: true,
      };
    }

    return {
      configuration: normalized,
      configurationName: String(normalized.name ?? 'qBook Attach'),
      sourcePath: source.path,
    };
  }

  return undefined;
}

export async function startDebuggingFromProjectLaunchConfig(
  workspaceFolder: vscode.WorkspaceFolder | undefined,
  launchSelection: LaunchConfigSelection | undefined,
  runId: number,
  startedAt: number,
  runtimeChannel: vscode.LogOutputChannel | undefined
): Promise<boolean> {
  if (!workspaceFolder) {
    runtimeChannel?.error(`Launch-config attach aborted (run=${runId}): no workspace folder resolved`);
    return false;
  }
  if (!launchSelection) {
    runtimeChannel?.error(`Launch-config attach aborted (run=${runId}): no attach configuration resolved`);
    return false;
  }

  const launchConfiguration = launchSelection.configuration as Record<string, unknown>;
  const hasProcessId =
    typeof launchConfiguration.processId === 'number' ||
    (typeof launchConfiguration.processId === 'string' && launchConfiguration.processId.trim().length > 0);
  const hasProcessName =
    typeof launchConfiguration.processName === 'string' && launchConfiguration.processName.trim().length > 0;
  runtimeChannel?.info(
    `Resolved launch config payload (run=${runId}): ${JSON.stringify({
      name: launchSelection.configurationName,
      file: launchSelection.sourcePath,
      type: launchConfiguration.type ?? null,
      request: launchConfiguration.request ?? null,
      processId: launchConfiguration.processId ?? null,
      processName: launchConfiguration.processName ?? null,
    })}`
  );
  if (!hasProcessId && !hasProcessName) {
    runtimeChannel?.error(
      `Launch-config attach aborted (run=${runId}): configuration '${launchSelection.configurationName}' has neither processId nor processName`
    );
    return false;
  }

  runtimeChannel?.info(
    `Starting resolved launch configuration '${launchSelection.configurationName}' (run=${runId}) from ${workspaceFolder.uri.fsPath}`
  );

  try {
    const started = await vscode.debug.startDebugging(workspaceFolder, launchSelection.configurationName);
    runtimeChannel?.info(`Project launch startDebugging returned ${started} (run=${runId})`);
    if (started) {
      runtimeChannel?.info(
        `==== qBook Attach Run #${runId} ATTACHED totalElapsed=${Date.now() - startedAt}ms sessionId=${vscode.debug.activeDebugSession?.id ?? 'n/a'} ====`
      );
    }
    return started;
  } catch (error: unknown) {
    const details = error instanceof Error ? error.message : String(error);
    runtimeChannel?.error(`Project launch attach failed (run=${runId}): ${details}`);
    return false;
  }
}
