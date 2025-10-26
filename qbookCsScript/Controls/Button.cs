using System.Web.UI.WebControls;

namespace QB.Controls
{
    public class Button : Label
    {
        string _SubText = null;
        public string SubText
        {
            get
            {
                return _SubText;
            }
            set
            {
                _SubText = value;
                if (SubTextFormat == null)
                {
                    SubTextFormat = new Format();
                    SubTextFormat.Font = new System.Drawing.Font(this.Format.Font.FontFamily, this.Format.Font.Size * 0.7f, this.Format.Font.Style);
                    SubTextFormat.Alignment = System.Drawing.ContentAlignment.BottomRight;
                    SubTextFormat.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }
        //public System.Drawing.ContentAlignment SubTextAlignment { get; set; } = System.Drawing.ContentAlignment.BottomRight;
        public Format SubTextFormat = null;

        /// <summary>
        /// Creates a Button control
        /// </summary>
        /// <param name="name">The identifier</param>
        /// <param name="text">The text to show on the button</param>
        /// <param name="x">X position (Top)</param>
        /// <param name="y">Y position (Left)</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="format">Apply a none-default formatting (->format string)</param>
        /// <param name="onClick">OnClick delegate/action; e.g. "(b)=>OnButtonClick()"</param>
        public Button(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15, string format = null, ClickEventHandler onClick = null) : base(name, text: text, x: x, y: y, w: w, h: h)
        {
            if (onClick != null)
                this.OnClick += onClick;
            this.Clickable = true;

            if (format != null)
                this.Format = Format.FromString(format);

            var stackTrace = new System.Diagnostics.StackTrace();
            var callerFrame = stackTrace.GetFrame(1); // 0 = aktuelle Methode, 1 = Aufrufer
            var callerType = callerFrame.GetMethod()?.DeclaringType;

            string temp = callerType?.Namespace;
        }

        public Button(int id, string text = null, double x = 0, double y = 0, double w = 30, double h = 15, string format = null, ClickEventHandler onClick = null) : this(id.ToString("00"), text: text, x: x, y: y, w: w, h: h, format: format, onClick: onClick)
        {
        }

        


        internal override void Render(Control parent)
        {
            if ((ParentPanel != null) && !ParentPanel.Visible)
                return;



            System.Drawing.Color color_ = Misc.ParseColor(Color);
            if (!Enabled)
                color_ = System.Drawing.Color.Silver;
            else
            {

            }

            System.Drawing.Color _color = System.Drawing.Color.Black;
            if (BackColor_ == null)
            {
             //   _color = System.Drawing.Color.FromArgb(10, color_);
             //   Draw.FillRectangle(Draw.GetBrush2(_color), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            }
            else
            {
                if (RightIcon != null)    
                    Draw.FillRectangle((System.Drawing.SolidBrush)Draw.GetBrush(BackColor_), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - Bounds.H  - 1, Bounds.H);
                else 
                    Draw.FillRectangle((System.Drawing.SolidBrush)Draw.GetBrush(BackColor_), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H);
                //Draw.FillRectangle((SolidBrush)Brushes.Yellow, bounds.X + 0.5f, bounds.Y + 0.5f, bounds.W - 1, bounds.H - 1);


                _color = System.Drawing.Color.FromArgb(50, color_);
                Draw.Rectangle(Draw.GetPen2(_color, 0.2f), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);


            }




            if (!string.IsNullOrEmpty(SubText))
            {
                var align = SubTextFormat.Alignment ?? System.Drawing.ContentAlignment.BottomRight;

                if (align == System.Drawing.ContentAlignment.MiddleRight)
                    Draw.Text(SubText, Bounds.X + Bounds.W, Bounds.Y + Bounds.H / 2 - 1.5f, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);
                else if (align == System.Drawing.ContentAlignment.TopRight)
                    Draw.Text(SubText, Bounds.X + Bounds.W, Bounds.Y, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);
                else if (align == System.Drawing.ContentAlignment.BottomRight)
                    Draw.Text(SubText, Bounds.X + Bounds.W, Bounds.Y + Bounds.H - SubTextFormat.Font.Size / 1.8f, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);

                else if (align == System.Drawing.ContentAlignment.MiddleLeft)
                    Draw.Text(SubText, Bounds.X, Bounds.Y + Bounds.H / 2 - 1.5f, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);
                else if (align == System.Drawing.ContentAlignment.TopLeft)
                    Draw.Text(SubText, Bounds.X, Bounds.Y, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);
                else if (align == System.Drawing.ContentAlignment.BottomLeft)
                    Draw.Text(SubText, Bounds.X + Bounds.W, Bounds.Y + Bounds.H - SubTextFormat.Font.Size / 1.8f, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);

                else
                    Draw.Text(SubText, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.5f, 0, SubTextFormat.Font, SubTextFormat.ForeColor, align);
            }

            if (LeftIcon != null)
            {
                if (LeftIcon.BackColor_ != null)
                {
                    System.Drawing.Color colorr = Misc.ParseColor(RightIcon.BackColor_);
                    Draw.FillCircle(Draw.GetBrush2(colorr), Bounds.X + 1.0f, Bounds.Y + 0.75f, Bounds.H - 2, Bounds.H - 2);
            }
               
                if (LeftIcon.Text != null)
                    Draw.Text(LeftIcon.Text, Bounds.X, Bounds.Y + Bounds.H / 5, Bounds.H, Draw.fontHeader1, System.Drawing.Color.Black, System.Drawing.ContentAlignment.MiddleCenter);

            }
            if (RightIcon != null)
            {
                // _color = System.Drawing.Color.FromArgb(10, color_);
                if (RightIcon.BackColor_ != null)
                {
                    System.Drawing.Color colorr = Misc.ParseColor(RightIcon.BackColor_);
                    Draw.FillCircle(Draw.GetBrush2(colorr), Bounds.X + Bounds.W - Bounds.H + 1f, Bounds.Y + 0.75f, Bounds.H - 2, Bounds.H - 2);
                }
                Draw.Circle(System.Drawing.Color.White, Bounds.X + Bounds.W - Bounds.H + 2.0f, Bounds.Y + 1.75f, Bounds.H - 4, Bounds.H - 4, 0.751f);


                if (RightIcon.Text != null)
                    Draw.Text(RightIcon.Text, Bounds.X + Bounds.W - Bounds.H, Bounds.Y + Bounds.H / 5, Bounds.H, Draw.fontHeader1, System.Drawing.Color.Black, System.Drawing.ContentAlignment.MiddleCenter);
            }
            base.Render(parent);

        }
    }
}
