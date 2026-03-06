import * as vscode from 'vscode';
import { TextDecoder, TextEncoder } from 'util';
import { cloneJsonValue, extractAttachOptionDefaults, parseJsonLoose } from './helpers.js';

const textDecoder = new TextDecoder('utf-8');
const textEncoder = new TextEncoder();

export type LaunchConfigSource = {
  path: string;
  configurations: Array<Record<string, unknown>>;
};

export async function readLaunchConfigurationSources(rootUri: vscode.Uri | undefined): Promise<LaunchConfigSource[]> {
  if (!rootUri) {
    return [];
  }

  const candidates = [vscode.Uri.joinPath(rootUri, '.vscode', 'launch.json')];
  const result: LaunchConfigSource[] = [];

  for (const candidate of candidates) {
    try {
      const raw = await vscode.workspace.fs.readFile(candidate);
      const text = textDecoder.decode(raw);
      if (!text.trim()) {
        continue;
      }

      const parsed = parseJsonLoose(text) as { configurations?: Array<Record<string, unknown>> };
      const configurations = Array.isArray(parsed?.configurations) ? parsed.configurations : [];
      if (configurations.length > 0) {
        result.push({ path: candidate.fsPath, configurations });
      }
    } catch {
      // optional file
    }
  }

  return result;
}

export async function writeAttachLaunchConfig(rootUri: vscode.Uri, config: vscode.DebugConfiguration): Promise<void> {
  const launchUri = vscode.Uri.joinPath(rootUri, '.vscode', 'launch.json');
  const launchDir = vscode.Uri.joinPath(rootUri, '.vscode');
  await vscode.workspace.fs.createDirectory(launchDir);

  let document: { version?: string; configurations?: Array<Record<string, unknown>> } = {
    version: '0.2.0',
    configurations: [],
  };

  try {
    const raw = await vscode.workspace.fs.readFile(launchUri);
    const text = textDecoder.decode(raw);
    if (text.trim()) {
      document = JSON.parse(text) as { version?: string; configurations?: Array<Record<string, unknown>> };
    }
  } catch {
    // file optional
  }

  const current = Array.isArray(document.configurations) ? document.configurations : [];
  const existingAttach = current.find((entry) => entry?.name === 'qBook Attach') ?? {};
  const filtered = current.filter((entry) => entry?.name !== 'qBook Attach');

  const processIdRaw = typeof config.processId === 'string' ? Number.parseInt(config.processId, 10) : config.processId;
  const processId = typeof processIdRaw === 'number' && Number.isFinite(processIdRaw) ? Math.trunc(processIdRaw) : undefined;
  const processName = typeof config.processName === 'string' && config.processName.trim() ? config.processName.trim() : undefined;

  const attachEntry: Record<string, unknown> = {
    ...(cloneJsonValue(existingAttach) as Record<string, unknown>),
    name: 'qBook Attach',
    type: config.type,
    request: 'attach',
    justMyCode: false,
    requireExactSource: false,
    logging: {
      moduleLoad: true,
      exceptions: true,
      programOutput: true,
    },
  };

  if (typeof processId === 'number') {
    attachEntry.processId = processId;
  }
  if (processName) {
    attachEntry.processName = processName;
  }

  filtered.push(attachEntry);

  document.version = '0.2.0';
  document.configurations = filtered;
  const serialized = JSON.stringify(document, null, 2) + '\n';
  await vscode.workspace.fs.writeFile(launchUri, textEncoder.encode(serialized));
}

export async function readLaunchAttachDefaults(rootUri?: vscode.Uri): Promise<Record<string, unknown>> {
  if (!rootUri) {
    return {};
  }

  const launchUri = vscode.Uri.joinPath(rootUri, '.vscode', 'launch.json');
  try {
    const raw = await vscode.workspace.fs.readFile(launchUri);
    const text = textDecoder.decode(raw);
    if (!text.trim()) {
      return {};
    }

    const document = JSON.parse(text) as { configurations?: Array<Record<string, unknown>> };
    const configurations = Array.isArray(document.configurations) ? document.configurations : [];
    const qbookAttach = configurations.find((entry) => entry?.name === 'qBook Attach');
    if (!qbookAttach) {
      return {};
    }

    return extractAttachOptionDefaults(qbookAttach);
  } catch {
    return {};
  }
}
