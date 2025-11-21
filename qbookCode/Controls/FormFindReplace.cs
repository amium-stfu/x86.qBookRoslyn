

using ScintillaNET;
using System.Data;
using System.Diagnostics;
using qbookCode.Controls.InputControls;

namespace qbookCode.Controls
{

    public partial class FormFindReplace : Form
    {
        private readonly FormCodeExplorer Root;
        public  DocumentEditor Editor;
        private readonly qbookCode.Roslyn.RoslynService _roslyn;

        string findText = "";
        string replaceText = "";
        string findIn = "Document";

        DataTable Result;
        DataGridView View;

        private bool isNewSearch = true;

        class FindReplaceOptions
        {
            public bool MatchCase { get; set; } = false;
            public bool MatchWholeWord { get; set; } = false;
            public bool UseRegularExpressions { get; set; } = false;
            public bool SearchUp { get; set; } = false;
            public bool InSelection { get; set; } = false;
            public bool InDocument { get; set; } = false;
            public bool InProject { get; set; } = false;
        }

        FindReplaceOptions Options = new FindReplaceOptions();

        TextBoxWithLabel tbSearch;
        TextBoxWithLabel tbReplace;
        TextBoxWithLabel tbFindIn;

        public FormFindReplace(FormCodeExplorer parent)
        {
            Root = parent;
            _roslyn = Core.Roslyn;
            InitializeComponent();
            btnFindPrevious.Visible = false;
            Init();



            tbSearch = new TextBoxWithLabel("Find:", () => findText, v => findText = v) {Dock = DockStyle.Fill };
            tbReplace = new TextBoxWithLabel("Replace:", () => replaceText, v => replaceText = v) { Dock = DockStyle.Fill };
            tbFindIn = new TextBoxWithLabel(
                "Page Format:",
                () => findIn,
                v => findIn = v,
                new List<string> { "Dokument", "Selection", "Book" }
            )
            { Dock = DockStyle.Fill };

            tbSearch.EnterPressed += async (sender, e) =>
            {
                isNewSearch = true;
                await findNext();
            };


            this.tableLayoutPanel1.Controls.Add(tbSearch, 1, 1);
            this.tableLayoutPanel1.Controls.Add(tbReplace, 1, 2);
            this.tableLayoutPanel1.Controls.Add(tbFindIn, 1, 3);

            this.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                this.Hide();
            };

            vBar.Init(dataGridResult);
            vBar.UpdateScrollBar();
        }

        void test()
        {
            Debug.WriteLine("test");
        }

        public void ApplyTheme()
        {
            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: Theme.IsDark);

            
            tableLayoutPanel1.BackColor = Theme.BackColor;

            vBar.SetBackColor = Theme.PanelBackColor;
            vBar.SetForeColor = Theme.ThumbColor;
            vBar.UpdateScrollBar();

            tbFindIn.ApplyTheme();
            tbReplace.ApplyTheme();
            tbSearch.ApplyTheme();

            btnFindNext.BackColor = Theme.GridBackColor;
            btnFindNext.ForeColor = Theme.ButtonForeColor;

            btnFindPrevious.BackColor = Theme.GridBackColor;
            btnFindPrevious.ForeColor = Theme.ButtonForeColor;


            btnReplaceAll.BackColor = Theme.GridBackColor;
            btnReplaceAll.ForeColor = Theme.ButtonForeColor;

            btnReplaceNext.BackColor = Theme.GridBackColor;
            btnReplaceNext.ForeColor = Theme.ButtonForeColor;


            btnMatchCase.BackColor = Theme.GridBackColor;
            btnMatchCase.ForeColor = Theme.ButtonForeColor;

     

            btnMatchCase.BackColor = Theme.TreeNodeSelectColor;

            dataGridResult.BackgroundColor = Theme.GridBackColor;
            dataGridResult.ForeColor = Theme.GridForeColor;

