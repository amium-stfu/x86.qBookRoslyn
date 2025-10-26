using QB.Controls;
using System;
using System.Drawing;

namespace qbook
{
    [Serializable]
    public class oImage : oControl
    {
        public oImage()
        {
        }
        public oImage(string name, string text) : base(name, text)
        {
        }

        /*
        string _Data;
        public string Data
        {
            get 
            { 
                return _Data; 
            }
            set 
            { 
                _Data = value;
                img = Draw.Base64ToImage(_Data);
            }
        }
        */
        public string Data;

        SolidBrush tGray = new SolidBrush(Color.FromArgb(50, Color.Gray));
        SolidBrush tDarkOrange = new SolidBrush(Color.FromArgb(50, Color.DarkOrange));


        private Image img = null;
        private string imgData = null;
        public override void Render()
        {

            base.Render();
            //Image img = Draw.Base64ToImage(Data);

            //do not re-decode the image unless changed
            if (Data != imgData)
            {
                imgData = Data;
                img = Draw.Base64ToImage(imgData);
            }

            if (img == null)
                return;
            Draw.Image(img, Bounds.X, Bounds.Y, Bounds.X + Bounds.W, Bounds.Y + Bounds.H);

            if (qbook.Core.ThisBook.DesignMode)
            {
                float y = 1;
                Draw.Text(TextL, Bounds.X + 1, Bounds.Y + y, 0, Draw.fontText, Selected ? Color.Orange : Color.Black);
            }
            Frame(true, true);
        }
    }
}
