import * as vscode from 'vscode';
import { logDebugSessionEvent } from './logging.js';
import { postDebugState, postStatusText } from '../ui/webviewStatusService.js';

export type DebugSessionLifecycleContext = {
  knownDebugSessions: Map<string, vscode.DebugSession>;
  isManagedDebugSession: (session: vscode.DebugSession) => boolean;
  getManagedDebugSession: () => vscode.DebugSession | undefined;
  setManagedDebugSession: (session: vscode.DebugSession | undefined) => void;
  getCurrentView: () => vscode.WebviewView | undefined;
  getRuntimeLogChannel: () => vscode.LogOutputChannel | undefined;
};

export function registerDebugSessionLifecycle(
  context: DebugSessionLifecycleContext
): vscode.Disposable[] {
  return [
    vscode.debug.onDidStartDebugSession((session: vscode.DebugSession) => {
      context.knownDebugSessions.set(session.id, session);
      logDebugSessionEvent(
        'start',
        session,
        (candidate) => context.isManagedDebugSession(candidate),
        context.getRuntimeLogChannel()
      );
      if (context.isManagedDebugSession(session)) {
        context.setManagedDebugSession(session);
        postDebugState(context.getCurrentView(), true);
          postStatusText(context.getCurrentView(), 'Debugger connected');
      }
    }),
    vscode.debug.onDidTerminateDebugSession((session: vscode.DebugSession) => {
      context.knownDebugSessions.delete(session.id);
      logDebugSessionEvent(
        'terminate',
        session,
        (candidate) => context.isManagedDebugSession(candidate),
        context.getRuntimeLogChannel()
      );
      const managedSession = context.getManagedDebugSession();
      if (managedSession && managedSession.id === session.id) {
        context.setManagedDebugSession(undefined);
        postDebugState(context.getCurrentView(), false);
          postStatusText(context.getCurrentView(), 'Debugger disconnected');
      }
    }),
    vscode.debug.onDidChangeActiveDebugSession((session: vscode.DebugSession | undefined) => {
      const runtimeChannel = context.getRuntimeLogChannel();
      runtimeChannel?.info(
        `Debug active-session changed: ${session ? `${session.name}#${session.id} type=${session.type}` : 'none'}`
      );
    }),
  ];
}