using Microsoft.VisualStudio.SolutionPersistence.Model;
using QB.Controls;
using qbook.CodeEditor;
using qbook.ScintillaEditor.InputControls;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using static IronPython.Modules._ast;
using static System.ComponentModel.Design.ObjectSelectorEditor;


namespace qbook.ScintillaEditor
{
    public partial class FormCodeEditor : Form
    {
      
        DataGridView GridViewDiagnosticOutput;
        DataGridView gridViewRuntimeOutput;
        BindingSource MethodenBindingSource = new BindingSource();

        FormBookTree BookTreeView;

        DocumentEditor Editor;
        public FormCodeEditor(DocumentEditor edior, FormBookTree bookTreeView = null)
        {
            Editor = edior;
            InitializeComponent();

            BookTreeView = bookTreeView;


            GridViewDiagnosticOutput = new DataGridView();
          
            gridViewRuntimeOutput = new DataGridView();

            InitGridViews();

            Editor.Dock = DockStyle.Fill;
            panelEditor.Controls.Add(Editor);
      
            vBarEditor.Init(Editor);
            hBarEditor.Init(Editor);


            vBarMethodes.Init(gridViewMethodes);

            if (bookTreeView == null)
            {
                ContainerLeft.Panel1Collapsed = true;

                ControlTab tab = new ControlTab(Editor)
                {
                    Height = 35
                };
                PanelTabs.Controls.Add(tab);

                tab.ApplyTheme();
                tab.Selected();

            }
            else 
            {
                ContainerLeft.Panel1.Controls.Add(bookTreeView);

                ContainerLeft.Panel1Collapsed = false;
               
            }

            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(GridViewDiagnosticOutput);
            vBarOutputs.Init(GridViewDiagnosticOutput);
            UpdateOutputButtons();
            InitIcons();

            _ = Editor.UpdateRoslyn("Initializing...");

        }

        public void SetTarget(DocumentEditor editor)
        {
            if (panelEditor.Controls[0] == editor)
                return;

            Editor = editor;
            panelEditor.Controls.Clear();
            Editor.Dock = DockStyle.Fill;
            panelEditor.Controls.Add(Editor);


            UpdateSources();
            ApplyTheme();

            vBarEditor.Init(Editor);
            hBarEditor.Init(Editor);
            vBarEditor.UpdateScrollBar();
            hBarEditor.UpdateScrollBar();
        }


        private void btnShowHidden_Click(object sender, EventArgs e)
        {
            Editor.ToggleHidenLines();
        }

