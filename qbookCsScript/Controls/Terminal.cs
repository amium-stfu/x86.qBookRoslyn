
using System;
using System.Collections.Generic;

namespace QB.Controls
{


    public class Terminal : Label
    {
        /// <param name="onClick">OnClick delegate/action; e.g. "(b)=>OnButtonClick()"</param>
        public Terminal(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15, string format = null, ClickEventHandler onClick = null) : base(name, text: text, x: x, y: y, w: w, h: h)
        {
            if (onClick != null)
                this.OnClick += onClick;
            this.Clickable = true;

            if (format != null)
                this.Format = Format.FromString(format);
        }

        List<string> log = new List<string>();

        public void Add(string text, int maxcount)
        {
            lock (log)
            {
                log.Add(text);
                while (log.Count > maxcount)
                    log.RemoveAt(0);
            }
        }

        public Terminal(int id, string text = null, double x = 0, double y = 0, double w = 30, double h = 15, string format = null, ClickEventHandler onClick = null) : this(id.ToString("00"), text: text, x: x, y: y, w: w, h: h, format: format, onClick: onClick)
        {
        }

        internal override void Render(Control parent)
        {
            System.Drawing.Color color_ = Misc.ParseColor(Color);
            if (!Enabled)
                color_ = System.Drawing.Color.Silver;
            else
            {

            }
            System.Drawing.Color _color = System.Drawing.Color.Black;
            if (BackColor_ == null)
            {
                _color = System.Drawing.Color.FromArgb(10, color_);
                Draw.FillRectangle(Draw.GetBrush2(_color), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
            }
            else
            {
                Draw.FillRectangle((System.Drawing.SolidBrush)Draw.GetBrush(BackColor_), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
                //Draw.FillRectangle((SolidBrush)Brushes.Yellow, bounds.X + 0.5f, bounds.Y + 0.5f, bounds.W - 1, bounds.H - 1);
            }
            _color = System.Drawing.Color.FromArgb(50, color_);
            Draw.Rectangle(Draw.GetPen2(_color, 0.2f), Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);

            int y = 0;
            lock (log)
            {
                foreach (String loge in log)
                {
                    Draw.Text(loge, Bounds.X, Bounds.Y + y, 0, Draw.fontFootnoteFixed, System.Drawing.Color.Black, System.Drawing.ContentAlignment.TopLeft);
                    y += 4;
                }
            }
            base.Render(parent);

        }
    }
}
