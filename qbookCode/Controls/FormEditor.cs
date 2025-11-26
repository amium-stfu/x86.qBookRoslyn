using qbookCode.Controls.InputControls;
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

using System.Windows.Forms;

using static System.ComponentModel.Design.ObjectSelectorEditor;


namespace qbookCode.Controls
{
    public partial class FormEditor : Form
    {
      
        DataGridView GridViewDiagnosticOutput;
        DataGridView gridViewRuntimeOutput;
        DataGridView gridLog;
        BindingSource MethodenBindingSource = new BindingSource();

        // Workaround VS Bug 
        private DoubleBufferedPanel PanelTabs;
        private Scrollbars.ScintillaVerticalBar vBarEditor;
        private Scrollbars.ScintillaHorizontalBar hBarEditor;
        private Scrollbars.GridViewVerticalBar vBarOutputs;
        private Scrollbars.GridViewVerticalBar vBarMethodes;

        FormBookTree BookTreeView;

        DocumentEditor Editor;
        public FormEditor(DocumentEditor edior, FormBookTree bookTreeView = null)
        {
            Editor = edior;
            InitializeComponent();
            InitializeCustomControl();
            UiDispatcher.Init();

        
            BookTreeView = bookTreeView;

            GridViewDiagnosticOutput = new DataGridView();
            gridViewRuntimeOutput = new DataGridView();
           
            gridLog = new DataGridView();

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

            this.Shown += (s, e) =>
            {
                UpdateTabs(Editor);
                ApplyTheme();
            };


            PanelTabs.HandleCreated += (s, e) =>
            {
                RefreshTabs("HandleCreated");
            };

        }

        public void Reset()
        {
            panelEditor.Controls.Clear();
            BookTreeView.CleanupBeforeLoad();
           

        }

        public async Task OpenNodeByName(string name)
        {
            await BookTreeView.OpenNodeByName(name);
        }

        public void SetTarget(DocumentEditor editor)
        {
            if (panelEditor.Controls.Count == 1)
            {
                if (panelEditor.Controls[0] == editor)
                    return;
            }

            Editor = editor;
            panelEditor.Controls.Clear();
            Editor.Dock = DockStyle.Fill;
            panelEditor.Controls.Add(Editor);

            vBarEditor.Init(Editor);
            hBarEditor.Init(Editor);

            UpdateSources();
            ApplyTheme();

        }


        private void btnShowHidden_Click(object sender, EventArgs e)
        {
            Program.LogInfo("Toggling hidden lines");
            Editor.ToggleHidenLines();
        }

        public void InitGridViews()
        {
            #region ComLog Output

            gridLog.AutoGenerateColumns = false;
            gridLog.ReadOnly = true;
            gridLog.AllowUserToAddRows = false;

            Logger.LogBindingSource.ListChanged += LogBindingSource_ListChanged;


            gridLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TimeStamp",
                DataPropertyName = "TimeStamp",
                HeaderText = "Time",
                Width = 150,

            });

            gridLog.Columns["TimeStamp"].DefaultCellStyle.Format = "yyyy-MM-dd HH.mm:ss";

            gridLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Level",
                DataPropertyName = "Level",
                HeaderText = "Level",
                Width = 80
            });

            gridLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Message",
                DataPropertyName = "Message",
                HeaderText = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            //  gridLog.DataSource = Logger.LogBindingSource;

            //gridLog.AllowUserToResizeColumns = false;
            //gridLog.AllowUserToAddRows = false;
            //gridLog.RowHeadersVisible = false;
            //gridLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //gridLog.MultiSelect = false;
            //gridLog.ReadOnly = true;
            //gridLog.BackgroundColor = Color.Red;
            //gridLog.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //gridLog.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            //gridLog.ColumnHeadersVisible = false;
            //gridLog.RowHeadersVisible = false;
            //gridLog.Dock = DockStyle.Fill;
            //gridLog.AllowUserToAddRows = false;
            //gridLog.AllowUserToDeleteRows = false;
            //gridLog.AllowUserToOrderColumns = true;
            //gridLog.AllowUserToResizeColumns = false;
            //gridLog.BackgroundColor = System.Drawing.Color.LightGray;
            //gridLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            //gridLog.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            //gridLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //gridLog.ColumnHeadersVisible = false;
            //gridLog.Dock = System.Windows.Forms.DockStyle.Fill;
            //gridLog.DefaultCellStyle.ForeColor = Color.Black;
            //gridLog.Location = new System.Drawing.Point(46, 25);
            //gridLog.Margin = new System.Windows.Forms.Padding(0);
            //gridLog.Name = "dataGridOutput";
            //gridLog.Size = new System.Drawing.Size(832, 115);
            //gridLog.TabIndex = 0;
            //gridLog.ScrollBars = System.Windows.Forms.ScrollBars.None;




            #endregion





            #region Roslyn Diagnostic Output


            GridViewDiagnosticOutput.DataSource = Editor.Output;
            GridViewDiagnosticOutput.DataBindingComplete += (s, e) =>
            {
                GridViewDiagnosticOutput.Columns["Page"].Width = 0;
                GridViewDiagnosticOutput.Columns["Class"].Width = 0;
                GridViewDiagnosticOutput.Columns["Position"].Width = 0;
                GridViewDiagnosticOutput.Columns["Length"].Width = 0;
                GridViewDiagnosticOutput.Columns["Type"].Width = 0;


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

            gridViewRuntimeOutput.AllowUserToResizeColumns = false;
            gridViewRuntimeOutput.DataSource = RuntimeManager.RuntimeErrors;
            gridViewRuntimeOutput.DataBindingComplete += (s, e) =>
            {
                gridViewRuntimeOutput.Columns["Count"].Width = 40;
                gridViewRuntimeOutput.Columns["RepeatMs"].Width = 50;
                gridViewRuntimeOutput.Columns["Key"].Visible = false;
                gridViewRuntimeOutput.Columns["File"].Width = 150;
                gridViewRuntimeOutput.Columns["Methode"].Width = 150;
                gridViewRuntimeOutput.Columns["Line"].Visible = false;
                gridViewRuntimeOutput.Columns["Col"].Visible = false;
                gridViewRuntimeOutput.Columns["Reason"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };
            gridViewRuntimeOutput.CellFormatting += (s, e) =>
            {

                if (gridViewRuntimeOutput.Columns[e.ColumnIndex].Name == "RepeatMs" && e.Value != null)
                {
                    if (int.TryParse(e.Value.ToString(), out int ms))
                    {
                        e.Value = $"{ms}ms";
                        e.FormattingApplied = true;
                    }
                }


            };

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
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        string file = gridViewRuntimeOutput.Rows[e.RowIndex].Cells["File"].Value.ToString();
                        int line = (int)gridViewRuntimeOutput.Rows[e.RowIndex].Cells["Line"].Value;
                        BookNode node = BookTreeView.GetNodeByFilename(file);
                        BookTreeView.OpenNodeByName(node.Name);
                        node.Editor.HighlightLine(line, Color.Red);
                        node.Editor.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Program.LogError("Error navigating to runtime error location", ex);
                    }

                }
            };
        }


        //Workaround VS Bug

       private void InitializeCustomControl() 
        {
            vBarEditor = new qbookCode.Controls.Scrollbars.ScintillaVerticalBar();
            hBarEditor = new qbookCode.Controls.Scrollbars.ScintillaHorizontalBar();
            vBarOutputs = new qbookCode.Controls.Scrollbars.GridViewVerticalBar();
            vBarMethodes = new qbookCode.Controls.Scrollbars.GridViewVerticalBar();
            PanelTabs = new DoubleBufferedPanel();

            // 
            // PanelTabs
            // 
            PanelTabs.Dock = DockStyle.Fill;
            PanelTabs.Location = new Point(56, 0);
            PanelTabs.Margin = new Padding(0);
            PanelTabs.Name = "PanelTabs";
            PanelTabs.Size = new Size(1263, 42);
            PanelTabs.TabIndex = 8;
            // 
            // vBarEditor
            // 
            vBarEditor.Dock = DockStyle.Fill;
            vBarEditor.Location = new Point(1319, 42);
            vBarEditor.Margin = new Padding(0);
            vBarEditor.Name = "vBarEditor";
            vBarEditor.SetBackColor = Color.White;
            vBarEditor.SetForeColor = Color.Black;
            vBarEditor.Size = new Size(23, 470);
            vBarEditor.TabIndex = 9;
            // 
            // hBarEditor
            // 
            hBarEditor.Dock = DockStyle.Fill;
            hBarEditor.Location = new Point(56, 512);
            hBarEditor.Margin = new Padding(0);
            hBarEditor.Name = "hBarEditor";
            hBarEditor.SetBackColor = Color.LightGray;
            hBarEditor.SetForeColor = Color.DodgerBlue;
            hBarEditor.Size = new Size(1263, 23);
            hBarEditor.TabIndex = 10;
            // 
            // vBarOutputs
            // 
            vBarOutputs.Dock = DockStyle.Fill;
            vBarOutputs.Location = new Point(1319, 34);
            vBarOutputs.Margin = new Padding(0);
            vBarOutputs.Name = "vBarOutputs";
            vBarOutputs.SetBackColor = Color.LightGray;
            vBarOutputs.SetForeColor = Color.DodgerBlue;
            vBarOutputs.Size = new Size(23, 107);
            vBarOutputs.TabIndex = 8;
            // 
            // vBarMethodes
            // 
            vBarMethodes.Dock = DockStyle.Fill;
            vBarMethodes.Location = new Point(351, 25);
            vBarMethodes.Margin = new Padding(0);
            vBarMethodes.Name = "vBarMethodes";
            vBarMethodes.SetBackColor = Color.LightGray;
            vBarMethodes.SetForeColor = Color.DodgerBlue;
            vBarMethodes.Size = new Size(23, 381);
            vBarMethodes.TabIndex = 4;

            EditorLayoutPanel.Controls.Add(PanelTabs, 2, 0);
            EditorLayoutPanel.Controls.Add(vBarEditor, 3, 1);
            EditorLayoutPanel.Controls.Add(hBarEditor, 2, 2);
            tblViewMethodes.Controls.Add(vBarMethodes, 1, 1);
            TablePanelOutputs.Controls.Add(vBarOutputs, 2, 2);



        }
       
        

   


            #endregion

        private void LogBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            // Nur reagieren, wenn eine neue Zeile hinzugekommen ist
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (gridLog.Rows.Count > 0)
                {
                    int lastRow = gridLog.Rows.Count - 1;

                    // Erste sichtbare Zeile nach unten schieben
                    gridLog.FirstDisplayedScrollingRowIndex = lastRow;

                    // Optional: letzte Zeile markieren
                    gridLog.ClearSelection();
                    gridLog.Rows[lastRow].Selected = true;
                }
            }
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
            //gridLog.BackgroundColor = Theme.GridBackColor;
            //gridLog.ForeColor = Theme.GridForeColor;

            //gridLog.BackgroundColor = Theme.GridBackColor;
            //gridLog.ForeColor = Theme.GridForeColor;




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
            RefreshTabs("UpdateTabs");
        }

        public void RemoveTab(string filename)
        {
            DictTabs.Remove(filename);
            RefreshTabs("RemoveTab");
        }

        private void CloseAllOtherTabs()
        {
            List<string> keysToRemove = new List<string>();
            foreach (var key in DictTabs.Keys)
            {
                if (DictTabs[key].FileName != BookTreeView.SelectedNode.Editor.Target.Filename)
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                DictTabs.Remove(key);
            }
            RefreshTabs("ClosedAllOther");
        }
        public void RefreshTabs(string sender)
        {
            PanelTabs.SuspendLayout();

            if (PanelTabs.IsHandleCreated)
            {
                PanelTabs.Invoke((System.Action)(() =>
                {
                    PanelTabs.Controls.Clear();
                    foreach (ControlTab tab in DictTabs.Values.OrderBy(t => t.Name))
                    {
                        PanelTabs.Controls.Add(tab);
                    }
                }));
            }
            else
            {
                // Optionally, defer the refresh until the handle is created
                PanelTabs.HandleCreated += (s, e) => RefreshTabs("DeferredByHandleCreated");
                return;
            }

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
            gridLog.Dock = DockStyle.Fill;
            panelOutput.Controls.Add(gridLog);
            vBarOutputs.Init(gridLog);
            UpdateOutputButtons();

        }


        public bool IsRuntimeGridVisible => this.panelOutput.Controls.Contains(this.gridViewRuntimeOutput);
        public bool IsEditorGridVisible => this.panelOutput.Controls.Contains(this.GridViewDiagnosticOutput);
        public bool IsFindReplaceGridVisible => this.panelOutput.Controls.Contains(this.gridLog);

        void UpdateOutputButtons()
        {

            if (!Theme.IsDark)
            {
                btnRuntime.ForeColor = Color.FromArgb(40, 40, 40);
                btnEditorOutput.ForeColor = Color.FromArgb(40, 40, 40);
                btnComLog.ForeColor = Color.FromArgb(40, 40, 40);

                Color sel = Color.FromArgb(220, 220, 220);
                Color usel = Color.FromArgb(190, 190, 190);
                btnEditorOutput.BackColor = IsEditorGridVisible ? sel : usel;
                btnRuntime.BackColor = IsRuntimeGridVisible ? sel : usel;
                btnComLog.BackColor = IsFindReplaceGridVisible ? sel : usel;
            }
            else
            {
                btnRuntime.ForeColor = Color.FromArgb(230, 230, 230);
                btnEditorOutput.ForeColor = Color.FromArgb(230, 230, 230);
                btnComLog.ForeColor = Color.FromArgb(230, 230, 230);

                Color sel = Color.FromArgb(70, 70, 70);
                Color usel = Color.FromArgb(50, 50, 50);
                btnEditorOutput.BackColor = IsEditorGridVisible ? sel : usel;
                btnRuntime.BackColor = IsRuntimeGridVisible ? sel : usel;
                btnComLog.BackColor = IsFindReplaceGridVisible ? sel : usel;
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
            RefreshTabs("Resize");
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
