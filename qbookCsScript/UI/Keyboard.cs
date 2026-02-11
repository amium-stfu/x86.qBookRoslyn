using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QB.UI
{
    public partial class Keyboard : Form
    {
        string[] qwertz = { "q", "w", "e", "r", "t", "z", "u", "i", "o", "p", "<sep>", "a", "s", "d", "f", "g", "h", "j", "k", "l", "<sep>", "<capslock>", "y", "x", "c", "v", "b", "n", "m", ".", ";", "_", "<space>", "+", "-", "°" };

        private Func<string> getStringTarget = null;
        private Action<string> setStringTarget = null;
        public Keyboard(Func<string> getter, Action<string> setter, string text)
        {
            InitializeComponent();

            getStringTarget = getter;
            setStringTarget = setter;
            btnCheck.Text = "\u2714";
            btnAbort.Text = "\u274C";
            // btnClear.Text = "\u2421";
            Text = text;
            tbResult.Text = getStringTarget();
            tbResult.Select();
        }

        public void ShowNumblock()
        {
            string[] num = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "." };
            for (int i = 0; i < num.Length; i++)
            {
                KeyboardButton kb = new KeyboardButton(num[i], tbResult);

                if (num[i] == "0") kb.Size = new Size(80, 40);




                kb.Dock = DockStyle.Left;
                kb.Anchor = AnchorStyles.Left;
                Numblock.Controls.Add(kb);
            }


        }

        public void Qertz()
        {
            for (int i = 0; i < qwertz.Length; i++)
            {
                if (qwertz[i] == "<capslock>")
                {
                    KeyboardButtonCapsLock capsLock = new KeyboardButtonCapsLock(tbResult, panelQuertz);
                    capsLock.Dock = DockStyle.Left;
                    capsLock.Anchor = AnchorStyles.Left;
                    panelQuertz.Controls.Add(capsLock);
                }
                else if (qwertz[i] == "<sep>")
                {
                    KeyboardButton kb = new KeyboardButton("", tbResult);
                    kb.Text = "";
                    kb.Width = 20;
                    kb.Dock = DockStyle.Left;
                    kb.Anchor = AnchorStyles.Left;
                    kb.BackColor = Color.Transparent;
                    panelQuertz.Controls.Add(kb);

                }

                else if (qwertz[i] == "<space>")
                {
                    KeyboardButton kb = new KeyboardButton(qwertz[i], tbResult);
                    kb.Text = " ";
                    kb.Width = 240;
                    kb.Dock = DockStyle.Left;
                    kb.Anchor = AnchorStyles.Left;
                    panelQuertz.Controls.Add(kb);
                }
                else
                {
                    KeyboardButton kb = new KeyboardButton(qwertz[i], tbResult);
                    kb.Dock = DockStyle.Left;
                    kb.Anchor = AnchorStyles.Left;
                    panelQuertz.Controls.Add(kb);
                }
            }


        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbResult.Text = "";
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btnCheck_Click(object sender, EventArgs e)
        {
            setStringTarget(tbResult.Text.ToString());
            this.Close();

        }

        private void tbResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                setStringTarget(tbResult.Text.ToString());
                this.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {

                Close();
            }

        }
    }
    public partial class KeyboardButton : Button
    {
        TextBox TbResult;
        public KeyboardButton(string keyValue, TextBox tbResult,int width=40, int height = 40)
        {
            TbResult = tbResult;
            Text = keyValue;
            Dock = System.Windows.Forms.DockStyle.Fill;
            Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Size = new System.Drawing.Size(width, height);
            UseVisualStyleBackColor = true;
            Margin = new Padding(0);
            Padding = new Padding(0);
            Click += new System.EventHandler(this.click);
        }

        private void click(object sender, EventArgs e)
        {
            TbResult.AppendText(this.Text);
        }

    }
    public partial class KeyboardButtonCapsLock : Button
    {
        TextBox TbResult;

        string[] quertz = null;

        FlowLayoutPanel panel = null;
        public KeyboardButtonCapsLock(TextBox tbResult, FlowLayoutPanel panel)
        {
            TbResult = tbResult;
            Text = "\u21E7";
            Dock = System.Windows.Forms.DockStyle.Fill;
            Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Size = new System.Drawing.Size(40, 40);
            UseVisualStyleBackColor = true;
            Margin = new Padding(0);
            Padding = new Padding(0);
            Click += new System.EventHandler(this.click);
            this.panel = panel;
        }

        private void click(object sender, EventArgs e)
        {
            if(this.Text == "\u21E7")
            {
                this.Text = "\u21E9";
                capslock();
            }
            else
            {
                this.Text = "\u21E7";
                unCapslock();
            }

        }

        private void capslock()
        {
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                if (panel.Controls[i] is KeyboardButton)
                {
                    KeyboardButton kb = (KeyboardButton)panel.Controls[i];
                    kb.Text = kb.Text.ToUpper();
                }
            }
        }

        private void unCapslock()
        {
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                if (panel.Controls[i] is KeyboardButton)
                {
                    KeyboardButton kb = (KeyboardButton)panel.Controls[i];
                    kb.Text = kb.Text.ToLower();
                }
            }
        }


    }







 }
