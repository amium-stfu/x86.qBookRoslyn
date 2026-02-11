using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.ScintillaEditor.InputControls
{
  
    public partial class ControlTab : UserControl
    {
        public Action SelectTab = null;
        public Action RemoveTab = null;
        public Action CloseOtherTabs = null;
        public Action InWindow = null;
        public CodeNode Node;
        public Form LoactionForm;
        public string NodeName;
        private ContextMenuStrip contextMenu;

        public string FileName;
        public ControlTab(CodeNode node)
        {
            Name = node.Name;
            InitializeComponent();
            Node = node;
            NodeName = node.Name;

            contextMenu = new ContextMenuStrip();
            var closeOthersItem = new ToolStripMenuItem("Close all other");
            var openInWindowItem = new ToolStripMenuItem("Open in Window");
            closeOthersItem.Image = null;
            openInWindowItem.Image = null;
            closeOthersItem.Click += (s, e) => CloseOtherTabs?.Invoke();
            openInWindowItem.Click += (s, e) => InWindow?.Invoke();
            contextMenu.Items.Add(closeOthersItem);
            contextMenu.Items.Add(openInWindowItem);
            contextMenu.ShowImageMargin = false;

            this.MouseUp += ControlTab_MouseUp;
            TabName.MouseUp += ControlTab_MouseUp;
            close.MouseUp += ControlTab_MouseUp;

            string pageName = "";
            string codeName = "";

            if (node.Type == CodeNode.NodeType.SubCode)
            {
                pageName = node.PageNode.Text;
                codeName = node.Text;
            }
            else
            {
                pageName = node.Text;
                codeName = "";
            }




            // Längste Zeile bestimmen
            string longestLine = pageName.Length >= codeName.Length ? pageName : codeName;



            // Textgröße der längsten Zeile berechnen
            Size textSize = TextRenderer.MeasureText(longestLine, TabName.Font);

            // Platz für Close-Button und etwas Padding
            int closeButton = 20;
            int padding = 10;

            this.Width = textSize.Width + closeButton + padding;

            // Text mit Zeilenumbrüchen anzeigen
            TabName.Text = pageName + "\r\n" + codeName;
        }

        public ControlTab(DocumentEditor editor, bool canClose = false)
        {
            Name = editor.Target.Filename;
            FileName = editor.Target.Filename;
            InitializeComponent();
      

            string pageName = "";
            string codeName = "";

            pageName = editor.Target.Filename.Split('.')[0];
            codeName = editor.Target.Filename.Split('.')[1];

            // Längste Zeile bestimmen
            string longestLine = pageName.Length >= codeName.Length ? pageName : codeName;
            // Textgröße der längsten Zeile berechnen
            Size textSize = TextRenderer.MeasureText(longestLine, TabName.Font);

            // Platz für Close-Button und etwas Padding
            int closeButton = 20;
            int padding = 10;

            this.Width = textSize.Width + closeButton + padding;

            // Text mit Zeilenumbrüchen anzeigen
            TabName.Text = pageName + "\r\n" + codeName;

            close.Visible = canClose;
        }

        

        private void ControlTab_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenu.Show(this, e.Location);
            }
        }

        private void TabName_Click(object sender, EventArgs e)
        {
            if(SelectTab != null)
            {
                SelectTab();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if(RemoveTab != null)
            {
                RemoveTab();
            }
        }

        Color SelectColor;
        void DarkTheme()
        {
            tableLayoutPanel1.BackColor = Color.FromArgb(60, 60, 60);
            TabName.ForeColor = Color.FromArgb(190, 190, 190);
            close.ForeColor = Color.FromArgb(190, 190, 190);
            SelectColor = Color.FromArgb(25, 25, 25);
        }

        void LightTheme()
        {
            tableLayoutPanel1.BackColor = Color.FromArgb(190, 190, 190);
            TabName.ForeColor = Color.Black;
            close.ForeColor = Color.Black;
            SelectColor = Color.White;
        }

        public void ApplyTheme()
        {
            if (Theme.IsDark)
            { DarkTheme(); }
            else
            { LightTheme(); }

         
        }

        public void Selected()
        {
            tableLayoutPanel1.BackColor = SelectColor;
        }
    }

}
