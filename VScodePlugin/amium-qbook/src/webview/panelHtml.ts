import * as vscode from 'vscode';

export function getWebviewHtml(webview: vscode.Webview, logoUri: vscode.Uri): string {
  const nonce = createNonce();
  const cspSource = webview.cspSource;
  const logoSrc = logoUri.toString();

  return `<!DOCTYPE html>
<html lang="de">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta http-equiv="Content-Security-Policy" content="default-src 'none'; img-src ${cspSource} https: data:; style-src 'unsafe-inline'; script-src 'nonce-${nonce}';" />
  <title>qBook Calibration</title>
  <style>
    body {
      font-family: var(--vscode-font-family);
      color: var(--vscode-foreground);
      background: var(--vscode-editor-background);
      margin: 0;
      padding: 12px;
    }

    .brand {
      display: flex;
      justify-content: center;
      margin-bottom: 8px;
    }

    .brand img {
      max-width: 140px;
      height: auto;
      opacity: 0.95;
      image-rendering: -webkit-optimize-contrast;
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      margin-bottom: 12px;
    }

    .pipe-status {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 10px;
      border-radius: 4px;
      font-size: 12px;
      margin-bottom: 12px;
      border: 1px solid transparent;
    }

    .pipe-status .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
      display: inline-block;
    }

    .pipe-status--connected {
      background: rgba(96, 175, 91, 0.16);
      border-color: rgba(96, 175, 91, 0.6);
      color: var(--vscode-testing-iconPassed, #60af5b);
    }

    .pipe-status--broken {
      background: rgba(241, 76, 76, 0.16);
      border-color: rgba(241, 76, 76, 0.6);
      color: var(--vscode-errorForeground, #f14c4c);
    }

    button {
      flex: 1 1 48%;
      min-width: 110px;
      padding: 6px 10px;
      border: 1px solid var(--vscode-button-border, transparent);
      background: var(--vscode-button-background);
      color: var(--vscode-button-foreground);
      cursor: pointer;
      border-radius: 4px;
      transition: background 0.2s ease, box-shadow 0.2s ease, color 0.2s ease;
    }

    button.secondary {
      background: var(--vscode-input-background);
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-input-border, #666);
    }

    button.runtime-status {
      background: linear-gradient(135deg, rgba(255, 210, 141, 0.95), rgba(255, 162, 51, 0.95));
      color: #2b1500;
      border-color: rgba(255, 162, 51, 0.9);
      box-shadow: 0 0 10px rgba(255, 162, 51, 0.45);
    }

    button.runtime-alert {
      background: linear-gradient(135deg, rgba(255, 182, 160, 0.98), rgba(255, 99, 71, 0.95));
      color: #2b0900;
      border-color: rgba(255, 99, 71, 0.9);
      box-shadow: 0 0 12px rgba(255, 99, 71, 0.45);
    }

    .form {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    label {
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      opacity: 0.85;
    }

    input,
    textarea,
    select {
      width: 100%;
      box-sizing: border-box;
      background: var(--vscode-input-background);
      color: var(--vscode-input-foreground);
      border: 1px solid var(--vscode-input-border, #666);
      border-radius: 4px;
      padding: 6px;
      font-family: inherit;
    }

    textarea {
      min-height: 90px;
      resize: vertical;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .status {
      margin-top: 12px;
      padding: 8px;
      border: 1px solid var(--vscode-panel-border, #444);
      border-radius: 4px;
      font-size: 12px;
      opacity: 0.9;
    }

    .tree {
      margin-top: 16px;
      padding-top: 12px;
      border-top: 1px solid var(--vscode-panel-border, #444);
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .tree-header-row {
      display: flex;
      flex-direction: column;
      align-items: stretch;
      gap: 6px;
      width: 100%;
    }

    .tree-header {
      font-weight: 600;
      font-size: 13px;
      letter-spacing: 0.02em;
      text-transform: none;
      color: var(--vscode-foreground);
      opacity: 0.8;
    }

    .icon-button {
      width: 32px;
      height: 32px;
      flex: 0 0 auto;
      border-radius: 4px;
      border: 1px solid var(--vscode-input-border, #666);
      background: var(--vscode-input-background);
      color: var(--vscode-foreground);
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 18px;
      line-height: 1;
      padding: 0;
      aspect-ratio: 1 / 1;
    }

    .tree-header-row .icon-button {
      width: 100%;
      height: 36px;
      margin-left: 0;
      aspect-ratio: auto;
    }

    .icon-button:hover {
      border-color: var(--vscode-focusBorder, #3794ff);
      color: var(--vscode-focusBorder, #3794ff);
    }

    .tree details {
      border: 1px solid var(--vscode-panel-border, #444);
      border-radius: 4px;
      padding: 6px 8px;
      background: var(--vscode-sideBar-background, rgba(255, 255, 255, 0.02));
    }

    .tree .page-entry {
      position: relative;
      margin-bottom: 6px;
    }

    .tree .page-entry.drop-before::before,
    .tree .page-entry.drop-after::after {
      content: '';
      position: absolute;
      left: 4px;
      right: 4px;
      height: 2px;
      background: var(--vscode-focusBorder, #3794ff);
    }

    .tree .page-entry.drop-before::before {
      top: -3px;
    }

    .tree .page-entry.drop-after::after {
      bottom: -3px;
    }

    .tree summary {
      cursor: pointer;
      font-weight: 600;
      outline: none;
      display: flex;
      align-items: flex-start;
      gap: 6px;
    }

    .tree summary .page-meta {
      flex: 1 1 auto;
      min-width: 0;
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 4px;
    }

    .tree summary .page-title {
      min-width: 0;
      word-break: break-word;
    }

    .tree .drag-handle {
      width: 14px;
      height: 14px;
      border-radius: 2px;
      border: 1px solid transparent;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      cursor: grab;
      color: var(--vscode-foreground);
      font-size: 10px;
      user-select: none;
    }

    .tree .drag-handle:hover {
      border-color: var(--vscode-focusBorder, #3794ff);
    }

    .tree ul {
      list-style: none;
      padding-left: 12px;
      margin: 6px 0 0 0;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .tree li {
      font-size: 12px;
      padding: 2px 4px;
      border-radius: 3px;
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid transparent;
      cursor: pointer;
      transition: background 0.15s ease;
    }

    .tree li:hover {
      background: rgba(255, 255, 255, 0.08);
    }

    .tree li.selected {
      background: rgba(255, 168, 0, 0.25);
      border-color: rgba(255, 168, 0, 0.7);
    }

    .tree li.error {
      border-color: tomato;
      background: rgba(255, 99, 71, 0.2);
    }

    .tree li.error.selected {
      box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.2);
    }

    .tree-empty,
    .tree-error {
      font-size: 12px;
      opacity: 0.8;
    }

    .tree-error {
      color: var(--vscode-editorError-foreground, #f14c4c);
    }

    .badge {
      font-size: 11px;
      padding: 1px 6px;
      border-radius: 999px;
      border: 1px solid transparent;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .badge.hidden {
      border-color: rgba(255, 255, 255, 0.2);
      background: rgba(255, 255, 255, 0.07);
      color: rgba(255, 255, 255, 0.8);
    }

    .menu-backdrop {
      position: fixed;
      inset: 0;
      background: transparent;
      display: none;
      z-index: 900;
    }

    .menu-backdrop.visible {
      display: block;
    }

    .context-menu {
      position: fixed;
      min-width: 240px;
      max-width: 280px;
      padding: 12px;
      background: var(--vscode-editorWidget-background, var(--vscode-sideBar-background));
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-widget-border, #555);
      border-radius: 8px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.35);
      display: none;
      z-index: 1000;
      gap: 10px;
    }

    .context-menu.visible {
      display: flex;
      flex-direction: column;
    }

    #contextMenu,
    #subnodeMenu {
      max-width: none;
      background: var(--vscode-editor-background, rgba(255, 255, 255, 0.08));
      border-color: rgba(255, 255, 255, 0.18);
      box-shadow: 0 12px 28px rgba(0, 0, 0, 0.45);
      padding: 16px;
    }

    .context-menu .menu-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 600;
    }

    .context-menu .menu-section {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .context-menu input[type="text"] {
      width: 100%;
      box-sizing: border-box;
      background: var(--vscode-input-background);
      color: var(--vscode-input-foreground);
      border: 1px solid var(--vscode-input-border, #666);
      border-radius: 4px;
      padding: 6px;
      font-family: inherit;
    }

    .context-menu .format-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    .context-menu .format-buttons .format {
      flex: 0 1 auto;
      min-width: 64px;
      padding: 4px 10px;
      background: var(--vscode-button-secondaryBackground, var(--vscode-input-background));
      color: var(--vscode-foreground);
      border: 1px solid var(--vscode-input-border, #666);
    }

    .context-menu .format-buttons .format.active {
      background: var(--vscode-button-background);
      color: var(--vscode-button-foreground);
      border-color: var(--vscode-button-border, transparent);
    }

    .context-menu .radio-group {
      display: flex;
      gap: 12px;
      font-size: 12px;
    }

    .context-menu .radio-group label {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .context-menu .menu-actions {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
      gap: 8px;
      margin-top: 4px;
    }

    .context-menu .menu-actions button {
      width: 100%;
    }

    #newPageMenu .menu-actions {
      grid-template-columns: 1fr;
    }

    .context-menu .menu-actions button.full-width {
      grid-column: 1 / -1;
    }

    .context-menu button.danger {
      background: rgba(241, 76, 76, 0.18);
      color: var(--vscode-errorForeground, #f14c4c);
      border-color: rgba(241, 76, 76, 0.6);
    }

    .context-menu button.danger:hover {
      background: rgba(241, 76, 76, 0.28);
    }

    .context-menu .menu-footer {
      margin-top: 10px;
      padding-top: 10px;
      border-top: 1px solid var(--vscode-widget-border, #555);
    }

    .context-menu .menu-footer button {
      width: 100%;
    }
  </style>
</head>
<body>
  <div class="brand">
    <img src="${logoSrc}" alt="amium qBook" />
  </div>
  <div id="pipeStatus" class="pipe-status pipe-status--broken">
    <span class="dot" aria-hidden="true"></span>
    <span id="pipeStatusText">Pipe broken</span>
  </div>
  <div class="toolbar">
    <button id="btnStop" class="secondary">Stop</button>
    <button id="btnRun" class="secondary">Run</button>
    <button id="btnRebuild" class="secondary">Rebuild</button>
    <button id="btnBackup" class="secondary">Backup</button>
    <button id="btnDebugToggle" class="secondary">Start Debugging</button>
  </div>

  <div class="status" id="status">Ready</div>

  <div class="tree">
    <div class="tree-header-row">
      <div id="treeHeaderTitle" class="tree-header">Pages</div>
      <button id="btnAddPage" class="icon-button" title="Add Page" aria-label="Add Page">+</button>
    </div>
    <div id="bookTree">
      <div class="tree-empty">Loading Book.json ...</div>
    </div>
  </div>

  <div id="menuBackdrop" class="menu-backdrop" aria-hidden="true"></div>
  <div id="contextMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span id="menuTitle">Page Options</span>
    </div>
    <div class="menu-section">
      <label for="pageInput">Page</label>
      <input id="pageInput" type="text" value="" autocomplete="off" />
    </div>
    <div class="menu-section">
      <label for="titleInput">Title</label>
      <input id="titleInput" type="text" placeholder="Title" autocomplete="off" />
    </div>
    <div class="menu-section">
      <label>Format</label>
      <div class="format-buttons">
        <button type="button" class="format" data-value="A4">A4</button>
        <button type="button" class="format" data-value="16/9">16/9</button>
        <button type="button" class="format" data-value="16/10">16/10</button>
      </div>
    </div>
    <div class="menu-section">
      <label>Visibility</label>
      <div class="radio-group">
        <label>
          <input type="radio" name="visibility" value="visible" checked /> Visible
        </label>
        <label>
          <input type="radio" name="visibility" value="hidden" /> Hidden
        </label>
      </div>
    </div>
    <div class="menu-actions">
      <button id="addSubcodeBtn" type="button" class="secondary">Add Subcode</button>
      <button id="deletePageBtn" type="button" class="danger full-width">Delete Page</button>
    </div>
    <div class="menu-footer">
      <button id="menuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <div id="subnodeMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span id="subnodeMenuTitle">Code Optionen</span>
    </div>
    <div class="menu-section">
      <label for="subnodeNameInput">Name</label>
      <input id="subnodeNameInput" type="text" autocomplete="off" />
    </div>
    <div class="menu-actions">
      <button id="subnodeDeleteBtn" type="button" class="danger">Delete</button>
    </div>
    <div class="menu-footer">
      <button id="subnodeMenuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <div id="newPageMenu" class="context-menu" role="dialog" aria-modal="true">
    <div class="menu-header">
      <span>New Page</span>
    </div>
    <div class="menu-actions">
      <button id="createPageBtn" type="button">New Page</button>
      <button id="importPageBtn" type="button" class="secondary">Import Page</button>
    </div>
    <div class="menu-footer">
      <button id="newMenuClose" type="button" class="secondary">Close</button>
    </div>
  </div>

  <script nonce="${nonce}">
    const vscode = acquireVsCodeApi();

    const btnRun = document.getElementById('btnRun');
    const btnStop = document.getElementById('btnStop');
    const btnRebuild = document.getElementById('btnRebuild');
    const btnBackup = document.getElementById('btnBackup');
    const btnDebugToggle = document.getElementById('btnDebugToggle');
    const treeHeaderTitle = document.getElementById('treeHeaderTitle');
    const pageInput = document.getElementById('pageInput');
    const titleInput = document.getElementById('titleInput');
    const formatButtons = Array.from(document.querySelectorAll('button.format'));
    const status = document.getElementById('status');
    const bookTree = document.getElementById('bookTree');
    const pipeStatusElement = document.getElementById('pipeStatus');
    const pipeStatusText = document.getElementById('pipeStatusText');
    const contextMenu = document.getElementById('contextMenu');
    const menuBackdrop = document.getElementById('menuBackdrop');
    const menuTitle = document.getElementById('menuTitle');
    const menuClose = document.getElementById('menuClose');
    const addSubcodeBtn = document.getElementById('addSubcodeBtn');
    const deletePageBtn = document.getElementById('deletePageBtn');
    const btnAddPage = document.getElementById('btnAddPage');
    const newPageMenu = document.getElementById('newPageMenu');
    const createPageBtn = document.getElementById('createPageBtn');
    const importPageBtn = document.getElementById('importPageBtn');
    const newMenuClose = document.getElementById('newMenuClose');
    const subnodeMenu = document.getElementById('subnodeMenu');
    const subnodeMenuTitle = document.getElementById('subnodeMenuTitle');
    const subnodeNameInput = document.getElementById('subnodeNameInput');
    const subnodeDeleteBtn = document.getElementById('subnodeDeleteBtn');
    const subnodeMenuClose = document.getElementById('subnodeMenuClose');
    const visibilityRadios = Array.from(document.querySelectorAll('input[name="visibility"]'));
    const treeContainer = document.querySelector('.tree');

    const treeState = {
      selectedPath: null,
      errorPaths: [],
      selectedFormat: 'A4',
      hiddenMode: 'visible'
    };

    const menuState = {
      folder: null,
      page: null,
    };

    const subnodeState = {
      page: null,
      fileName: null,
    };

    const dragState = {
      sourcePage: null,
      targetPage: null,
      position: 'before'
    };

    const runtimeButtons = {
      run: btnRun,
      stop: btnStop,
      rebuild: btnRebuild,
    };

    const runtimeState = {
      run: null,
      stop: null,
      rebuild: null,
    };

    const runtimeButtonKeys = ['run', 'stop', 'rebuild'];
    const debugState = {
      active: false,
    };

    function updateRuntimeButtonsState(nextState) {
      const snapshot = nextState && typeof nextState === 'object' ? nextState : {};
      runtimeButtonKeys.forEach((key) => {
        const incoming = snapshot[key];
        const normalized = incoming === 'status' || incoming === 'alert' ? incoming : null;
        runtimeState[key] = normalized;
        const target = runtimeButtons[key];
        if (target) {
          target.classList.toggle('runtime-status', normalized === 'status');
          target.classList.toggle('runtime-alert', normalized === 'alert');
        }
      });
    }

    function applyDebugButtonsState(nextState) {
      const active = Boolean(nextState && typeof nextState === 'object' && nextState.active);
      debugState.active = active;
      if (btnDebugToggle) {
        btnDebugToggle.textContent = active ? 'Stop Debugging' : 'Start Debugging';
        btnDebugToggle.title = active ? 'Stop Debugging' : 'Start Debugging';
        btnDebugToggle.setAttribute('aria-label', active ? 'Stop Debugging' : 'Start Debugging');
        btnDebugToggle.classList.toggle('runtime-status', active);
        btnDebugToggle.classList.toggle('runtime-alert', false);
      }
    }

    function encodeHtml(value) {
      if (typeof value !== 'string') {
        return '';
      }
      return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function encodeAttr(value) {
      return encodeHtml(value).replace(/"/g, '&quot;');
    }

    function applyPipeStatus(state) {
      if (!pipeStatusElement || !pipeStatusText) {
        return;
      }

      const normalized = state === 'connected' ? 'connected' : 'broken';
      pipeStatusElement.classList.toggle('pipe-status--connected', normalized === 'connected');
      pipeStatusElement.classList.toggle('pipe-status--broken', normalized !== 'connected');
      pipeStatusText.textContent = normalized === 'connected' ? 'Pipe connected' : 'Pipe broken';
    }

    function updateTreeState(partial) {
      if (Object.prototype.hasOwnProperty.call(partial, 'selectedPath')) {
        treeState.selectedPath = partial.selectedPath ?? null;
      }

      if (Object.prototype.hasOwnProperty.call(partial, 'errorPaths')) {
        treeState.errorPaths = Array.isArray(partial.errorPaths) ? partial.errorPaths : [];
      }

      updateTreeHighlights();
    }

    function updateTreeHighlights() {
      if (!bookTree) {
        return;
      }

      const selectedLower = typeof treeState.selectedPath === 'string' ? treeState.selectedPath.toLowerCase() : '';
      const errorSet = new Set(
        Array.isArray(treeState.errorPaths)
          ? treeState.errorPaths
              .map((entry) => (typeof entry === 'string' ? entry.toLowerCase() : ''))
              .filter(Boolean)
          : []
      );

      const nodes = bookTree.querySelectorAll('li[data-file]');
      nodes.forEach((li) => {
        const rel = (li.getAttribute('data-file') || '').toLowerCase();
        li.classList.toggle('selected', Boolean(selectedLower) && rel === selectedLower);
        li.classList.toggle('error', errorSet.has(rel));
      });
    }

    function updateHiddenRadios() {
      const currentMode = treeState.hiddenMode === 'hidden' ? 'hidden' : 'visible';
      visibilityRadios.forEach((radio) => {
        if (radio instanceof HTMLInputElement) {
          radio.checked = radio.value === currentMode;
        }
      });
    }

    function setHiddenMode(isHidden) {
      treeState.hiddenMode = isHidden ? 'hidden' : 'visible';
      updateHiddenRadios();
    }

    function resolveNodePageValue() {
      if (typeof menuState.folder === 'string' && menuState.folder.length > 0) {
        return menuState.folder;
      }
      if (pageInput && 'value' in pageInput && pageInput.value) {
        return pageInput.value;
      }
      return '';
    }

    function sendHiddenCommand(isHidden) {
      vscode.postMessage({
        type: 'toggleHidden',
        hidden: Boolean(isHidden),
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function sendTitleCommand() {
      vscode.postMessage({
        type: 'updateTitle',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function sendFormatCommand(formatValue) {
      const normalized = typeof formatValue === 'string' ? formatValue : treeState.selectedFormat;
      vscode.postMessage({
        type: 'updateFormat',
        format: normalized,
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        folder: menuState.folder ?? undefined,
        nodePage: resolveNodePageValue() || undefined,
      });
    }

    function collectPageOrderFromDom() {
      if (!bookTree) {
        return [];
      }
      const entries = Array.from(bookTree.querySelectorAll('.page-entry'));
      return entries
        .map((entry) => (entry instanceof HTMLElement ? entry.getAttribute('data-page') : null))
        .filter((value) => typeof value === 'string' && value.length > 0);
    }

    function findPageEntry(page) {
      if (!bookTree || !page) {
        return null;
      }
      const entries = bookTree.querySelectorAll('.page-entry');
      for (const entry of entries) {
        if (entry instanceof HTMLElement && entry.getAttribute('data-page') === page) {
          return entry;
        }
      }
      return null;
    }

    function clearDropIndicators() {
      if (!bookTree) {
        return;
      }
      const entries = bookTree.querySelectorAll('.page-entry');
      entries.forEach((entry) => {
        entry.classList.remove('drop-before', 'drop-after');
      });
    }

    function applyDropIndicator(entry, isAfter) {
      if (!entry) {
        return;
      }
      entry.classList.toggle('drop-before', !isAfter);
      entry.classList.toggle('drop-after', isAfter);
    }

    function reorderDomEntries(sourcePage, targetEntry, position) {
      if (!bookTree) {
        return false;
      }
      const sourceEntry = findPageEntry(sourcePage);
      if (!sourceEntry || !targetEntry || sourceEntry === targetEntry) {
        return false;
      }

      if (position === 'after') {
        const next = targetEntry.nextSibling;
        bookTree.insertBefore(sourceEntry, next);
      } else {
        bookTree.insertBefore(sourceEntry, targetEntry);
      }
      return true;
    }

    function publishPageOrder() {
      const order = collectPageOrderFromDom();
      if (!order.length) {
        return;
      }
      vscode.postMessage({ type: 'reorderPages', order });
    }

    function resetDragState() {
      dragState.sourcePage = null;
      dragState.targetPage = null;
      dragState.position = 'before';
      clearDropIndicators();
    }

    function payload(type) {
      return {
        type,
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        pageText: '',
        timestamp: new Date().toISOString()
      };
    }

    function setStatus(text) {
      if (status) status.textContent = text;
    }

    btnRun?.addEventListener('click', () => {
      setStatus('Run started');
      vscode.postMessage(payload('run'));
    });

    btnStop?.addEventListener('click', () => {
      setStatus('Stop requested');
      vscode.postMessage(payload('stop'));
    });

    btnRebuild?.addEventListener('click', () => {
      setStatus('Rebuild running');
      vscode.postMessage(payload('rebuild'));
    });

    btnBackup?.addEventListener('click', () => {
      setStatus('Backup running');
      vscode.postMessage(payload('backup'));
    });

    btnDebugToggle?.addEventListener('click', () => {
      const shouldStop = debugState.active;
      setStatus(shouldStop ? 'Debugger disconnecting ...' : 'Debugger connecting ...');
      vscode.postMessage({ type: shouldStop ? 'debugStop' : 'debugStart' });
    });

    formatButtons.forEach((btn) => {
      btn.addEventListener('click', () => {
        const value = btn.getAttribute('data-value');
        treeState.selectedFormat = value;
        updateFormatButtons();
        sendFormatCommand(value);
      });
    });

    visibilityRadios.forEach((radio) => {
      radio.addEventListener('change', () => {
        if (!(radio instanceof HTMLInputElement) || !radio.checked) {
          return;
        }
        const selectedHidden = radio.value === 'hidden';
        setHiddenMode(selectedHidden);
        sendHiddenCommand(selectedHidden);
      });
    });

    if (titleInput) {
      titleInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          sendTitleCommand();
          closeContextMenu();
        }
      });
      titleInput.addEventListener('change', () => {
        sendTitleCommand();
      });
    }

    if (pageInput) {
      const sendRename = () => {
        vscode.postMessage({
          type: 'renamePage',
          page: pageInput && 'value' in pageInput ? pageInput.value : '',
          title: titleInput && 'value' in titleInput ? titleInput.value : '',
          format: treeState.selectedFormat ?? 'A4',
          hidden: treeState.hiddenMode === 'hidden',
          folder: menuState.folder ?? undefined,
          nodePage: resolveNodePageValue() || undefined,
        });
      };

      pageInput.addEventListener('change', sendRename);
      pageInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          sendRename();
          closeContextMenu();
        }
      });
    }

    function updateFormatButtons() {
      formatButtons.forEach((btn) => {
        const value = btn.getAttribute('data-value');
        btn.classList.toggle('active', value === treeState.selectedFormat);
      });
    }

    function positionMenuElement(element, x, y) {
      if (!element) {
        return;
      }
      const menuWidth = element.offsetWidth || 260;
      const menuHeight = element.offsetHeight || 240;
      const posX = Math.max(8, Math.min(x, window.innerWidth - menuWidth - 8));
      const posY = Math.max(8, Math.min(y, window.innerHeight - menuHeight - 8));
      element.style.left = posX + 'px';
      element.style.top = posY + 'px';
    }

    function positionNodeMenu(element, anchorY) {
      if (!element) {
        return;
      }
      if (!(treeContainer instanceof HTMLElement)) {
        positionMenuElement(element, 12, anchorY);
        return;
      }
      const rect = treeContainer.getBoundingClientRect();
      const menuHeight = element.offsetHeight || 260;
      const top = Math.max(8, Math.min(anchorY, window.innerHeight - menuHeight - 8));
      element.style.left = rect.left + 'px';
      element.style.top = top + 'px';
      const targetWidth = Math.max(rect.width, 0);
      element.style.width = targetWidth + 'px';
      element.style.minWidth = targetWidth + 'px';
      element.style.maxWidth = targetWidth + 'px';
      element.style.boxSizing = 'border-box';
    }

    function updateBackdropVisibility() {
      const pageMenuVisible = Boolean(contextMenu && contextMenu.classList.contains('visible'));
      const subnodeMenuVisible = Boolean(subnodeMenu && subnodeMenu.classList.contains('visible'));
      const newMenuVisible = Boolean(newPageMenu && newPageMenu.classList.contains('visible'));
      if (pageMenuVisible || subnodeMenuVisible || newMenuVisible) {
        menuBackdrop?.classList.add('visible');
      } else {
        menuBackdrop?.classList.remove('visible');
      }
    }

    function closeContextMenu() {
      contextMenu?.classList.remove('visible');
      if (contextMenu) {
        contextMenu.style.width = '';
        contextMenu.style.left = '';
        contextMenu.style.top = '';
        contextMenu.style.minWidth = '';
        contextMenu.style.maxWidth = '';
      }
      menuState.folder = null;
      menuState.page = null;
      updateBackdropVisibility();
    }

    function closeSubnodeMenu() {
      subnodeMenu?.classList.remove('visible');
      if (subnodeNameInput && 'value' in subnodeNameInput) {
        subnodeNameInput.value = '';
      }
      subnodeState.page = null;
      subnodeState.fileName = null;
      if (subnodeMenu) {
        subnodeMenu.style.width = '';
        subnodeMenu.style.left = '';
        subnodeMenu.style.top = '';
        subnodeMenu.style.minWidth = '';
        subnodeMenu.style.maxWidth = '';
      }
      updateBackdropVisibility();
    }

    function closeNewMenu() {
      newPageMenu?.classList.remove('visible');
      if (newPageMenu) {
        newPageMenu.style.width = '';
        newPageMenu.style.minWidth = '';
        newPageMenu.style.maxWidth = '';
        newPageMenu.style.left = '';
        newPageMenu.style.top = '';
      }
      updateBackdropVisibility();
    }

    function closeAllMenus() {
      closeContextMenu();
      closeNewMenu();
      closeSubnodeMenu();
      closeNewMenu();
    }

    function openContextMenu(event, pageData) {
      if (!contextMenu || !menuBackdrop) {
        return;
      }

      closeSubnodeMenu();
      closeNewMenu();

      const pageValue = pageData?.page ?? '';
      const titleValue = pageData?.title ?? '';
      const formatValue = pageData?.format ?? 'A4';
      const isHidden = Boolean(pageData?.hidden);

      if (pageInput && 'value' in pageInput) {
        pageInput.value = pageValue;
      }

      if (titleInput && 'value' in titleInput) {
        titleInput.value = titleValue;
      }

      treeState.selectedFormat = formatValue;
      setHiddenMode(isHidden);
      updateFormatButtons();

      if (menuTitle) {
        menuTitle.textContent = titleValue || pageValue || 'Page Options';
      }

      menuState.folder = pageData?.folder ?? null;
      menuState.page = pageValue;

      contextMenu.style.visibility = 'hidden';
      contextMenu.classList.add('visible');
      updateBackdropVisibility();

      requestAnimationFrame(() => {
        positionNodeMenu(contextMenu, event.clientY);
        if (contextMenu) {
          contextMenu.style.visibility = 'visible';
        }
      });
    }

    function extractCodeName(page, fileName) {
      if (!page || !fileName) {
        return '';
      }
      const prefix = page + '.';
      const suffix = '.cs';
      if (!fileName.startsWith(prefix) || !fileName.endsWith(suffix)) {
        return '';
      }
      return fileName.slice(prefix.length, fileName.length - suffix.length);
    }

    function openSubnodeMenu(event, fileData) {
      if (!subnodeMenu || !subnodeNameInput) {
        return;
      }

      const page = fileData?.page ?? '';
      const fileName = fileData?.fileName ?? '';
      const locked = Boolean(fileData?.locked);
      if (!page || !fileName) {
        return;
      }

      const codeName = extractCodeName(page, fileName);
      if (!codeName) {
        setStatus('This file cannot be edited.');
        return;
      }

      if (locked || codeName.toLowerCase() === 'qpage') {
        setStatus('qPage files cannot be modified.');
        return;
      }

      closeContextMenu();

      subnodeState.page = page;
      subnodeState.fileName = fileName;

      subnodeNameInput.value = codeName;
      if (subnodeMenuTitle) {
        subnodeMenuTitle.textContent = codeName;
      }

      subnodeMenu.style.visibility = 'hidden';
      subnodeMenu.classList.add('visible');
      updateBackdropVisibility();

      requestAnimationFrame(() => {
        positionNodeMenu(subnodeMenu, event.clientY);
        if (subnodeMenu) {
          subnodeMenu.style.visibility = 'visible';
        }
        if (subnodeNameInput) {
          subnodeNameInput.focus();
          subnodeNameInput.select();
        }
      });
    }

    menuBackdrop?.addEventListener('click', () => closeAllMenus());
    menuClose?.addEventListener('click', () => closeContextMenu());
    subnodeMenuClose?.addEventListener('click', () => closeSubnodeMenu());
    newMenuClose?.addEventListener('click', () => closeNewMenu());
    window.addEventListener('keydown', (event) => {
      if (event.key === 'Escape') {
        closeAllMenus();
      }
    });
    window.addEventListener('resize', () => closeAllMenus());

    btnAddPage?.addEventListener('click', (event) => {
      event.preventDefault();
      event.stopPropagation();
      if (newPageMenu?.classList.contains('visible')) {
        closeNewMenu();
        return;
      }
      closeContextMenu();
      closeSubnodeMenu();
      if (!newPageMenu) {
        return;
      }
      newPageMenu.style.visibility = 'hidden';
      newPageMenu.classList.add('visible');
      updateBackdropVisibility();
      requestAnimationFrame(() => {
        positionNodeMenu(newPageMenu, btnAddPage.getBoundingClientRect().bottom + 4);
        if (newPageMenu) {
          newPageMenu.style.visibility = 'visible';
        }
      });
    });

    createPageBtn?.addEventListener('click', () => {
      vscode.postMessage({ type: 'createPage' });
      closeNewMenu();
    });

    importPageBtn?.addEventListener('click', () => {
      vscode.postMessage({ type: 'importPage' });
      closeNewMenu();
    });

    function submitSubnodeRename() {
      if (!subnodeNameInput || !subnodeState.page || !subnodeState.fileName) {
        return;
      }
      const nextValue = subnodeNameInput.value.trim();
      if (!nextValue) {
        setStatus('Name cannot be empty.');
        return;
      }
      vscode.postMessage({
        type: 'renameSubnode',
        page: subnodeState.page,
        fileName: subnodeState.fileName,
        codeName: nextValue,
      });
      closeSubnodeMenu();
    }

    subnodeNameInput?.addEventListener('keydown', (event) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        submitSubnodeRename();
      }
    });

    subnodeDeleteBtn?.addEventListener('click', () => {
      if (!subnodeState.page || !subnodeState.fileName) {
        setStatus('No file selected.');
        return;
      }
      vscode.postMessage({
        type: 'deleteSubnode',
        page: subnodeState.page,
        fileName: subnodeState.fileName,
      });
      closeSubnodeMenu();
    });

    addSubcodeBtn?.addEventListener('click', () => {
      vscode.postMessage({
        type: 'addSubcode',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        folder: menuState.folder ?? undefined,
      });
      closeAllMenus();
    });

    deletePageBtn?.addEventListener('click', () => {
      vscode.postMessage({
        type: 'deletePage',
        page: pageInput && 'value' in pageInput ? pageInput.value : '',
        title: titleInput && 'value' in titleInput ? titleInput.value : '',
        format: treeState.selectedFormat ?? 'A4',
        hidden: treeState.hiddenMode === 'hidden',
        folder: menuState.folder ?? undefined,
      });
      closeAllMenus();
    });

    function renderTree(payload) {
      if (!bookTree) {
        return;
      }

      const nodes = Array.isArray(payload?.nodes) ? payload.nodes : [];
      const projectLabel = payload?.projectName ?? 'qBook Projekt';

      if (treeHeaderTitle) {
        treeHeaderTitle.textContent = projectLabel || 'Pages';
      }

      if (!nodes.length) {
        bookTree.innerHTML = '<p class="tree-empty">No pages found.</p>';
        return;
      }

      const treeMarkup = nodes
        .map((node) => {
          const children = Array.isArray(node.files)
            ? node.files
                .map((file) => {
                  const rel = typeof file.relativePath === 'string' ? file.relativePath : '';
                  const encodedRel = encodeAttr(rel);
                  const displayName = encodeHtml(file.displayName ?? file.name ?? '');
                  const rawFileName = typeof file.name === 'string' ? file.name : '';
                  const encodedFileName = encodeAttr(rawFileName);
                  const parentPage = encodeAttr(node.page ?? '');
                  const isLocked = rawFileName.toLowerCase().endsWith('.qpage.cs');
                  const lockedAttr = isLocked ? ' data-locked="true"' : '';
                  if (!rel) {
                    return '<li>' + displayName + '</li>';
                  }

                  const attrs =
                    '<li data-file="' +
                    encodedRel +
                    '" data-page="' +
                    parentPage +
                    '" data-filename="' +
                    encodedFileName +
                    '"' +
                    lockedAttr +
                    ' title="' +
                    encodedRel +
                    '">';
                  return attrs + displayName + '</li>';
                })
                .join('')
            : '';
          const pageLabel = encodeHtml(node.page ?? '');
          const hiddenBadge = node.metadata?.hidden
            ? '<span class="badge hidden" title="Hidden">Hidden</span>'
            : '';
          const pageMeta =
            '<div class="page-meta"><span class="page-title">' + pageLabel + '</span>' + hiddenBadge + '</div>';
          const normalizedPage = encodeAttr(node.page ?? '');
          const dragHandle =
            '<span class="drag-handle" draggable="true" data-page="' +
            normalizedPage +
            '" title="Move page" aria-label="Move page">&#8942;&#8942;</span>';
          const summaryAttributes =
            ' data-page="' +
            encodeAttr(node.metadata?.name ?? node.page ?? '') +
            '" data-title="' +
            encodeAttr(node.metadata?.title ?? node.metadata?.name ?? node.page ?? '') +
            '" data-format="' +
            encodeAttr(node.metadata?.format ?? 'A4') +
            '" data-hidden="' +
            (node.metadata?.hidden ? 'true' : 'false') +
            '" data-folder="' +
            encodeAttr(node.page ?? '') +
            '"';
          return (
            '<details class="page-entry" data-page="' +
            normalizedPage +
            '" open><summary' +
            summaryAttributes +
            '>' +
            dragHandle +
            pageMeta +
            '</summary><ul>' +
            children +
            '</ul></details>'
          );
        })
        .join('');

      bookTree.innerHTML = treeMarkup;
      updateTreeHighlights();
    }

    function renderTreeError(message) {
      if (!bookTree) {
        return;
      }

      const text = message ?? 'Failed to load tree.';
      const safeText = encodeHtml(text);
      if (treeHeaderTitle) {
        treeHeaderTitle.textContent = 'Book';
      }
      bookTree.innerHTML = '<p class="tree-error">' + safeText + '</p>';
      updateTreeHighlights();
    }

    bookTree?.addEventListener('click', (event) => {
      closeAllMenus();
      const target = event.target;
      if (!target) {
        return;
      }

      const element = target instanceof HTMLElement ? target : target.parentElement;
      if (!element || typeof element.closest !== 'function') {
        return;
      }

      const li = element.closest('li[data-file]');
      if (!li) {
        return;
      }

      const relativePath = li.getAttribute('data-file');
      if (relativePath) {
        vscode.postMessage({ type: 'openFile', relativePath });
      }
    });

    bookTree?.addEventListener('contextmenu', (event) => {
      const target = event.target instanceof HTMLElement ? event.target : null;
      if (!target) {
        return;
      }

      const fileElement = target.closest('li[data-file]');
      if (fileElement) {
        event.preventDefault();
        const fileData = {
          page: fileElement.getAttribute('data-page') ?? '',
          fileName: fileElement.getAttribute('data-filename') ?? '',
          locked: fileElement.getAttribute('data-locked') === 'true',
        };
        openSubnodeMenu(event, fileData);
        return;
      }

      const element = target.closest('summary[data-page]');
      if (!element) {
        return;
      }

      event.preventDefault();
      const data = {
        page: element.getAttribute('data-page') ?? '',
        title: element.getAttribute('data-title') ?? '',
        format: element.getAttribute('data-format') ?? 'A4',
        hidden: element.getAttribute('data-hidden') === 'true',
        folder: element.getAttribute('data-folder') ?? '',
      };

      openContextMenu(event, data);
    });

    bookTree?.addEventListener('dragstart', (event) => {
      const handle = event.target instanceof HTMLElement ? event.target.closest('.drag-handle') : null;
      if (!handle) {
        return;
      }

      const page = handle.getAttribute('data-page');
      if (!page) {
        return;
      }

      dragState.sourcePage = page;
      dragState.targetPage = null;
      dragState.position = 'before';
      if (event.dataTransfer) {
        event.dataTransfer.setData('text/plain', page);
        event.dataTransfer.effectAllowed = 'move';
      }
    });

    bookTree?.addEventListener('dragover', (event) => {
      if (!dragState.sourcePage) {
        return;
      }
      const entry = event.target instanceof HTMLElement ? event.target.closest('.page-entry') : null;
      if (!entry) {
        return;
      }
      const page = entry.getAttribute('data-page');
      if (!page || page === dragState.sourcePage) {
        dragState.targetPage = null;
        clearDropIndicators();
        return;
      }
      event.preventDefault();
      const rect = entry.getBoundingClientRect();
      const isAfter = event.clientY > rect.top + rect.height / 2;
      dragState.targetPage = page;
      dragState.position = isAfter ? 'after' : 'before';
      clearDropIndicators();
      applyDropIndicator(entry, isAfter);
      if (event.dataTransfer) {
        event.dataTransfer.dropEffect = 'move';
      }
    });

    bookTree?.addEventListener('drop', (event) => {
      if (!dragState.sourcePage) {
        return;
      }
      event.preventDefault();
      const { sourcePage, targetPage, position } = dragState;
      resetDragState();
      if (!sourcePage || !targetPage) {
        return;
      }
      const targetEntry = findPageEntry(targetPage);
      if (!targetEntry) {
        return;
      }
      const moved = reorderDomEntries(sourcePage, targetEntry, position);
      if (moved) {
        publishPageOrder();
      }
    });

    document.addEventListener('dragend', () => {
      resetDragState();
    });

    document.addEventListener('drop', (event) => {
      if (!bookTree) {
        return;
      }
      const node = event.target instanceof Node ? event.target : null;
      if (node && bookTree.contains(node)) {
        return;
      }
      resetDragState();
    });

    window.addEventListener('message', (event) => {
      const { type, payload, message, status: incomingPipeStatus, text } = event.data ?? {};
      if (type === 'bookData') {
        renderTree(payload);
      } else if (type === 'bookError') {
        renderTreeError(message);
      } else if (type === 'bookStatus') {
        const update = {};
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'selectedPath')) {
          update.selectedPath = payload.selectedPath;
        }
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'errorPaths')) {
          update.errorPaths = Array.isArray(payload.errorPaths) ? payload.errorPaths : [];
        }
        if (payload && Object.prototype.hasOwnProperty.call(payload, 'form')) {
          applyFormValues(payload.form);
        }
        updateTreeState(update);
      } else if (type === 'pipeStatus') {
        applyPipeStatus(typeof incomingPipeStatus === 'string' ? incomingPipeStatus : 'disconnected');
      } else if (type === 'runtimeState') {
        updateRuntimeButtonsState(payload);
      } else if (type === 'debugState') {
        applyDebugButtonsState(payload);
      } else if (type === 'statusText' && typeof text === 'string') {
        setStatus(text);
      }
    });
    function applyFormValues(formPayload) {
      if (!formPayload) {
        return;
      }

      if (pageInput && 'value' in pageInput && typeof formPayload.page === 'string') {
        pageInput.value = formPayload.page;
      }

      if (titleInput && 'value' in titleInput && typeof formPayload.title === 'string') {
        titleInput.value = formPayload.title;
      }

      const formatValue = typeof formPayload.format === 'string' ? formPayload.format : 'A4';
      treeState.selectedFormat = formatValue;
      updateFormatButtons();

      const hiddenValue = typeof formPayload.hidden === 'boolean' ? formPayload.hidden : false;
      setHiddenMode(hiddenValue);
    }

    updateFormatButtons();
    updateHiddenRadios();
    applyPipeStatus('disconnected');
    updateRuntimeButtonsState(runtimeState);
    applyDebugButtonsState({ active: false });

    vscode.postMessage({ type: 'requestTree' });
  </script>
</body>
</html>`;
}

function createNonce(): string {
  const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let value = '';

  for (let i = 0; i < 16; i++) {
    value += possible.charAt(Math.floor(Math.random() * possible.length));
  }

  return value;
}
