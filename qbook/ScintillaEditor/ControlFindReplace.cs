using ActiproSoftware.UI.WinForms.Drawing;
using CSScripting;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Outlook;
using MQTTnet.Internal;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using qbook.CodeEditor;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Forms;
using static IronPython.Modules._ast;

using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.Design.AxImporter;

namespace qbook.ScintillaEditor
{

    public partial class ControlFindReplace : System.Windows.Forms.UserControl
    {
        private readonly FormScintillaEditor Root;
        public  Scintilla Editor;
        private readonly RoslynService _roslyn;

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

        public ControlFindReplace(FormScintillaEditor parent, RoslynService roslyn, DataGridView view)
        {
            Root = parent;
            _roslyn = roslyn;
          //  Editor = Root.SelectedNode.Editor;
            InitializeComponent();
            btnFindPrevious.Visible = false;
            View = view;
            Hide();
            Init();

            cbTarget.Items.AddRange(new string[] { "Document","Selection", "Project" });
            cbTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTarget.DrawMode = DrawMode.OwnerDrawFixed;
            cbTarget.DrawItem += CbTarget_DrawItem;

            cbTarget.Text = cbTarget.Items[0].ToString();
        }

        private void CbTarget_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

           
            e.DrawBackground();

          
            var brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                ? SystemBrushes.HighlightText
                : new SolidBrush(cbTarget.ForeColor);

          
            e.Graphics.DrawString(cbTarget.Items[e.Index].ToString(), cbTarget.Font, brush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void Init()
        {

            Result = new DataTable();

            Result.Columns.Add("Page", typeof(string));
            Result.Columns.Add("Position", typeof(int));
            Result.Columns.Add("Length", typeof(int));
            Result.Columns.Add("Description", typeof(string));
            Result.Columns.Add("Node", typeof(CodeNode));

            View.DataBindingComplete += (s, e) =>
            {
                View.Columns["Page"].Width = 100;
                View.Columns["Position"].Width = 100;
                View.Columns["Node"].Visible = false;
                View.Columns["Length"].Visible = false;
                View.Columns["Position"].Visible = true;

                View.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };
            View.AllowUserToResizeColumns = false;

            View.DataSource = Result;
            View.AllowUserToResizeColumns = false;
            View.AllowUserToAddRows = false;
            View.RowHeadersVisible = false;
            View.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            View.MultiSelect = false;
            View.ReadOnly = true;
            View.BackgroundColor = Color.White;
            View.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            View.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            View.ColumnHeadersVisible = false;
            View.RowHeadersVisible = false;
            View.Dock = DockStyle.Fill;
            View.AllowUserToAddRows = false;
            View.ScrollBars = System.Windows.Forms.ScrollBars.None;
            View.AllowUserToDeleteRows = false;
            View.AllowUserToOrderColumns = true;
            View.AllowUserToResizeColumns = false;
            View.BackgroundColor = System.Drawing.Color.LightGray;
            View.BorderStyle = System.Windows.Forms.BorderStyle.None;
            View.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            View.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            View.ColumnHeadersVisible = false;
            View.Dock = System.Windows.Forms.DockStyle.Fill;
            View.Location = new System.Drawing.Point(46, 25);
            View.Margin = new System.Windows.Forms.Padding(0);
            View.Name = "dataGridFindReplace";
            View.Size = new System.Drawing.Size(832, 115);
            View.TabIndex = 0;
            View.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            View.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            View.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    string pageName = View.Rows[e.RowIndex].Cells["Page"].Value.ToString();
                    int pos = (int)View.Rows[e.RowIndex].Cells["Position"].Value;
                    int length = Convert.ToInt32(View.Rows[e.RowIndex].Cells["Length"].Value);
                    CodeNode node = (CodeNode)View.Rows[e.RowIndex].Cells["Node"].Value;
                    selectedItem = e.RowIndex;
                    JumpToPosition(pos, length, node);
                }
            };
        }

