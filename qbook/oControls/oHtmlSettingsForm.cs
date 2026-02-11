using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace qbook.oControls
{
    public partial class oHtmlSettingsForm : Form
    {
        public oHtmlSettingsForm()
        {
            InitializeComponent();
        }

        public string CodeHtml { get => textBoxHtml.Text; set => textBoxHtml.Text = value; }
        public string CodeCss { get => textBoxCss.Text; set => textBoxCss.Text = value; }
        public string CodeSettings { get => textBoxSettings.Text; set => textBoxSettings.Text = value; }
        private void oHtmlSettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            OnCallback(this, "apply");
        }



        private void radioButtonHtml_CheckedChanged(object sender, EventArgs e)
        {
            textBoxHtml.Visible = true;
            textBoxCss.Visible = false;
            listBoxLog.Visible = false;
        }

        private void radioButtonCss_CheckedChanged(object sender, EventArgs e)
        {
            textBoxHtml.Visible = false;
            textBoxCss.Visible = true;
            listBoxLog.Visible = false;
        }

        private void radioButtonLog_CheckedChanged(object sender, EventArgs e)
        {
            textBoxHtml.Visible = false;
            textBoxCss.Visible = false;
            listBoxLog.Visible = true;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            OnCallback(this, "delete");
        }


        public class CallbackEventArgs : EventArgs
        {
            public string Action = "";
        }
        public delegate void CallbackEventHandler(object sender, CallbackEventArgs e);
        public event CallbackEventHandler CallbackEvent;
        void OnCallback(object sender, string action)
        {
            if (CallbackEvent != null)
            {
                CallbackEventArgs ea = new CallbackEventArgs() { Action = action };
                CallbackEvent(sender, ea);
            }
        }


        BindingList<string> LogList = new BindingList<string>();
        int _count = 0;
        public void AddLog(string text)
        {
            if (_isClosing)
                return;

            _count++;
            lock (LogList)
            {
                LogList.Add("#" + _count + ":" + text);
                while (LogList.Count >= 100)
                    LogList.RemoveAt(0);
            }
            if (checkBoxScroll.Checked)
            {
                if (listBoxLog != null && !listBoxLog.IsDisposed && listBoxLog.IsHandleCreated)
                {
                    listBoxLog.Invoke((MethodInvoker)(() =>
                    {
                        //listBoxLog.TopIndex = listBoxLog.Items.Count - 1;
                        listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                    }));
                }
            }

            if (listBoxLog.DataSource == null)
                listBoxLog.DataSource = LogList;

            //textBoxLog.BeginInvoke((MethodInvoker)(() =>
            //{

            //    textBoxLog.AppendText("\r\n" + text);
            //}));
        }

        private void buttonLogToClip_Click(object sender, EventArgs e)
        {
            lock (LogList)
            {
                Clipboard.SetText(string.Join(Environment.NewLine, LogList));
            }
        }

        private void buttonLogClear_Click(object sender, EventArgs e)
        {
            lock (LogList)
            {
                LogList.Clear();
            }
        }

        private void oHtmlSettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnCallback(this, "closed");
        }

        bool _isClosing = false;
        private void oHtmlSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isClosing = true;
        }
    }
}
