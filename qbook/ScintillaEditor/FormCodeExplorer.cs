using Microsoft.VisualStudio.SolutionPersistence.Model;
using QB.Controls;
using qbook.CodeEditor;
using qbook.ScintillaEditor.InputControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace qbook.ScintillaEditor
{
    public partial class FormCodeExplorer : Form
    {
        public FormCodeEditor Editor;
        public FormFindReplace FindReplace;
        FormBookTree BookTree;
        oPage SelectedPage = new oPage("","");
        public FormCodeExplorer()
        {
            InitializeComponent();

            this.Load += async (s, e) =>
            {
                BookTree = new FormBookTree(this);
                BookTree.TopLevel = false;
                BookTree.FormBorderStyle = FormBorderStyle.None;
                BookTree.Dock = DockStyle.Fill;
                BookTree.ShowInTaskbar = false;

                await BookTree.CreateTree();

                Editor = new FormCodeEditor(BookTree.GetSelectedEditor(), BookTree);
                Editor.TopLevel = false;
                Editor.FormBorderStyle = FormBorderStyle.None;
                Editor.Dock = DockStyle.Fill;
                Editor.ShowInTaskbar = false;
                panelExplorer.Controls.Add(Editor);
          
                
                BookTree.Show();
                Editor.Show();
                
                InitIcons();
                Editor.UpdateTabs(BookTree.GetSelectedEditor());

                flowLayoutPageData.Controls.Add(new TextBoxWithLabel("Page Name:", () => SelectedPage.Name, v => SelectedPage.Name = v) { ReadOnly = true });
                flowLayoutPageData.Controls.Add(new TextBoxWithLabel("Page Text:", () => SelectedPage.Text, v => SelectedPage.Text = v));

                flowLayoutPageData.Controls.Add(
                    new TextBoxWithLabel(
                        "Page Format:",
                        () => SelectedPage.Format,
                        v => SelectedPage.Format = v,
                        new List<string> { "A4", "16/9", "16/10" }
                    )
                );

                ApplyTheme();



            };  
            
            this.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                this.Hide();
            };
            FindReplace = new FormFindReplace(this);
        }
        public void SetTarget(DocumentEditor editor) 
        {

           

            Editor.SetTarget(editor);
            Editor.UpdateTabs(editor);

            if(editor.Page != null)
            {
                SelectedPage = editor.Page;
                RefreshPageData();
            }
            else
            {
                SelectedPage = new oPage("","");
              
            }
            FindReplace.Editor = SelectedCodeNode.Editor;
            RefreshPageData();
        }
        private void ApplyTheme()
        {
            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: Theme.IsDark);
            FindReplace.ApplyTheme();

            LayoutRoot.BackColor = Theme.FormBackColor;
            panelHeader.BackColor = Theme.BackColor;
            panelFooter.BackColor = Theme.BackColor;

            foreach (System.Windows.Forms.Control control in panelHeader.Controls)
            {

                if (control is System.Windows.Forms.Button)
                {
                    System.Windows.Forms.Button b = control as System.Windows.Forms.Button;
                    b.BackColor = Theme.BackColor;
                    //  b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    b.ForeColor = Theme.ButtonForeColor;
                    b.FlatAppearance.BorderColor = Theme.BackColor;
                    Bitmap pic = b.Image as Bitmap;
                
                    pic = BitmapTools.ReplaceColor(pic, Theme.IconColorBefore, Theme.ButtonIconColor, 50);
                
                    pic = BitmapTools.ResizeExact(pic, 28, 28);
                    var old = b.Image;
                    b.Image = pic;
                    old?.Dispose();
                }
            }
            Editor?.ApplyTheme();
            BookTree?.ApplyTheme();
            Editor.RefreshTabs();

            flowLayoutPageData.BackColor = Theme.BackColor;
            foreach (System.Windows.Forms.Control c in flowLayoutPageData.Controls)
            {
                if (c is InputControls.TextBoxWithLabel tb) tb.ApplyTheme();

            }
        }
        public void InitIcons()
        {
            foreach (System.Windows.Forms.Control control in panelHeader.Controls)
            {

                if (control is System.Windows.Forms.Button)
                {
                    System.Windows.Forms.Button b = control as System.Windows.Forms.Button;
                    b.BackColor = Theme.BackColor;
                    //  b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    b.ForeColor = Theme.ButtonForeColor;
                    b.FlatAppearance.BorderColor = Theme.BackColor;
                    Bitmap pic = b.Image as Bitmap;
                    pic = BitmapTools.ReplaceColor(pic, Color.Black, Theme.ButtonIconColor, 100);
                    pic = BitmapTools.ResizeExact(pic, 28, 28);
                    var old = b.Image;
                    b.Image = pic;
                    old?.Dispose();
                }
            }
            Theme.IconColorBefore = Theme.ButtonIconColor;

        }
        private void btnToggleTheme_Click(object sender, EventArgs e)
        {
            if(Theme.IsDark)
                Theme.Current = Theme.EditorTheme.Light;
            else
                Theme.Current = Theme.EditorTheme.Dark;

            ApplyTheme();
        }
        private async void btnRebuild_Click(object sender, EventArgs e)
        {

            bool codeError = BookTree.CheckCode;

            if(codeError)
            {
                SetStatusText("qbook rebuild failed! Please fix code errors first.", Color.Red);
                return;
            }

            bool rebuildSuccess = await VisualRebuild();

            if (rebuildSuccess)
            {
                BookRuntime.InitializeAll();
                MainForm.SetStatusText("qbook rebuils successfully!", 3000);
                SetStatusText("qbook rebuils successfully!");
            }
        }
        public async Task<bool> VisualRebuild()
        {
 
            var task = BookRuntime.BuildAssembly();
            while (!task.IsCompleted)
            {
                SetStatusText(BookRuntime.BuildResult);
            }
            await task;
      
            if (!BookRuntime.BuildSuccess)
            {
                return false;
            }
            return true;
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            BookRuntime.RunAll();

            SetStatusText("[Build] running...");
        }
        public void SetStatusText(string text, Color? color = null, Color? backColor = null)
        {
            if (color == null)
                color = Color.Black;
            if (backColor == null)
                backColor = SystemColors.Control;

            labelStatus.BeginInvoke((System.Action)(() =>
            {
                labelStatus.Text = text;
            }));
        }
        private async void btnSave_Click(object sender, EventArgs e)
        {
            await Core.SaveInFolder();
            SetStatusText("qbook saved successfully!");
        }
        private async void btnReload_Click(object sender, EventArgs e)
        {
            await Core.OpenQbookAsync(System.IO.Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename));
            await BookTree.CreateTree();
            SetStatusText("qbook reloaded successfully!");

        }

        public async Task GoToDefinition() => await BookTree.GoToDefinition();
        public async Task RenameSymbol() => await BookTree.RenameSymbol();  

        public BookNode SelectedCodeNode => BookTree.SelectedCodeNode;

        public BookTreeView ProjectTree => BookTree.bookTreeView;


        public void RefreshPageData()
        {
            foreach (System.Windows.Forms.Control c in flowLayoutPageData.Controls)
            {
                if (c is InputControls.TextBoxWithLabel tb)
                {
                    tb.RefreshValue();
                }
            }
        }



    }

    public static class Theme
    {
        public static Color FormBackColor = Color.White;

        public static Color PanelSplitterColor = Color.FromArgb(180, 180, 180);
        public static Color PanelBackColor = Color.FromArgb(180, 180, 180);
        public static Color ThumbColor = Color.FromArgb(180, 180, 180);
        public static Color BackColor = Color.FromArgb(230, 230, 230);
        public static Color GridBackColor = Color.FromArgb(220, 220, 220);
        public static Color GridForeColor = Color.Black;
        public static Color ButtonBackColor = Color.FromArgb(230, 230, 230);
        public static Color ButtonForeColor = Color.Black;
        public static Color ButtonIconColor = Color.FromArgb(100, 100, 100);
        public static Color StringInputColor = Color.FromArgb(200, 200, 200);
        public static Color TreeNodeSelectColor = Color.DarkOrchid;
        public static Color TreeNodeDefaultColor = Color.Black;

        public static Color IconColorBefore = Color.Black;
        public enum EditorTheme { Light, Dark }

        static EditorTheme current = EditorTheme.Light;
        public static EditorTheme Current
        {
            get => current;
            set
            {
                if (value == EditorTheme.Dark)
                    ApplyDark();
                else
                    ApplyLight();
                current = value;
            }

        }
        public static bool IsDark => Current == EditorTheme.Dark;

        static void ApplyDark()
        {

            IconColorBefore = ButtonIconColor;
            FormBackColor = Color.Black;
            PanelSplitterColor = Color.FromArgb(70, 70, 70);
            PanelBackColor = Color.FromArgb(60, 60, 60);
            ThumbColor = Color.FromArgb(100, 100, 100);
            BackColor = Color.FromArgb(40, 40, 40);
            GridForeColor = Color.FromArgb(220, 220, 220);
            GridBackColor = Color.FromArgb(70, 70, 70);
            ButtonBackColor = Color.FromArgb(40, 40, 40);
            ButtonForeColor = Color.FromArgb(190, 190, 190);
            ButtonIconColor = Color.FromArgb(150, 150, 150);
            StringInputColor = Color.FromArgb(40, 40, 40);
            TreeNodeSelectColor = Color.Plum;
            TreeNodeDefaultColor = Color.FromArgb(220, 220, 220);

        }
        static void ApplyLight()
        {
            IconColorBefore = ButtonIconColor;
            FormBackColor = Color.White;
            PanelSplitterColor = Color.FromArgb(180, 180, 180);
            PanelBackColor = Color.FromArgb(230, 230, 230);
            ThumbColor = Color.FromArgb(180, 180, 180);
            BackColor = Color.FromArgb(210, 210, 210);
            GridBackColor = Color.FromArgb(220, 220, 220);
            GridForeColor = Color.Black;
            ButtonBackColor = Color.FromArgb(230, 230, 230);
            ButtonForeColor = Color.Black;
            ButtonIconColor = Color.FromArgb(100, 100, 100);
            StringInputColor = Color.FromArgb(200, 200, 200);
            TreeNodeSelectColor = Color.DarkOrchid;
            TreeNodeDefaultColor = Color.Black;

        }

    }


}
