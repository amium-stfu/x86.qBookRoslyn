import * as vscode from 'vscode';

export function logDebugLandscape(
  scope: string,
  knownDebugSessions: Map<string, vscode.DebugSession>,
  isManagedDebugSession: (session: vscode.DebugSession) => boolean,
  runtimeChannel: vscode.LogOutputChannel | undefined
): void {
  if (!runtimeChannel) {
    return;
  }

  const active = vscode.debug.activeDebugSession;
  const sessions = Array.from(knownDebugSessions.values());
  runtimeChannel.info(
    `Debug landscape (${scope}): active=${active ? `${active.name}#${active.id} type=${active.type}` : 'none'} totalSessions=${sessions.length}`
  );
  if (sessions.length > 0) {
    runtimeChannel.info(
      `Debug sessions (${scope}): ${sessions
        .map((session) => `${session.name}#${session.id} type=${session.type} managed=${isManagedDebugSession(session)}`)
        .join(' | ')}`
    );
  }

  const breakpoints = vscode.debug.breakpoints;
  const sourceBreakpoints = breakpoints.filter(
    (breakpoint): breakpoint is vscode.SourceBreakpoint => breakpoint instanceof vscode.SourceBreakpoint
  );
  const functionBreakpoints = breakpoints.filter(
    (breakpoint): breakpoint is vscode.FunctionBreakpoint => breakpoint instanceof vscode.FunctionBreakpoint
  );
  const otherBreakpoints = breakpoints.length - sourceBreakpoints.length - functionBreakpoints.length;

  runtimeChannel.info(
    `Breakpoints (${scope}): total=${breakpoints.length} source=${sourceBreakpoints.length} function=${functionBreakpoints.length} other=${otherBreakpoints}`
  );

  const perFile = new Map<string, number>();
  for (const bp of sourceBreakpoints) {
    const key = bp.location.uri.fsPath;
    perFile.set(key, (perFile.get(key) ?? 0) + 1);
  }
  if (perFile.size > 0) {
    const summary = Array.from(perFile.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([file, count]) => `${file}:${count}`)
      .join(' | ');
    runtimeChannel.info(`Breakpoints by file (${scope}): ${summary}`);
  }
}

export function logDebugProviderState(channel: vscode.LogOutputChannel | undefined): void {
  if (!channel) {
    return;
  }

  const csharp = vscode.extensions.getExtension('ms-dotnettools.csharp');
  const devkit = vscode.extensions.getExtension('ms-dotnettools.csdevkit');
  channel.info(
    `Debug provider state: csharp(installed=${Boolean(csharp)}, active=${Boolean(csharp?.isActive)}) csdevkit(installed=${Boolean(devkit)}, active=${Boolean(devkit?.isActive)})`
  );
}

export function logDebugSessionEvent(
  eventType: 'start' | 'terminate',
  session: vscode.DebugSession,
  isManagedDebugSession: (session: vscode.DebugSession) => boolean,
  runtimeChannel: vscode.LogOutputChannel | undefined
): void {
  if (!runtimeChannel) {
    return;
  }

  const configuration = session.configuration as Record<string, unknown>;
  runtimeChannel.info(
    `Debug session ${eventType}: name=${session.name} id=${session.id} type=${session.type} managed=${isManagedDebugSession(session)} config=${JSON.stringify({
      type: configuration?.type ?? null,
      request: configuration?.request ?? null,
      processId: configuration?.processId ?? null,
      processName: configuration?.processName ?? null,
      __amiumQbookDebug: configuration?.__amiumQbookDebug ?? null,
    })}`
  );
}
