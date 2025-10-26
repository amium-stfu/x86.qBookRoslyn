using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace qbook
{
    public partial class LogForm : Form
    {
        //private ListControl listControlVars;

        public LogForm() //ScriptingClass variables)
        {
            InitializeComponent();
            listBoxLog.DataSource = qbook.Core.LogItems;
            qbook.Core.LogItems.ListChanged += LogItems_ListChanged;
            //listBoxLog.DisplayMember = "Text";
            //listBoxLog.ValueMember = "Text";


            //listenThread = new Thread(StartUdpListenThread);
            //listenThread.IsBackground = true;
            //listenThread.Start();

            this.Text = "LOG VIEW [...]";
        }

        private void LogItems_ListChanged(object sender, ListChangedEventArgs e)
        {
            modifided = true;
        }

        bool modifided = true;

        string typeFilter = "";
        void UpdateTypeFilter()
        {
            typeFilter = "";
            if (checkBoxL.Checked) typeFilter += 'L';
            if (checkBoxE.Checked) typeFilter += 'E';
            if (checkBoxW.Checked) typeFilter += 'W';
            if (checkBoxI.Checked) typeFilter += 'I';
            if (checkBoxD.Checked) typeFilter += 'D';
        }
        private void timerIdle_Tick(object sender, EventArgs e)
        {
            this.Text = $"LOG VIEW [{Program.AppUdpLogListener.UdpLoggerStatus}]"; ;

            if (modifided)
            {
                modifided = false;

                if (false)
                {
                    if (checkBoxScroll.Checked)
                    {
                        listBoxLog.Invoke((MethodInvoker)(() =>
                        {
                            //listBoxLog.Enabled = false;
                            //listBoxLog.BeginUpdate();
                            listBoxLog.DataSource = null;
                            listBoxLog.DataSource = qbook.Core.LogItems;
                            listBoxLog.DisplayMember = Text;
                            listBoxLog.TopIndex = listBoxLog.Items.Count - 1;
                            //listBoxLog.EndUpdate();
                        }));
                    }
                }

                if (checkBoxScroll.Checked)
                {
                    var lockLogItems = qbook.Core.LogItems;
                    lock (lockLogItems)
                    {
                        var preFilteredItems = qbook.Core.LogItems.Where(i => typeFilter.Contains(i.Type));
                        if (textBoxFilter.Text.Length > 0)
                        {
                            if (checkBoxFilterIsRegex.Checked)
                            {
                                try
                                {
                                    Regex filterRegex = new Regex(textBoxFilter.Text, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                    listControl.Items = new List<object>(preFilteredItems.Where(i => filterRegex.IsMatch(i.Text)));
                                    textBoxFilter.BackColor = Color.LightGreen;
                                }
                                catch (Exception ex)
                                {
                                    textBoxFilter.BackColor = Color.LightCoral;
                                }
                            }
                            else
                            {
                                listControl.Items = new List<object>(preFilteredItems.Where(i => i.Text.ToLower().Contains(textBoxFilter.Text.ToLower())));
                                textBoxFilter.BackColor = Color.LightGreen;
                            }
                        }
                        else
                        {
                            listControl.Items = new List<object>(preFilteredItems);
                            textBoxFilter.BackColor = Color.White;
                        }
                    }
                    listControl.ScrollToEnd();
                }
            }
        }

        private void ScriptingLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //    udpClient?.Close();
            //    listenThread?.Abort();
        }



        System.Timers.Timer _textChangedDelayedTimer = null;
        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            if (_textChangedDelayedTimer == null)
            {
                _textChangedDelayedTimer = new System.Timers.Timer(500);
                _textChangedDelayedTimer.Elapsed += _textChangedDelayedTimer_Elapsed;
            }
            _textChangedDelayedTimer.Stop();
            _textChangedDelayedTimer.Start();
        }

        private void _textChangedDelayedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

        private void buttonClearFilter_Click(object sender, EventArgs e)
        {
            textBoxFilter.Text = "";
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            UpdateTypeFilter();
        }

        private void checkBoxTypes_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTypeFilter();
            modifided = true;
        }

        private void listControl_Scroll(object sender, ScrollEventArgs e)
        {
            //this.Text = $"{e.NewValue} / {listControl.Items.Count}";
            if (e.NewValue >= listControl.Items.Count - 1)
                checkBoxScroll.Checked = true;
            else
                checkBoxScroll.Checked = false;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            var lockLogItems = qbook.Core.LogItems;
            lock (lockLogItems)
                qbook.Core.LogItems.Clear();
            modifided = true;
        }

        private void buttonCopyToClip_Click(object sender, EventArgs e)
        {
            string text = string.Join(Environment.NewLine, qbook.Core.LogItems.ToList());
            Clipboard.SetText(text);
        }

        private void checkBoxShowExtendedInfo_CheckedChanged(object sender, EventArgs e)
        {
            listControl.ShowExtendedInfo = checkBoxShowExtendedInfo.Checked;
            listControl.Invalidate();
        }
    }
}
