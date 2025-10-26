
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace QB.Controls
{
    public class Label : Control
    {
        public Label(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15) : base(name, x: x, y: y, w: w, h: h)
        {
<<<<<<< HEAD
            this.Bounds = new Rectangle(x, y, w, h);
            Text = text;
            Clickable = false; //default
=======
            if (text == null)
                Text = "#" + name;
            else
                Text = text;
>>>>>>> 2e559cf9a745ab27a7168025976681e3e8872392
        }

        //public Draw.Alignment alignment = Draw.Alignment.C;
        public Format Format = new Format();

        internal override void Render(Control parent)
        {
            base.Render(parent);

<<<<<<< HEAD
            System.Drawing.Color color_ = Misc.ParseColor(Color);
            
            if (!Enabled)
=======
            System.Drawing.Color color_ = System.Drawing.Color.FromArgb(180, Misc.ParseColor(Color));

            if (!Enable)
>>>>>>> 2e559cf9a745ab27a7168025976681e3e8872392
                color_ = System.Drawing.Color.Silver;
            else
            {

            }

            System.Drawing.Color _color = System.Drawing.Color.FromArgb(30, color_);

            if (false) //Button only
            {
                Draw.FillRectangle(Draw.GetBrush2(_color), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
                _color = System.Drawing.Color.FromArgb(120, color_);
                Draw.Rectangle(Draw.GetPen2(_color, 0.2f), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            }

            if (BackColor != null)
            {
                Draw.FillRectangle((System.Drawing.SolidBrush)Draw.GetBrush(BackColor), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            }



            if (Text == null)
                return;

       //SCAN     if (Name == null)
       //SCAN         Name = $"[#{Name}]";
            var splits = Text.Split('\n');

            //---FORMAT
            System.Drawing.ContentAlignment align = Format.Alignment ?? System.Drawing.ContentAlignment.MiddleCenter;
            //if (Format.Alignment != null)
            //{
            //    align = Draw.AlignmentFromString(Format.AlignmentConfig);
            //}
            if (Text != null && splits.Length > 1)
            {
                Draw.Text(splits[0], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 4.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.DodgerBlue : color_, align);
                Draw.Text(splits[1], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + 0.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.DodgerBlue : color_, align);
            }
            else
            {
<<<<<<< HEAD
                if (align == System.Drawing.ContentAlignment.MiddleRight || align == System.Drawing.ContentAlignment.TopRight || align == System.Drawing.ContentAlignment.BottomRight)
                    Draw.Text(Text, Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.SteelBlue : color_, align);
                else if (align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.TopLeft || align == System.Drawing.ContentAlignment.BottomLeft)
                        Draw.Text(Text, Bounds.X, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.SteelBlue : color_, align);
=======
                if (align == Draw.Alignment.R)
                    Draw.Text(Text, Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.DodgerBlue : color_, align);
                else if (align == Draw.Alignment.L)
                    Draw.Text(Text, Bounds.X, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.DodgerBlue : color_, align);
>>>>>>> 2e559cf9a745ab27a7168025976681e3e8872392
                else
                    Draw.Text(Text, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Hover ? System.Drawing.Color.DodgerBlue : color_, align);

            }
        }
    }
}
