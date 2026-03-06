import { BridgeMessage } from '../../types/extensionTypes.js';

export function resolveNodePageIdentifier(message: BridgeMessage): string | undefined {
  if (typeof message.nodePage === 'string' && message.nodePage.trim()) {
    return message.nodePage.trim();
  }
  if (typeof message.folder === 'string' && message.folder.trim()) {
    return message.folder.trim();
  }
  if (typeof message.page === 'string' && message.page.trim()) {
    return message.page.trim();
  }
  return undefined;
}