            dataGridResult.DefaultCellStyle.BackColor = Theme.GridBackColor;
            dataGridResult.DefaultCellStyle.ForeColor = Theme.GridForeColor;

        }

        private void Init()
        {

            Result = new DataTable();

            Result.Columns.Add("Page", typeof(string));
            Result.Columns.Add("Position", typeof(int));
            Result.Columns.Add("Length", typeof(int));
            Result.Columns.Add("Description", typeof(string));
            Result.Columns.Add("Node", typeof(BookNode));


            dataGridResult.DataBindingComplete += (s, e) =>
            {
                dataGridResult.Columns["Page"].Width = 200;
                dataGridResult.Columns["Position"].Visible = false;
                dataGridResult.Columns["Node"].Visible = false;
                dataGridResult.Columns["Length"].Visible = false;
                dataGridResult.Columns["Position"].Visible = true;

                dataGridResult.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };
            dataGridResult.AllowUserToResizeColumns = false;

            dataGridResult.DataSource = Result;
            dataGridResult.AllowUserToResizeColumns = false;
            dataGridResult.AllowUserToAddRows = false;
            dataGridResult.RowHeadersVisible = false;
            dataGridResult.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridResult.MultiSelect = false;
            dataGridResult.ReadOnly = true;
            dataGridResult.BackgroundColor = Color.White;
            dataGridResult.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridResult.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridResult.ColumnHeadersVisible = false;
            dataGridResult.RowHeadersVisible = false;
            dataGridResult.Dock = DockStyle.Fill;
            dataGridResult.AllowUserToAddRows = false;
            dataGridResult.ScrollBars = System.Windows.Forms.ScrollBars.None;
            dataGridResult.AllowUserToDeleteRows = false;
            dataGridResult.AllowUserToOrderColumns = true;
            dataGridResult.AllowUserToResizeColumns = false;
            dataGridResult.BackgroundColor = System.Drawing.Color.LightGray;
            dataGridResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridResult.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridResult.ColumnHeadersVisible = false;
            dataGridResult.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridResult.Location = new System.Drawing.Point(46, 25);
            dataGridResult.Margin = new System.Windows.Forms.Padding(0);
            dataGridResult.Name = "dataGridFindReplace";
            dataGridResult.Size = new System.Drawing.Size(832, 115);
            dataGridResult.TabIndex = 0;
            dataGridResult.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridResult.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridResult.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    string pageName = dataGridResult.Rows[e.RowIndex].Cells["Page"].Value.ToString();
                    int pos = (int)dataGridResult.Rows[e.RowIndex].Cells["Position"].Value;
                    int length = Convert.ToInt32(dataGridResult.Rows[e.RowIndex].Cells["Length"].Value);
                    BookNode node = (BookNode)dataGridResult.Rows[e.RowIndex].Cells["Node"].Value;
                    selectedItem = e.RowIndex;
                    JumpToPosition(pos, length, node);
                }
            };
        }

        Color _matchCaseForeColor;

        public void FocusFind()
        {
           tbSearch.Focus();
        }

        public void Add(string page, int position, int length, string description, BookNode node)
        {
            if (Result == null) return;
            Result.Rows.Add(page, position, length, description.TrimStart(), node);

            Result.AcceptChanges();
            dataGridResult.Refresh();
        }
        void JumpToPosition(int pos, int length, BookNode node)
        {
            
            Root.SetTarget("Find Jump To",node.Editor);
            Editor = node.Editor;

            int p = pos;
            int lineNumber = Editor.LineFromPosition(p);
            int col = Editor.GetColumn(p);
            //   Debug.WriteLine("Column number " + col);
            int lineStartPos = Editor.Lines[lineNumber].Position;
            Editor.GotoPosition(lineStartPos);
            Editor.SelectionStart = p;
            Editor.SelectionEnd = p + length;
        }
        public void JumpToIndex(int next)
        {

            try
            {
               if(dataGridResult.Rows.Count == 0)
                {
                    btnFindNext.Visible = false;
                    btnFindPrevious.Visible = false;
                    return;
                }
                selectedItem += next;
                if (selectedItem < 0) selectedItem = 0;
                if (selectedItem >= itemsFound) selectedItem = itemsFound;

                Debug.WriteLine("SelectedRow = " + selectedItem);

                if (selectedItem < 0 || selectedItem >= itemsFound) return;

               
                int pos = (int)dataGridResult.Rows[selectedItem].Cells["Position"].Value;
                int length = Convert.ToInt32(dataGridResult.Rows[selectedItem].Cells["Length"].Value);
                BookNode node = (BookNode)dataGridResult.Rows[selectedItem].Cells["Node"].Value;
                JumpToPosition(pos, length, node);

                btnFindNext.Visible = selectedItem >= itemsFound ? false : true;
                btnFindPrevious.Visible = selectedItem == 0 ? false : true;
                dataGridResult.Rows[selectedItem].Selected = true;

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("[Searchbar] Error in JumpToIndex: " + ex.Message);

            }
        }
        void findInDocument()
        {
            try
            {
                string text = findText;
                if (string.IsNullOrEmpty(text)) return;
                string name = Root.SelectedCodeNode.Name;

                int pos = 0;
                StringComparison comparison = Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                while ((pos = Editor.Text.IndexOf(text, pos, comparison)) != -1)
                {
                    int lineNumber = Editor.LineFromPosition(pos);
                    string lineText = Editor.Lines[lineNumber].Text;


                    Add(name, pos, text.Length, lineText, Root.SelectedCodeNode);
                    pos += text.Length;
                }

                Result.AcceptChanges();
            }
            catch(System.Exception ex)
            {
            Debug.WriteLine("[FromFindReplace] " + ex.Message);
            }
        }
        void findInSelection()
        {
            string text = findText;

            if (string.IsNullOrEmpty(text)) return;

    

            string name = Root.SelectedCodeNode.Name;
            int selStart = Editor.SelectionStart;
            int selEnd = Editor.SelectionEnd;
            int pos = selStart;

            StringComparison comparison = Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            while ((pos = Editor.Text.IndexOf(text, pos, selEnd - pos, comparison)) != -1 && pos < selEnd)
            {
                int lineNumber = Editor.LineFromPosition(pos);
                string lineText = Editor.Lines[lineNumber].Text;
                Add(name, pos, text.Length, lineText, Root.SelectedCodeNode);
                pos += text.Length;
            }
        }
        async Task findInProject()
        {
            string text = findText;
            if (string.IsNullOrEmpty(text)) return;
            Scintilla editor = new Scintilla();

            foreach (BookNode pageNode in Root.ProjectTree.Nodes[0].Nodes)
            {
                await findInNode(pageNode);
                Debug.WriteLine(pageNode.Text);
                foreach (BookNode subNode in pageNode.Nodes)
                {
                    await findInNode(subNode);
                }
            }
        }
        private async Task findInNode(BookNode node)
        {
            if(node.Editor == null) return;

            await node.Editor.UpdateRoslyn("find");
            string text = findText;
            string name = node.Name;
            Scintilla editor = node.Editor;
            int pos = 0;

            StringComparison comparison = Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            while ((pos = editor.Text.IndexOf(text, pos, comparison)) != -1)
            {
                int lineNumber = editor.LineFromPosition(pos);
                string lineText = editor.Lines[lineNumber].Text;

                if (lineText.Contains("//+include"))
                {
                    pos += text.Length;
                    continue;
                }

                Add(name, pos, text.Length, lineText, node);
                pos += text.Length;
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
            Result.Clear();
        }
        private void btnFindPrevious_Click(object sender, EventArgs e)
        {
            JumpToIndex(-1);
        }

        int itemsFound = 0;
        int selectedItem = 0;
        private async void btnFindNext_Click(object sender, EventArgs e)
        {
            await findNext();
        }

        private async Task findNext()
        {
            switch (findIn)
            {
                case "Document": Options.InDocument = true; break;
                case "Selection": Options.InSelection = true; break;
                case "Book": Options.InProject = true; break;
            }


            if (isNewSearch)
            {
                Result.Clear();
                selectedItem = 0;
                isNewSearch = false;

                btnFindPrevious.Visible = false;
                
                if (Options.InDocument) findInDocument();
                if (Options.InSelection) findInSelection();
                if (Options.InProject) await findInProject();


                itemsFound = Result.Rows.Count;
                JumpToIndex(0);

            }
            else
            {
                JumpToIndex(+1);
            }
        }

        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            isNewSearch = true;
        }

        private void btnMatchCase_Click(object sender, EventArgs e)
        {
           
            Options.MatchCase = !Options.MatchCase;
            btnMatchCase.BackColor = Options.MatchCase ? Color.Orange : Theme.ButtonBackColor;

        }

        private void cbTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private async void tbFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                isNewSearch = true;

               await findNext();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        private async void btnReplaceNext_Click(object sender, EventArgs e)
        {
            int start = Editor.SelectionStart;
            int end = Editor.SelectionEnd;
            int offset = replaceText.Length - findIn.Length;

            if (start != end)
            {
                string selectedText = Editor.GetTextRange(start, end - start);
                Editor.ReadOnly = false;
                Editor.SetTargetRange(start, end); 
                int replaced = Editor.ReplaceTarget(tbReplace.Text);
  
            }

            await Task.Delay(50);
            await updatePositions(Root.SelectedCodeNode, selectedItem, offset);

            JumpToIndex(+1);
        }



        private async void btnReplaceAll_Click(object sender, EventArgs e)
        {

            if (Options.InDocument || Options.InSelection) 
                await replaceInDocOrSel();

            if (Options.InProject)
                await replaceInProject();
     
        }

        private async Task replaceInProject()
        {
            Dictionary<BookNode, DataTable> edit = new Dictionary<BookNode, DataTable>();
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)row["Position"];
                int length = Convert.ToInt32(row["Length"]);
                BookNode node = row["Node"] as BookNode;

                if (!edit.ContainsKey(node))
                {
                    DataTable table = new DataTable();
                    table.Columns.Add("Index", typeof(int));
                    table.Columns.Add("Position", typeof(int));
                    table.Columns.Add("Length", typeof(int));
                    table.Columns.Add("Node", typeof(BookNode));
                    table.Rows.Add(i, pos, length, node);

                    edit[node] = table;
                }
                else
                {
                    edit[node].Rows.Add(i, pos, length, node);
                }
                i++;
            }

            foreach (var update in edit)
            {
                await replaceInNode(update.Key, update.Value);
            }

            await Root.SelectedCodeNode.Editor.UpdateRoslyn("ReplaceInProject");
        }

        private async Task replaceInNode(BookNode node, DataTable toReplace)
        {
            Scintilla editor = node.Editor;
            int offset = replaceText.Length - findText.Length;

            int r = 0;
            foreach (DataRow row in toReplace.Rows)
            {
                int pos = (int)row["Position"] + offset * r;
                int length = Convert.ToInt32(row["Length"]);

                if (node.Name == Root.SelectedCodeNode.Name)
                {

                    Editor.SelectionStart = pos;
                    Editor.SelectionEnd = pos + length;
                    Editor.SetTargetRange(pos, pos + length);
                    Editor.ReplaceTarget(replaceText);
                    await node.Editor.UpdateRoslyn("Replace");

                }
                else
                {
                    editor.SetTargetRange(pos, pos + length);
                    editor.ReplaceTarget(replaceText);
                    await node.Editor.UpdateRoslyn("Replace");
                    r++;
                }
            }

           
        }


        private async Task replaceInDocOrSel()
        {
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)dataGridResult.Rows[i].Cells["Position"].Value;
                int length = Convert.ToInt32(dataGridResult.Rows[i].Cells["Length"].Value);
                BookNode node = (BookNode)dataGridResult.Rows[i].Cells["Node"].Value;

                if (node.Name == Root.SelectedCodeNode.Name)
                {
                    Editor.SelectionStart = pos;
                    Editor.SelectionEnd = pos + length;
                    Editor.SetTargetRange(Editor.SelectionStart, Editor.SelectionEnd);
                    int replaced = Editor.ReplaceTarget(tbReplace.Text);
                   // await Root.RefreshEditor("FindReplaceAll");

                    int offset = replaceText.Length - findText.Length;
                    await updatePositions(node, i, offset);
                }
                i++;
            }

        }

        private async Task updatePositions(BookNode nodeFilter, int index, int offset)
        {
            Debug.WriteLine(".");
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)dataGridResult.Rows[i].Cells["Position"].Value;
                int length = Convert.ToInt32(dataGridResult.Rows[i].Cells["Length"].Value);
                BookNode node = (BookNode)dataGridResult.Rows[i].Cells["Node"].Value;

                if (node.Name == nodeFilter.Name)
                {
                    if (i > index) 
                    {
                        dataGridResult.Rows[i].Cells["Position"].Value = pos + offset;
                        Result.AcceptChanges();
                        int npos = (int)dataGridResult.Rows[i].Cells["Position"].Value;
                    }
                }
                i++;
            }
            

        }

        private void FormFindReplace_Shown(object sender, EventArgs e)
        {            
            tbSearch.Focus();
        }
    }
}
