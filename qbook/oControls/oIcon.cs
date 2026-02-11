using QB;
using QB.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace qbook
{
    public enum Icon
    {
        None, Object, Text, Button, Footnote, Section, Select, Visible, Delete, Add, Clone, CopyToClipboard, PasteFromClipboard, MoveUp, MoveDown, CenterLayout, LeftLayout,
        Save, Pdf, Filename, Filedate, User, Home, End, Back10, Forward10, CodeEdit,PageNumber, PageFunction
    };
    public enum oObject
    {
        None, Tag, Text, Wire, Part
    };
    [Serializable]
    public class oIcon
    {
        public oControl Parent;
        public delegate void CallBackFunction(object sender);
        CallBackFunction OnClickCallback;
        CallBackFunction OnRightClickCallback;
        public oIcon(oControl parent, Icon icon, Pen pen, string text, float x, float y, float w, float h, CallBackFunction onClick, CallBackFunction onRightClick = null)// Action<string> callback)
        {
            Parent = parent;
            this.Icon = icon;
            Pen = pen;
            Text = text;
            X = x;
            Y = y;
            W = w;
            H = h;
            OnClickCallback = onClick;
            OnRightClickCallback = onRightClick;
        }

        public Icon Icon;
        public Pen Pen;
        public string Text;
        public float X;
        public float Y;
        public float W;
        public float H;

        public MouseEventArgs e;

        static Pen whitePen = new Pen(Color.White, 3.5f);
        public bool MouseDown(MouseEventArgs e)
        {
            if (e.X < X)
                return false;
            if (e.X > (X + W))
                return false;
            if (e.Y < Y)
                return false;
            if (e.Y > (Y + H))
                return false;

            this.e = e;

            if (e.Button == MouseButtons.Left)
            {
                if (OnClickCallback != null)
                {
                    OnClickCallback(this);
                    return false;
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                if (OnRightClickCallback != null)
                {
                    OnRightClickCallback(this);
                    return false;
                }
            }

            return true;
        }


        static Font fontText = new Font("Calibri", 11, FontStyle.Bold);
        static Font fontPage = new Font("Calibri", 10f, FontStyle.Bold);
        static Font fontFootnote = new Font("Calibri", 8);
        static Font IconFont = new Font("Arial", 12, FontStyle.Bold);
        static Font font1 = new Font("Calibri", 11);

      

        public void Paint(Graphics g)
        {
            float y_2 = Y + ((H - 3) / 2);
            float x_2 = X + ((W - 3) / 2);
            float xmax = X + W - 3;
            float ymax = Y + H - 3;
            float xmin = X + 2;
            float ymin = Y + 2;

            if (Icon == Icon.Delete)
            {
                g.DrawLine(Pen, x_2 - 5, y_2 - 5, x_2 + 5, y_2 + 5);
                g.DrawLine(Pen, x_2 - 5, y_2 + 5, x_2 + 5, y_2 - 5);
            }
            else if (Icon == Icon.MoveUp)
            {
                g.DrawLine(Pen, x_2, ymin, x_2, ymax);
                g.DrawLine(Pen, x_2, ymin, x_2 - 3, ymin + 5);
                g.DrawLine(Pen, x_2, ymin, x_2 + 3, ymin + 5);
            }
            else if (Icon == Icon.MoveDown)
            {
                g.DrawLine(Pen, x_2, ymin, x_2, ymax);
                g.DrawLine(Pen, x_2, ymax, x_2 - 3, ymax - 5);
                g.DrawLine(Pen, x_2, ymax, x_2 + 3, ymax - 5);
            }
            else if (Icon == Icon.CopyToClipboard)
            {
                g.DrawRectangle(Pen, xmax - W / 2, ymin, W / 2, H - 3);

                g.DrawLine(whitePen, xmin, y_2, xmax - W / 5 + 1, y_2);
                g.DrawLine(Pen, xmin, y_2, xmax - W / 5 + 1, y_2);
                g.DrawLine(Pen, xmax - W / 5 + 1, y_2, xmax - W / 5 + 1 - 5, y_2 - 3);
                g.DrawLine(Pen, xmax - W / 5 + 1, y_2, xmax - W / 5 + 1 - 5, y_2 + 3);
            }
            else if (Icon == Icon.PasteFromClipboard)
            {
                g.DrawRectangle(Pen, xmax - W / 2, ymin, W / 2, H - 3);
                g.DrawLine(whitePen, xmin - 1, y_2, xmax - W / 5 + 1, y_2);
                g.DrawLine(Pen, xmin - 1, y_2, xmax - W / 5 + 1, y_2);
                g.DrawLine(Pen, xmin - 1, y_2, xmin + 4, y_2 - 3);
                g.DrawLine(Pen, xmin - 1, y_2, xmin + 4, y_2 + 3);
            }
            else if (Icon == Icon.Clone)
            {
                g.DrawRectangle(Pen, xmin + W / 4 - 2, ymin - 1, W / 3 + 3, H - 4);
                g.DrawRectangle(whitePen, xmin + W / 4 + 2, ymin + 2, W / 3 + 3, H - 4);
                g.DrawRectangle(Pen, xmin + W / 4 + 2, ymin + 2, W / 3 + 3, H - 4);

            }
            else if (Icon == Icon.CenterLayout)
            {
                g.DrawRectangle(Pen, xmin + W / 4, ymin, W / 3, H - 3);
                g.DrawLine(Pen, xmin, ymin + 1, xmin + W / 5 - 1, ymin + 1);
                g.DrawLine(Pen, xmin, ymin + 3, xmin + W / 5 - 1, ymin + 3);
                g.DrawLine(Pen, xmin, ymin + 5, xmin + W / 5 - 1, ymin + 5);
                g.DrawLine(Pen, xmax, ymin + 1, xmax - W / 5 + 1, ymin + 1);
                g.DrawLine(Pen, xmax, ymin + 3, xmax - W / 5 + 1, ymin + 3);
                g.DrawLine(Pen, xmax, ymin + 5, xmax - W / 5 + 1, ymin + 5);
            }
            else if (Icon == Icon.LeftLayout)
            {
                g.DrawRectangle(Pen, xmin, ymin, W / 3, H - 3);
                g.DrawLine(Pen, xmax, ymin + 1, xmax - W / 3, ymin + 1);
                g.DrawLine(Pen, xmax, ymin + 3, xmax - W / 3, ymin + 3);
                g.DrawLine(Pen, xmax, ymin + 5, xmax - W / 3, ymin + 5);
            }
            else if (Icon == Icon.Text)
            {
                g.FillRectangle(Brushes.WhiteSmoke, X + 1, Y + 1, W - 2, H - 2);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, xmin, ymin - 2);
            }
            else if (Icon == Icon.Button)
            {
                bool selected = (Parent != null) && Parent.Selected;
                SolidBrush sb = new SolidBrush(Pen.Color);
                
                g.DrawRectangle(selected ? Pens.Orange : Pens.LightGray, X + 1, Y + 1, W - 2, H - 2);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, selected ? Brushes.LightSlateGray : sb, x_2 - size.Width / 2, ymin - 2);
            }
            else if (Icon == Icon.Footnote)
            {
                bool selected = (Parent != null) && Parent.Selected;
                g.DrawString(Text, fontFootnote, selected ? Brushes.DarkOrange : Brushes.LightSlateGray, xmin, ymin - 2);
            }
            else if (Icon == Icon.Section)
            {
                g.FillRectangle(Brushes.WhiteSmoke, X + 1, Y + 1, W - 2, H - 2);
                bool selected = (Parent != null) && Parent.Selected;
                g.DrawString(Text, fontText, Brushes.LightSlateGray, xmin, ymin - 2);
            }
            else if (Icon == Icon.Add)
            {
                g.DrawString(Text, fontText, Brushes.LightSlateGray, xmin - 2, ymin);
            }
            else if (Icon == Icon.Pdf)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.Home)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.Forward10)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.Back10)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.End)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.Save)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Draw.GetBrush2(Pen.Color), x_2 - size.Width / 2, ymin);
            }
            else if (Icon == Icon.Filename)
            {
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                g.DrawString(Text, fontText, /*Brushes.LightSlateGray*/Draw.GetBrush2(Pen.Color), xmin - 2, ymin);
            }
            else if (Icon == Icon.User)
            {
                g.FillRectangle(Brushes.WhiteSmoke, X + 2, Y + 2, W - 5, H - 5);
                g.DrawRectangle(Pens.LightGray, X, Y, W, H);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, xmin - 2, ymin);
            }
            else if (Icon == Icon.Select)
            {
                g.DrawRectangle(Pens.LightGray, X + 2, Y + 2, W - 5, H - 5);

                if (Parent.Selected)
                    g.FillRectangle(Brushes.DarkOrange, X + 2, Y + 2, W - 5, H - 5);
                SizeF size = g.MeasureString(Text, fontPage);
                g.DrawString(Text, fontPage, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin - 1);
            }
            else if (Icon == Icon.Visible)
            {
                g.DrawRectangle(Pens.LightGray, X + 4, Y + 4, W - 9, H - 9);
                if (Parent.Visible)
                    g.FillRectangle(Brushes.DarkOrange, X + 6, Y + 6, W - 13, H - 13);
            }
            else if (Icon == Icon.CodeEdit)
            {
        
                bool selected = (Parent != null) && Parent.Selected;

                if(!selected) return;

                g.FillRectangle(Brushes.LightGray, X + 1, Y + 1, W - 2, H - 2);
                SolidBrush sb = new SolidBrush(Pen.Color);

                g.DrawRectangle(Pens.Transparent, X + 1, Y + 1, W - 2, H - 2);
                SizeF size = g.MeasureString(Text, IconFont);
                g.DrawString(Text, IconFont,  Brushes.LightSlateGray, X+1, Y+size.Height/2);

            }
            else if (Icon == Icon.PageNumber)
            {

                bool selected = (Parent != null) && Parent.Selected;
                SolidBrush sb = new SolidBrush(Pen.Color);

                g.FillRectangle(selected?Brushes.Orange:Brushes.LightGray, X + 1, Y + 1, W - 2, H - 2);
                SizeF size = g.MeasureString(Text, IconFont);
                g.DrawString(Text, IconFont, Brushes.LightSlateGray, X + H/3, Y + size.Height / 2);

            }
            else if (Icon == Icon.PageFunction)
            {

                bool selected = (Parent != null) && Parent.Selected;

                if (!selected) return;

                SolidBrush sb = new SolidBrush(Pen.Color);

                g.DrawRectangle(Pens.Transparent, X + 1, Y + 1, W - 2, H - 2);
                SizeF size = g.MeasureString(Text, fontText);
                g.DrawString(Text, fontText, Brushes.LightSlateGray, X + 1, Y);

            }

            else
            {
                g.DrawRectangle(Pen, X, Y, W, H);
            }
        }

    }
}
