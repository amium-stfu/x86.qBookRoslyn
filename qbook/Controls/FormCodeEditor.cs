using ActiproSoftware.Text;
using ActiproSoftware.Text.Languages.CSharp.Implementation;
using ActiproSoftware.Text.Languages.DotNet;
using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Languages.DotNet.Resolution;
using ActiproSoftware.Text.Languages.Python.Implementation;
using ActiproSoftware.Text.Languages.Python.Reflection;
using ActiproSoftware.Text.Languages.Python.Reflection.Implementation;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.Implementation;
using ActiproSoftware.Text.Parsing.LLParser;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.UI.WinForms.Controls.Docking;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using ActiproSoftware.UI.WinForms.Drawing;
using CSScripting;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;
using QB.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static qbook.MainForm;
using static qbook.Core;
using ActiproSoftware.Text.Implementation;
using System.Threading;
using ActiproSoftware.UI.WinForms.Controls.Rendering;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.Implementation;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Text.Utility;
using qbook.Extensions;
using QB; //qbookCsScript
using static IronPython.Modules._ast;
using System.Text;

namespace qbook.CodeEditor
{
    /// <summary>
    /// A form to test the dock controls.
    /// </summary>
	public partial class FormCodeEditor : System.Windows.Forms.Form
    {

        private int documentWindowIndex = 1;
        private int toolWindowIndex = 1;

        private bool ignoreModifiedDocumentClose = false;

        /// <summary>
        /// Creates an instance of the <c>DockForm</c> class.
        /// </summary>
        public FormCodeEditor()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Select the first item
            if (toolWindowPropertyGridComboBox.Items.Count > 0)
                toolWindowPropertyGridComboBox.SelectedIndex = 0;
            if (dockManagerPropertyGridComboBox.Items.Count > 0)
                dockManagerPropertyGridComboBox.SelectedIndex = 0;

            // Create documents
            //this.CreateCodeEditorDocument(null, "This is a read-only document.  Notice the lock context image in the tab.", true).Activate();
            //this.CreateCodeEditorDocument(null, null, false).Activate();

            // Set the size for screenshots
            // this.Size = new Size(408, 377);
        }