        void InitGridViews()
        {
            #region Roslyn Diagnostic Output

            
            GridViewDiagnosticOutput.DataSource = Editor.Output;
            GridViewDiagnosticOutput.DataBindingComplete += (s, e) =>
            {
                GridViewDiagnosticOutput.Columns["Page"].Width = 0;
                GridViewDiagnosticOutput.Columns["Class"].Width = 0;
                GridViewDiagnosticOutput.Columns["Position"].Width = 0;
                GridViewDiagnosticOutput.Columns["Length"].Width = 0;
                GridViewDiagnosticOutput.Columns["Type"].Width = 0;
                GridViewDiagnosticOutput.Columns["Node"].Width = 0;

                GridViewDiagnosticOutput.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };

            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GridViewDiagnosticOutput.MultiSelect = false;
            GridViewDiagnosticOutput.ReadOnly = true;
            GridViewDiagnosticOutput.BackgroundColor = Color.Red;
            GridViewDiagnosticOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            GridViewDiagnosticOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            GridViewDiagnosticOutput.ColumnHeadersVisible = true;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = DockStyle.Fill;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.AllowUserToDeleteRows = false;
            GridViewDiagnosticOutput.AllowUserToOrderColumns = true;
            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.BackgroundColor = System.Drawing.Color.LightGray;
            GridViewDiagnosticOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            GridViewDiagnosticOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            GridViewDiagnosticOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridViewDiagnosticOutput.ColumnHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            GridViewDiagnosticOutput.Location = new System.Drawing.Point(46, 25);
            GridViewDiagnosticOutput.Margin = new System.Windows.Forms.Padding(0);
            GridViewDiagnosticOutput.Name = "dataGridOutput";
            GridViewDiagnosticOutput.Size = new System.Drawing.Size(832, 115);
            GridViewDiagnosticOutput.TabIndex = 0;
            GridViewDiagnosticOutput.ScrollBars = System.Windows.Forms.ScrollBars.None;
            GridViewDiagnosticOutput.CellFormatting += (s, e) =>
            {
                if (GridViewDiagnosticOutput.Columns[e.ColumnIndex].Name == "Type")
                {
                    string severity = e.Value?.ToString();
                    switch (severity)
                    {
                        case "Error":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                            break;
                        case "Warning":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                        case "Info":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            };
            GridViewDiagnosticOutput.CellClick += (s, e) => Task.Run(async () =>
            {
                if (e.RowIndex >= 0)
                {
                    string pos = GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Position"].Value.ToString();
                    int length = Convert.ToInt32(GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Length"].Value);
                    if (Editor.InvokeRequired)
                    {
                        Editor.Invoke(new System.Action(() =>
                        {
                            int p = Convert.ToInt32(pos);
                            Editor.SelectRange(p, length);
                        }));
                    }
                    else
                    {
                        int p = Convert.ToInt32(pos);
                        Editor.SelectRange(p, length);
                    }
                }
            });

            #endregion

            #region Methodes

            
            MethodenBindingSource = new BindingSource();

            DataTable MethodeDummy = new DataTable();
            MethodeDummy.Columns.Add("Row", typeof(int));
            MethodeDummy.Columns.Add("Name", typeof(string));

            //Methodes Gridview
            MethodenBindingSource.DataSource = MethodeDummy;
            gridViewMethodes.DataSource = MethodenBindingSource;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridViewMethodes.MultiSelect = false;
            gridViewMethodes.ReadOnly = true;
            gridViewMethodes.BackgroundColor = Color.White;
            gridViewMethodes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gridViewMethodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.Dock = DockStyle.Fill;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.AllowUserToDeleteRows = false;
            gridViewMethodes.AllowUserToOrderColumns = true;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.BackgroundColor = System.Drawing.Color.LightGray;
            gridViewMethodes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            gridViewMethodes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridViewMethodes.Location = new System.Drawing.Point(46, 25);
            gridViewMethodes.TabIndex = 0;

            gridViewMethodes.DataBindingComplete += (s, e) =>
            {
                gridViewMethodes.Columns["Row"].Width = 40;
                gridViewMethodes.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };

            gridViewMethodes.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {

                    int line = (int)gridViewMethodes.Rows[e.RowIndex].Cells["Row"].Value;
                    if (Editor.InvokeRequired)
                    {
                        Editor.Invoke(new System.Action(() =>
                        {

                          Editor.HighlightLine(line, Color.Yellow);
                        }));
                    }
                    else
                    {
                        Editor.HighlightLine(line, Color.Yellow);
                    }
                }
            };

            #endregion

            #region Runtime Output

           
            gridViewRuntimeOutput.DataBindingComplete += (s, e) =>
            {
                gridViewRuntimeOutput.Columns["Count"].Width = 40;
                gridViewRuntimeOutput.Columns["RepeatMs"].Width = 40;
                gridViewRuntimeOutput.Columns["Key"].Visible = false;
                gridViewRuntimeOutput.Columns["File"].Width = 150;
                gridViewRuntimeOutput.Columns["Methode"].Width = 150;
                gridViewRuntimeOutput.Columns["Line"].Visible = false;
                gridViewRuntimeOutput.Columns["Col"].Visible = false;

                gridViewRuntimeOutput.Columns["Reason"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;



            };
            gridViewRuntimeOutput.AllowUserToResizeColumns = false;

            gridViewRuntimeOutput.DataSource = QB.GlobalExceptions.RuntimeErrors;
            gridViewRuntimeOutput.AllowUserToResizeColumns = false;
            gridViewRuntimeOutput.AllowUserToAddRows = false;
            gridViewRuntimeOutput.RowHeadersVisible = false;
            gridViewRuntimeOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridViewRuntimeOutput.MultiSelect = false;
            gridViewRuntimeOutput.ReadOnly = true;
            gridViewRuntimeOutput.BackgroundColor = Color.White;
            gridViewRuntimeOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gridViewRuntimeOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            gridViewRuntimeOutput.ColumnHeadersVisible = false;
            gridViewRuntimeOutput.RowHeadersVisible = false;
            gridViewRuntimeOutput.Dock = DockStyle.Fill;
            gridViewRuntimeOutput.AllowUserToAddRows = false;
            gridViewRuntimeOutput.AllowUserToDeleteRows = false;
            gridViewRuntimeOutput.AllowUserToOrderColumns = true;
            gridViewRuntimeOutput.AllowUserToResizeColumns = false;
            gridViewRuntimeOutput.BackgroundColor = System.Drawing.Color.LightGray;
            gridViewRuntimeOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            gridViewRuntimeOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            gridViewRuntimeOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridViewRuntimeOutput.ColumnHeadersVisible = false;
            gridViewRuntimeOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            gridViewRuntimeOutput.Location = new System.Drawing.Point(46, 25);
            gridViewRuntimeOutput.Margin = new System.Windows.Forms.Padding(0);
            gridViewRuntimeOutput.Name = "dataGridOutput";
            gridViewRuntimeOutput.Size = new System.Drawing.Size(832, 115);
            gridViewRuntimeOutput.TabIndex = 0;
            gridViewRuntimeOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gridViewRuntimeOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            gridViewRuntimeOutput.AllowUserToAddRows = false;
            gridViewRuntimeOutput.DefaultCellStyle.BackColor = Color.Tomato;
            gridViewRuntimeOutput.CellClick += (s, e) =>
            {
                //if (e.RowIndex >= 0)
                //{
                //    string file = dataGridViewBuildOutput.Rows[e.RowIndex].Cells["File"].Value.ToString();
                //    int line = (int)dataGridViewBuildOutput.Rows[e.RowIndex].Cells["Line"].Value;
                //    CodeNode node = GetNodeByFilename(file);
                //    OpenNode(node);
                //    node.Editor.HighlightLine(line, Color.Red);
                //    node.Editor.Refresh();
                //}
            };


            #endregion


        }
        public void UpdateSources()
        {
            GridViewDiagnosticOutput.DataSource = Editor.Output;
            MethodenBindingSource.DataSource = Editor.MethodesClasses;

        }

        #region Theme

        public void ApplyTheme()
        {
            
            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: Theme.IsDark);

            BackColor = Theme.FormBackColor;

            ContainerLeft.BackColor = BackColor;

            EditorLayoutPanel.BackColor = Theme.PanelBackColor;
            panelEditor.BackColor = Theme.PanelBackColor;
            panelSplitter2.BackColor = Theme.FormBackColor;
            panelSplitter3.BackColor = Theme.FormBackColor;
            panelSplitter4.BackColor = Theme.FormBackColor;
            panelSplitter2.Visible = false;
            panelSplitter3.Visible = false;
            panelSplitter4.Visible = false;



            vBarEditor.SetBackColor = Theme.PanelBackColor;
            vBarEditor.SetForeColor = Theme.ThumbColor;
            hBarEditor.SetBackColor = Theme.PanelBackColor;
            hBarEditor.SetForeColor = Theme.ThumbColor;

            vBarMethodes.SetBackColor = Theme.PanelBackColor;
            vBarMethodes.SetForeColor = Theme.ThumbColor;

            vBarOutputs.SetBackColor = Theme.PanelBackColor;
            vBarOutputs.SetForeColor = Theme.ThumbColor;

        
            TablePanelOutputs.BackColor = Theme.PanelBackColor;

            tblViewMethodes.BackColor = Theme.PanelBackColor;
            lblMethodes.BackColor = Theme.PanelBackColor;
            lblMethodes.ForeColor = Theme.GridForeColor;
            gridViewMethodes.BackgroundColor = Theme.GridBackColor;
            gridViewMethodes.BackColor = Theme.GridForeColor;
            gridViewMethodes.RowsDefaultCellStyle.BackColor = Theme.GridBackColor;
            gridViewMethodes.RowsDefaultCellStyle.ForeColor = Theme.GridForeColor;
            gridViewRuntimeOutput.BackgroundColor = Theme.GridBackColor;
            GridViewDiagnosticOutput.BackgroundColor = Theme.GridBackColor;
            dataGridViewFindReplace.BackgroundColor = Theme.GridBackColor;
            dataGridViewFindReplace.ForeColor = Theme.GridForeColor;
       


            vBarMethodes.SetBackColor = Theme.PanelBackColor;
            vBarMethodes.SetForeColor = Theme.ThumbColor;

   

            Editor.ApplyTheme();


            //vBarProjectTree.SetBackColor = Theme.BackColor;
            //vBarProjectTree.SetForeColor = Theme.ThumbColor;


            tbMethodeFilter.BackColor = Theme.StringInputColor;
            tbMethodeFilter.ForeColor = Theme.GridForeColor;

            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = Theme.PanelBackColor;
                b.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                b.ForeColor = System.Drawing.Color.Black;
                b.FlatAppearance.BorderColor = Theme.PanelBackColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Theme.IconColorBefore, Theme.ButtonIconColor, 100); //Init Icon Color
             
                pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }

            UpdateOutputButtons();

            foreach (ControlTab tab in PanelTabs.Controls)
            {
                tab.ApplyTheme();
                if (PanelTabs.Controls.Count == 1) tab.Selected();
            }

 

            vBarEditor.UpdateScrollBar();
            hBarEditor.UpdateScrollBar();

        }

