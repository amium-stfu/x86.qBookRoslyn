import * as vscode from 'vscode';

export function isDotnetAttachSession(session: vscode.DebugSession): boolean {
  const configuration = session.configuration as { type?: unknown; request?: unknown };
  const type = typeof configuration?.type === 'string' ? configuration.type.toLowerCase() : '';
  const request = typeof configuration?.request === 'string' ? configuration.request.toLowerCase() : '';
  return request === 'attach' && (type === 'coreclr' || type === 'clr');
}

export function isManagedQbookDebugSession(session: vscode.DebugSession): boolean {
  const configuration = session.configuration as { __amiumQbookDebug?: unknown };
  if (configuration?.__amiumQbookDebug === true) {
    return true;
  }

  const sessionName = (session.name ?? '').toLowerCase();
  return isDotnetAttachSession(session) && sessionName.includes('qbook');
}

export function pickAttachConfiguration(
  configurations: Array<Record<string, unknown>>,
  preferredNames: string[]
): Record<string, unknown> | undefined {
  const attachConfigs = configurations.filter((entry) => {
    const request = typeof entry.request === 'string' ? entry.request.toLowerCase() : '';
    const type = typeof entry.type === 'string' ? entry.type.toLowerCase() : '';
    return request === 'attach' && (type === 'coreclr' || type === 'clr');
  });

  if (attachConfigs.length === 0) {
    return undefined;
  }

  for (const preferredName of preferredNames) {
    const match = attachConfigs.find(
      (entry) => typeof entry.name === 'string' && entry.name.toLowerCase() === preferredName.toLowerCase()
    );
    if (match) {
      return match;
    }
  }

  return attachConfigs[0];
}

export function parseJsonLoose(text: string): unknown {
  const withoutBlockComments = text.replace(/\/\*[\s\S]*?\*\//g, '');
  const withoutLineComments = withoutBlockComments.replace(/^\s*\/\/.*$/gm, '');
  return JSON.parse(withoutLineComments);
}

export function cloneJsonValue(value: unknown): unknown {
  if (value === null || typeof value !== 'object') {
    return value;
  }

  try {
    return JSON.parse(JSON.stringify(value)) as unknown;
  } catch {
    return value;
  }
}

export function extractAttachOptionDefaults(configuration: Record<string, unknown>): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  const keys: Array<keyof Record<string, unknown>> = [
    'justMyCode',
    'requireExactSource',
    'sourceFileMap',
    'symbolOptions',
    'suppressJITOptimizations',
    'logging',
  ];

  for (const key of keys) {
    if (!Object.prototype.hasOwnProperty.call(configuration, key)) {
      continue;
    }
    const value = configuration[key];
    if (value === undefined) {
      continue;
    }
    result[key] = cloneJsonValue(value);
  }

  return result;
}
