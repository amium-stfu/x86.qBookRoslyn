using System.ComponentModel;
using System;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;


namespace QB.UI
{
    public partial class CameraDialog : Form
    {
        private QB.Controls.CameraCapture videoSource;
        private volatile bool captureClosing;
        private int framePending;

        public CameraDialog()
        {
            InitializeComponent();
            Shown += (sender, args) =>
            {
                try
                {
                    var devices = QB.Controls.CameraDevice.Enumerate();
                    if (devices.Count == 0) throw new InvalidOperationException("No camera is available.");
                    videoSource = new QB.Controls.CameraCapture(devices[0].Index, QueueFrame, preferDialogResolution: true);
                }
                catch (Exception ex) { MessageBox.Show("Cannot start camera: " + ex.Message); }
            };
        }

        private void QueueFrame(Bitmap bitmap)
        {
            if (captureClosing || System.Threading.Interlocked.Exchange(ref framePending, 1) != 0) return;
            var frame = (Bitmap)bitmap.Clone();
            try
            {
                BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        if (captureClosing || IsDisposed) { frame.Dispose(); return; }
                        var previous = pictureBox1.BackgroundImage;
                        pictureBox1.BackgroundImage = frame;
                        previous?.Dispose();
                    }
                    finally { System.Threading.Interlocked.Exchange(ref framePending, 0); }
                }));
            }
            catch (InvalidOperationException)
            {
                frame.Dispose();
                System.Threading.Interlocked.Exchange(ref framePending, 0);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Result { get; set; } = null;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title { get => this.Text; set => this.Text = value; }
               [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
               public Image Value { get => pictureBox1.BackgroundImage; set => this.pictureBox1.BackgroundImage = value; }



        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            
            DialogResult = DialogResult.OK;
            this.Close();
            
        }

        public static DialogResult ShowDialog(string title, ref Image value)
        {
            using var dialog = new QB.UI.CameraDialog();
            dialog.Title = title; // "CELL VALUE";
            dialog.Value = value == null ? null : (Image)value.Clone(); // value.ToString();

            dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            var parentForm = System.Windows.Forms.Application.OpenForms[0];
            dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            var dr = dialog.ShowDialog();
            value = dialog.Value == null ? null : (Image)dialog.Value.Clone();
            dialog.Value?.Dispose();
            dialog.Value = null;

            return dr;
        }

        private void buttonOpenKeyboard_Click(object sender, EventArgs e)
        {
            try
            {
                QB.UI.Windows.OpenTouchKeyboard();
                if (!QB.UI.Windows.ToggleTabTip())
                {
                    MessageBox.Show("Cannot open Touch-Keyboard");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open Touch-Keyboard (2)");
            }
        }

        private void CameraDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            captureClosing = true;
            try
            {
                videoSource?.Dispose();
                videoSource = null;
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                MessageBox.Show("Cannot stop camera: " + ex.Message);
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