        void InitIcons()
        {
            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = Theme.PanelBackColor;
                b.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                b.ForeColor = System.Drawing.Color.Black;
                b.FlatAppearance.BorderColor = Theme.PanelBackColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Theme.ButtonIconColor, 100); //Init Icon Color
              
                pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }
            Theme.IconColorBefore = Theme.ButtonIconColor;
        }


        #endregion

        #region Tab Control

        public Dictionary<string, ControlTab> DictTabs = new Dictionary<string, ControlTab>();
        public void UpdateTabs(DocumentEditor editor)
        {
            if (!DictTabs.ContainsKey(Editor.Target.Filename))
            {
                ControlTab tab = new ControlTab(Editor, canClose:true)
                {
                    SelectTab = () => BookTreeView.OpenNodeByName(editor.Target.Filename),
                    RemoveTab = () => RemoveTab(editor.Target.Filename),
                    CloseOtherTabs = () => CloseAllOtherTabs(),
                    LoactionForm = this,
                    Height = 35
                };

                DictTabs.Add(Editor.Target.Filename, tab);

              
            }
            RefreshTabs();
        }

        public void RemoveTab(string filename)
        {
            DictTabs.Remove(filename);
            RefreshTabs();
        }

        private void CloseAllOtherTabs()
        {
            List<string> keysToRemove = new List<string>();
            foreach (var key in DictTabs.Keys)
            {
                if (DictTabs[key].Node != BookTreeView.SelectedNode)
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                DictTabs.Remove(key);
            }
            RefreshTabs();
        }
        public void RefreshTabs()
        {

            PanelTabs.SuspendLayout();
            PanelTabs.Invoke((System.Action)(() =>
            {
                PanelTabs.Controls.Clear();
                foreach (ControlTab tab in DictTabs.Values.OrderBy(t => t.Name))
                {
                    PanelTabs.Controls.Add(tab);
                }
            }));

            double controlWidth = 0;
            int count = 0;
            foreach (ControlTab tab in PanelTabs.Controls)
            {
                count++;
                controlWidth += tab.Width;
                tab.ApplyTheme();
                if (tab.FileName == Editor.Target.Filename)
                {
                    tab.Selected();
                    
                }
            }
            int rows = (int)Math.Ceiling(controlWidth / PanelTabs.Width);

            EditorLayoutPanel.RowStyles[0].Height = rows * 35;

            PanelTabs.ResumeLayout();
        }

        #endregion




        private void btnEditorOutput_Click(object sender, EventArgs e)
        {
            UpdateSources();
            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(GridViewDiagnosticOutput);
            vBarOutputs.Init(GridViewDiagnosticOutput);
            UpdateOutputButtons();
        

        }

        private void btnBuildOutput_Click(object sender, EventArgs e)
        {
            UpdateSources();
            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(gridViewRuntimeOutput);
            vBarOutputs.Init(gridViewRuntimeOutput);
            UpdateOutputButtons();
        }

        private void btnShowFindReplaceOutput_Click(object sender, EventArgs e)
        {
            UpdateSources();
            panelOutput.Controls.Clear();
            dataGridViewFindReplace.Dock = DockStyle.Fill;
            panelOutput.Controls.Add(dataGridViewFindReplace);
            vBarOutputs.Init(dataGridViewFindReplace);
            UpdateOutputButtons();
        }

        void UpdateOutputButtons()
        {
            bool isBuild = this.panelOutput.Controls.Contains(this.gridViewRuntimeOutput);
            bool isEditor = this.panelOutput.Controls.Contains(this.GridViewDiagnosticOutput);
            bool isFindReplace = this.panelOutput.Controls.Contains(this.dataGridViewFindReplace);



            if (!Theme.IsDark)
            {
                btnBuildOutput.ForeColor = Color.FromArgb(40, 40, 40);
                btnEditorOutput.ForeColor = Color.FromArgb(40, 40, 40);
                btnShowFindReplaceOutput.ForeColor = Color.FromArgb(40, 40, 40);

                Color sel = Color.FromArgb(220, 220, 220);
                Color usel = Color.FromArgb(190, 190, 190);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }
            else
            {
                btnBuildOutput.ForeColor = Color.FromArgb(230, 230, 230);
                btnEditorOutput.ForeColor = Color.FromArgb(230, 230, 230);
                btnShowFindReplaceOutput.ForeColor = Color.FromArgb(230, 230, 230);

                Color sel = Color.FromArgb(70, 70, 70);
                Color usel = Color.FromArgb(50, 50, 50);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }
        }

        private void panelOutput_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tbMethodeFilter_TextChanged(object sender, EventArgs e)
        {
            string filterText = tbMethodeFilter.Text.Trim();

            if (string.IsNullOrEmpty(filterText))
            {
                MethodenBindingSource.RemoveFilter(); // zeigt alles an
            }
            else
            {
                MethodenBindingSource.Filter = $"Name LIKE '%{filterText}%'";
            }
        }

        private void FormCodeEditor_Shown(object sender, EventArgs e)
        {
            UpdateSources();
        }

        private void btnParagraph_Click(object sender, EventArgs e)
        {
            Editor.ToggleViewEol();
        }

        private async void btnFormat_Click(object sender, EventArgs e)
        {
            int pos = Editor.CurrentPosition;
            int firstVisibleLine = Editor.FirstVisibleLine;
            await Editor.FormatDocumentAsync();
            await Editor.UpdateRoslyn("Format Code");
            Editor.FirstVisibleLine = firstVisibleLine;
            Editor.GotoPosition(pos);
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            Core.Explorer.FindReplace.Show();
        }

        private void btnFindReplace_Click(object sender, EventArgs e)
        {
            Core.Explorer.FindReplace.Show();
        }

        private void FormCodeEditor_ResizeEnd(object sender, EventArgs e)
        {
            vBarEditor.UpdateScrollBar();
            hBarEditor.UpdateScrollBar();
        }

        private void panelEditor_SizeChanged(object sender, EventArgs e)
        {
            vBarEditor.UpdateScrollBar();
            hBarEditor.UpdateScrollBar();
        }

        private void panelEditor_FontChanged(object sender, EventArgs e)
        {
            vBarEditor.UpdateScrollBar();
            hBarEditor.UpdateScrollBar();
        }
    }


}
