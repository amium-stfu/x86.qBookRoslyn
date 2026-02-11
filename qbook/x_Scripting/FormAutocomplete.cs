using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace qbook.Scripting
{
    public partial class FormAutocomplete : Form
    {
        public FormAutocomplete()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public class AutoCompleteItem
        {
            public AutoCompleteItem()
            {

            }

            public string Name;
            public string Params;
            //public Type Type;
            public string Kind; //f=field, p=property, m=method
            public string TypeName;
            public string Parent;
            public List<string> Methods;
            public List<string> Properties;

            public override string ToString()
            {
                //if (TypeName == "prop")
                //    return "[] " + Name + ":" + TypeName;
                //else if (TypeName == "func")
                //    return "() " + Name + ":" + TypeName;
                //else
                //    return Name + " [" + TypeName + "]";

                return $"[{Kind}] {Name}{(string.IsNullOrEmpty(Params) ? "" : "(" + Params + ")")} :{TypeName.ToLower()}";
            }
        }

        List<AutoCompleteItem> _Items;
        public List<AutoCompleteItem> Items
        {
            get
            {
                //return listBoxItems.Items.Cast<String>().ToList();
                return _Items;
            }
            set
            {
                //listBoxItems.Items.Clear();
                _Items = value;
                Filter = "";
                //listBoxItems.Items.AddRange(value.ToArray());
            }
        }

        public string SelectedText
        {
            get
            {
                AutoCompleteItem item = listBoxItems.SelectedItem as AutoCompleteItem;
                string name = item.Name;
                if (name.StartsWith("\\") || item.Name.StartsWith("."))
                    name = name.Substring(1);

                return name;
            }
        }

        public void NextItem()
        {
            if (listBoxItems.Items == null || listBoxItems.Items.Count < 2)
                return;
            if (listBoxItems.SelectedIndex < listBoxItems.Items.Count - 1)
            {
                listBoxItems.SelectedIndex++;
            }
        }
        public void PrevItem()
        {
            if (listBoxItems.Items == null || listBoxItems.Items.Count < 2)
                return;
            if (listBoxItems.SelectedIndex > 0)
            {
                listBoxItems.SelectedIndex--;
            }
        }

        string _filter = "";
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                listBoxItems.Items.Clear();
                listBoxItems.Items.AddRange(Items.Where(i => i.Name.ToLower().Contains(_filter.ToLower())).ToArray());
                if (listBoxItems.Items.Count > 0)
                    listBoxItems.SelectedIndex = 0;
            }

        }

        class FormToolTip : Form
        {
            Label labelTitle;
            Label labelInfo;
            public FormToolTip()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                labelTitle = new Label();
                labelTitle.Height = 20;
                labelTitle.Dock = DockStyle.Top;
                labelTitle.Font = new Font("Tahoma", 9, FontStyle.Regular);
                labelInfo = new Label();
                labelInfo.Dock = DockStyle.Fill;
                labelInfo.Font = new Font("Tahoma", 8);
                this.Controls.Add(labelInfo);
                this.Controls.Add(labelTitle);
                labelTitle.Text = "Heading";
                labelInfo.Text = "Info1\r\nInfo2\r\nInfo3";
                labelTitle.BackColor = Color.Yellow;
                labelInfo.BackColor = Color.LightYellow;

                //this.Enabled = false;
            }

            public void ShowTooltip(Form owner)
            {
                var sizeTitle = TextRenderer.MeasureText(Title, labelTitle.Font);
                var sizeInfo = TextRenderer.MeasureText(Info, labelInfo.Font);
                this.Height = sizeInfo.Height + 22;
                this.Width = Math.Max(sizeTitle.Width, sizeInfo.Width) + 6;

                if (!this.Visible)
                    base.Show(owner);
            }

            public string Title { get => labelTitle.Text; set => labelTitle.Text = value; }
            public string Info
            {
                get => labelInfo.Text;
                set
                {
                    labelInfo.Text = value;
                }
            }
        }
        FormToolTip fTooltip = null;

        private void listBoxItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] infoLines = null;
            //var aci = listBoxItems.SelectedItem as AutoCompleteItem;
            var aci = listBoxItems.Items[listBoxItems.SelectedIndex] as AutoCompleteItem;
            if (aci != null)
            {
                OnInfoCallback(aci.Parent, aci.Name, out infoLines);
                string title = "";
                if (infoLines != null)
                    title = $"{aci.Name} [{infoLines.Count()}]";
                else
                    title = $"{aci.Name} [?]";
                this.toolTip.BackColor = Color.GreenYellow;
                string info = "";
                if (infoLines != null)
                    info = string.Join("\r\n", infoLines);
                //this.toolTip.RemoveAll();
                ////this.toolTip.SetToolTip(this.listBoxItems, )
                //this.toolTip.ToolTipTitle = title;
                ////this.toolTip.Show(info, this.listBoxItems);
                //this.toolTip.RemoveAll();
                //this.toolTip.SetToolTip(this.listBoxItems, title);


                if (true)
                {
                    if (fTooltip == null || fTooltip.IsDisposed || !fTooltip.IsHandleCreated)
                    {
                        fTooltip = new FormToolTip();
                        //fTooltip.TopMost = true;
                        fTooltip.ShowInTaskbar = false;
                        fTooltip.StartPosition = FormStartPosition.Manual;
                    }
                    fTooltip.Title = title;
                    fTooltip.Info = info;
                    fTooltip.Location = new Point(this.Location.X + this.Width, this.Location.Y);
                    fTooltip.ShowTooltip(this);
                    fTooltip.BringToFront();
                    this.Focus();
                    this.VisibleChanged += FormAutocomplete_VisibleChanged;
                }


                if (false)
                {
                    this.toolTip.Active = true;
                    this.toolTip.AutomaticDelay = 0;
                    this.toolTip.AutoPopDelay = 30000;
                    this.toolTip.ReshowDelay = 50;
                    this.toolTip.ShowAlways = true;
                    this.toolTip.Show(info, this.listBoxItems); //.listBoxItems);
                    this.toolTip.ToolTipTitle = title;
                    this.toolTip.Show(info, this.listBoxItems); //.listBoxItems);
                }
            }
            else
            {
                this.toolTip.Hide(this.listBoxItems);
            }
        }

        private void FormAutocomplete_VisibleChanged(object sender, EventArgs e)
        {
            fTooltip?.Hide();
        }

        public class InfoCallbackEventArgs : EventArgs
        {
            public string Parent;
            public string Name;
            public string[] InfoLines;
        }
        public delegate void SelectedPageChangedEventHandler(InfoCallbackEventArgs e);
        public event SelectedPageChangedEventHandler InfoCallbackEvent;
        void OnInfoCallback(string parent, string name, out string[] infoLines)
        {
            infoLines = null;
            if (InfoCallbackEvent != null)
            {
                InfoCallbackEventArgs ea = new InfoCallbackEventArgs() { Parent = parent, Name = name };
                InfoCallbackEvent(ea);
                infoLines = ea.InfoLines;
            }
        }

        private void listBoxItems_SelectedValueChanged(object sender, EventArgs e)
        {

        }
    }
}
