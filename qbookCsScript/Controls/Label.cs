namespace QB.Controls
{
    public class Label : Control
    {
        public Label(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15) : base(name, x: x, y: y, w: w, h: h)
        {
            if (text == null)
                Text = "#" + name;
            else
                Text = text;

            this.Clickable = false;
            this.Format.ForeColor = System.Drawing.Color.Black;//.Gray; //.Silver?
        }

        //public Draw.Alignment alignment = Draw.Alignment.C;
        public Format Format = new Format();


        public string Text_TL = null;
        public string Text_TC = null;
        public string Text_TR = null;
        public string Text_BL = null;
        public string Text_BC = null;
        public string Text_BR = null;


        internal override void Render(Control parent)
        {
            base.Render(parent);


            this.Format.Font = Draw.fontText;


            //System.Drawing.Color color_ = System.Drawing.Color.FromArgb(180, Misc.ParseColor(Color));

            //if (!Enabled)
            //    color_ = System.Drawing.Color.Silver;
            //else
            //{

            //}

            //System.Drawing.Color _color = System.Drawing.Color.FromArgb(30, color_);

            //if (false) //Button only
            //{
            //    Draw.FillRectangle(Draw.GetBrush2(_color), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            //    _color = System.Drawing.Color.FromArgb(120, Format.ForeColor);
            //    Draw.Rectangle(Draw.GetPen2(_color, 0.2f), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            //}





               if (Text == null)
                return;

            //SCAN     if (Name == null)
            //SCAN         Name = $"[#{Name}]";
            var splits = Text.Split('\n');

            //---FORMAT
            System.Drawing.ContentAlignment align = Format.Alignment ?? System.Drawing.ContentAlignment.MiddleCenter;
            //if (Format.Alignment != null)
            //{
            //    align = Draw.AlignmentFromString(Format.AlignmentInfo);
            //}

            float ytOffset = 5.5f;
            float ybOffset = 2.0f;

            if ((Text_TL == null) &&
                   (Text_TC == null) &&
                   (Text_TR == null) &&
                   ((Text_BL == null) ||
                   (Text_BC == null) ||
                   (Text_BR == null))
                   )
                ybOffset = 3.0f;


            if (Text_TL != null)
            {
                Draw.Text(Text_TL, Bounds.X + 1, Bounds.Y + Bounds.H / 2 - ytOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleLeft);
            }

            if (Text_TC != null)
            {
                Draw.Text(Text_TC, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - ytOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
            }

            if (Text_TR != null)
            {
                Draw.Text(Text_TR, Bounds.X + Bounds.W - 1, Bounds.Y + Bounds.H / 2 - ytOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleRight);
            }

            if (Text_BL != null)
            {
                Draw.Text(Text_BL, Bounds.X + 1, Bounds.Y + Bounds.H / 2 + ybOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleLeft);
            }
            if (Text_BC != null)
            {
                Draw.Text(Text_BC, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + ybOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
            }
            if (Text_BR != null)
            {
                Draw.Text(Text_BR, Bounds.X + Bounds.W - 1, Bounds.Y + Bounds.H / 2 + ybOffset, 0, Draw.fontTerminal, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleRight);
            }



            if (Text != null && splits.Length > 4)
            {
                Draw.Text(splits[0], Bounds.X, Bounds.Y + Bounds.H / 2 - 7.1f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleLeft);
                Draw.Text(splits[1], Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 - 7.1f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleRight);
                Draw.Text(splits[2], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.2f, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.Black, align);
                Draw.Text(splits[3], Bounds.X, Bounds.Y + Bounds.H / 2 + 2.5f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleLeft);
                Draw.Text(splits[4], Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 + 2.5f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleRight);
            }
            else if (Text != null && splits.Length > 3)
            {
                Draw.Text(splits[0], Bounds.X, Bounds.Y + Bounds.H / 2 - 7.1f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleLeft);
                Draw.Text(splits[1], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 3.5f, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.Black, align);
                Draw.Text(splits[2], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + 0.6f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);
                Draw.Text(splits[3], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + 3.5f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);
            }
            else if (Text != null && splits.Length > 2)
            {
                Draw.Text(splits[0], Bounds.X, Bounds.Y + Bounds.H / 2 - 7.1f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.MiddleLeft);
                Draw.Text(splits[1], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.2f, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : System.Drawing.Color.Black, align);
                Draw.Text(splits[2], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + 2.5f, 0, Draw.fontFootnote, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);
            }
            else if (Text != null && splits.Length > 1)
            {
                Draw.Text(splits[0], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 4.5f, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);
                Draw.Text(splits[1], Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 + 0.5f, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);
            }
            else
            {
                float offset = 2.5f;

                if ((Text_TL == null) &&
                    (Text_TC == null) &&
                    (Text_TR == null) &&
                    ((Text_BL == null) ||
                    (Text_BC == null) ||
                    (Text_BR == null))
                    )
                    offset = 3.5f;

                if ((Text_TL != null) ||
                    (Text_TC != null) ||
                    (Text_TR != null) ||
                    (Text_BL != null) ||
                    (Text_BC != null) ||
                    (Text_BR != null)
                    )
                {
                    Draw.Text(Text, Bounds.X + 1, Bounds.Y + Bounds.H / 2 - offset, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, System.Drawing.ContentAlignment.BottomLeft);

                }


                else if (align == System.Drawing.ContentAlignment.MiddleRight || align == System.Drawing.ContentAlignment.TopRight || align == System.Drawing.ContentAlignment.BottomRight)
                    Draw.Text(Text, Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 - offset, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);

                else if (align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.TopLeft || align == System.Drawing.ContentAlignment.BottomLeft)
                    Draw.Text(Text, Bounds.X, Bounds.Y + Bounds.H / 2 - offset, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);

                else
                {
                    if (Text.Length == 1)
                        Draw.Text(Text, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - offset, 0, Draw.fontHeader1, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);

                    else
                        Draw.Text(Text, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - offset, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);

                }
              
            }
        }
    }
}
