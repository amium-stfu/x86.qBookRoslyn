export type PipeConfigSource = 'project' | 'settings';

export type ActivePipeConfiguration = {
  serverPipe: string;
  clientPipe: string;
  reconnectDelay: number;
  source: PipeConfigSource;
};

export type BridgeMessage = {
  type?:
    | 'run'
    | 'stop'
    | 'save'
    | 'rebuild'
    | 'backup'
    | 'debugStart'
    | 'debugStop'
    | 'requestTree'
    | 'openFile'
    | 'toggleHidden'
    | 'updateTitle'
    | 'updateFormat'
    | 'renamePage'
    | 'reorderPages'
    | 'renameSubnode'
    | 'deleteSubnode'
    | 'addSubcode'
    | 'createPage'
    | 'importPage'
    | string;
  page?: string;
  title?: string;
  format?: string;
  pageText?: string;
  relativePath?: string;
  timestamp?: string;
  hidden?: boolean;
  folder?: string;
  nodePage?: string;
  order?: string[];
  fileName?: string;
  codeName?: string;
};

export type BookTreeLeaf = {
  name: string;
  relativePath: string;
  displayName: string;
};

export type PageMetadata = {
  name: string;
  title: string;
  format: string;
  hidden: boolean;
};

export type BookTreeNode = {
  page: string;
  files: BookTreeLeaf[];
  metadata?: PageMetadata;
};

export type BookTreePayload = {
  projectName?: string;
  rootPath: string;
  nodes: BookTreeNode[];
};

export type FormState = {
  page?: string;
  title?: string;
  format?: string;
  hidden?: boolean;
};

export type OPageDocument = {
  CodeOrder?: string[];
  Includes?: string[];
  Name?: string;
  Text?: string;
  Format?: string;
  Hidden?: boolean;
  [key: string]: unknown;
};