        Color _matchCaseForeColor;
        public void DarkTheme()
        {
            BackColor = Color.FromArgb(45, 45, 48);
            tableLayoutPanel1.BackColor = BackColor;


            tbFind.BackColor = Color.Black;
            tbFind.ForeColor = Color.FromArgb(225, 225, 225);

            tbReplace.BackColor = Color.Black;
            tbReplace.ForeColor = Color.FromArgb(225, 225, 225);

            cbTarget.BackColor = Color.Black;
            cbTarget.ForeColor = Color.FromArgb(225, 225, 225);
            cbTarget.FlatStyle = FlatStyle.Flat;
            

            btnClose.BackColor = BackColor;
            btnClose.ForeColor = Color.FromArgb(225, 225, 225);

            btnFindNext.BackColor = BackColor;
            btnFindNext.ForeColor = Color.FromArgb(225, 225, 225);

            btnFindPrevious.BackColor = BackColor;
            btnFindPrevious.ForeColor = Color.FromArgb(225, 225, 225);


            btnReplaceAll.BackColor = BackColor;
            btnReplaceAll.ForeColor = Color.FromArgb(225, 225, 225);

            btnReplaceNext.BackColor = BackColor;
            btnReplaceNext.ForeColor = Color.FromArgb(225, 225, 225);


            btnMatchCase.BackColor = BackColor;
            btnMatchCase.ForeColor = Color.FromArgb(225, 225, 225);

            _matchCaseForeColor = btnMatchCase.ForeColor;

           
        }
        public void LightTheme()
        {
            BackColor = Color.FromArgb(225, 225, 225);
            tableLayoutPanel1.BackColor = BackColor;


            tbFind.BackColor = Color.White;
            tbFind.ForeColor = Color.FromArgb(45, 45, 45);

            tbReplace.BackColor = Color.White;
            tbReplace.ForeColor = Color.FromArgb(45, 45, 45);

            cbTarget.BackColor = Color.White;
            cbTarget.ForeColor = Color.FromArgb(45, 45, 45);
            cbTarget.FlatStyle = FlatStyle.Flat;


            btnClose.BackColor = BackColor;
            btnClose.ForeColor = Color.FromArgb(45, 45, 45);

            btnFindNext.BackColor = BackColor;
            btnFindNext.ForeColor = Color.FromArgb(45, 45, 45);

            btnFindPrevious.BackColor = BackColor;
            btnFindPrevious.ForeColor = Color.FromArgb(45, 45, 45);


            btnReplaceAll.BackColor = BackColor;
            btnReplaceAll.ForeColor = Color.FromArgb(45, 45, 45);

            btnReplaceNext.BackColor = BackColor;
            btnReplaceNext.ForeColor = Color.FromArgb(45, 45, 45);


            btnMatchCase.BackColor = BackColor;
            btnMatchCase.ForeColor = Color.FromArgb(45, 45, 45);

            _matchCaseForeColor = btnMatchCase.ForeColor;

          
        }
        public void ShowFind()
        {
            Font font = Root.GetFont();
            int padding = 20;
            int cellHeight = TextRenderer.MeasureText("Wg", font).Height + padding;
            int matchWith = TextRenderer.MeasureText("Aa", font).Width +20;

            tbFind.Font = font;
            tbReplace.Font = font;
            cbTarget.Font = font;

            btnMatchCase.Font = font;

            tbFind.Text = "Find...";
            Height = cellHeight * 2;
            tbReplace.Visible = false;
            btnReplaceAll.Visible = false;
            btnReplaceNext.Visible = false;

            btnMatchCase.Width = matchWith;

            tableLayoutPanel1.RowStyles[0].Height = cellHeight;
            tableLayoutPanel1.RowStyles[1].Height = 0;
            tableLayoutPanel1.RowStyles[2].Height = cellHeight;
            tbFind.Focus();

        }
        public void ShowReplace()
        {
            Font font = Root.GetFont();
            int padding = 20;
            int cellHeight = TextRenderer.MeasureText("Wg", font).Height + padding;
            int matchWith = TextRenderer.MeasureText("Aa", font).Width + 20;

            tbFind.Font = font;
            tbReplace.Font = font;
            cbTarget.Font = font;

            btnMatchCase.Font = font;

            tbFind.Text = "Find...";
            tbReplace.Text = "Replace...";
            Height = cellHeight * 3;
            tbReplace.Visible = true;
            btnReplaceAll.Visible = true;
            btnReplaceNext.Visible = true;

            btnMatchCase.Width = matchWith;

            tableLayoutPanel1.RowStyles[0].Height = cellHeight;
            tableLayoutPanel1.RowStyles[1].Height = cellHeight;
            tableLayoutPanel1.RowStyles[2].Height = cellHeight;
            tbFind.Focus();

        }
        public void FocusFind()
        {
            tbFind.Focus();
        }