        //HACK: Forms with a ToolStrip requiere two clicks after they lost focus
        //below code takes care of this issue :/
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_PARENTNOTIFY = 0x0210;
            if (m.Msg == WM_PARENTNOTIFY)
            {
                if (!Focused)
                    Activate();
            }
            base.WndProc(ref m);
        }

        //Main.oPage _StartupPage = null;
        //public FormCodeEditor(Main.oPage page)
        //	: this()
        //{
        //	_StartupPage = page;
        //      }

        static private bool TryGetTagger(SyntaxEditor syntaxEditor, out HighlightRangeTagger tagger)
        {

            // Implementation Note:
            //
            // When associated with an ICodeDocument, the ISyntaxLanguage will use the registered
            // CodeDocumentTaggerProvider<HighlightRangeTagger> service to create a new instance of HighlightRangeTagger
            // and persist that instance in the ICodeDocument.Properties collection as long as the language
            // is active on the document.

            // Try to get the tagger that was created for the active document
            return syntaxEditor.Document.Properties.TryGetValue(typeof(HighlightRangeTagger), out tagger);
        }

        class SyntaxEditorDocumentWindow : DocumentWindow
        {
            public SyntaxEditorDocumentWindow(DockManager dockManager, string key, string text, int imageIndex, Control control)
                : base(dockManager, key, text, imageIndex, control)
            {
                if (key.StartsWith("py:"))
                    language = "py";
            }

            string language = "cs";
            public SyntaxEditor SyntaxEditor;
            public qbook.oPage Page;
            public TreeNode TreeNode;
            public string SubCodeKey;
            public string HeaderText;
            public string FooterText;

            public void SetCsScriptEditorCaretLineCharacter(int line, int character, int endLine = -1, int endCharacter = -1, bool selectWord = false, bool highlightWord = false)
            {
                try
                {
                    if (line < 0)
                        return;

                    var startTextPosition = new ActiproSoftware.Text.TextPosition(line, character);
                    SyntaxEditor.ActiveView.Selection.CaretPosition = SyntaxEditor.ActiveView.Selection.EndPosition;
                    SyntaxEditor.ActiveView.Selection.StartPosition = startTextPosition;

                    if (selectWord && endLine == -1 && endCharacter == -1)
                    {
                        //string text = SyntaxEditor.ActiveView.GetCurrentWordText();
                        var range = SyntaxEditor.ActiveView.GetCurrentWordTextRange();
                        SyntaxEditor.ActiveView.Selection.StartOffset = range.StartOffset;
                        SyntaxEditor.ActiveView.Selection.EndOffset = range.EndOffset;
                    }
                    else
                    {

                        if (endLine < 0 && endCharacter < 0)
                            SyntaxEditor.ActiveView.Selection.EndPosition = startTextPosition;
                        else
                        {
                            try
                            {
                                if (endCharacter > 998)
                                    endCharacter = SyntaxEditor.Text.Replace("\r", "").Split('\n')[endLine].Length;
                            }
                            catch (Exception ex)
                            {

                            }
                            SyntaxEditor.ActiveView.Selection.EndPosition = new ActiproSoftware.Text.TextPosition(endLine, endCharacter);
                        }
                    }
                    //SyntaxEditor.ActiveView.Scroller.ScrollIntoView(new ActiproSoftware.Text.TextPosition(line-10, 1), true);
                    //SyntaxEditor.ActiveView.Selection.CaretPosition = SyntaxEditor.ActiveView.Selection.EndPosition;
                    SyntaxEditor.ActiveView.Scroller.ScrollIntoView(startTextPosition, true);

                    if (highlightWord)
                    {
                        if (TryGetTagger(SyntaxEditor, out HighlightRangeTagger tagger))
                        {
                            //tagger.Clear();
                            tagger.HighlightRange(SyntaxEditor.ActiveView.Selection.SnapshotRange);
                        }
                    }
                }
                catch { }

                SyntaxEditor.Focus();
            }


            public void SetCsScriptEditorCaretOffset(int startOffset, int endOffset = -1)
            {
                SyntaxEditor.ActiveView.Selection.StartPosition = SyntaxEditor.ActiveView.OffsetToPosition(endOffset);
                if (endOffset < 0)
                    SyntaxEditor.ActiveView.Selection.EndPosition = SyntaxEditor.ActiveView.Selection.StartPosition;
                else
                    SyntaxEditor.ActiveView.Selection.EndPosition = SyntaxEditor.ActiveView.OffsetToPosition(startOffset);
                SyntaxEditor.Focus();
                SyntaxEditor.ActiveView.Scroller.ScrollIntoView(SyntaxEditor.ActiveView.OffsetToPosition(startOffset), true);
                //SyntaxEditor.ActiveView.Scroller.ScrollToCaret();
            }

            public void SaveCode()
            {
                if (language == "py")
                {
                    if (string.IsNullOrEmpty(this.SubCodeKey))
                    {
                        this.Page.PyCode = SyntaxEditor.Text;
                        this.Page.PyCodeHeader = this.HeaderText;
                        this.Page.PyCodeFooter = this.FooterText;
                    }
                    else
                    {
                        if (this.Page.PyCodeExtra.ContainsKey(this.SubCodeKey))
                            this.Page.PyCodeExtra[this.SubCodeKey] = SyntaxEditor.Text;
                        else
                            this.Page.PyCodeExtra.Add(this.SubCodeKey, SyntaxEditor.Text);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.SubCodeKey))
                    {
                        this.Page.CsCode = SyntaxEditor.Text;
                        this.Page.CsCodeHeader = this.HeaderText;
                        this.Page.CsCodeFooter = this.FooterText;
                    }
                    else
                    {
                        if (this.Page.CsCodeExtra.ContainsKey(this.SubCodeKey))
                            this.Page.CsCodeExtra[this.SubCodeKey] = SyntaxEditor.Text;
                        else
                            this.Page.CsCodeExtra.Add(this.SubCodeKey, SyntaxEditor.Text);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a text document.
        /// </summary>
        /// <returns>The <see cref="DocumentWindow"/> that was created.</returns>
        private SyntaxEditorDocumentWindow ShowCodeEditorDocument(qbook.oPage page, TreeNode treeNode, string language = "cs", string subCodeKey = null, bool readOnly = false)
        {
            SyntaxEditorDocumentWindow documentWindow;

            if (page == null)
                return null;

            string documentId = null;
            string documentName = null;

            if (language == "py")
            {
                documentId = "py:" + page.FullName + (!string.IsNullOrEmpty(subCodeKey) ? ("." + subCodeKey) : "");
                documentName = "py:" + page.Name + (!string.IsNullOrEmpty(subCodeKey) ? ("." + subCodeKey) : "");
            }
            else
            {
                documentId = page.FullName + (!string.IsNullOrEmpty(subCodeKey) ? ("." + subCodeKey) : "");
                documentName = page.Name + (!string.IsNullOrEmpty(subCodeKey) ? ("." + subCodeKey) : "");
            }

            toolStripTextBoxPageText.Text = page.TextL;
            toolStripTextBoxPageText.Enabled = (subCodeKey == null);


            // If the document is already open, show a message
            if (dockManager.DocumentWindows[documentId] != null)
            {
                try
                {
                    dockManager.DocumentWindows[documentId]?.Activate();
                }
                catch (Exception ex)
                {

                }
                //MessageBox.Show(this, String.Format("The file '{0}' is already open.", fileName), "File Already Open", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return dockManager.DocumentWindows[documentId] as SyntaxEditorDocumentWindow;
            }

            SyntaxEditor syntaxEditor = new SyntaxEditor();
            syntaxEditor.IsIndicatorMarginVisible = true;
            syntaxEditor.IsLineNumberMarginVisible = true;
            syntaxEditor.IsRulerMarginVisible = true;
            syntaxEditor.IsCurrentLineHighlightingEnabled = true;
            syntaxEditor.IsDefaultContextMenuEnabled = false;
            syntaxEditor.UserInterfaceUpdate += SyntaxEditor_UserInterfaceUpdate;
            syntaxEditor.DocumentParseDataChanged += SyntaxEditor_DocumentParseDataChanged;
            syntaxEditor.MouseWheel += SyntaxEditor_MouseWheel;
            
            documentWindow = new SyntaxEditorDocumentWindow(dockManager, documentId, documentName, 4, syntaxEditor);
            documentWindow.SyntaxEditor = syntaxEditor;
            documentWindow.Page = page;
            documentWindow.TreeNode = treeNode;
            documentWindow.SubCodeKey = subCodeKey;
            documentWindow.Page.MySyntaxEditor = syntaxEditor;
            syntaxEditor.DocumentTextChanged += SyntaxEditor_DocumentTextChanged;
            if (documentId != null)
            {
                documentWindow.FileName = documentId; //???
                documentWindow.FileType = null;
            }


            if (language == "py")
            {
                syntaxEditor.BackColor = Color.FromArgb(255, 255, 237, 168);//,0xFFFFE56B); // Color.Khaki;
                if (string.IsNullOrEmpty(subCodeKey))
                    syntaxEditor.Text = page.PyCode;
                else
                {
                    if (page.PyCodeExtra.ContainsKey(subCodeKey))
                        syntaxEditor.Text = page.PyCodeExtra[subCodeKey];
                    else
                        syntaxEditor.Text = "#new file";
                }
                documentWindow.HeaderText = page.PyCodeHeader;
                documentWindow.FooterText = page.PyCodeFooter;
            }
            else
            {
                if (string.IsNullOrEmpty(subCodeKey))
                    syntaxEditor.Text = page.CsCode;
                else
                {
                    if (page.CsCodeExtra.ContainsKey(subCodeKey))
                        syntaxEditor.Text = page.CsCodeExtra[subCodeKey];
                    else
                        syntaxEditor.Text = "//new file";
                }
                documentWindow.HeaderText = page.CsCodeHeader;
                documentWindow.FooterText = page.CsCodeFooter;
            }
            documentWindow.ContextMenuStrip = contextMenuStrip1;

            syntaxEditor.DocumentIsModifiedChanged -= CodeEditor_DocumentIsModifiedChanged;
            syntaxEditor.DocumentIsModifiedChanged += CodeEditor_DocumentIsModifiedChanged;

            this.CancelButton = null;

            //HALE: change Highlighting-Style: "number" -> Magenta
            var ok = ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.AmbientHighlightingStyleRegistry.Instance.Register(
                ActiproSoftware.Text.ClassificationTypes.Number, new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.Implementation.HighlightingStyle(Color.Magenta), true);
            //HALE: change Highlighting-Style: "string" -> italics
            ok = ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.AmbientHighlightingStyleRegistry.Instance.Register(
                ActiproSoftware.Text.ClassificationTypes.String, new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.Implementation.HighlightingStyle(Color.DarkRed, null, false, true, ActiproSoftware.UI.WinForms.Controls.Rendering.LineKind.None), true);


            if (language == "py")
            {
                //CombineCodeFragments(page, page.PyCodeHeader, page.PyCodeFooter);
            }
            else
            {
                //CombineCodeFragments(page, page.CsCodeHeader, page.CsCodeFooter);
            }

            //ShowHideRightPanel(checkBoxShowEditorReferences.Checked);

            //astOutputEditor.SetTabStopWidth(1);

            //
            // NOTE: Make sure that you've read through the add-on language's 'Getting Started' topic
            //   since it tells you how to set up an ambient parse request dispatcher and an ambient
            //   code repository within your application startup code, and add related cleanup in your
            //   application OnExit code.  These steps are essential to having the add-on perform well.
            //
            AmbientParseRequestDispatcherProvider.Dispatcher = new ThreadedParseRequestDispatcher();

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Amium\qbook\Package Repository");
            AmbientPackageRepositoryProvider.Repository = new FileBasedPackageRepository(appDataPath);


            // Initialize the project assembly (enables support for automated IntelliPrompt features)
            //CSharpSyntaxLanguage language = null;// new CSharpSyntaxLanguage();

            //if (projectAssembly == null)
            //{
            //    projectAssembly = new CSharpProjectAssembly("qbScript");
            //    var ref1 = projectAssembly.AssemblyReferences.AddFrom(System.IO.Path.GetFullPath("qbookCsScript.dll"));
            //    //var ref2 = projectAssembly.AssemblyReferences.AddMsCorLib();
            //    projectAssembly.AssemblyReferences.ItemAdded += AssemblyReferences_Changed;
            //    projectAssembly.AssemblyReferences.ItemRemoved += AssemblyReferences_Changed;
            //    var assemblyLoader = new BackgroundWorker();
            //    assemblyLoader.DoWork += DotNetProjectAssemblyReferenceLoader;
            //    assemblyLoader.RunWorkerAsync();

            //    // Load the .NET Languages Add-on C# language and register the project assembly on it
            //    //var language = new CSharpSyntaxLanguage();
            //    language = new CSharpSyntaxLanguage();
            //    //language.RegisterProjectAssembly(projectAssembly);
            //    language.RegisterService(projectAssembly); //HALE
            //}

            RefreshReferenceListCallback(null);
            qbook.Core.CsScriptAssembly.AssemblyReferences.ItemAdded -= AssemblyReferences_Changed;
            qbook.Core.CsScriptAssembly.AssemblyReferences.ItemAdded += AssemblyReferences_Changed;
            qbook.Core.CsScriptAssembly.AssemblyReferences.ItemRemoved -= AssemblyReferences_Changed;
            qbook.Core.CsScriptAssembly.AssemblyReferences.ItemRemoved += AssemblyReferences_Changed;

            if (page.CsCodeSourceFileKey != null)
            {
                //remove this SourceFile if it already exist, before re-adding it here in the "real" SyntaxEditor
                if (qbook.Core.CsScriptAssembly.SourceFiles.Contains(page.CsCodeSourceFileKey))
                    qbook.Core.CsScriptAssembly.SourceFiles.Remove(page.CsCodeSourceFileKey);
            }
            if (language == "py")
            {
                syntaxEditor.Document.Language = qbook.Core.PyScriptLanguage;
                syntaxEditor.Document.TabSize = 4;
                syntaxEditor.Document.AutoConvertTabsToSpaces = true;
            }
            else
            {
                syntaxEditor.Document.Language = qbook.Core.CsScriptLanguage;
            }

            page.CsCodeSourceFileKey = qbook.Core.CsScriptAssembly.SourceFiles.LastOrDefault()?.Key; //update the key

            var fooFiles = qbook.Core.CsScriptAssembly.SourceFiles;

            if (language == "py")
            {
                //TODO: no snippets for python yet
            }
            else
            {
                // @".\SyntaxEditor.Snippets"
                var snippetFolder = LoadSampleCSharpCodeSnippetsFromResources();
                syntaxEditor.Document.Language.RegisterService(new CSharpCodeSnippetProvider() { RootFolder = snippetFolder });
            }


            /*
            // Determine the type of file
            string fileType = "Text";
			if (documentId != null) {
				switch (Path.GetExtension(documentId).ToLower()) {
					case ".bmp":
					case ".gif":
					case ".ico":
					case ".jpg":
					case ".png":
						fileType = "Image";
						readOnly = true;
						break;
				}
			}

			switch (fileType) {
				case "Image": {
					// Create a PictureBox for the document
					PictureBox pictureBox = new PictureBox();
					pictureBox.Image = Image.FromFile(documentId);

					// Create the document window
					documentWindow = new DocumentWindow(dockManager, documentId, Path.GetFileName(documentId), 4, pictureBox);
					if (documentId != null) {
						documentWindow.FileName = documentId;
						documentWindow.FileType = String.Format("Image File (*{0})", Path.GetExtension(documentId).ToLower());
					}
					break;
				}
				default: {
					// Create a TextBox for the document
					RichTextBox textBox = new RichTextBox();
					textBox.Multiline = true;
					textBox.Font = new Font("Courier New", 10);
					textBox.BorderStyle = BorderStyle.None;
					textBox.HideSelection = false;
					textBox.ReadOnly = readOnly;	
					textBox.ScrollBars = RichTextBoxScrollBars.Both;
					textBox.WordWrap = false;

					// If no data was passed in, generate some
					if (documentId == null)
						documentId = String.Format("Document{0}.txt", documentWindowIndex++);
					if (text == null)
						text = "Visit our web site to learn more about Actipro WinForms Studio or our other controls:\r\nhttps://www.actiprosoftware.com/\r\n\r\nThis document was created at " + DateTime.Now.ToString() + ".";

					// Create the document window
					textBox.Text = text;
					textBox.TextChanged += new EventHandler(this.textBox_TextChanged);
					documentWindow = new DocumentWindow(dockManager, documentId, Path.GetFileName(documentId), 3, textBox);
					if (documentId != null) {
						documentWindow.FileName = documentId;
						documentWindow.FileType = String.Format("Text File (*{0})", Path.GetExtension(documentId).ToLower());
					}
					break;
				}
			}
			*/

            if (readOnly)
            {
                // Load a read-only context image 

                //
                documentWindow.ContextImage = ActiproSoftware.Products.Docking.AssemblyInfo.Instance.GetImage(ActiproSoftware.Products.Docking.ImageResource.ContextReadOnly);
            }

            return documentWindow;
        }

        private void SyntaxEditor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                int topVisibleLine = ActiveSyntaxEditor.ActiveView.VisibleViewLines.FirstVisiblePosition.Line - 2; //.VisibleStartPosition
                if (topVisibleLine < 0)
                    topVisibleLine = 0;
                var scrollState = new TextViewScrollState(new TextPosition(topVisibleLine, 0), TextViewVerticalAnchorPlacement.Top, 0, 0);

                if (e.Delta < 0)
                {
                    if (ActiveSyntaxEditor.Font.Size > 6)
                        ActiveSyntaxEditor.Font = new Font(ActiveSyntaxEditor.Font.FontFamily, ActiveSyntaxEditor.Font.Size - 1);
                }
                else if (e.Delta > 0)
                {
                    if (ActiveSyntaxEditor.Font.Size < 64)
                        ActiveSyntaxEditor.Font = new Font(ActiveSyntaxEditor.Font.FontFamily, ActiveSyntaxEditor.Font.Size + 1);
                }

                ActiveSyntaxEditor.ActiveView.Scroller.ScrollTo(scrollState);

                UpdateEverySyntaxEditorToActiveSyntaxEditor();
            }
        }

        Regex classNameRegex = new Regex(@"\s*((?<modifier>(public|internal|private))\s+)?class\s+\@class_(?<name>[_a-zA-Z][_a-zA-Z0-9]*)?.*");
        private void SyntaxEditor_DocumentTextChanged(object sender, EventArgs e)
        {
            SyntaxEditor se = sender as SyntaxEditor;
            string line = se.ActiveView.CurrentViewLine.Text; //.Document.vie.text.Lines[se.ActiveView.Selection.CaretPosition.Line].Text;
            if (classNameRegex.IsMatch(line))
            {
                Match m = classNameRegex.Match(line);
                if (m.Success)
                {
                    string modifier = m.Groups["identifier"].Value;
                    string className = m.Groups["name"].Value;
                    if (ActiveSyntaxEditorDocumentWindow.Page.Name != className)
                    {
                        ActiveSyntaxEditorDocumentWindow.Page.Name = className;
                        ActiveSyntaxEditorDocumentWindow.TreeNode.Text = className;
                        skipNextCheckPageTreeStringChanged = true;
                    }
                }
            }
        }

        private void SyntaxEditor_DocumentParseDataChanged(object sender, EventArgs e)
        {
            //
            // NOTE: The parse data here is generated in a worker thread... this event handler is called 
            //         back in the UI thread immediately when the worker thread completes... it is best
            //         practice to delay UI updates until the end user stops typing... we will flag that
            //         there is a pending parse data change, which will be handled in the 
            //         UserInterfaceUpdate event
            //

            hasPendingParseData = true;
        }
        private void SyntaxEditor_UserInterfaceUpdate(object sender, EventArgs e)
        {
            var syntaxEditor = sender as SyntaxEditor;
            if (syntaxEditor == null)
                return;

            // If there is a pending parse data change...
            if (hasPendingParseData)
            {
                // Clear flag
                hasPendingParseData = false;

                var parseData = syntaxEditor.Document.ParseData as ILLParseData;
                if (parseData != null)
                {
                    if (syntaxEditor.Document.CurrentSnapshot.Length < 10000)
                    {
                        // Show the AST
                        if (parseData.Ast != null)
                            astOutputEditor.Text = parseData.Ast.ToTreeString(0).Replace("\t", " ");
                        else
                            astOutputEditor.Text = null;
                    }
                    else
                        astOutputEditor.Text = "(Not displaying large AST for performance reasons)";

                    // Output errors
                    this.RefreshErrorList(parseData.Errors);

                    foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>())
                    {
                        if (doc.SyntaxEditor == syntaxEditor)
                        {
                            if (parseData.Errors.Count() == 0)
                                doc.Text = doc.FileName;
                            else
                                doc.Text = doc.FileName + "#";
                        }
                    }
                }
                else
                {
                    // Clear UI
                    astOutputEditor.Text = null;
                    foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>())
                    {
                        if (doc.SyntaxEditor == syntaxEditor)
                            doc.Text = doc.FileName;
                    }
                    this.RefreshErrorList(null);
                }
            }
        }

        private void CodeEditor_DocumentIsModifiedChanged(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            (sender as SyntaxEditor).Document.IsModified = false; //HACK //TODO trigger for next time 
        }

        private void RefreshReferenceListCallback(object stateInfo)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((System.Action)(() => this.RefreshReferenceListCallback(stateInfo)));
                return;
            }

            referencesListBox.Items.Clear();
            foreach (var assemblyRef in qbook.Core.CsScriptAssembly.AssemblyReferences.ToArray())
                referencesListBox.Items.Add(assemblyRef.Assembly.Name);
        }

        private bool hasPendingParseData;
        private System.Threading.Timer refreshReferencesTimer;
        private void AssemblyReferences_Changed(object sender, ActiproSoftware.Text.Utility.CollectionChangeEventArgs<IProjectAssemblyReference> e)
        {
            if (refreshReferencesTimer is null)
                refreshReferencesTimer = new System.Threading.Timer(RefreshReferenceListCallback);

            // Reset the timer each time a new event is raised (without auto-restart)
            refreshReferencesTimer.Change(dueTime: 250, period: System.Threading.Timeout.Infinite);
        }

        public static ICodeSnippetFolder LoadSampleCSharpCodeSnippetsFromResources()
        {
            // NOTE: If you have file system access, the static CodeSnippetFolder.LoadFrom(path) method easily
            //       loads snippets within a specified file path and should be used instead

            //string[] childPaths = new string[] {
            //    SnippetsPath + "CSharp.Sample_Child_Folder.while.snippet",
            //};
            //ICodeSnippetFolder childFolder = LoadCodeSnippetFolderFromResources("Sample Child Folder", childPaths);

            CodeSnippetSerializer serializer = new CodeSnippetSerializer();
            List<string> rootPaths = new List<string>();
            ICodeSnippetFolder folder = new CodeSnippetFolder("CsSnippets");

            if (Directory.Exists(@".\CsSnippets"))
                foreach (string file in Directory.GetFiles(@".\CsSnippets", "*.snippet"))
                {
                    rootPaths.Add(file);
                    IEnumerable<ICodeSnippet> snippets = serializer.LoadFromFile(file);
                    if (snippets != null)
                    {
                        foreach (ICodeSnippet snippet in snippets)
                            folder.Items.Add(snippet);
                    }
                }
            //ICodeSnippetFolder rootFolder = LoadCodeSnippetFolderFromResources("Root", rootPaths.ToArray());
            //rootFolder.Folders.Add(childFolder);
            return folder;
        }


        /// <summary>
        /// Returns whether the currently selected document is a text document.
        /// </summary>
        /// <returns>
        /// <c>true</c> if it is a document; otherwise, <c>false</c>.
        /// </returns>
        //private bool IsTextDocumentSelected()
        //{
        //    return ((dockManager.SelectedDocument != null) && (dockManager.SelectedDocument is DocumentWindow) &&
        //        (dockManager.SelectedDocument.Controls.Count == 1) && (dockManager.SelectedDocument.Controls[0] is RichTextBox));
        //}

        ///// <summary>
        ///// Update the border styles of child controls.
        ///// </summary>
        //private void UpdateChildControlBorderStyles()
        //{
        //    bool showBorders = (dockManager.DockRenderer.GetType() == typeof(VisualStudio2002DockRenderer));
        //    foreach (ToolWindow toolWindow in dockManager.ToolWindows)
        //    {
        //        bool changeToolWindowBorder = false;
        //        foreach (Control control in toolWindow.Controls)
        //        {
        //            if (control is TextBox)
        //            {
        //                ((TextBox)control).BorderStyle = (showBorders ? BorderStyle.Fixed3D : BorderStyle.None);
        //                changeToolWindowBorder = true;
        //            }
        //            else if (control is RichTextBox)
        //            {
        //                ((RichTextBox)control).BorderStyle = (showBorders ? BorderStyle.Fixed3D : BorderStyle.None);
        //                changeToolWindowBorder = true;
        //            }
        //            else if (control is ListBox)
        //            {
        //                ((ListBox)control).BorderStyle = (showBorders ? BorderStyle.Fixed3D : BorderStyle.None);
        //                changeToolWindowBorder = true;
        //            }
        //        }

        //        if (changeToolWindowBorder)
        //            toolWindow.Border = (showBorders ? null : new SimpleBorder(SimpleBorderStyle.Solid, SystemColors.ControlDark));
        //    }
        //}

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises the <c>Closing</c> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    // Call the base method
        //    base.OnClosing(e);

        //    if (!e.Cancel)
        //    {
        //        // Loop through and close all the documents and see if the cancel should be aborted
        //        for (int index = dockManager.DocumentWindows.Count - 1; index >= 0; index--)
        //        {
        //            // Activate the document window to examine
        //            DocumentWindow documentWindow = dockManager.DocumentWindows[0];
        //            documentWindow.Activate();

        //            // If a document is being closed and it has been modified...
        //            if (documentWindow.Modified)
        //            {
        //                if (MessageBox.Show(this, String.Format("The document '{0}' has been modified.  Would you like to close it without saving?", documentWindow.Key),
        //                    "Close Modified Document", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
        //                {
        //                    e.Cancel = true;
        //                    break;
        //                }
        //            }

        //            // Close the document window
        //            ignoreModifiedDocumentClose = true;
        //            documentWindow.Close();
        //            ignoreModifiedDocumentClose = false;
        //        }
        //    }
        //}

        string GetPageTreeString()
        {
            StringBuilder sb = new StringBuilder();
            var pageList = qbook.Core.ThisBook.Main.Objects.OfType<qbook.oPage>();
            foreach (var page in pageList)
            {
                sb.Append(page.FullName);
                if (!String.IsNullOrEmpty(page.PyCode))
                    sb.Append("py0");
                foreach (var extra in page.PyCodeExtra)
                    sb.Append(extra.Key);
                if (!String.IsNullOrEmpty(page.CsCode))
                    sb.Append("cs0");
                foreach (var extra in page.CsCodeExtra)
                    sb.Append(extra.Key);
            }
            return String.Join("|", sb);
        }
        int idxRoot = 0;
        int idxPage = 1;
        int idxMainCsCode = 2;
        int idxCsCode = 3;
        int idxPyCode = 4;
        int idxPageHidden = 5;
        void PopulatePageTreeView(bool selectFirstPage = false, bool keepSelectedNode = true)
        {
            bool lastSelectedNodeRestored = false;
            try
            {
                string selectedNodePath = null;
                if (keepSelectedNode)
                    selectedNodePath = treeViewCodePages.SelectedNode?.FullPath.Replace('\\', '/');

                treeViewCodePages.Nodes.Clear();
                var pageList = qbook.Core.ThisBook.Main.Objects.OfType<qbook.oPage>();
                //var ListOfObjectsSorted = ListOfObjects.OrderBy(r => r.Nr).ToList();
                var mainGroup = "Pages/Code";
                var topNode = new TreeNode(mainGroup, idxRoot, idxRoot);
                treeViewCodePages.Nodes.Add(topNode);
                string currentGroup = mainGroup; // ListOfObjectsSorted.First().Name;
                var treeNodes = new List<TreeNode>();
                var childNodes = new List<TreeNode>();
                foreach (var page in pageList)
                {
                    //TEMP: move PyCode(main) to a sub-code
                    if (!string.IsNullOrEmpty(page.PyCode))
                    {
                        if (!page.PyCodeExtra.ContainsKey("py0"))
                            page.PyCodeExtra.Add("py0", page.PyCode);
                        else
                            page.PyCodeExtra["py0"] = page.PyCode;
                        page.PyCode = null;
                    }

                    if (currentGroup == mainGroup) // rule.Group)
                    {
                        TreeNode pageNode;
                        if (page.Hidden)
                            pageNode = new TreeNode(page.Name, idxPageHidden, idxPageHidden);
                        else
                            pageNode = new TreeNode(page.Name, idxPage, idxPage);
                        pageNode.Tag = page;
                        childNodes.Add(pageNode);

                        var mainCodeNode = new TreeNode(/*page.Name*/"@class", idxMainCsCode, idxMainCsCode);
                        mainCodeNode.Tag = page;
                        pageNode.Nodes.Add(mainCodeNode);
                        foreach (var csSubCode in page.CsCodeExtra.OrderBy(s => s.Key))
                        {
                            var subCodeNode = new TreeNode(csSubCode.Key, idxCsCode, idxCsCode);
                            subCodeNode.Tag = page;
                            pageNode.Nodes.Add(subCodeNode);
                        }

                        if (!string.IsNullOrEmpty(page.PyCode))
                        {
                            var pyCodeNode0 = new TreeNode(page.Name, idxPyCode, idxPyCode);
                            pyCodeNode0.Tag = page;
                            pageNode.Nodes.Add(pyCodeNode0);
                        }
                        foreach (var pySubCode in page.PyCodeExtra.OrderBy(s => s.Key))
                        {
                            var subCodeNode = new TreeNode(pySubCode.Key, idxPyCode, idxPyCode);
                            subCodeNode.Tag = page;
                            pageNode.Nodes.Add(subCodeNode);
                        }
                    }
                    else
                    {
                        if (childNodes.Count > 0)
                        {
                            treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
                            childNodes = new List<TreeNode>();
                        }
                        childNodes.Add(new TreeNode(page.Name));
                        currentGroup = mainGroup; // item.Group;
                    }
                }
                //if (childNodes.Count > 0)
                //{
                //    treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));

                //}
                //topNode.Nodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
                topNode.Nodes.AddRange(childNodes.ToArray());

                treeViewCodePages.Nodes[0].Nodes.AddRange(treeNodes.ToArray());
                treeViewCodePages.ExpandAll();

                //restore previously selected node
                if (selectedNodePath != null)
                {
                    // Split the full path into individual node names
                    string[] nodeNames = selectedNodePath.Split('/');

                    // Start with the root node of the TreeView
                    TreeNode currentNode = treeViewCodePages.Nodes[0];

                    foreach (string nodeName in nodeNames.Skip(2))
                    {
                        // Find the child node with the specified name
                        TreeNode nextNode = null;
                        foreach (TreeNode childNode in currentNode.Nodes)
                        {
                            if (childNode.Text == nodeName)
                            {
                                nextNode = childNode;
                                break;
                            }
                        }

                        // If the node is found, set it as the current node for the next iteration
                        if (nextNode != null)
                        {
                            currentNode = nextNode;
                        }
                        else
                        {
                            // Node not found, break the loop or handle the situation accordingly
                            break;
                        }
                    }

                    // Select the final node if it was found
                    if (currentNode != null)
                    {
                        treeViewCodePages.SelectedNode = currentNode;
                        lastSelectedNodeRestored = true;
                    }
                }
                else if (selectFirstPage)
                {
                    if (topNode.Nodes.Count > 0)
                        treeViewCodePages.SelectedNode = topNode.Nodes[0];
                }
            }
            catch (Exception ex)
            {
                SetStatusText("#EX populating tree: " + ex.Message, Color.White, Color.Red);
            }
            if (!lastSelectedNodeRestored)
                UpdatePageTreeView(treeViewCodePages.Nodes);
        }

        void UpdatePageTreeView(TreeNodeCollection startNode)
        {
            try
            {
                foreach (TreeNode node in startNode)
                {
                    if (node.Nodes.Count > 0)
                    {
                        UpdatePageTreeView(node.Nodes);
                    }

                    if (node.Level == 1)
                    {
                        qbook.oPage page = node.Tag as qbook.oPage;
                        if (page != null)
                        {
                            if (page.Hidden)
                            {
                                //node.NodeFont = new Font(node.NodeFont, node.NodeFont.Style | FontStyle.Strikeout);
                                node.ImageIndex = idxPageHidden;
                                node.SelectedImageIndex = idxPageHidden;
                            }
                            else
                            {
                                //node.NodeFont = new Font(node.NodeFont, node.NodeFont.Style & ~FontStyle.Strikeout);
                                node.ImageIndex = idxPage;
                                node.SelectedImageIndex = idxPage;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        void SetStatusText(string text, Color? color = null, Color? backColor = null)
        {
            if (color == null)
                color = Color.Black;
            if (backColor == null)
                backColor = SystemColors.Control;

            this.EnsureBeginInvoke(() =>
            {
                statusLabel.Text = text;
                if (color != null)
                    statusLabel.ForeColor = (Color)color;
                if (backColor != null)
                    statusLabel.BackColor = (Color)backColor;
            });
        }   

        //void x_PopulatePageTreeView(bool selectFirstPage = false)
        //{
        //    treeViewCodePages.Nodes.Clear();
        //    var pageList = qbook.Core.ThisBook.Main.Objects.OfType<qbook.oPage>();
        //    //var ListOfObjectsSorted = ListOfObjects.OrderBy(r => r.Nr).ToList();
        //    var mainGroup = "Items/Pages";
        //    var topNode = new TreeNode(mainGroup);
        //    treeViewCodePages.Nodes.Add(topNode);
        //    string currentGroup = mainGroup; // ListOfObjectsSorted.First().Name;
        //    var treeNodes = new List<TreeNode>();
        //    var childNodes = new List<TreeNode>();
        //    foreach (var page in pageList)
        //    {
        //        if (currentGroup == mainGroup) // rule.Group)
        //        {
        //            var pageNode = new TreeNode(page.Name);
        //            pageNode.Tag = page;
        //            childNodes.Add(pageNode);

        //            foreach (var subCode in page.CsCodeExtra)
        //            {
        //                var subCodeNode = new TreeNode(subCode.Key);
        //                pageNode.Nodes.Add(subCodeNode);
        //            }
        //        }
        //        else
        //        {
        //            if (childNodes.Count > 0)
        //            {
        //                treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
        //                childNodes = new List<TreeNode>();
        //            }
        //            childNodes.Add(new TreeNode(page.Name));
        //            currentGroup = mainGroup; // item.Group;
        //        }
        //    }
        //    //if (childNodes.Count > 0)
        //    //{
        //    //    treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));

        //    //}
        //    //topNode.Nodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
        //    topNode.Nodes.AddRange(childNodes.ToArray());

        //    treeViewCodePages.Nodes[0].Nodes.AddRange(treeNodes.ToArray());
        //    treeViewCodePages.ExpandAll();

        //    if (selectFirstPage)
        //    {
        //        if (topNode.Nodes.Count > 0)
        //            treeViewCodePages.SelectedNode = topNode.Nodes[0];
        //    }

        //}

        private TreeNode FindTreeNode(TreeView treeView, qbook.oPage page)
        {
            foreach (TreeNode childNode in treeView.Nodes)
            {
                var foundPage = FindTreeNode(childNode, page);
                if (foundPage != null)
                    return foundPage;
            }
            return null;
        }
        private TreeNode FindTreeNode(TreeNode node, qbook.oPage page)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Tag == page)
                    return childNode;

                FindTreeNode(childNode, page);
            }
            return null;
        }

        bool isFirstVisible = true;
        private void toolWndPageExplorer_VisibleChanged(object sender, EventArgs e)
        {
            if (toolWndPageExplorer.Visible)
            {
                if (isFirstVisible)
                    PopulatePageTreeView(true);
                else
                    PopulatePageTreeView();

                isFirstVisible = false;
            }
        }

        public void SelectPage(qbook.oPage page)
        {
            var foundNode = FindTreeNode(treeViewCodePages, page);
            if (foundNode != null)
            {
                treeViewCodePages.SelectedNode = foundNode;
                //treeViewCodePages.Focus();
                //treeViewCodePages.SelectedNode = foundNode;
                //treeViewCodePages.Focus();
                foundNode.EnsureVisible();
            }
            //_StartupPage = null;
        }

        private void FormCodeEditor_Load(object sender, EventArgs e)
        {
            var tw = dockManager.ToolWindows;
            toolStripButtonShowFullCode.Visible = Debugger.IsAttached;

            //replace the "[x] Hidden" dummy with a real CheckBox
            int itemIndex = menuStrip.Items.IndexOf(toolStripCheckBox1);
            if (itemIndex >= 0)
            {
                if (menuStrip.Items[itemIndex].Visible)
                {
                    var alignment = menuStrip.Items[itemIndex].Alignment;
                    CheckBox ctrl = new CheckBox();
                    ctrl.Text = menuStrip.Items[itemIndex].Text.Replace("[x]", "").Trim();
                    ctrl.BackColor = Color.Transparent; // to show the toolstrip background.
                    ctrl.CheckAlign = ContentAlignment.MiddleRight;
                    ctrl.CheckStateChanged += checkBoxHidePage_CheckStateChanged;
                    ToolStripControlHost host = new ToolStripControlHost(ctrl);
                    menuStrip.Items.RemoveAt(itemIndex);
                    menuStrip.Items.Insert(itemIndex, host);
                    menuStrip.Items[itemIndex].Alignment = alignment;
                }
            }

            //replace the "[x] Hidden" dummy with a real CheckBox
            itemIndex = menuStrip.Items.IndexOf(toolStripLabelPageLabel);
            if (itemIndex >= 0)
            {
                if (menuStrip.Items[itemIndex].Visible)
                {
                    var alignment = menuStrip.Items[itemIndex].Alignment;
                    Label ctrl = new Label();
                    ctrl.TextAlign = ContentAlignment.MiddleLeft;
                    ctrl.Text = menuStrip.Items[itemIndex].Text; //.Replace("[x]", "").Trim();
                    ctrl.BackColor = Color.Transparent; // to show the toolstrip background.
                    ToolStripControlHost host = new ToolStripControlHost(ctrl);
                    menuStrip.Items.RemoveAt(itemIndex);
                    menuStrip.Items.Insert(itemIndex, host);
                    menuStrip.Items[itemIndex].Alignment = alignment;
                }
            }


            UpdateStatusVersion();
        }

        private void checkBoxHidePage_CheckStateChanged(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditorDocumentWindow != null && ActiveSyntaxEditorDocumentWindow.Page != null)
                ActiveSyntaxEditorDocumentWindow.Page.Hidden = (sender as CheckBox).Checked;
            UpdatePageTreeView(treeViewCodePages.Nodes);
        }

        void UpdateStatusVersion()
        {
            statusVersion.Text = "Version " + qbook.Core.ThisBook.Version;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(qbook.Core.ThisBook.VersionEpoch);
            statusLabelVersionEpoch.Text = "  Saved " + dateTimeOffset.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"); //("yyyy-MM-dd HH:mm:ss '(UTC'zzz')'")            
        }

        qbook.oPage SelectedPage = null;
        private void treeViewCodePages_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                var page = e.Node.Tag as qbook.oPage;
                if (page != null)
                {
                    SetStatusText("ready");
                    SelectedPage = (e.Node.Tag as qbook.oPage);
                    if (e.Node.ImageIndex == idxPage || e.Node.ImageIndex == idxPageHidden)
                    {
                        if (SelectedPage != null)
                            qbook.Core.SelectedPage = SelectedPage; //show page in mainform
                        //this.ShowCodeEditorDocument(page, e.Node, "cs", null, false).Activate();
                        var syntaxEditorDocumentWindow = this.ShowCodeEditorDocument(page, e.Node, "cs", null, false);
                        if (syntaxEditorDocumentWindow != null)
                        {
                            try
                            {
                                //syntaxEditorDocumentWindow.Activate(true);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    else if (e.Node.ImageIndex == idxMainCsCode)
                    {
                        this.ShowCodeEditorDocument(page, e.Node, "cs", null, false).Activate();
                    }
                    else if (e.Node.ImageIndex == idxCsCode)
                    {
                        this.ShowCodeEditorDocument(page, e.Node, "cs", e.Node.Text, false).Activate();
                    }
                    else if (e.Node.ImageIndex == idxPyCode)
                    {
                        this.ShowCodeEditorDocument(page, e.Node, "py", e.Node.Text, false).Activate();
                    }
                    //toolStripTextBoxPageName.Text = SelectedPage.Name.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
                    //toolStripTextBoxPageName.Enabled = true;
                }
            }
            catch (Exception ex)
            {

            }
            //else if (e.Node.Parent?.Tag is Main.oPage)
            //{
            //	SelectedPage = (e.Node.Parent?.Tag as Main.oPage);
            //	//toolStripTextBoxPageName.Text = SelectedPage.Name.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            //	//toolStripTextBoxPageName.Enabled = false;
            //}
            //else
            //{
            //	SelectedPage = null;
            //	//toolStripTextBoxPageName.Text = "";
            //	//toolStripTextBoxPageName.Enabled = false;
            //}


            //if (page != null)
            //         {
            //	if (radioButtonLangPython.Checked)
            //	{   //Python
            //                 this.ShowCodeEditorDocument(page, e.Node, "py", null, false).Activate();
            //             }
            //	else
            //	{
            //		//C#
            //		this.ShowCodeEditorDocument(page, e.Node, "cs", null, false).Activate();
            //	}
            //         }
            //else
            //         {
            //             if (e.Node.Parent?.Tag is Main.oPage)
            //	{
            //		page = e.Node.Parent.Tag as Main.oPage;
            //		if (page != null && page.CsCodeExtra.ContainsKey(e.Node.Text))
            //		{
            //			if (radioButtonLangPython.Checked)
            //			{   //Python
            //				this.ShowCodeEditorDocument(page, e.Node, "py", e.Node.Text, false).Activate();
            //			}
            //			else
            //                     {
            //				//CS
            //                         this.ShowCodeEditorDocument(page, e.Node, "cs", e.Node.Text, false).Activate();
            //                     }
            //		}
            //	}
            //	else
            //	{
            //		return;
            //	}
            //}		

            //textBoxHeader.Text = page.CsCodeHeader?.Replace("\r", "").Replace("\n", "\r\n"); //HACK: \n -> \r\n (why is the \r lost?)
            //textBoxFooter.Text = page.CsCodeFooter?.Replace("\r", "").Replace("\n", "\r\n"); //HACK: \n -> \r\n
        }

        private void buttonDefaultHeader_Click(object sender, EventArgs e)
        {
            textBoxHeader.Text = @"using System;
using System.Collections.Generic;
using System.Linq;
using static QbRoot;

//namespace qb
//{
    public class @class_{pagePath}
    {";
            textBoxHeader.Text = textBoxHeader.Text.Replace("{pagePath}", ActiveSyntaxEditorDocumentWindow.Page.FullName);
            //CombineCodeFragments(ActiveSyntaxEditorDocumentWindow.Page, ActiveSyntaxEditorDocumentWindow.HeaderText, ActiveSyntaxEditorDocumentWindow.FooterText);
        }

        private void buttonDefaultFooter_Click(object sender, EventArgs e)
        {
            textBoxFooter.Text = @"    }
//}";
            //CombineCodeFragments(ActiveSyntaxEditorDocumentWindow.Page, ActiveSyntaxEditorDocumentWindow.HeaderText, ActiveSyntaxEditorDocumentWindow.FooterText);
        }

        SyntaxEditorDocumentWindow ActiveSyntaxEditorDocumentWindow
        {
            get
            {
                if (dockManager.SelectedDocument != null)
                {
                    return dockManager.SelectedDocument as SyntaxEditorDocumentWindow; //.documentI
                }
                else
                    return null;
            }
        }

        SyntaxEditor ActiveSyntaxEditor
        {
            get
            {
                if (dockManager.SelectedDocument != null)
                    return (dockManager.SelectedDocument as SyntaxEditorDocumentWindow)?.SyntaxEditor;
                else
                    return null;
            }
        }

        //void CombineCodeFragments(qbook.oPage page, string headerText, string footerText)
        //{
        //    if (page == null)
        //        return;

        //    textBoxHeader.Text = textBoxHeader.Text.Replace("{pagePath}", page.FullName);
        //    page.MySyntaxEditor.Document.SetHeaderAndFooterText(headerText.Replace("\r\n", "\n"), footerText.Replace("\r\n", "\n"));
        //}

        private void textBoxHeader_TextChanged(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditorDocumentWindow == null)
                return;
            ActiveSyntaxEditorDocumentWindow.HeaderText = textBoxHeader.Text;
            //CombineCodeFragments(ActiveSyntaxEditorDocumentWindow.Page, ActiveSyntaxEditorDocumentWindow.HeaderText, ActiveSyntaxEditorDocumentWindow.FooterText);
        }

        private void textBoxFooter_TextChanged(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditorDocumentWindow == null)
                return;
            ActiveSyntaxEditorDocumentWindow.FooterText = textBoxFooter.Text;
            //CombineCodeFragments(ActiveSyntaxEditorDocumentWindow.Page, ActiveSyntaxEditorDocumentWindow.HeaderText, ActiveSyntaxEditorDocumentWindow.FooterText);
        }

        private void toolStripButtonRebuild_Click(object sender, EventArgs e)
        {
            DoRebuild();
        }

        internal async void ReInit()
        {
            foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>().ToList())
                doc.Close();

            textBoxOutput.Clear();
            errorListView.Clear();

            PopulatePageTreeView(true);

            foreach (var page in qbook.Core.ThisBook.Main.Objects)
            {
                //var codeEditorTemp =
                page.MySyntaxEditor = new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor();
                page.MySyntaxEditor.Document.Language = qbook.Core.CsScriptLanguage;
                page.MySyntaxEditor.Document.SetHeaderAndFooterText(page.CsCodeHeader, page.CsCodeFooter);
                page.MySyntaxEditor.Text = page.CsCode;
                page.CsCodeSourceFileKey = qbook.Core.CsScriptAssembly.SourceFiles.Last().Key;
            }
        }

        internal async void DoRebuild()
        {
            //toolStripButtonRebuild.Enabled = false;
            foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>().ToList())
            {
                if (TryGetTagger(doc.SyntaxEditor, out HighlightRangeTagger tagger))
                {
                    tagger.Clear();
                }
            }

            foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>())
                doc.SaveCode();

            await RebuildCodeAllAsync();
            if (qbook.Core.LastBuildResult == null)
            {
                this.EnsureBeginInvoke(() =>
                {
                    toolStripButtonRebuild.BackColor = Color.LightGreen;
                    //toolStripButtonRebuild.Enabled = true;
                });
            }
            else
            {
                this.EnsureBeginInvoke(() =>
                {
                    toolStripButtonRebuild.BackColor = Color.LightCoral;
                    //toolStripButtonRebuild.Enabled = false;
                });
            }
        }


        Regex sourceCodeOffsetRegex = new Regex(@"////cs:(?<source>[^@]+)@ln:(?<line>\d+)");
        Regex sourceCodeIncludeStart = new Regex(@"//\+include .* --- start ---");
        Regex sourceCodeIncludeEnd = new Regex(@"//\+include .* --- end ---");
        async Task RebuildCodeAllAsync()
        {
            SetStatusText("rebuilding...");

            log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders();

            //            var udpAppenders = hierarchy.Root.Appenders.OfType<log4net.Appender.UdpAppender>().ToList();
            //            foreach (var appender in udpAppenders)
            //                hierarchy.Root.RemoveAppender(appender);

            QB.Logger.Info("Rebuilding Script...");
            //Main.Qb.Widget.Chart chart = new Main.Qb.Widget.Chart();
            //chart.curves.add(null);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            textBoxOutput.Clear();
            sw.Restart();
            try
            {
                SetOutputColor();
                this.Cursor = Cursors.WaitCursor;

                //EditObjectForm.ApplyAllCodeChanges();

                //if (false)
                //{
                //    //TODO: for now, just save this editor's code
                //    Item.CsCode = codeEditor.Text;
                //    Item.CsCodeHeader = textBoxHeader.Text.Replace("\r", "").Replace("\n", "\r\n");
                //    Item.CsCodeFooter = textBoxFooter.Text.Replace("\r", "").Replace("\n", "\r\n");
                //    Main.Qb.Book.Serialize();
                //    Main.Qb.Book.Modified = false;
                //}

                try
                {
                    await qbook.Core.CsScriptRebuildAll(); //BuildCsScriptAll();
                    sw.Stop();
                    this.EnsureBeginInvoke(() =>
                    {
                        SetStatusText("Success: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms");
                    });
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    this.EnsureBeginInvoke((System.Action)(() =>
                    {
                        SetStatusText("#ERR: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms", Color.Red);
                        SetOutputColor(Color.LightCoral);
                        //tabControl3.SelectedTab = tabPageOutput;
                        toolWindowOutput.Select();

                        //OutputWriteLine("--- ERRORS ---\r\n" + ex.Message);
                        var errorLines = ex.Message.Replace("\r", "").Split('\n');
                        int includeLevel = 0;
                        for (int errorLineNr = 0; errorLineNr < errorLines.Length; errorLineNr++)
                        {
                            string line = errorLines[errorLineNr];
                            Match m = lineColErrorRegex.Match(line);
                            if (m.Success)
                            {
                                int lineNr = int.Parse(m.Groups["line"].Value) - 1;
                                int col = int.Parse(m.Groups["col"].Value) - 1;
                                string error = m.Groups["error"].Value;
                                //now scan backwards in FullCode until we find a reference like "////cs:<source>@ln:<line>"
                                int scanLineNr = lineNr--;
                                int linesUp = 0;
                                while (lineNr >= 0)
                                {
                                    if (includeLevel == 0
                                    && sourceCodeOffsetRegex.IsMatch(qbook.Core.LastCsScriptBuildCodeLines[lineNr]))
                                    {
                                        Match m1 = sourceCodeOffsetRegex.Match(qbook.Core.LastCsScriptBuildCodeLines[lineNr]);
                                        if (m1.Success)
                                        {
                                            string source = m1.Groups["source"].Value;
                                            int offset = int.Parse(m1.Groups["line"].Value);
                                            errorLines[errorLineNr] = $"<{source}>({linesUp + offset + 0},{col + 1}): " + error;
                                            break;
                                        }
                                    }

                                    if (sourceCodeIncludeEnd.IsMatch(qbook.Core.LastCsScriptBuildCodeLines[lineNr]))
                                    {
                                        //we reached a '//+include * --- end ---' tag (so we're in plain-code, not included)
                                        includeLevel--;
                                    }
                                    if (sourceCodeIncludeStart.IsMatch(qbook.Core.LastCsScriptBuildCodeLines[lineNr]))
                                    {
                                        //we reached a '//+include * --- start ---' tag (so we're in plain-code, not included)
                                        includeLevel++;
                                        linesUp++;
                                    }

                                    if (includeLevel == 0)
                                        linesUp++;
                                    lineNr--;
                                }
                            }

                        }
                        OutputWriteLine("--- ERRORS ---\r\n" + string.Join("\r\n", errorLines));
                        OutputWriteScrollToLine(0);
                    }));
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                this.EnsureBeginInvoke((System.Action)(() =>
                {
                    SetStatusText("#ERR: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms", Color.Red);
                    QB.Logger.Error("#ERR Rebuilding Script: " + ex.Message);
                    //tabControl3.SelectedTab = tabPageOutput;
                    toolWindowOutput.Select();
                    OutputWriteLine("--- ERRORS ---\r\n" + ex.Message);
                    OutputWriteScrollToLine(0);
                }));
            }
            finally
            {
                sw.Stop();
                this.EnsureBeginInvoke(() => this.Cursor = Cursors.Default);
                QB.Logger.Info($"Rebuilding Script... took {sw.ElapsedMilliseconds}ms");
            }

            if (true) //auto-highlight all errors
            {
                if (textBoxOutput.Text.startswith("--- ERRORS ---"))
                {
                    foreach (var line in textBoxOutput.Lines.Skip(1).Reverse())
                    {
                        HighlightError(line, highlightWord: true, selectWord: true, openDocumentsOnly:true);
                    }
                }
                
            }
        }

        private async void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            await DoRun();
        }

        private async Task DoRun()
        {
            if (qbook.Core.csScript == null)
            {
                try
                {
                    await qbook.Core.CsScriptRebuildAll(); //BuildCsScriptAll();
                }
                catch (Exception ex)
                {
                    SetStatusText("#ERR: rebuilding...", Color.Red);
                    //tabControl3.SelectedTab = tabPageOutput;
                    toolWindowOutput.Select();
                    OutputWriteLine("--- ERRORS ---\r\n" + ex.Message);
                    return;
                }
            }

            try
            {
                SetStatusText("running...");
                statusStrip1.Refresh();

                var buildRunTrialPath = Path.Combine(qbook.Core.ThisBook.Directory, qbook.Core.ThisBook.Filename) + "~buildrun~";
                if (File.Exists(buildRunTrialPath))
                    File.Delete(buildRunTrialPath);

                qbook.Core.RunCsScript_Run();
                //script.Main(null);
                //script.page1.Go();
                toolStripButtonRun.BackColor = SystemColors.ButtonFace;
            }
            catch (Exception ex)
            {
                SetStatusText("#ERR running... (see Output)", Color.Red);
                Console.WriteLine("#EX CsScript: " + ex.Message);
                OutputWriteLine("#EX: " + ex.Message);
            }
        }

        void OutputWrite(string text)
        {
            textBoxOutput.Text += text;
            textBoxOutput.SelectionStart = textBoxOutput.TextLength - 1;
            textBoxOutput.ScrollToCaret();
        }
        void OutputWriteLine(string text)
        {
            OutputWrite(text + "\r\n");
        }

        void OutputWriteScrollToLine(int line)
        {
            textBoxOutput.SelectionStart = textBoxOutput.GetFirstCharIndexFromLine(line);
            textBoxOutput.SelectionLength = 0;
            textBoxOutput.ScrollToCaret();
        }

        void SetOutputColor(Color? backColor = null, Color? foreColor = null)
        {
            textBoxOutput.BackColor = backColor ?? Color.White;
            textBoxOutput.ForeColor = foreColor ?? Color.Black;
        }


        Regex lineColErrorRegex = new Regex(@"^<(?<source>[^>]*)>\((?<line>\d+),(?<col>\d+)\)\: (?<error>.*)$", RegexOptions.Compiled);
        Regex pageNameRegex = new Regex(@"^//=== class(?<sub>.sub)? '(?<name>[^']+)' ===\s*$", RegexOptions.Compiled);
        private void textBoxOutput_DoubleClick(object sender, EventArgs e)
        {
            int currentLine = textBoxOutput.GetLineFromCharIndex(textBoxOutput.SelectionStart);
            int lineStartIndex = textBoxOutput.GetFirstCharIndexFromLine(currentLine);
            int lineLength = textBoxOutput.Lines[currentLine].Length;
            textBoxOutput.Select(lineStartIndex, lineLength);
            string errorText = textBoxOutput.Lines[textBoxOutput.GetLineFromCharIndex(textBoxOutput.SelectionStart)];
            HighlightError(errorText, highlightWord: false, selectWord: true);
        }

        void HighlightError(string errorText, bool highlightWord = false, bool selectWord = false, bool openDocumentsOnly=false)
        {
            try
            {
                Match m = lineColErrorRegex.Match(errorText);
                if (m.Success)
                {
                    string source = m.Groups["source"].Value.Replace('_', '.');
                    int lineNr = int.Parse(m.Groups["line"].Value) - 0;
                    int col = int.Parse(m.Groups["col"].Value) - 1;
                    string error = m.Groups["error"].Value;

                    qbook.oPage page = null;
                    string subCodeKey = null;
                    foreach (var p in qbook.Core.ThisBook.Main.Objects.OfType<qbook.oPage>())
                    {
                        if (p.FullName.Replace('_', '.') == source)
                        {
                            page = p;
                            break;
                        }
                        foreach (var subPage in p.CsCodeExtra)
                        {
                            if (p.FullName + "." + subPage.Key == source)
                            {
                                page = p;
                                subCodeKey = subPage.Key;
                                break;
                            }
                        }
                    }

                    if (openDocumentsOnly)
                    {
                        bool pageIsOpen = false;
                        foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>().ToList())
                        {
                            if (doc.Page == page)
                            {
                                pageIsOpen = true;
                                break;
                            }
                        }
                        
                        if (!pageIsOpen)
                            return;
                    }

                    SyntaxEditorDocumentWindow docWindow = null;
                    if (page != null)
                    {
                        try
                        {
                            TreeNode myTreeNode = null; //TODO: find matching treenode
                            docWindow = ShowCodeEditorDocument(page as qbook.oPage, myTreeNode, "cs", subCodeKey, false);
                            docWindow.Activate();
                            //var range = docWindow.SyntaxEditor.ActiveView.GetCurrentWordTextRange();
                            docWindow.SetCsScriptEditorCaretLineCharacter(lineNr - 1, col, highlightWord: highlightWord, selectWord: selectWord);
                        }
                        catch
                        {
                            MessageBox.Show("Cannot open page-editor for erroneous code.\r\n(Possibly qbook-framework related)\r\n\r\nLine causing the error is:\r\n---\r\n" + qbook.Core.LastCsScriptBuildCodeLines[lineNr-1] + "\r\n---", $"ERROR at {lineNr - 0},{col}");
                        }
                    }
                    else
                    {
                        int origLineNr = lineNr;
                        MessageBox.Show("Cannot open page-editor for erroneous code.\r\n(Possibly qbook-framework related)\r\n\r\nLine causing the error is:\r\n---\r\n" + qbook.Core.LastCsScriptBuildCodeLines[origLineNr-1] + "\r\n---", $"ERROR at {origLineNr - 0 },{col + 1}");
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        void GotoDefinition()
        {
            //HALE: seems to only work for known types (of other assemblies?)
            /*
             * https://www.actiprosoftware.com/community/thread/24255/go-to-definition-functionality
             * While we don't have anything built-in specifically for go to definition, it should be possible to implement it with the information we provide.  
             * You could create a CSharpContextFactory and call its CreateContext method to get an IDotNetContext object back.  Then call that object's Resolve method 
             * to get a IResolverResultSet.  Look at the first result if there is one.  Each IResolverResult has an ITypeReference.  
             * If there was a fully resolved result, this ITypeReference will be an ITypeDefinition.  
             * ITypeDefinition has a SourceFileLocations collection that returns an ISourceFileLocation instance.  
             * Each one of these consists of a string Key (which is generally a file path or document UniqueId GUID) and the NavigationOffset within that document.  
             * By opening the related document and going to the NavigationOffset, you can achieve go to definition functionality.
             */
            CSharpContextFactory ctxFactory = new CSharpContextFactory();
            //TextSnapshotOffset snapshotOffset = syntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            TextSnapshotOffset snapshotOffset = ActiveSyntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            ActiproSoftware.Text.Languages.DotNet.IDotNetContext ctx = ctxFactory.CreateContext(snapshotOffset);
            IResolverResultSet resolverResultSet = ctx.Resolve();
            if (resolverResultSet != null && resolverResultSet.Results.Count > 0)
            {
                foreach (IResolverResult result in resolverResultSet.Results)
                {
                    //ITypeDefinition typeDef = result.Type as ITypeDefinition;
                    if (ActiveSyntaxEditor.Document.Language is PythonSyntaxLanguage)
                    {
                        ActiproSoftware.Text.Languages.DotNet.Reflection.ITypeDefinition typeDef = result.Type as ActiproSoftware.Text.Languages.DotNet.Reflection.ITypeDefinition;
                        if (typeDef != null)
                        {
                            foreach (var location in typeDef.SourceFileLocations)
                            {
                            }
                        }
                    }
                    else
                    {
                        ActiproSoftware.Text.Languages.Python.Reflection.ITypeDefinition typeDef = result.Type as ActiproSoftware.Text.Languages.Python.Reflection.ITypeDefinition;
                        if (typeDef != null)
                        {
                            //foreach (var location in typeDef.SourceFileLocations)
                            //{
                            //}
                        }
                    }
                    //string guid = result.Key;
                    //int offset = result.NavigationOffset;
                }
            }

        }

        private void gotoDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GotoDefinition2();
        }

        class FindResult
        {
            public qbook.oItem Page;
            public string SubCodeKey = null;
            public SyntaxEditor Editor = null;
            public string SyntaxEditorKey = null;
            public int StartOffset = -1;
            public int EndOffset = -1;
            public int LineNr = -1;
            public int StartCharacter = -1;
            public int EndCharacter = -1;
            public string Code = null;

        }


        private void findReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindAllReferencesInCode();
        }

        void FindAllReferencesInCode(string replaceText = null)
        {
            List<FindResult> findList = new List<FindResult>();
            CSharpContextFactory ctxFactory = new CSharpContextFactory();
            //TextSnapshotOffset snapshotOffset = syntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            TextSnapshotOffset snapshotOffset = ActiveSyntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            ActiproSoftware.Text.Languages.DotNet.IDotNetContext ctx = ctxFactory.CreateContext(snapshotOffset);
            IResolverResultSet resolverResultSet = ctx.Resolve();
            if (resolverResultSet != null && resolverResultSet.Results.Count > 0)
            {
                string findTerm = resolverResultSet.Results[0].Name;

                if (replaceText == "$$$askUserForReplaceText")
                {
                    replaceText = findTerm;
                    var dr = QB.UI.InputDialog.ShowDialog("RENAME SYMBOL v0.1", "!!!Rename does not yet work reliably!!!\r\n\r\n" + $"Change '{findTerm}' to:", ref replaceText);
                    if (dr != DialogResult.OK)
                        return;
                }

                Regex findFullWordOutsideQuotes = new Regex(@"""[^""]*""|(\b" + findTerm + @"\b)");
                foreach (var page in qbook.Core.ThisBook.Main.Objects)
                {
                    if (page.MySyntaxEditor != null)
                    {
                        if (page == ActiveSyntaxEditorDocumentWindow.Page) //HACK?: make sure page.MySyntaxEditor.Text is updated
                            page.MySyntaxEditor.Text = page.MySyntaxEditor.Text;

                        int lineNr = 0;
                        //string code = page.MySyntaxEditor.Text;
                        //foreach (string line in code.Replace("\r", "").Split('\n'))
                        string[] codeLines = page.MySyntaxEditor.Text.GetLines();
                        foreach (string line in codeLines)
                        {
                            lineNr++;
                            string actLine = line;
                            string actLineComment = "";
                            int commentPos = line.IndexOf("//");
                            if (commentPos >= 0)
                            {
                                actLine = line.Substring(0, commentPos);
                                actLineComment = line.Substring(commentPos);
                            }

                            bool found = false;
                            foreach (Match match in findFullWordOutsideQuotes.Matches(actLine))
                            {
                                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                                {
                                    found = true;
                                    findList.Add(new FindResult() { Page = page, Editor = page.MySyntaxEditor, Code = line, LineNr = lineNr, StartCharacter = match.Index, EndCharacter = match.Index + findTerm.Length });
                                }
                            }

                            if (found && replaceText != null)
                            {
                                //actLine = findFullWordOutsideQuotes.Replace(actLine, replaceText);
                                actLine = findFullWordOutsideQuotes.Replace(actLine, match => ReplaceOutsideQuotesEvaluator(match, replaceText)); //new MatchEvaluator(ReplaceIdentifierOutsideQuotesEvaluator));
                                codeLines[lineNr - 1] = actLine + actLineComment;
                            }
                        }
                        if (replaceText != null)
                        {
                            if (page == ActiveSyntaxEditorDocumentWindow.Page)
                            {
                                var syntaxEditor = ActiveSyntaxEditor;
                                //var firstVisiblePosition = codeEditor.ActiveView.VisibleViewLines[0].VisibleStartPosition;
                                var scrollState = syntaxEditor.ActiveView.ScrollState;
                                var currentSelection = syntaxEditor.ActiveView.Selection.StartOffset;
                                page.MySyntaxEditor.Text = string.Join(Environment.NewLine, codeLines);
                                syntaxEditor.Text = page.MySyntaxEditor.Text;
                                syntaxEditor.ActiveView.Scroller.ScrollVerticallyByLine(scrollState.VerticalAnchorTextPosition.Line);
                                syntaxEditor.ActiveView.Selection.StartOffset = currentSelection;
                                syntaxEditor.Focus();
                            }
                        }
                    }
                }

                listViewFindResults.Items.Clear();
                //tabControl3.SelectedTab = tabPageFindResults;
                toolWindowFindResults.Select();
                if (findList.Count == 0)
                {
                    listViewFindResults.Items.Add($"'{findTerm}' not found...");
                }
                foreach (var findItem in findList)
                {
                    var item = new ListViewItem(new string[] {
                        findItem.Page.FullName, $"{findItem.LineNr},{findItem.StartCharacter}", findItem.Code });
                    item.Tag = findItem;
                    listViewFindResults.Items.Add(item);
                }


            }
        }

        static string ReplaceOutsideQuotesEvaluator(Match m, string replaceText)
        {
            if (m.Groups[1].Success)
            {
                return replaceText;
            }
            else
            {
                return m.ToString();
            }
        }

        void RenameSymbol()
        {
            List<FindResult> findList = new List<FindResult>();
            CSharpContextFactory ctxFactory = new CSharpContextFactory();
            TextSnapshotOffset snapshotOffset = ActiveSyntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            IDotNetContext ctx = ctxFactory.CreateContext(snapshotOffset);
            IResolverResultSet resolverResultSet = ctx.Resolve();
            if (resolverResultSet == null || resolverResultSet.Results.Count == 0)
            {
                //string identifierUnderCaret = "Num1";
                ctx = ctxFactory.CreateContext(snapshotOffset, DotNetContextKind.SelfAndSiblings);
                resolverResultSet = ctx.Resolve();
            }
            if (resolverResultSet != null)
            {
                //foundResult = resolverResultSet.Results.Where(r => r.Kind is ResolverResultKind.TypeMember).FirstOrDefault(r => r.Name == identifierUnderCaret);
                //foundResult = resolverResultSet.Results.FirstOrDefault(r => r.Name == identifierUnderCaret);
                foreach (var resolverResult in resolverResultSet.Results)
                {
                    ISourceFileLocation sourceFileLocation = null;
                    switch (resolverResult)
                    {
                        case INamespaceResolverResult namespaceResolverResult:
                            //sourceFileLocation = namespaceResolverResult.*.SourceFileLocation;
                            break;
                        case IParameterResolverResult parameterResolverResult:
                            sourceFileLocation = parameterResolverResult.Parameter.SourceFileLocation;
                            break;
                        case IVariableResolverResult variableResolverResult:
                            sourceFileLocation = variableResolverResult.Variable.SourceFileLocation;
                            break;
                        case ITypeMemberResolverResult typeMemberResolverResult:
                            sourceFileLocation = typeMemberResolverResult.Member.SourceFileLocation;
                            break;
                        case ITypeResolverResult typeResolverResult:
                            //sourceFileLocation = typeResolverResult.*.SourceFileLocation;
                            break;
                        //case IResolverResult resolverResult:
                        //    //sourceFileLocation = resolverResult.*.SourceFileLocation;
                        //    break;
                        default:
                            break;
                    }

                    if (sourceFileLocation != null)
                    {
                        findList.Add(new FindResult()
                        {
                            SyntaxEditorKey = sourceFileLocation.Key
                            ,
                            Code = "",
                            LineNr = sourceFileLocation.NavigationOffset ?? -1
                            ,
                            StartCharacter = sourceFileLocation.TextRange.StartOffset
                            ,
                            EndCharacter = sourceFileLocation.TextRange.EndOffset
                        });
                    }
                }
            }

            listViewFindResults.Items.Clear();
            //tabControl3.SelectedTab = tabPageFindResults;
            toolWindowFindResults.Select();
            if (findList.Count == 0)
            {
                string findTerm = "xxx";
                listViewFindResults.Items.Add(new ListViewItem(new string[] { "", "", $"'{findTerm}' not found..." }));
            }
            foreach (var findItem in findList)
            {
                var page = qbook.Core.ThisBook.Main.Objects.FirstOrDefault(o => o.CsCodeSourceFileKey == findItem.SyntaxEditorKey);
                string pageName = "?";
                int lineNr = -1;
                int startCharacter = -1;
                int endCharacter = -1;
                string lineText = "";
                if (page != null)
                {
                    pageName = page?.FullName;
                    var editor = page.MySyntaxEditor;

                    var pos = editor.ActiveView.OffsetToPosition(findItem.StartCharacter);
                    lineNr = pos.Line + 1;
                    startCharacter = pos.Character;
                    pos = editor.ActiveView.OffsetToPosition(findItem.EndCharacter);
                    endCharacter = pos.Character;

                    lineText = editor.ActiveView.GetViewLine(findItem.StartCharacter).Text;

                    findItem.Page = page;
                    findItem.LineNr = lineNr;
                    findItem.StartCharacter = startCharacter;
                    findItem.EndCharacter = endCharacter;

                    //var editForm = EditObjectForm.Edit(0, 0, page);
                    //editForm?.SetCsScriptEditorCaretPosition(findItem.StartCharacter, findItem.EndCharacter);
                }
                var item = new ListViewItem(new string[] {
                        pageName, $"{lineNr},{startCharacter}", lineText });
                item.Tag = findItem;
                listViewFindResults.Items.Add(item);
            }
        }

        void GotoDefinition2()
        {
            string subCodeKey = null;
            List<FindResult> findList = new List<FindResult>();
            CSharpContextFactory ctxFactory = new CSharpContextFactory();
            TextSnapshotOffset snapshotOffset = ActiveSyntaxEditor.ActiveView.Selection.EndSnapshotOffset;
            IDotNetContext ctx = ctxFactory.CreateContext(snapshotOffset);
            IResolverResultSet resolverResultSet = ctx.Resolve();
            IResolverResult foundResult = null;
            if (resolverResultSet != null)
            {
                foreach (var resolverResult in resolverResultSet.Results)
                {
                    ISourceFileLocation sourceFileLocation = null;
                    switch (resolverResult)
                    {
                        case INamespaceResolverResult namespaceResolverResult:
                            //sourceFileLocation = namespaceResolverResult.*.SourceFileLocation;
                            break;
                        case IParameterResolverResult parameterResolverResult:
                            sourceFileLocation = parameterResolverResult.Parameter.SourceFileLocation;
                            break;
                        case IVariableResolverResult variableResolverResult:
                            sourceFileLocation = variableResolverResult.Variable.SourceFileLocation;
                            break;
                        case ITypeMemberResolverResult typeMemberResolverResult:
                            sourceFileLocation = typeMemberResolverResult.Member.SourceFileLocation;
                            break;
                        case ITypeResolverResult typeResolverResult:
                            //sourceFileLocation = typeResolverResult.*.SourceFileLocation;
                            break;
                        //case IResolverResult resolverResult:
                        //    //sourceFileLocation = resolverResult.*.SourceFileLocation;
                        //    break;
                        default:
                            break;
                    }

                    if (sourceFileLocation != null)
                    {
                        TreeNode myTreeNode = null; //TODO: find matching treenode
                        var page = qbook.Core.ThisBook.Main.Objects.FirstOrDefault(o => o.CsCodeSourceFileKey == sourceFileLocation.Key);
                        if (page != null)
                        {
                            //var editForm = EditObjectForm.Edit(0, 0, page);
                            //editForm?.SetCsScriptEditorCaretOffset(sourceFileLocation.TextRange.StartOffset, sourceFileLocation.TextRange.EndOffset);
                            SyntaxEditorDocumentWindow docWindow = null;
                            if (radioButtonLangPython.Checked)
                                docWindow = ShowCodeEditorDocument(page as qbook.oPage, myTreeNode, "py", subCodeKey, false);
                            else
                                docWindow = ShowCodeEditorDocument(page as qbook.oPage, myTreeNode, "cs", subCodeKey, false);
                            docWindow.Activate();
                            docWindow.SetCsScriptEditorCaretOffset(sourceFileLocation.TextRange.StartOffset, sourceFileLocation.TextRange.EndOffset);
                        }
                    }
                }

            }
        }

        private void errorListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = errorListView.HitTest(e.X, e.Y).Item;
            if (item != null)
            {
                var error = item.Tag as IParseError;
                if (error != null)
                {
                    if (error.PositionRange.StartPosition.Line < 0)
                    {
                        //somewhere in the header...
                    }
                    else
                    {
                        ActiveSyntaxEditor.ActiveView.Selection.StartPosition = error.PositionRange.StartPosition;
                        ActiveSyntaxEditor.Focus();
                    }
                }
            }

        }

        private void RefreshErrorList(IEnumerable<IParseError> errors)
        {
            errorListView.Items.Clear();

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    var item = new ListViewItem(new string[] {
                        error.PositionRange.StartPosition.DisplayLine.ToString(), error.PositionRange.StartPosition.DisplayCharacter.ToString(), error.Description
                    });
                    item.Tag = error;
                    errorListView.Items.Add(item);
                }

            }
        }

        private void renameSymbolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindAllReferencesInCode("$$$askUserForReplaceText");
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.OutliningManager.ExpandAllOutlining(); //.Outlining.RootNode.ExpandDescendants();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.OutliningManager.CollapseToDefinitions();
        }

        private void showDebuggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowCsScriptingForm();
        }

        private void showLogConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }

        string _oldNodeLabelText = null;
        private void treeViewCodePages_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            _oldNodeLabelText = e.Node?.Text;
        }

        private void treeViewCodePages_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
                return;

            string newLabel = e.Label;
            if (string.IsNullOrEmpty(newLabel))
            {
                MessageBox.Show("Entered name is empty");
                return;
            }

            if (e.Node != null && e.Node.Tag is qbook.oPage)
            {
                var page = e.Node.Tag as qbook.oPage;
                if (page != null)
                {
                    if (e.Node.ImageIndex == idxPage || e.Node.ImageIndex == idxPageHidden)
                    {
                        string newFullName = null;
                        int index = page.FullName.LastIndexOf(".");
                        if (index == -1)
                            newFullName = e.Label.Trim();
                        else
                            newFullName = page.FullName.Substring(0, index) + e.Label.Trim();

                        //TODO: ensure name is unique
                        if (qbook.Core.ThisBook.Main.Objects.Count(i => i.FullName == newFullName) > 0)
                        {
                            MessageBox.Show($"A page with the name '{newFullName}' already exists.\r\nPlease choose a different name."
                                , "PAGE NAME EXISTS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            e.CancelEdit = true;
                            e.Node.BeginEdit();
                            return;
                        }
                        else
                        {
                            e.Node.EndEdit(false);
                            if (SelectedPage != null && e.Label.Trim() != origNodeText)
                            {
                                SelectedPage.Name = e.Label.Trim();
                                //TODO: change @class_<page> in page.CsCode and (if open) also in the SyntaxEditorDocument
                                var csCodeLines = page.CsCode.Replace("\r", "").Split('\n');
                                for (int i = 0; i < csCodeLines.Length; i++)
                                {
                                    string line = csCodeLines[i];
                                    if (classNameRegex.IsMatch(line))
                                    {
                                        Match m = classNameRegex.Match(line);
                                        int nameStartIndex = 0;
                                        int nameEndIndex = 0;
                                        if (m.Groups["name"].Length > 0)
                                        {
                                            nameStartIndex = m.Groups["name"].Index;
                                            nameEndIndex = nameStartIndex + m.Groups["name"].Length;
                                        }
                                        else //no name yet? add it after @class_
                                        {
                                            nameStartIndex = line.IndexOf("@class_") + 7;
                                            nameEndIndex = nameStartIndex;
                                        }
                                        string newLine = line.Substring(0, nameStartIndex) + newFullName.Replace('.', '_') + line.Substring(nameEndIndex);
                                        csCodeLines[i] = newLine;
                                        page.CsCode = string.Join("\n", csCodeLines);
                                        break;
                                    }
                                }
                                foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>().ToList())
                                {
                                    if (doc.Page == page)
                                    {
                                        var currentSelection = doc.SyntaxEditor.ActiveView.Selection;
                                        doc.SyntaxEditor.Text = page.CsCode;
                                        //try
                                        //{
                                        //	doc.SyntaxEditor.ActiveView.Selection.StartPosition = currentSelection.StartPosition;
                                        //	doc.SyntaxEditor.ActiveView.Selection.EndPosition = currentSelection.EndPosition;
                                        //} 
                                        //catch (Exception ex) { }
                                        break;
                                    }
                                }
                                //PopulatePageTreeView();
                            }
                            qbook.Core.ThisBook.Modified = true;
                            return;
                        }
                    }

                    if (e.Node.ImageIndex == idxCsCode)
                    {
                        //TODO: ensure name is unique
                        if (page.CsCodeExtra.FirstOrDefault(c => c.Key == newLabel).Key != null)
                        {
                            MessageBox.Show("Duplicate Key");
                            e.CancelEdit = true;
                            e.Node.BeginEdit();
                            return;
                        }

                        if (_oldNodeLabelText != null && page.CsCodeExtra.ContainsKey(_oldNodeLabelText))
                        {
                            //rename
                            string code = page.CsCodeExtra[_oldNodeLabelText];
                            page.CsCodeExtra.Remove(_oldNodeLabelText);
                            page.CsCodeExtra.Add(newLabel, code);
                            qbook.Core.ThisBook.Modified = true;

                        }
                    }

                    if (e.Node.ImageIndex == idxPyCode)
                    {
                        //TODO: ensure name is unique
                        if (page.PyCodeExtra.FirstOrDefault(c => c.Key == newLabel).Key != null)
                        {
                            MessageBox.Show("Duplicate Key");
                            e.CancelEdit = true;
                            e.Node.BeginEdit();
                            return;
                        }

                        if (_oldNodeLabelText != null && page.PyCodeExtra.ContainsKey(_oldNodeLabelText))
                        {
                            //rename
                            string code = page.PyCodeExtra[_oldNodeLabelText];
                            page.PyCodeExtra.Remove(_oldNodeLabelText);
                            page.PyCodeExtra.Add(newLabel, code);
                        }
                    }

                    return;
                }
            }
        }

        TreeNode _lastClickedNode = null;
        private void treeViewCodePages_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node.Level == 1)
            {
                _lastClickedNode = e.Node;
                //var clickedNode = ((TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                newCsCodeToolStripMenuItem.Visible = true;
                newPyCodeToolStripMenuItem.Visible = true;
                toolStripMenuItemSeparator1.Visible = false;
                deleteCodeClassToolStripMenuItem.Visible = false;
                hidePageToolStripMenuItem.Visible = true;
                if (_lastClickedNode.Tag is qbook.oPage)
                    hidePageToolStripMenuItem.Checked = (_lastClickedNode.Tag as qbook.oPage).Hidden;
                contextMenuPageTree.Show(treeViewCodePages, e.Location);
            }
            else if (e.Button == MouseButtons.Right && e.Node.Level == 2)
            {
                _lastClickedNode = e.Node;
                //_lastClickedNode = e.Node;
                //var clickedNode = ((TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                newCsCodeToolStripMenuItem.Visible = false;
                newPyCodeToolStripMenuItem.Visible = false;
                toolStripMenuItemSeparator1.Visible = false;
                deleteCodeClassToolStripMenuItem.Visible = true;
                hidePageToolStripMenuItem.Visible = false;
                contextMenuPageTree.Show(treeViewCodePages, e.Location);
            }
            else
            {
                _lastClickedNode = e.Node;
            }
        }


        private void deleteCodeClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_lastClickedNode == null)
                return;

            string codeKind = "n/a";
            if (_lastClickedNode.ImageIndex == idxCsCode)
                codeKind = "C# Code";
            if (_lastClickedNode.ImageIndex == idxPyCode)
                codeKind = "Python Code";
            var dr = MessageBox.Show($"Delete {codeKind} '{_lastClickedNode.Text}'?", "DELETE CODE", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.OK)
            {
                if (_lastClickedNode.Tag is qbook.oPage)
                {
                    if (_lastClickedNode.ImageIndex == idxCsCode)
                    {
                        qbook.oPage page = _lastClickedNode.Tag as qbook.oPage;
                        if (page.CsCodeExtra.ContainsKey(_lastClickedNode.Text))
                            page.CsCodeExtra.Remove(_lastClickedNode.Text);
                    }
                    if (_lastClickedNode.ImageIndex == idxPyCode)
                    {
                        qbook.oPage page = _lastClickedNode.Tag as qbook.oPage;
                        if (page.PyCodeExtra.ContainsKey(_lastClickedNode.Text))
                            page.PyCodeExtra.Remove(_lastClickedNode.Text);
                    }
                }
                _lastClickedNode.Remove();

                foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>().ToList())
                {
                    if (doc.Page == _lastClickedNode.Tag)
                        doc.Close();
                }

                qbook.Core.ThisBook.Modified = true;
            }
        }

        private void toolStripButtonHeaderFooter_Click(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditorDocumentWindow == null)
                return;

            bool visible = toolStripButtonShowFullCode.Checked;

            textBoxHeader.Visible = visible;
            textBoxFooter.Visible = visible;
            buttonDefaultHeader.Visible = visible;
            buttonDefaultFooter.Visible = visible;

            if (visible)
            {
                textBoxHeader.Text = ActiveSyntaxEditorDocumentWindow.Page.CsCodeHeader?.Replace("\r", "").Replace("\n", "\r\n"); //HACK: \n -> \r\n (why is the \r lost?)
                textBoxFooter.Text = ActiveSyntaxEditorDocumentWindow.Page.CsCodeFooter?.Replace("\r", "").Replace("\n", "\r\n"); //HACK: \n -> \r\n

                textBoxHeader.BringToFront();
                buttonDefaultHeader.BringToFront();
                textBoxFooter.BringToFront();
                buttonDefaultFooter.BringToFront();
            }
            else
            {
                ActiveSyntaxEditorDocumentWindow.Page.CsCodeHeader = textBoxHeader.Text;
                ActiveSyntaxEditorDocumentWindow.Page.CsCodeFooter = textBoxFooter.Text;
            }


            qbook.Core.UpdateProjectAssemblyQbRoot("toolStripButtonHeaderFooter_Click");
        }

        private void toolStripTextBoxPageName_Validated(object sender, EventArgs e)
        {

        }

        string origNodeText = "";
        private void toolStripTextBoxPageName_Enter(object sender, EventArgs e)
        {
            origNodeText = SelectedPage?.Name;
        }

        private void treeViewCodePages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (treeViewCodePages.SelectedNode != null && treeViewCodePages.SelectedNode.Tag is qbook.oPage)
                {
                    //page-node
                    treeViewCodePages.LabelEdit = true;
                    treeViewCodePages.SelectedNode.BeginEdit();
                }

                if (treeViewCodePages.SelectedNode != null && treeViewCodePages.SelectedNode.Parent?.Tag is qbook.oPage)
                {
                    //subcode-node
                    treeViewCodePages.LabelEdit = true;
                    treeViewCodePages.SelectedNode.BeginEdit();
                }
            }
        }

        string keySequence = "";
        private void FormCodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (keySequence == "k")
            {
                if (e.KeyCode == Keys.C)
                {
                    ActiveSyntaxEditor.ActiveView.TextChangeActions.CommentLines();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.U)
                {
                    ActiveSyntaxEditor.ActiveView.TextChangeActions.UncommentLines();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F)
                {
                    if (string.IsNullOrEmpty(ActiveSyntaxEditor.ActiveView.SelectedText))
                        ActiveSyntaxEditor.ActiveView.TextChangeActions.FormatDocument();
                    else
                        ActiveSyntaxEditor.ActiveView.TextChangeActions.FormatSelection();
                    e.Handled = true;
                }
                else
                {
                    keySequence = "";
                }
                return;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.K && keySequence == "")
            {
                keySequence = "k";
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.B)
            {
                //build
                DoRebuild();
                e.Handled = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.S)
            {
                //save
                SaveAllCode();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                //run
                _ = DoRun();
                e.Handled = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.F)
            {
                //find in whole project
                DoFindTextInCode();
                e.Handled = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.L)
            {
                qbook.Core.ShowLogForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                //find next
                if (lastActiveToolWindow == toolWindowFindResults)
                {
                    if (listViewFindResults.SelectedItems.Count > 0 && listViewFindResults.SelectedItems[0].Index < listViewFindResults.Items.Count - 1)
                    {

                        ListViewItem lvi = listViewFindResults.Items[listViewFindResults.SelectedItems[0].Index + 1];
                        listViewFindResults.SelectedItems.Clear();
                        lvi.Selected = true;
                        lvi.EnsureVisible();
                        GotoFindItem(lvi, false);
                    }
                    else
                    {
                        if (listViewFindResults.Items.Count > 0)
                        {
                            ListViewItem lvi = listViewFindResults.Items[0];
                            listViewFindResults.SelectedItems.Clear();
                            lvi.Selected = true;
                            lvi.EnsureVisible();
                            GotoFindItem(lvi, false);
                        }
                    }
                }
                if (lastActiveToolWindow == toolWindowOutput)
                {
                    int lineCount = textBoxOutput.Lines.TakeWhile(l => l!=null && l.Trim().Length > 0).Count();
                    int currentLine = -1;
                    if (e.Modifiers == Keys.Shift) //up
                    {
                        currentLine = textBoxOutput.GetLineFromCharIndex(textBoxOutput.SelectionStart);
                        if (currentLine > 1) 
                            currentLine--;
                        else
                            currentLine = lineCount - 1;
                    }
                    else //down
                    {
                        currentLine = textBoxOutput.GetLineFromCharIndex(textBoxOutput.SelectionStart);
                        if (currentLine < lineCount - 1) //down
                            currentLine++;
                        else
                            currentLine = 1;
                    }
                    if (currentLine != -1)
                    {
                        int lineStartIndex = textBoxOutput.GetFirstCharIndexFromLine(currentLine);
                        int lineLength = textBoxOutput.Lines[currentLine].Length;
                        textBoxOutput.Select(lineStartIndex, lineLength);
                        textBoxOutput.ScrollToCaret();
                        string errorText = textBoxOutput.Lines[textBoxOutput.GetLineFromCharIndex(textBoxOutput.SelectionStart)];
                        HighlightError(errorText, highlightWord: false, selectWord: true);
                    }
                }
                if (lastActiveToolWindow == toolWindowErrors)
                {
                }
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.E)
            {
                //this.SendToBack();
                Application.OpenForms[0].BringToFront();
                e.Handled = true;
            }


            if (ModifierKeys != Keys.Control)
                keySequence = "";
        }

        private void DoFindTextInCode()
        {
            string text = "";
            if (Clipboard.ContainsText())
                text = Clipboard.GetText().Replace("\r", "").Split('\n')[0];
            var dr = QB.UI.InputDialog.ShowDialog("FIND IN CODE", "Find Text:\r\n(prefix find text with #rx# to use RegeEx)", ref text);
            if (dr == DialogResult.OK)
            {
                if (text.TrimStart().StartsWith("#rx#"))
                {
                    text = text.Trim().Substring(4);
                    FindTextInCode(text, true);
                }
                else
                {
                    FindTextInCode(text, false);
                }
            }
        }

        void FindTextInCode(string findTerm, bool useRegex = false)
        {
            List<FindResult> findList = new List<FindResult>();
            foreach (var page in qbook.Core.ThisBook.Main.Objects.OfType<qbook.oPage>())
            {
                string[] codeLines = page.CsCode.GetLines(); // .Replace("\r", "").Split('\n');
                int lineNr = 0;
                Regex findRegex = null;
                if (useRegex)
                    findRegex = new Regex(findTerm, RegexOptions.IgnoreCase);
                foreach (string line in codeLines)
                {
                    lineNr++;
                    if (findRegex != null)
                        foreach (Match match in findRegex.Matches(line))
                            findList.Add(new FindResult() { Page = page, SubCodeKey = null, Editor = null, Code = line, LineNr = lineNr, StartCharacter = match.Index, EndCharacter = match.Index + findTerm.Length });
                    else
                    {
                        string lineLower = line.ToLower();
                        string findTermLower = findTerm.ToLower();
                        int idx = 0;
                        while (idx < line.Length)
                        {
                            int findPos = lineLower.IndexOf(findTermLower, idx);
                            if (findPos >= 0)
                            {
                                findList.Add(new FindResult() { Page = page, SubCodeKey = null, Editor = null, Code = line, LineNr = lineNr, StartCharacter = findPos, EndCharacter = findPos + findTerm.Length });
                                idx = findPos + 1;
                            }
                            else
                            {
                                idx = line.Length;
                            }
                        }
                    }
                }
                foreach (var subCode in page.CsCodeExtra)
                {
                    lineNr = 0;
                    foreach (string line in subCode.Value.GetLines())
                    {
                        lineNr++;
                        if (findRegex != null)
                            foreach (Match match in findRegex.Matches(line))
                                findList.Add(new FindResult() { Page = page, SubCodeKey = subCode.Key, Editor = null, Code = line, LineNr = lineNr, StartCharacter = match.Index, EndCharacter = match.Index + findTerm.Length });
                        else
                        {
                            string lineLower = line.ToLower();
                            string findTermLower = findTerm.ToLower();
                            int idx = 0;
                            while (idx < line.Length)
                            {
                                int findPos = lineLower.IndexOf(findTermLower, idx);
                                if (findPos >= 0)
                                {
                                    findList.Add(new FindResult() { Page = page, SubCodeKey = subCode.Key, Editor = null, Code = line, LineNr = lineNr, StartCharacter = findPos, EndCharacter = findPos + findTerm.Length });
                                    idx = findPos + 1;
                                }
                                else
                                {
                                    idx = line.Length;
                                }
                            }
                        }
                    }
                }

            }
            listViewFindResults.Items.Clear();
            //toolWindowFindResults.Select();
            toolWindowFindResults.Activate();
            if (findList.Count == 0)
            {
                listViewFindResults.Items.Add(new ListViewItem(new string[] { "", "", $"'{findTerm}' not found..." }));
            }

            int maxFindResults = 100;
            if (findList.Count > maxFindResults)
            {
                var infoResult = new FindResult() { Page = null, SubCodeKey = null, Editor = null, Code = $"### find results for '{findTerm.Left(20, "...")}' limited to first {maxFindResults} enties ###", LineNr = 0, StartCharacter = 0, EndCharacter = 0 };
                findList = findList.Take(maxFindResults).ToList();
                findList.Insert(0, infoResult);
            }
            foreach (var findItem in findList)
            {
                string source = findItem.Page?.FullName;
                if (findItem.SubCodeKey != null)
                    source += "." + findItem.SubCodeKey;
                var item = new ListViewItem(new string[] {
                        source, $"{findItem.LineNr},{findItem.StartCharacter}", findItem.Code });
                item.Tag = findItem;
                listViewFindResults.Items.Add(item);
            }
        }

        ScriptEngine pyEngine = null;
        private void toolStripButtonTestPython_Click(object sender, EventArgs e)
        {
            dynamic scope;
            try
            {
                textBoxOutput.Clear();
                SetOutputColor();
                OutputWriteLine("Running python code...");

                if (ActiveSyntaxEditor.Document.Language is PythonSyntaxLanguage)
                {
                    if (pyEngine == null)
                        pyEngine = Python.CreateEngine();
                    scope = pyEngine.CreateScope();
                    pyEngine.Execute(ActiveSyntaxEditor.Text, scope);

                    OutputWriteLine("Python code executed successfully!");
                    SetStatusText("Python code executed successfully!");
                    OutputWriteScrollToLine(0);
                }
                else
                {
                    MessageBox.Show("not a python-code");
                }
            }
            catch (Microsoft.Scripting.SyntaxErrorException ex)
            {
                this.EnsureBeginInvoke(() =>
                {
                    SetStatusText("#ERR: executing python-code", Color.Red);
                    SetOutputColor(Color.LightCoral);
                    toolWindowOutput.Select();
                    OutputWriteLine("--- ERRORS ---\r\n" + ex.Message + " (" + ex.GetType().Name + ")");
                    OutputWriteScrollToLine(0);
                });

                var docWindow = ActiveSyntaxEditorDocumentWindow;
                docWindow.Focus();
                docWindow.SetCsScriptEditorCaretLineCharacter(ex.RawSpan.Start.Line - 1, ex.RawSpan.Start.Column, ex.RawSpan.End.Line - 1, ex.RawSpan.End.Column);
            }
            catch (Exception ex)
            {
                //if (ex is IronPython.Runtime.UnboundNameException)
                {
                    if (false)
                    {
                        var exInfo = pyEngine.GetService<ExceptionOperations>().FormatException(ex);
                        MessageBox.Show(exInfo, "EX: " + ex.GetType().FullName);
                    }

                    if (true)
                    {
                        var stackFrames = PythonOps.GetDynamicStackFrames(ex);
                        //StringBuilder sb = new StringBuilder();
                        //foreach (var frame in stackFrames)
                        //{
                        //	sb.Append(frame.ToString());
                        //}

                        //SyntaxEditorDocumentWindow docWindow = null;
                        //if (radioButtonLangPython.Checked)
                        //    docWindow = ShowCodeEditorDocument(page as Main.oPage, "py", subCodeKey, false);
                        //else
                        //    docWindow = ShowCodeEditorDocument(page as Main.oPage, "cs", subCodeKey, false);

                        var exInfo = pyEngine.GetService<ExceptionOperations>().FormatException(ex);
                        //MessageBox.Show(exInfo, "EX: " + ex.GetType().FullName);
                        this.EnsureBeginInvoke(() =>
                        {
                            SetStatusText("#ERR: executing python-code", Color.Red);
                            SetOutputColor(Color.LightCoral);
                            toolWindowOutput.Select();
                            OutputWriteLine("--- ERRORS ---\r\n" + ex.Message + " (" + ex.GetType().Name + ")\r\n" + exInfo);
                            OutputWriteScrollToLine(0);
                        });

                        var docWindow = ActiveSyntaxEditorDocumentWindow;
                        docWindow.Focus();
                        int lineNr = 0;
                        if (stackFrames.Length > 0)
                            lineNr = stackFrames[0].GetFileLineNumber() - 1;
                        docWindow.SetCsScriptEditorCaretLineCharacter(lineNr, 0, lineNr, 999);

                    }

                    //Func<PythonTuple> exc_info = pyEngine.Operations.GetMember<Func<PythonTuple>>(pyEngine.GetSysModule(), "exc_info");
                    //TraceBack tb = (TraceBack)exc_info()[2];


                }
                //else
                //{
                //	MessageBox.Show(ex.ToString(), "ERROR");
                //}
            }
        }

        private void showWhitespacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.IsWhitespaceVisible = true;
        }

        private void toolStripTextBoxPageText_MouseLeave(object sender, EventArgs e)
        {
        }

        private void toolStripDropDownButtonOpen_Click(object sender, EventArgs e)
        {
            //Main.Qb.ShowOpenQbookFileDialog(sender);
        }

        private void toolStripDropDownButtonOpen_DropDownOpening(object sender, EventArgs e)
        {
            toolStripDropDownButtonOpen.DropDownItems.Clear();
            List<MruFilesManager.MruItem> mruItems = Core.MruFilesManager.MruItems; //.GetMruStringList();

            ToolStripMenuItemEx[] items = new ToolStripMenuItemEx[mruItems.Count]; // You would obviously calculate this value at runtime
            for (int i = 0; i < items.Length; i++)
            {
                var mruItem = mruItems[i];

                items[i] = new ToolStripMenuItemEx();
                items[i].Name = mruItem.FullPath;
                items[i].Tag = null;

                string qbookname = mruItem.FullPath.GetFileNameWithoutExtension();
                string directory = mruItem.FullPath.GetDirName();
                string lastModifiedStr = "xxxx-xx-xx xx:xx:xx";
                if (mruItem.LastModified != null)
                    lastModifiedStr = mruItem.LastModified?.ToString("yyyy-MM-dd HH:mm:ss");
                items[i].Text = (mruItem.FileExists ? "" : "[x] ") + qbookname + " @" + lastModifiedStr + " [" + directory + "\\]";
                items[i].MouseUp += MruItemClicked;
                items[i].DeleteItemClicked += MruItemDeleteIconClicked;
            }
            toolStripDropDownButtonOpen.DropDownItems.AddRange(items);

            ////open qbook menuitem
            //contextMenuMru.Items.Add(new ToolStripSeparator());
            //var openMenuItem = new ToolStripMenuItem("Open qbook...");
            //openMenuItem.MouseUp += MruItemOpen;
            //contextMenuMru.Items.Add(openMenuItem);


            //if (false && contextMenuMru.Items.Count > 0)
            //{
            //    contextMenuMru.Items.Add(new ToolStripSeparator());
            //    var clearMruListMenuItem = new ToolStripMenuItem("Clear MRU List");
            //    clearMruListMenuItem.MouseUp += MruItemClearList;
            //    contextMenuMru.Items.Add(clearMruListMenuItem);
            //}

            //contextMenuMru.Show(Cursor.Position);
        }

        private void MruItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            if (!System.IO.File.Exists(clickedItem.Name))
            {
                var dr = MessageBox.Show($"The file:\r\n\r\n{clickedItem.Name}\r\n\r\ncannot be found.\r\n\r\nRemove from List?", "QBOOK NOT FOUND", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    qbook.Core.MruFilesManager.Remove(clickedItem.Name);
                    qbook.Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                    qbook.Properties.Settings.Default.Save();
                }
                return;
            }
            else
            {
                //MessageBox.Show($"nyi: {clickedItem.Name}", "OPEN");
                qbook.Core.OpenQbookAsync(clickedItem.Name);
                if (qbook.Core.ThisBook != null)
                {
                    try
                    {
                        Application.OpenForms[0].Bounds = qbook.Core.ThisBook.Bounds;
                    }
                    catch { }
                }
                ReInit();
            }
        }

        private void MruItemDeleteIconClicked(object sender, ToolStripMenuItemEx.DeleteItemClickedEventArgs e)
        {
            string filename = (sender as ToolStripMenuItem)?.Name;
            if (filename != null)
            {
                qbook.Core.MruFilesManager.Remove(filename);
                qbook.Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                qbook.Properties.Settings.Default.Save();

                List<ToolStripItem> itemsToRemove = new List<ToolStripItem>();
                foreach (ToolStripItem item in toolStripDropDownButtonOpen.DropDownItems)
                {
                    if (item.Name == filename)
                        itemsToRemove.Add(item);
                }
                foreach (var item in itemsToRemove)
                    toolStripDropDownButtonOpen.DropDownItems.Remove(item);
            }
        }

        private void toolStripButtonShowFullCode_Click(object sender, EventArgs e)
        {
            string usingsCode = qbook.Core.GetUsingsCode();
            string csScriptCode = qbook.Core.CsScriptCombineCode();
            string fullCode = usingsCode + "\r\n" + qbook.Core.ProgramMainCode + csScriptCode;

            string tempFilename = Path.GetTempFileName() + ".cs";
            File.WriteAllText(tempFilename, fullCode);

            Process.Start(tempFilename);
        }

        private void radioButtonLangPython_CheckedChanged(object sender, EventArgs e)
        {
            var selectedNode = treeViewCodePages.SelectedNode;
            treeViewCodePages.SelectedNode = null;
            treeViewCodePages.SelectedNode = selectedNode;
        }

        private void radioButtonLangCS_CheckedChanged(object sender, EventArgs e)
        {
            //handled in radioButtonLangPython_CheckedChanged()
        }
        private void newCsCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newNode = new System.Windows.Forms.TreeNode("newCs", idxCsCode, idxCsCode);
            newNode.Tag = _lastClickedNode.Tag;
            _lastClickedNode.Nodes.Add(newNode);
            _lastClickedNode.ExpandAll();
            treeViewCodePages.LabelEdit = true;
            newNode.BeginEdit();
        }

        private void newPyCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newNode = new System.Windows.Forms.TreeNode("newPy", idxPyCode, idxPyCode);
            newNode.Tag = _lastClickedNode.Tag;
            _lastClickedNode.Nodes.Add(newNode);
            _lastClickedNode.ExpandAll();
            treeViewCodePages.LabelEdit = true;
            newNode.BeginEdit();
        }

        string _lastGetPageTreeString = "";
        bool skipNextCheckPageTreeStringChanged = false;
        private void timerIdle_Tick(object sender, EventArgs e)
        {
            if (qbook.Core.ThisBook.Modified)
            {
                //toolStripButtonSave.Enabled = true;
                toolStripButtonSave.BackColor = Color.Salmon;
            }
            else
            {
                //toolStripButtonSave.Enabled = false;
                toolStripButtonSave.BackColor = SystemColors.Control;
            }

            if (skipNextCheckPageTreeStringChanged)
            {
                skipNextCheckPageTreeStringChanged = false;
            }
            else
            {
                string pageTreeString = GetPageTreeString();
                if (pageTreeString != _lastGetPageTreeString)
                {
                    //PopulatePageTreeView(false, true);
                    _lastGetPageTreeString = pageTreeString;
                }
            }
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            SaveAllCode();
        }

        private void SaveAllCode()
        {
            foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>())
                doc.SaveCode();

          //  qbook.Core.ThisBook.Serialize();
            qbook.Core.ThisBook.Modified = false;
            UpdateStatusVersion();
        }

        private void debugScriptInitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var script = new DebugScript.QbScript();
            //script._InitClasses(null);
        }

        private void listViewFindResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listViewFindResults.HitTest(e.X, e.Y).Item;
            if (item != null)
            {
                GotoFindItem(item, true);
            }
        }

        private void GotoFindItem(ListViewItem item, bool activateDocWindow = false)
        {
            var findItem = item.Tag as FindResult;
            if (findItem != null)
            {
                var docWindow = ShowCodeEditorDocument(findItem.Page as qbook.oPage, null, subCodeKey: findItem.SubCodeKey);
                if (activateDocWindow)

                    docWindow.Activate();
                docWindow?.SetCsScriptEditorCaretLineCharacter(findItem.LineNr - 1, findItem.StartCharacter, findItem.LineNr - 1, findItem.EndCharacter);
            }
        }

        private void toolStripButtonFormat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ActiveSyntaxEditor.ActiveView.SelectedText))
                ActiveSyntaxEditor.ActiveView.TextChangeActions.FormatDocument();
            else
                ActiveSyntaxEditor.ActiveView.TextChangeActions.FormatSelection();
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.UndoHistory.Undo();
        }

        private void toolStripButtonRedo_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.UndoHistory.Redo();
        }

        private void toolStripButtonComment_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.TextChangeActions.CommentLines();
        }

        private void toolStripButtonUncomment_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.TextChangeActions.UncommentLines();
        }

        private void toolStripButtonFind_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Focus();
            SendKeys.Send("^f"); //Strl+F
        }

        private void toolStripButtonFindInCode_Click(object sender, EventArgs e)
        {
            DoFindTextInCode();
        }

        private void toolStripButtonExpand_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.OutliningManager.ExpandAllOutlining(); //.Outlining.RootNode.ExpandDescendants();
        }

        private void toolStripButtonCollapse_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.Document.OutliningManager.CollapseToDefinitions();
        }

        private void toolStripButtonShowWhitespaces_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.IsWhitespaceVisible = toolStripButtonShowWhitespaces.Checked;
        }

        private void toolStripButtonShowLogger_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }

        private void toolStripButtonCut_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.CutToClipboard();
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.CopyToClipboard();
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.PasteFromClipboard();
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.SelectedText = String.Empty;
        }

        private void attachDebuggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var debuggerFilename = System.IO.Path.GetFullPath(System.IO.Path.Combine("dnSpy", "dnSpy-x86.exe"));
            if (System.IO.File.Exists(debuggerFilename))
            {
                List<string> args = new List<string>();
                if (!string.IsNullOrEmpty(qbook.Core.LastScriptDllFilename))
                    args.Add("\"" + qbook.Core.LastScriptDllFilename + "\"");
                args.Add("--pid " + Process.GetCurrentProcess().Id);
                args.Add("--search-for \"Class\"");
                args.Add("--search \"class_" + SelectedPage.Name + "\"");


                System.Diagnostics.Process.Start(debuggerFilename, string.Join(" ", args));
            }
        }

        System.Reflection.Assembly _ScriptDllAssembly = null;
        AppDomain scriptDomain = null;
        private void tESTLoadScriptDllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (false)
            {
                if (_ScriptDllAssembly != null)
                {
                    //System.Reflection.Assembly.LoadFrom(@"C:\Users\HannesLechner\.nuget\packages\system.runtime.loader\4.0.0\lib\netstandard1.5\System.Runtime.Loader.dll");
                    _ScriptDllAssembly = null;
                }

                string assFile = @"E:\amium\GO\GO43.qbook\Main\bin\Debug.VSCodium\Script\ScriptDll\bin\Debug\net472\ScriptDll.dll";
                _ScriptDllAssembly = System.Reflection.Assembly.LoadFrom(assFile);
                dynamic csScript = _ScriptDllAssembly.CreateObject("*");
                //Main.Qb.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?! 
                //Main.Qb.CsScript_Init();
                //Main.Qb.LastBuildResult = null;

                try
                {
                    //SCAN UDL.Client.ResetClients();
                    //Ak.Client.ResetClients();
                    //SCAN  Net.AK.Server.ResetClients();
                    QB.Root.ResetObjectDict();
                    QB.Root.ResetWidgetDict();
                    //SCAN         Main.Qb.Automation.Signalgenerator.SignalGenList.Clear();
                    // System.Threading.Thread.Sleep(1000);

                    //csScript._InitClasses(null);
                    csScript.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("#EX CsScript: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                }
            }

            if (true)
            {
                for (int i = 0; i < 1; i++) //TEST only
                {
                    int assCount1 = AppDomain.CurrentDomain.GetAssemblies().Length;
                    if (scriptDomain != null)
                    {
                        //csScript._DestroyClasses(null);
                        AppDomain.Unload(scriptDomain);
                        scriptDomain = null;
                    }
                    int assCount2 = AppDomain.CurrentDomain.GetAssemblies().Length;
                    /*var*/
                    scriptDomain = AppDomain.CreateDomain("MyAppDomain", null, new AppDomainSetup
                    {
                        ApplicationName = "MyAppDomain",
                        ShadowCopyFiles = "true",
                        PrivateBinPath = "MyAppDomainBin",
                        //LoaderOptimization = LoaderOptimization.MultiDomainHost,
                    });

                    qbook.Core.LastScriptDllFilename = null;
                    var job = (qbook.Core.Job)scriptDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(qbook.Core.Job).FullName);
                    var dllFile = job.DoExternal(qbook.Core.ThisBook);
                    dllFile = @"E:\amium\GO\GO43.qbook\Main\bin\Debug.VSCodium\Script\ScriptDll\bin\Debug\net472\ScriptDll.dll";
                    var pdbFile = Path.ChangeExtension(dllFile, ".pdb");

                    string finalDllFilename = null;
                    string finalPdbFilename = null;
                    bool useDllClone = false;
                    if (useDllClone)
                    {
                        var dllFileTemp = @"E:\amium\GO\GO43.qbook\Main\bin\Debug.VSCodium\Script\ScriptDll\bin\Debug\net472\ScriptDll.temp."
                            + Guid.NewGuid() + ".dll";
                        try
                        {
                            if (File.Exists(dllFileTemp))
                                File.Delete(dllFileTemp);
                            File.Copy(dllFile, dllFileTemp);
                        }
                        catch
                        {
                            MessageBox.Show($"could not copy\r\n{dllFile}\r\nto\r\n{dllFileTemp}", "ERROR");
                        }

                        var pdbFileTemp = Path.ChangeExtension(dllFileTemp, ".pdb");
                        try
                        {
                            if (File.Exists(pdbFileTemp))
                                File.Delete(pdbFileTemp);
                            File.Copy(pdbFile, pdbFileTemp);
                        }
                        catch
                        {
                            MessageBox.Show($"could not copy\r\n{pdbFile}\r\nto\r\n{pdbFileTemp}", "ERROR");
                        }
                        finalDllFilename = dllFileTemp;
                        finalPdbFilename = pdbFileTemp;
                    }
                    else
                    {
                        finalDllFilename = dllFile;
                        finalPdbFilename = pdbFile;
                    }

                    qbook.Core.LastScriptDllFilename = finalDllFilename;
                    //Main.Qb.Book = book;

                    if (true)
                    {
                        int assCount3 = AppDomain.CurrentDomain.GetAssemblies().Length;
                        //Assembly ass2 = Assembly.LoadFrom(finalDllFilename);


                        Assembly ass2 = Assembly.LoadFrom(finalDllFilename);
                        //Assembly ass2 = Assembly.Load(File.ReadAllBytes(finalDllFilename), File.ReadAllBytes(finalPdbFilename));

                        int assCount4 = AppDomain.CurrentDomain.GetAssemblies().Length;

                        Console.WriteLine($"assCount: {assCount1}/{assCount2}/{assCount3}/{assCount4}");


                        dynamic csScript2 = ass2.CreateObject("*");

                        //csScript.Print("foo1");
                        //csScript.Test01();
                        //csScript.ShowDialog("Hello World!");

                        //Main.Qb.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?! 
                        //CsScript_Init();

                        try
                        {
                            //SCAN UDL.Client.ResetClients();
                            //Ak.Client.ResetClients();
                            //SCAN  Net.AK.Server.ResetClients();
                            QB.Root.ResetObjectDict();
                            QB.Root.ResetWidgetDict();
                            //SCAN         Main.Qb.Automation.Signalgenerator.SignalGenList.Clear();
                            // System.Threading.Thread.Sleep(1000);

                            //csScript._InitClasses(null);
                            csScript2.Initialize();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("#EX CsScript: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                        }

                        qbook.Core.LastBuildResult = null;
                    }
                }
            }

        }

        private void toolStripMenuItemDevelop_DropDownOpening(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
                tESTLoadScriptDllToolStripMenuItem.Visible = true;
            else
                tESTLoadScriptDllToolStripMenuItem.Visible = false;
        }


        class ParamItem
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Param { get; set; }
            public object Value { get; set; }
        }



        private void tESTWriteParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //foreach (Main.oPage page in Main.Qb.ActualMain.Objects.Where(item => item is Main.oPage))
                //{
                //    foreach (var o in page.Objects)
                //    {
                //        if (o is Main.Qb.Signal)
                //        {

                //        }
                //    }
                //}
                List<ParamItem> paramList = new List<ParamItem>();
                foreach (QB.Item obj in QB.Root.ObjectDict.Values)
                {
                    //if (obj is Main.Qb.Signal)
                    {
                        //Main.Qb.Signal signal = obj as Main.Qb.Signal;
                        var fields = obj.GetType().GetFields();
                        foreach (var f in fields)
                        {
                            if (f.GetCustomAttributes(false).Contains(BrowsableAttribute.Yes))
                            {
                                var fieldValue = f.GetValue(obj);
                                if (fieldValue is QB.Signal)
                                    paramList.Add(new ParamItem() { Path = obj.Directory, Name = obj.Name, Param = f.Name, Value = (fieldValue as QB.Signal).Value });
                            }
                        }
                    }
                }

                //StringBuilder sb = new StringBuilder();
                //foreach (var param in paramList)
                //{
                //    sb.Append(param.Key + "=" + param.Value + "\r\n");
                //}
                //File.WriteAllText(@"c:\temp\test01.qbook.param", sb.ToString());
                //string json = Newtonsoft.Json.JsonSerializer.Serialize(paramList);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(paramList, Newtonsoft.Json.Formatting.Indented);
                string filename = Path.Combine(qbook.Core.ThisBook.DataDirectory, "params.last.json");
                File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tESTReadParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = Path.Combine(qbook.Core.ThisBook.DataDirectory, "params.last.json");
                var paramString = File.ReadAllText(filename);
                //List<ParamItem> paramList = (List<ParamItem>)Newtonsoft.Json.JsonConvert.DeserializeObject(paramString);
                List<ParamItem> paramList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ParamItem>>(paramString);
                List<string> errorList = new List<string>();
                foreach (ParamItem pi in paramList)
                {
                    //find param and update -> slow! optimize!!!
                    foreach (QB.Item obj in QB.Root.ObjectDict.Values)
                    {
                        if (pi.Path == obj.Directory
                            && pi.Name == obj.Name)
                        {
                            var fields = obj.GetType().GetFields(); //.Where(f => f.GetType() is Main.Qb.Signal);
                            foreach (var f in fields)
                            {
                                if (f.FieldType.FullName == "Main.Qb.Signal" //use (xyz is Main.Qb.Signal)
                                    && f.GetCustomAttributes(false).Contains(BrowsableAttribute.Yes))
                                {
                                    if (pi.Param == f.Name)
                                    {
                                        try
                                        {
                                            if (obj is QB.Signal)
                                            {
                                                //now set .Value of the Main.Qb.Signal
                                                ((QB.Signal)obj.GetType().GetField(f.Name).GetValue(obj)).Value = pi.Value.ToDouble();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            errorList.Add("#EX: " + ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (errorList.Count > 0)
                {
                    MessageBox.Show(string.Join("\r\n\r\n", errorList), "ERRORS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void hACKRestoreToolWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var toolWindows = dockManager.ToolWindows;
            toolWindowOutput.Activate();
            toolWindowOutput.State = ToolWindowState.DockableInsideHost;
            toolWindowErrors.Activate();
            toolWindowErrors.State = ToolWindowState.DockableInsideHost;
            //toolWindowOutput.Activate();
            //toolWindowOutput.State = ToolWindowState.DockableInsideHost;
            //toolWindowOutput.CanDockHostBottom = ActiproSoftware.UI.WinForms.DefaultableBoolean.True;
            //toolWindowOutput.CanFloat = ActiproSoftware.UI.WinForms.DefaultableBoolean.True;
            toolWindowOutput.Show();
            //toolWindowOutput.BringToFront();
            //toolWindowErrors.Show();
            //ToolWindow toolWindowOuptut = null;
            //foreach (ToolWindow wnd in dockManager.ToolWindows)
            //    if (wnd.Text == "Output")
            //        toolWindowOuptut = wnd;
        }

        private void statusVersion_Click(object sender, EventArgs e)
        {
            if (false)
            {
                string newVersion = qbook.Core.ThisBook.Version;
                var dr = QB.UI.InputDialog.ShowDialog("SET QBOOK VERSION", "Please enter the new version number for this qbook:", ref newVersion);
                if (dr == DialogResult.OK)
                {
                    qbook.Core.ThisBook.Version = newVersion;
                }
            }

            VersionForm versionForm = new VersionForm();
            versionForm.FormClosed += VersionForm_FormClosed;
            versionForm.Show();
        }

        private void VersionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateStatusVersion();
        }

        private void statusVersion_MouseMove(object sender, MouseEventArgs e)
        {
            if (statusStrip1.RectangleToScreen(statusVersion.Bounds).Contains(Cursor.Position))
                Cursor.Current = Cursors.Hand;
            else
                Cursor.Current = Cursors.Default;
        }

        private void toolStripButtonFontSizeMinus_Click(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditor.Font.Size > 6)
                ActiveSyntaxEditor.Font = new Font(ActiveSyntaxEditor.Font.FontFamily, ActiveSyntaxEditor.Font.Size - 1);
            UpdateEverySyntaxEditorToActiveSyntaxEditor();
        }

        private void toolStripButtonFontSizePlus_Click(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditor.Font.Size < 64)
                ActiveSyntaxEditor.Font = new Font(ActiveSyntaxEditor.Font.FontFamily, ActiveSyntaxEditor.Font.Size + 1);
            UpdateEverySyntaxEditorToActiveSyntaxEditor();
        }

        void UpdateEverySyntaxEditorToActiveSyntaxEditor()
        {
            foreach (var doc in dockManager.ActiveDocuments.OfType<SyntaxEditorDocumentWindow>())
            {
                if (doc.SyntaxEditor != null)
                    doc.SyntaxEditor.Font = ActiveSyntaxEditor.Font;
            }
        }

        private void hidePageToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_lastClickedNode != null && _lastClickedNode.Tag is qbook.oPage)
                (_lastClickedNode.Tag as qbook.oPage).Hidden = (sender as ToolStripMenuItem).Checked;
            UpdatePageTreeView(treeViewCodePages.Nodes);
        }

        private void toolStripTextBoxPageText_Validated(object sender, EventArgs e)
        {
            if (ActiveSyntaxEditorDocumentWindow != null)
                ActiveSyntaxEditorDocumentWindow.Page.TextL = toolStripTextBoxPageText.Text;
        }

        private void toolStripButtonInsertSnippet_Click(object sender, EventArgs e)
        {
            ActiveSyntaxEditor.ActiveView.IntelliPrompt.RequestInsertSnippetSession();
        }

        ToolWindow lastActiveToolWindow = null;
        private void toolWindowOutput_Enter(object sender, EventArgs e)
        {
            lastActiveToolWindow = toolWindowOutput;
        }

        private void toolWindowErrors_Enter(object sender, EventArgs e)
        {
            lastActiveToolWindow = toolWindowErrors;
        }

        private void toolWindowFindResults_Enter(object sender, EventArgs e)
        {
            lastActiveToolWindow = toolWindowFindResults;
        }

        private void refreshTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopulatePageTreeView(false, true);
        }

        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (SelectedPage == null)
                MessageBox.Show("no page selected");
            else
            {
                qbook.Core.ThisBook.Main.Objects.Remove(SelectedPage);
                PopulatePageTreeView(false, true);
            }
        }

        private void newPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewPageBelowSelectedPage();
        }

        private void newPageBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewPageBelowSelectedPage(true);
        }

        private void buttonAddPage_Click(object sender, EventArgs e)
        {
            AddNewPageBelowSelectedPage();
        }

        private void AddNewPageBelowSelectedPage(bool beforeSelectedPage = false)
        {
            qbook.Core.ThisBook.Modified = true;
            //qbook.Core.ActualMain = (sender as oIcon).Parent;

            string newName = "newPage";


        _AddPageStart:
            if (DialogResult.OK == QB.UI.InputDialog.ShowDialog("Add New Page", "Page Name:", ref newName))
            {
                bool pageNameExists = false;
                foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
                {
                    string newFullName = null;
                    int index = page.FullName.LastIndexOf(".");
                    if (index == -1)
                        newFullName = newName.Trim();
                    else
                        newFullName = page.FullName.Substring(0, index) + newName.Trim();
                    if (page.FullName == newFullName)
                    {
                        pageNameExists = true;
                        break;
                    }
                }
                if (pageNameExists)
                {
                    MessageBox.Show($"A page with the name '{newName}' already exists.\r\nPlease choose a different name."
                        , "PAGE NAME EXISTS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    goto _AddPageStart;
                }

                oPage newPage = new oPage(newName, newName);

                //TODO: insert after selected page/node
                //int indx = (sender as oIcon).Parent.Objects.IndexOf(qbook.Core.SelectedPage);
                int indx = 0;
                if (SelectedPage == null)
                    indx = 0;
                else
                    indx = qbook.Core.ThisBook.Main.Objects.OfType<oPage>().ToList().IndexOf(SelectedPage);

                if (beforeSelectedPage)
                    indx = indx+0;
                else
                    indx = indx+1;

                qbook.Core.ThisBook.Main.Objects.Insert(indx, newPage);
                qbook.Core.SelectedPage = newPage;

                //   Main.Qb.SelectedLayer.Add(newItem);
                qbook.Core.ThisBook.Modified = true;
                PopulatePageTreeView(false, true);
            }
        }

        private void hidePageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
