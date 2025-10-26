using System;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;

namespace QB.UI
{
    public partial class CameraDialog : Form
    {
        public CameraDialog()
        {
            InitializeComponent();
            string name = "";
            try
            {
                VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices)
                {
                    name = VideoCaptureDevice.Name;
                    //   comboBox1.Items.Add(VideoCaptureDevice.Name);
                }
                // comboBox1.SelectedIndex = 0;
                videoSource = new VideoCaptureDevice(VideoCaptureDevices[0].MonikerString);// comboBox1.SelectedIndex].MonikerString);
                                                                                          //FinalVideo.VideoResolution = new VideoCapabilities()
                videoSource.VideoResolution = selectResolution(videoSource);// ** ADDED LINE **

                videoSource.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                //FinalVideo.DesiredFrameRate = 1;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("no camera");
                return;
            }
            videoSource.Start();

            System.Threading.Thread idle = new System.Threading.Thread(Idle);
            idle.Start();
            idle.IsBackground = true;

        }

        private VideoCapabilities selectResolution(VideoCaptureDevice device)
        {
            foreach (var cap in device.VideoCapabilities)
            {
                if (cap.FrameSize.Height == 600)
                    return cap;
            

                if (cap.FrameSize.Height == 1080)
                    return cap;
                if (cap.FrameSize.Width == 1920)
                    return cap;
            }
            return device.VideoCapabilities.Last();
        }

        static Image img = null;

        private static FilterInfoCollection VideoCaptureDevices;
        private static VideoCaptureDevice videoSource;
        static bool captured = false;
        void Idle()
        {
          //  while (t)
           // Bitmap video = (Bitmap)eventArgs.Frame.Clone();
           // img = video;
        }


        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap video = (Bitmap)eventArgs.Frame.Clone();
                pictureBox1.BackgroundImage= video;
              //  img = video;
            }
            catch { }
        }



        public string Result { get; set; } = null;
        public string Title { get => this.Text; set => this.Text = value; }
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
            QB.UI.CameraDialog dialog = new QB.UI.CameraDialog();
            dialog.Title = title; // "CELL VALUE";
            dialog.Value = value; // value.ToString();

            dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            var parentForm = System.Windows.Forms.Application.OpenForms[0];
            dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            var dr = dialog.ShowDialog();
            value = dialog.Value;

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
            try
            {
                videoSource.Stop();
            }
            catch { }
        }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