        public void Add(string page, int position, int length, string description, CodeNode node)
        {
            if (Result == null) return;
            Result.Rows.Add(page, position, length, description, node);

            Result.AcceptChanges();
            View.Refresh();
        }
        internal async void JumpToPosition(int pos, int length, CodeNode node)
        {
            await Root.OpenNode(node);
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
               if(View.Rows.Count == 0)
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

               
                int pos = (int)View.Rows[selectedItem].Cells["Position"].Value;
                int length = Convert.ToInt32(View.Rows[selectedItem].Cells["Length"].Value);
                CodeNode node = (CodeNode)View.Rows[selectedItem].Cells["Node"].Value;
                JumpToPosition(pos, length, node);

                btnFindNext.Visible = selectedItem >= itemsFound ? false : true;
                btnFindPrevious.Visible = selectedItem == 0 ? false : true;
                View.Rows[selectedItem].Selected = true;

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("[Searchbar] Error in JumpToIndex: " + ex.Message);

            }
        }
        async Task findInDocument()
        {
            string text = tbFind.Text;
            if (string.IsNullOrEmpty(text)) return;

            Root.TblFindReplaceOutputs.Clear();
            Result.Clear();
            string name = Root.SelectedNode.Name;

            int pos = 0;
            StringComparison comparison = Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            while ((pos = Editor.Text.IndexOf(text, pos, comparison)) != -1)
            {
                int lineNumber = Editor.LineFromPosition(pos);
                string lineText = Editor.Lines[lineNumber].Text;


                Add(name, pos, text.Length, lineText, Root.SelectedNode);
                pos += text.Length;
            }

            Result.AcceptChanges();
        }
        async Task findInSelection()
        {
            string text = tbFind.Text;

            if (string.IsNullOrEmpty(text)) return;
            Root.TblFindReplaceOutputs.Clear();
            Result.Clear();

            string name = Root.SelectedNode.Name;
            int selStart = Editor.SelectionStart;
            int selEnd = Editor.SelectionEnd;
            int pos = selStart;

            StringComparison comparison = Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            while ((pos = Editor.Text.IndexOf(text, pos, selEnd - pos, comparison)) != -1 && pos < selEnd)
            {
                int lineNumber = Editor.LineFromPosition(pos);
                string lineText = Editor.Lines[lineNumber].Text;
                Add(name, pos, text.Length, lineText, Root.SelectedNode);
                pos += text.Length;
            }
        }
        async Task findInProject()
        {
            
            string text = tbFind.Text;
            if (string.IsNullOrEmpty(text)) return;
            Scintilla editor = new Scintilla();
            Root.TblFindReplaceOutputs.Clear();
            Result.Clear();

            foreach (CodeNode pageNode in Root.ProjectTree.Nodes)
            {
                await findInNode(pageNode);

                foreach (CodeNode subNode in pageNode.Nodes)
                {
                    await findInNode(subNode);
                }
            }
        }
        private async Task findInNode(CodeNode node)
        {
            if(node.Editor == null) return;

            await node.UpdateRoslyn();
            string text = tbFind.Text;
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
            switch (cbTarget.SelectedIndex)
            {
                case 0: Options.InDocument = true; break;
                case 1: Options.InSelection = true; break;
                case 2: Options.InProject = true; break;
            }


            if (isNewSearch)
            {
                selectedItem = 0;
                isNewSearch = false;

                btnFindPrevious.Visible = false;
                if (Options.InDocument) await findInDocument();
                if (Options.InSelection) await findInSelection();
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
            btnMatchCase.ForeColor = Options.MatchCase ? Color.Orange : _matchCaseForeColor;

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
            int offset = tbReplace.Text.Length - tbFind.Text.Length;

            if (start != end)
            {
                string selectedText = Editor.GetTextRange(start, end - start);
                Editor.ReadOnly = false;
                Editor.SetTargetRange(start, end); 
                int replaced = Editor.ReplaceTarget(tbReplace.Text);
  
            }

            await Task.Delay(50);
            await updatePositions(Root.SelectedNode, selectedItem, offset);

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
            Dictionary<CodeNode, DataTable> edit = new Dictionary<CodeNode, DataTable>();
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)row["Position"];
                int length = Convert.ToInt32(row["Length"]);
                CodeNode node = row["Node"] as CodeNode;

                if (!edit.ContainsKey(node))
                {
                    DataTable table = new DataTable();
                    table.Columns.Add("Index", typeof(int));
                    table.Columns.Add("Position", typeof(int));
                    table.Columns.Add("Length", typeof(int));
                    table.Columns.Add("Node", typeof(CodeNode));
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

            await Root.SelectedNode.UpdateRoslyn();
        }

        private async Task replaceInNode(CodeNode node, DataTable toReplace)
        {
            Scintilla editor = node.Editor;
            int offset = tbReplace.Text.Length - tbFind.Text.Length;

            int r = 0;
            foreach (DataRow row in toReplace.Rows)
            {
                int pos = (int)row["Position"] + offset * r;
                int length = Convert.ToInt32(row["Length"]);

                if (node.Name == Root.SelectedNode.Name)
                {

                    Editor.SelectionStart = pos;
                    Editor.SelectionEnd = pos + length;
                    Editor.SetTargetRange(pos, pos + length);
                    Editor.ReplaceTarget(tbReplace.Text);
                    await node.UpdateRoslyn();

                }
                else
                {
                    editor.SetTargetRange(pos, pos + length);
                    editor.ReplaceTarget(tbReplace.Text);
                    await node.UpdateRoslyn();
                    r++;
                }
            }

           
        }


        private async Task replaceInDocOrSel()
        {
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)View.Rows[i].Cells["Position"].Value;
                int length = Convert.ToInt32(View.Rows[i].Cells["Length"].Value);
                CodeNode node = (CodeNode)View.Rows[i].Cells["Node"].Value;

                if (node.Name == Root.SelectedNode.Name)
                {
                    Editor.SelectionStart = pos;
                    Editor.SelectionEnd = pos + length;
                    Editor.SetTargetRange(Editor.SelectionStart, Editor.SelectionEnd);
                    int replaced = Editor.ReplaceTarget(tbReplace.Text);
                   // await Root.RefreshEditor("FindReplaceAll");

                    int offset = tbReplace.Text.Length - tbFind.Text.Length;
                    await updatePositions(node, i, offset);
                }
                i++;
            }

        }

        private async Task updatePositions(CodeNode nodeFilter, int index, int offset)
        {
            Debug.WriteLine(".");
            int i = 0;
            foreach (DataRow row in Result.Rows)
            {
                int pos = (int)View.Rows[i].Cells["Position"].Value;
                int length = Convert.ToInt32(View.Rows[i].Cells["Length"].Value);
                CodeNode node = (CodeNode)View.Rows[i].Cells["Node"].Value;

                if (node.Name == nodeFilter.Name)
                {
                    if (i > index) 
                    {
                        View.Rows[i].Cells["Position"].Value = pos + offset;
                        Result.AcceptChanges();
                        int npos = (int)View.Rows[i].Cells["Position"].Value;
                    }
                }
                i++;
            }
            

        }
    }
}
