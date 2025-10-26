using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System;
using CSScripting;
using System.Windows.Forms;
using FontAwesome.Sharp;
using System.Windows.Input;
using System.IO;
using Svg;
using System.Diagnostics;

//using Npgsql.Internal;
//using PdfSharp.Charting;

namespace QB.Controls
{
    public class Box : Control
    {
        //public Box(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15) : base(name, x: x, y: y, w: w, h: h)
        //{
        //    if (text == null)
        //        Text = "#" + name;
        //    else
        //        Text = text;

        //    this.Clickable = false;
        //    //this.Format.ForeColor = System.Drawing.Color.Black;//.Gray; //.Silver?
        //}

        bool BoundsValid = false;

        public void InvalidateBounds()
        {
            BoundsValid = false;
            foreach (Box box in this.Boxes ?? Enumerable.Empty<Box>())
            {
                box.InvalidateBounds();
            }
        }



        public delegate void ClickEventHandler(Control s);
        public event ClickEventHandler OnBoxClick;
        internal override void RaiseOnClick(PointF point)
        {
            try
            {
                ClickPosition = point;
                if (OnBoxClick != null)
                {
                    OnBoxClick(this); //default handler
                }
                if (EditMask != null)
                {
                    //in-place edit
                    TextBox editBox = new TextBox();
                    editBox.Font = Draw.GetFont(this._Font.FontFamily.Name, Math.Max(10, this._Font.Size * 0.30 * Draw.mmToPx), this._Font.Style);
                    var parentForm = System.Windows.Forms.Application.OpenForms[0];
                    System.Windows.Forms.Control pageControl = parentForm.Controls.OfType<System.Windows.Forms.Control>().FirstOrDefault(c => c.GetType().Name == "PageControl");
                    if (pageControl != null)
                    {
                        parentForm.Controls.Add(editBox);
                        editBox.Name = "BoxEditor";
                        editBox.Tag = this;

                        if (this.Text is Delegate)
                        {
                            editBox.Text = ((Delegate)_Text).DynamicInvoke().ToString();
                        }
                        else
                        {
                            editBox.Text = Text.ToString();
                        }

                        switch (TextAlignment)
                        {
                            case ContentAlignment.TopRight:
                            case ContentAlignment.MiddleRight:
                            case ContentAlignment.BottomRight:
                                editBox.TextAlign = HorizontalAlignment.Right;
                                break;
                            case ContentAlignment.TopCenter:
                            case ContentAlignment.MiddleCenter:
                            case ContentAlignment.BottomCenter:
                                editBox.TextAlign = HorizontalAlignment.Center;
                                break;
                            default:
                                editBox.TextAlign = HorizontalAlignment.Left;
                                break;
                        }
                        editBox.KeyPress += EditBox_KeyPress;
                        editBox.LostFocus += EditBox_LostFocus;
                        editBox.Location = new Point((int)(Bounds.X * Draw.mmToPx) + pageControl.Left + 1, (int)(Bounds.Y * Draw.mmToPx) + pageControl.Top - 2);
                        editBox.Size = new Size((int)(Bounds.W * Draw.mmToPx), (int)(Bounds.H * Draw.mmToPx));
                        editBox.Show();
                        editBox.BringToFront();
                        editBox.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX in OnBoxClick: " + ex.Message);
            }
        }

        public class TextEventArgs : EventArgs
        {
            public object OldText;
            public object NewText;
        }
        public delegate void AcceptEventHandler(TextEventArgs ea);
        public event AcceptEventHandler OnAccept;
        internal void RaiseOnAccept(object oldText, object newText)
        {
            try
            {
                if (OnAccept != null)
                {
                    TextEventArgs ea = new TextEventArgs() { OldText = oldText, NewText = newText };
                    OnAccept(ea); //default handler
                }
            }
            catch (Exception ex) { }
        }

        public delegate void CancelEventHandler(TextEventArgs ea);
        public event CancelEventHandler OnCancel;
        internal void RaiseOnCancel(object oldText, object newText)
        {
            try
            {
                if (OnCancel != null)
                {
                    TextEventArgs ea = new TextEventArgs() { OldText = oldText, NewText = newText };
                    OnCancel(ea); //default handler
                }
            }
            catch (Exception ex) { }
        }


        private void EditBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27) //ESC
            {
                (sender as TextBox).Hide();
                (sender as TextBox).Dispose();
                RaiseOnCancel(this.Text, this.Text);
            }
            if (e.KeyChar == 13 && (System.Windows.Forms.Control.ModifierKeys & Keys.Alt) == 0) //ENTER
            {
                object oldText = this.Text;
                this.Text = (sender as TextBox).Text;
                (sender as TextBox).Hide();
                (sender as TextBox).Dispose();
                RaiseOnAccept(oldText, this.Text);
            }
        }
        private void EditBox_LostFocus(object sender, EventArgs e)
        {
            (sender as TextBox).Hide();
            (sender as TextBox).Dispose();
        }


        //public Box(string format)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="format">obsolete! use style!!!</param>
        /// <param name="style"></param>
        /// <param name="boxes"></param>
        /// <param name="onClick"></param>
        public Box(string name,
            string text = null, object x = null, object y = null, object w = null, object h = null,
            string format = null, string style = null,
            Box[] boxes = null, ClickEventHandler onClick = null,
            AcceptEventHandler onAccept = null,
            CancelEventHandler onCancel = null,
            string editMask = null,
            string icon = null,
            System.Drawing.Color? hoverColor = null
            )
        : base(name, x: 0, y: 0, w: 0, h: 0)
        {
            this.Left = x; this.Top = y; this.Width = w; this.Height = h;

            Text = text;
            Font = DefaultFont;
            TextColor = System.Drawing.Color.Black;
            TextAlignment = ContentAlignment.MiddleCenter;
            EditMask = editMask;
            Icon = icon;
            if (hoverColor != null)
                BackgroundColorHover = hoverColor;

            this.Clickable = false;

            if (editMask != null)
            {
                this.OnBoxClick += onClick;
                this.Clickable = true;
            }

            if (onClick != null)
            {
                this.OnBoxClick += onClick;
                this.Clickable = true;
            }

            if (onAccept != null)
            {
                this.OnAccept += onAccept;
            }

            if (onCancel != null)
            {
                this.OnCancel += onCancel;
            }




            // Set default values
            BackgroundColor = System.Drawing.Color.Transparent;
            BorderTColor = System.Drawing.Color.Transparent;
            BorderTWidth = 0.0;
            BorderRColor = System.Drawing.Color.Transparent;
            BorderRWidth = 0.0;
            BorderBColor = System.Drawing.Color.Transparent;
            BorderBWidth = 0.0;
            BorderLColor = System.Drawing.Color.Transparent;
            BorderLWidth = 0.0;

            Boxes = boxes?.ToList();

            // Parse the format string and update properties accordingly
            if (style != null)
                format = style; //format obsolete; style replaces format
            FormatString = format;
            ParseFormatString(format);

            if (Boxes != null)
            {
                foreach (var box in boxes)
                {
                    box.Parent = this;
                }
            }
        }


        //2025-02-18 STFU

        public Box(string name,
        Func<string> textFunction, object x = null, object y = null, object w = null, object h = null,
        string format = null, string style = null,
        Box[] boxes = null, ClickEventHandler onClick = null,
        AcceptEventHandler onAccept = null,
        CancelEventHandler onCancel = null,
        string editMask = null,
        string icon = null,
        System.Drawing.Color? hoverColor = null
        )
: base(name, x: 0, y: 0, w: 0, h: 0)
        {
            this.Left = x; this.Top = y; this.Width = w; this.Height = h;

            Text = textFunction;
            Font = DefaultFont;
            TextColor = System.Drawing.Color.Black;
            TextAlignment = ContentAlignment.MiddleCenter;
            EditMask = editMask;
            Icon = icon;
            if (hoverColor != null)
                BackgroundColorHover = hoverColor;

            this.Clickable = false;

            if (editMask != null)
            {
                this.OnBoxClick += onClick;
                this.Clickable = true;
            }

            if (onClick != null)
            {
                this.OnBoxClick += onClick;
                this.Clickable = true;
            }

            if (onAccept != null)
            {
                this.OnAccept += onAccept;
            }

            if (onCancel != null)
            {
                this.OnCancel += onCancel;
            }




            // Set default values
            BackgroundColor = System.Drawing.Color.Transparent;
            BorderTColor = System.Drawing.Color.Transparent;
            BorderTWidth = 0.0;
            BorderRColor = System.Drawing.Color.Transparent;
            BorderRWidth = 0.0;
            BorderBColor = System.Drawing.Color.Transparent;
            BorderBWidth = 0.0;
            BorderLColor = System.Drawing.Color.Transparent;
            BorderLWidth = 0.0;

            Boxes = boxes?.ToList();

            // Parse the format string and update properties accordingly
            if (style != null)
                format = style; //format obsolete; style replaces format
            FormatString = format;
            ParseFormatString(format);

            if (Boxes != null)
            {
                foreach (var box in boxes)
                {
                    box.Parent = this;
                }
            }
        }

        const double A4_WIDTH = 297;
        const double A4_HEIGHT = 210;
        void CalculateAbsoluteBounds()
        {
            //if (Bounds != null || BoundsValid)
            //    return; //already calculated! (reset to null, if size has changed)

            if (Parent == null)
            {
                double x = 0.0 + ParsePosition(this.Left, A4_WIDTH) ?? 0.0;
                double y = 0.0 + ParsePosition(this.Top, A4_HEIGHT) ?? 0.0;
                double w = ParsePosition(this.Width, A4_WIDTH) ?? A4_WIDTH;
                double h = ParsePosition(this.Height, A4_HEIGHT) ?? A4_HEIGHT;
                //Bounds = new RectangleF((float)x, (float)y, (float)w, (float)h);
                Bounds.X = x;
                Bounds.Y = y;
                Bounds.W = w;
                Bounds.H = h;

                BoundsValid = true;
            }
            else
            {
                (Parent as Box).CalculateAbsoluteBounds();
                if (Parent.Bounds != null)
                {
                    //RectangleF parentBounds = (RectangleF)ParentBox.Bounds;
                    var parentBounds = Parent.Bounds;
                    double x = (parentBounds.X + ParsePosition(this.Left, parentBounds.W)) ?? parentBounds.X;
                    double y = (parentBounds.Y + ParsePosition(this.Top, parentBounds.H)) ?? parentBounds.Y;
                    double w = ParsePosition(this.Width, parentBounds.W) ?? parentBounds.W;
                    double h = ParsePosition(this.Height, parentBounds.H) ?? parentBounds.H;
                    //Bounds = new RectangleF((float)x, (float)y, (float)w, (float)h);
                    Bounds.X = x;
                    Bounds.Y = y;
                    Bounds.W = w;
                    Bounds.H = h;

                    BoundsValid = true;
                }
            }
        }
        Rectangle CalculateAbsoluteBounds2()
        {
            Rectangle bounds = new Rectangle();
            if (Parent == null)
            {
                double x = 0.0 + ParsePosition(this.Left, A4_WIDTH) ?? 0.0;
                double y = 0.0 + ParsePosition(this.Top, A4_HEIGHT) ?? 0.0;
                double w = ParsePosition(this.Width, A4_WIDTH) ?? A4_WIDTH;
                double h = ParsePosition(this.Height, A4_HEIGHT) ?? A4_HEIGHT;
                //Bounds = new RectangleF((float)x, (float)y, (float)w, (float)h);
                bounds.X = x;
                bounds.Y = y;
                bounds.W = w;
                bounds.H = h;

                //BoundsValid = true;
            }
            else
            {
                return (Parent as Box).CalculateAbsoluteBounds2();
            }
            return bounds;
        }
        //private double? ParsePosition(object position, double? coord)
        //{
        //    if (position is null) return null;
        //    if (coord is null) return null;

        //    double pos = 0;
        //    if (position is string)
        //    {
        //        var left = (position as string).Trim();
        //        if (left.EndsWith("%"))
        //            pos = (double)coord * double.Parse(left.Substring(0, left.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture) / 100.0;
        //        else
        //            pos = double.Parse(left.Substring(0, left.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture);

        //    }

        //    if (position is double)
        //        pos = (double)position;

        //    if (position is float)
        //        pos = Convert.ToDouble(position);

        //    if (position is int)
        //        pos = Convert.ToDouble(position);

        //    return pos;
        //}


        //internal void UpdatePageBounds()
        //{
        //    Bounds.X = (0 + ParsePosition(this.Left, 0)) ?? 0;
        //    Bounds.Y = (0 + ParsePosition(this.Top, 0)) ?? 0;
        //    Bounds.W = ParsePosition(this.Width, 0) ?? 0;
        //    Bounds.H = ParsePosition(this.Height, 0) ?? 0;
        //}

        //internal Box ParentBox = null;

        private string FormatString = null; //keeps the original format

        [XmlIgnore]
        public Color BackgroundColor { get; set; }
        [XmlIgnore]
        public Color? BackgroundColorHover { get; set; } = null;
        [XmlIgnore]
        public Color? BackgroundColorClick { get; set; } = null;

        [XmlIgnore]
        public Color BorderTColor { get; set; }

        [XmlIgnore]
        public Color BorderRColor { get; set; }

        [XmlIgnore]
        public Color BorderBColor { get; set; }

        [XmlIgnore]
        public Color BorderLColor { get; set; }

        [XmlIgnore]
        public double BorderTWidth { get; set; }

        [XmlIgnore]
        public double BorderRWidth { get; set; }

        [XmlIgnore]
        public double BorderBWidth { get; set; }

        [XmlIgnore]
        public double BorderLWidth { get; set; }
        //public string Text { get; set; }
        //public ContentAlignment TextAlignment { get; set; }
        //public Color TextColor { get; set; }

        private Font _Font;
        [XmlIgnore]
        //public Font Font { get; set; }
        public object Font
        {
            get
            {
                if (_Font == null || !(_Font is Font))
                    return DefaultFont;
                else
                    return _Font;
            }
            set
            {
                if (value is String)
                {
                    var splits = (value as String).Split(new char[] { ':' });
                    _Font = ParseFormatFont(splits);
                }
                else if (value is Font)
                {
                    _Font = value as Font;
                }
            }
        }


        [XmlIgnore]
        private Font DefaultFont { get; set; } = new Font("Tahoma", 12.0f, FontStyle.Regular);

        [XmlIgnore]
        internal List<Box> Boxes { get; set; } = new List<Box>();



        public string Id { get; set; } = null;
        //public Color BackgroundColor { get; set; }
        [XmlIgnore]
        public object _Text = null;
        public object Text { get => _Text; set => _Text = value; }

        [XmlIgnore]
        public string Icon { get; set; } //SVG? Base64-PNG? ...? //TODO: not yet supported
        [XmlIgnore]
        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.MiddleCenter;
        [XmlIgnore]
        public string EditMask { get; set; } = null;


        [XmlIgnore]
        internal object Left { get; set; } = null;
        [XmlIgnore]
        public object Top { get; set; } = null;
        [XmlIgnore]
        public object Width { get; set; } = null;
        [XmlIgnore]
        public object Height { get; set; } = null;

        [XmlIgnore]
        public Color TextColor { get; set; }


        void ParseFormatString(string format)
        {
            var items = SplitOutsideSingleQuotes(format, keepBrackets: true);
            if (items == null) return;
            foreach (var item in items)
            {
                var kvp = item.Split(new char[] { ':' });
                var key = kvp[0].ToLower();
                string[] values = kvp.Skip(1).ToArray();

                switch (key)
                {
                    case "background":
                    case "backgroundcolor":
                    case "backcolor":
                    case "bg":
                        //color
                        BackgroundColor = ParseFormatColor(values, System.Drawing.Color.Transparent);
                        break;
                    case "background-hover":
                    case "backgroundcolor-hover":
                    case "backcolor-hover":
                    case "bg-hover":
                        //color
                        BackgroundColorHover = ParseFormatColor(values, System.Drawing.Color.Transparent);
                        break;
                    case "background-click":
                    case "backgroundcolor-click":
                    case "backcolor-click":
                    case "bg-click":
                        //color
                        BackgroundColorClick = ParseFormatColor(values, System.Drawing.Color.Transparent);
                        break;
                    case "border":
                        //color:width:style
                        {
                            Color color = System.Drawing.Color.Transparent;
                            double width = 1.0;
                            string style = null;
                            ParseColorAndWidth(values, ref color, ref width, ref style);
                            BorderTColor = color;
                            BorderRColor = color;
                            BorderBColor = color;
                            BorderLColor = color;
                            BorderTWidth = width;
                            BorderRWidth = width;
                            BorderBWidth = width;
                            BorderLWidth = width;
                        }
                        break;
                    case "border-top":
                        //color:width:style
                        {
                            Color color = System.Drawing.Color.Transparent;
                            double width = 1.0;
                            string style = null;
                            ParseColorAndWidth(values, ref color, ref width, ref style);
                            BorderTColor = color;
                            BorderTWidth = width;
                        }
                        break;
                    case "border-right":
                        //color:width:style
                        {
                            Color color = System.Drawing.Color.Transparent;
                            double width = 1.0;
                            string style = null;
                            ParseColorAndWidth(values, ref color, ref width, ref style);
                            BorderRColor = color;
                            BorderRWidth = width;
                        }
                        break;
                    case "border-bottom":
                        //color:width:style
                        {
                            Color color = System.Drawing.Color.Transparent;
                            double width = 1.0;
                            string style = null;
                            ParseColorAndWidth(values, ref color, ref width, ref style);
                            BorderBColor = color;
                            BorderBWidth = width;
                        }
                        break;
                    case "border-left":
                        //color:width:style
                        {
                            Color color = System.Drawing.Color.Transparent;
                            double width = 1.0;
                            string style = null;
                            ParseColorAndWidth(values, ref color, ref width, ref style);
                            BorderLColor = color;
                            BorderLWidth = width;
                        }
                        break;
                    case "textcolor":
                    case "forecolor":
                    case "color":
                        //color
                        TextColor = ParseFormatColor(values, System.Drawing.Color.Black);
                        break;
                    case "font":
                        //:fontname:size:style:weight
                        Font = ParseFormatFont(values);
                        break;

                    case "textalignment":
                    case "alignment":
                    case "align":
                        TextAlignment = ParseFormatAlignment(values);
                        break;
                    case "left":
                    case "x":
                        //Labels[0].Left = values[0];
                        break;
                    case "top":
                    case "y":
                        //Labels[0].Top = values[0];
                        break;
                    case "width":
                    case "w":
                        //Labels[0].Width = values[0];
                        break;
                    case "height":
                    case "h":
                        //Labels[0].Height = values[0];
                        break;
                        //case "label":
                        //    {
                        //        string labelFormat = item.Substring("label:".Length).Trim();
                        //        if (labelFormat.StartsWith("[") && labelFormat.EndsWith("]"))
                        //        {
                        //            labelFormat = labelFormat.Substring(1, labelFormat.Length - 2);
                        //            BoxLabel label = new BoxLabel();
                        //            label.MyButton = this;
                        //            label.TextColor = System.Drawing.Color.Black;
                        //            var items1 = SplitOutsideSingleQuotes(labelFormat, keepBrackets: false);
                        //            foreach (var item1 in items1)
                        //            {
                        //                var kvp1 = item1.Split(new char[] { ':' });
                        //                var key1 = kvp1[0].ToLower();
                        //                string[] values1 = kvp1.Skip(1).ToArray();

                        //                switch (key1)
                        //                {
                        //                    case "id":
                        //                        label.Id = values1.Length > 0 ? values1[0] : $"{Labels.Count}";
                        //                        break;
                        //                    case "text":
                        //                        label.Text = values1.Length > 0 ? values1[0] : "";
                        //                        break;
                        //                    case "textcolor":
                        //                    case "color":
                        //                        label.TextColor = ParseFormatColor(values1, System.Drawing.Color.Black);
                        //                        break;
                        //                    case "font":
                        //                        label.Font = ParseFormatFont(values1);
                        //                        break;

                        //                    case "textalignment":
                        //                    case "alignment":
                        //                    case "align":
                        //                        label.TextAlignment = ParseFormatAlignment(values1);
                        //                        break;
                        //                    case "left":
                        //                    case "x":
                        //                        label.Left = values1[0];
                        //                        break;
                        //                    case "top":
                        //                    case "y":
                        //                        label.Top = values1[0];
                        //                        break;
                        //                    case "width":
                        //                    case "w":
                        //                        label.Width = values1[0];
                        //                        break;
                        //                    case "height":
                        //                    case "h":
                        //                        label.Height = values1[0];
                        //                        break;

                        //                    case "background":
                        //                    case "backgroundcolor":
                        //                    case "backcolor":
                        //                    case "bg":
                        //                        label.BackgroundColor = ParseFormatColor(values1, System.Drawing.Color.Transparent);
                        //                        break;
                        //                }
                        //            }

                        //            if (label.Id == null && Labels.Count > 0 && Labels[0].Id == "#<null>")
                        //            {
                        //                Labels.RemoveAt(0);
                        //                Labels.Insert(0, label);
                        //            }
                        //            else
                        //            {
                        //                Labels.Add(label);
                        //            }
                        //        }
                        //        break;

                        //    }

                }
            }
        }

        private Color ParseFormatColor(string[] values, Color? defaultColor = null)
        {
            if (defaultColor == null)
                defaultColor = System.Drawing.Color.Black;

            //if (values.Length > 0 && values[0].Trim().EndsWith("%"))
            //{
            //    Color color = this.BackgroundColor;
            //    int.TryParse(values[0].Trim().Trim('%'), out int opacityPercentage);
            //    if (opacityPercentage > 100)
            //        opacityPercentage = 100;
            //    if (opacityPercentage < 0)
            //        opacityPercentage = 0;
            //    int alpha = (int)((opacityPercentage / 100.0) * 255);
            //    return System.Drawing.Color.FromArgb(alpha, color.R, color.G, color.B);
            //}
            //else
            {
                Color color = values.Length > 0 ? ParseColor(values[0]) : (Color)defaultColor;
                return color;
            }
        }

        private ContentAlignment ParseFormatAlignment(string[] values)
        {
            var alignment = ContentAlignment.MiddleCenter;
            switch (values[0].ToLower())
            {
                case "topleft":
                case "tl":
                    alignment = ContentAlignment.TopLeft;
                    break;
                case "topcenter":
                case "tc":
                    alignment = ContentAlignment.TopCenter;
                    break;
                case "topright":
                case "tr":
                    alignment = ContentAlignment.TopRight;
                    break;
                case "middleleft":
                case "ml":
                    alignment = ContentAlignment.MiddleLeft;
                    break;
                case "middlecenter":
                case "mc":
                    alignment = ContentAlignment.MiddleCenter;
                    break;
                case "middleright":
                case "mr":
                    alignment = ContentAlignment.MiddleRight;
                    break;
                case "bottomleft":
                case "bl":
                    alignment = ContentAlignment.BottomLeft;
                    break;
                case "bottomcenter":
                case "bc":
                    alignment = ContentAlignment.BottomCenter;
                    break;
                case "bottomright":
                case "br":
                    alignment = ContentAlignment.BottomRight;
                    break;
            }
            return alignment;
        }

        private Font ParseFormatFont(string[] values)
        {
            string fontname = values.Length > 0 ? values[0] : "Tahoma";
            double size = 12.0;
            FontStyle fontStyle = FontStyle.Regular;
            if (values.Length > 1)
                if (!Double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out size))
                    size = 12.0;
            if (values.Length > 2)
            {
                var styles = values[2].Split('|');
                foreach (var style in styles)
                {
                    switch (style.ToLower())
                    {
                        case "bold":
                        case "b":
                            fontStyle |= FontStyle.Bold;
                            break;
                        case "italic":
                        case "i":
                            fontStyle |= FontStyle.Italic;
                            break;
                        case "strikeout":
                        case "strike":
                        case "s":
                            fontStyle |= FontStyle.Strikeout;
                            break;
                        case "underline":
                        case "line":
                        case "u":
                            fontStyle |= FontStyle.Underline;
                            break;
                    }
                }
            }
            double weight = 200;
            if (values.Length > 3)
            {
                Double.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out weight);
                if (weight >= 600)
                    fontStyle |= FontStyle.Bold;
            }

            return new Font(fontname, (float)size, fontStyle); //weight?!
        }

        public override string ToString()
        {
            List<string> items = new List<string>();
            if (BackgroundColor != System.Drawing.Color.Transparent)
                items.Add("bg:" + ColorToString(BackgroundColor));

            if (
                ((BorderTColor == BorderRColor) && (BorderTColor == BorderBColor) && (BorderTColor == BorderLColor) && (BorderTColor != System.Drawing.Color.Transparent))
                || ((BorderTWidth == BorderRWidth) && (BorderTWidth == BorderBWidth) && (BorderTWidth == BorderLWidth) && (BorderTWidth != 0.0))
                )
            {
                items.Add("border:" + ColorToString(BorderTColor) + ":" + BorderTWidth.ToString("0.0", CultureInfo.InvariantCulture));
            }
            else
            {
                string s = "";
                if (BorderTColor != System.Drawing.Color.Transparent || BorderTWidth != 0.0)
                    s += "border-top:" + ColorToString(BorderTColor) + ":" + BorderTWidth.ToString("0.0", CultureInfo.InvariantCulture) + ",";

                if (BorderRColor != System.Drawing.Color.Transparent || BorderRWidth != 0.0)
                    s += "border-right:" + ColorToString(BorderRColor) + ":" + BorderRWidth.ToString("0.0", CultureInfo.InvariantCulture) + ",";

                if (BorderBColor != System.Drawing.Color.Transparent || BorderBWidth != 0.0)
                    s += "border-bottom:" + ColorToString(BorderBColor) + ":" + BorderBWidth.ToString("0.0", CultureInfo.InvariantCulture) + ",";

                if (BorderLColor != System.Drawing.Color.Transparent || BorderLWidth != 0.0)
                    s += "border-left:" + ColorToString(BorderLColor) + ":" + BorderLWidth.ToString("0.0", CultureInfo.InvariantCulture) + ",";
                if (!string.IsNullOrEmpty(s))
                    items.Add(s.TrimEnd(','));
            }

            //if (!string.IsNullOrEmpty(Labels[0].Text))
            //{
            //    items.Add("text:'" + Labels[0].Text + "'");
            //}

            //if (Labels[0].TextColor != System.Drawing.Color.Black)
            //{
            //    items.Add("color:" + ColorToString(Labels[0].TextColor));
            //}

            if (Font != null && Font != DefaultFont)
            {
                string style = "";
                if ((_Font.Style & FontStyle.Bold) > 0)
                    style += "b|";
                if ((_Font.Style & FontStyle.Italic) > 0)
                    style += "i|";
                if ((_Font.Style & FontStyle.Strikeout) > 0)
                    style += "s|";
                if ((_Font.Style & FontStyle.Underline) > 0)
                    style += "u|";
                style = style.TrimEnd('|');
                string s = "font:" + _Font.FontFamily.Name + ":" + _Font.Size.ToString("0.0", CultureInfo.InvariantCulture);
                if (style.Length > 0)
                    s += ":" + style;
                items.Add(s); // + ":";
            }

            //if (Labels[0].TextAlignment != ContentAlignment.MiddleCenter)
            //{
            //    string align = "mc";
            //    switch (Labels[0].TextAlignment)
            //    {
            //        case ContentAlignment.TopLeft:
            //            align = "tl";
            //            break;
            //        case ContentAlignment.TopCenter:
            //            align = "tc";
            //            break;
            //        case ContentAlignment.TopRight:
            //            align = "rt";
            //            break;
            //        case ContentAlignment.MiddleLeft:
            //            align = "ml";
            //            break;
            //        case ContentAlignment.MiddleCenter:
            //            align = "mc";
            //            break;
            //        case ContentAlignment.MiddleRight:
            //            align = "mr";
            //            break;
            //        case ContentAlignment.BottomLeft:
            //            align = "bl";
            //            break;
            //        case ContentAlignment.BottomCenter:
            //            align = "bc";
            //            break;
            //        case ContentAlignment.BottomRight:
            //            align = "rb";
            //            break;
            //    }
            //    items.Add("align:" + align);
            //}

            return string.Join(", ", items);
        }

        void ParseColorAndWidth(string format, ref Color color, ref double width, ref string style)
        {
            //color:width:style
            string[] values = format.Split(':').ToArray();
            ParseColorAndWidth(values, ref color, ref width, ref style);

            //color = values.Length > 0 ? ParseColor(values[0]) : Color.Transparent;
            //if (values.Length > 1)
            //{
            //	width = 1.0;
            //	Double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out width);
            //}
        }
        void ParseColorAndWidth(string[] values, ref Color color, ref double width, ref string style)
        {
            //color:width:style
            color = values.Length > 0 ? ParseColor(values[0]) : System.Drawing.Color.Transparent;
            if (values.Length > 1)
            {
                width = 1.0;
                Double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out width);
            }
        }



        /*
        static List<string> SplitOutsideSingleQuotes(string input, char splitChar = ',', char quoteChar = '\'')
        {
            char escapeChar = '\\';
            List<string> items = new List<string>();
            bool inQuotes = false;
            int pos = 0;
            StringBuilder sb = new StringBuilder();
            while (pos < input.Length)
            {
                char c = input[pos];
                if (c == escapeChar)
                {
                    if (pos < input.Length - 1) //we need 2 char's here
                    {
                        char c1 = input[pos + 1];
                        if (c1 == quoteChar)
                        {
                            sb.Append(c1);
                            pos++;
                            pos++;
                        }
                        else
                        {
                            sb.Append(c);
                            pos++;
                            c = input[pos];
                            sb.Append(c);
                            pos++;
                        }
                    }
                }
                else if (c == quoteChar)
                {
                    inQuotes = !inQuotes;
                    pos++;
                }
                else if (c == splitChar && !inQuotes)
                {
                    items.Add(sb.ToString());
                    sb.Clear();
                    pos++;
                }
                else
                {
                    sb.Append(c);
                    pos++;
                }
            }
            items.Add(sb.ToString());
            return items;
        }
        */

        static List<string> SplitOutsideSingleQuotes(string input, char splitChar = ',', char quoteChar = '\'', bool keepBrackets = false)
        {
            if (input == null)
                return null;
            char escapeChar = '\\';
            List<string> items = new List<string>();
            bool inQuotes = false;
            int bracketCount = 0;
            int pos = 0;
            StringBuilder sb = new StringBuilder();
            while (pos < input.Length)
            {
                char c = input[pos];
                if (c == escapeChar)
                {
                    if (pos < input.Length - 1) //we need 2 char's here
                    {
                        char c1 = input[pos + 1];
                        if (c1 == quoteChar)
                        {
                            sb.Append(c1);
                            pos++;
                            pos++;
                        }
                        else
                        {
                            sb.Append(c);
                            pos++;
                            c = input[pos];
                            sb.Append(c);
                            pos++;
                        }
                    }
                }
                else if (c == quoteChar && (!keepBrackets || (keepBrackets && bracketCount == 0)))
                {
                    inQuotes = !inQuotes;
                    pos++;
                }
                else if (keepBrackets && !inQuotes && c == '[')
                {
                    bracketCount++;
                    sb.Append(c);
                    pos++;
                }
                else if (keepBrackets && !inQuotes && c == ']')
                {
                    bracketCount--;
                    sb.Append(c);
                    pos++;
                }
                else if (c == splitChar && !inQuotes && (!keepBrackets || (keepBrackets && bracketCount == 0)))
                {
                    items.Add(sb.ToString());
                    sb.Clear();
                    pos++;
                }
                else
                {
                    sb.Append(c);
                    pos++;
                }
            }
            items.Add(sb.ToString());
            return items;
        }


        static string ReplaceOutsideQuotes(string input, char[] charactersToReplace, string replacementString)
        {
            bool inQuotes = false;
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            while (pos < input.Length)
            {
                char c = input[pos];
                if (c == '\'' && pos > 0 && input[pos - 1] != '\\')
                    inQuotes = !inQuotes;

                if (!inQuotes && charactersToReplace.Contains(c))
                    sb.Append(replacementString);
                else
                    sb.Append(c);
                pos++;
            }
            return sb.ToString();
        }

        static Color ParseColor(string colorString)
        {
            if (colorString.StartsWith("#") && (colorString.Length == 7 || colorString.Length == 9))
            {
                try
                {
                    return ColorTranslator.FromHtml(colorString);
                }
                catch (Exception)
                {
                    // Handle invalid color string
                    return System.Drawing.Color.Black; // Default color for errors
                }
            }
            else
            {
                try
                {
                    return System.Drawing.Color.FromName(colorString);
                }
                catch (Exception)
                {
                    // Handle unknown color names
                    return System.Drawing.Color.Black; // Default color for unknown names
                }
            }
        }

        static string ColorToString(Color color)
        {
            if (color.IsNamedColor)
                return color.Name.ToLower();
            else
            {
                string s = "#";
                if (color.A != 0xff)
                    s += color.A.ToString("X2");
                s += color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                return s;
            }
        }



        internal override void Render(Control parent)
        {

            //base.Render(parent); //Control.Render(...)

            if (Bounds == null || !BoundsValid)
                CalculateAbsoluteBounds();
            double x = Bounds.X;
            double y = Bounds.Y;
            double w = Bounds.W;
            double h = Bounds.H;

            //var absoluteBounds = CalculateAbsoluteBounds2();
            //double x = absoluteBounds.X;
            //double y = absoluteBounds.Y;
            //double w = absoluteBounds.W;
            //double h = absoluteBounds.H;


            //TEST
            //Draw.Rectangle(new Pen(System.Drawing.Color.Red, 0.2f), x, y, w, h);

            if (true)
            {
                //draw background
                Brush backgroundBrush = null;
                if (Hover)
                {
                    if (BackgroundColorHover != null && BackgroundColorHover != System.Drawing.Color.Transparent)
                        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColorHover).ToArgb().ToString("X8"));
                    //else
                    //    backgroundBrush = Draw.GetBrush("#AAEEEEEE");
                    else
                        if (BackgroundColor != null && BackgroundColor != System.Drawing.Color.Transparent)
                        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColor).ToArgb().ToString("X8"));
                }
                else
                {
                    if (BackgroundColor != null && BackgroundColor != System.Drawing.Color.Transparent)
                        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColor).ToArgb().ToString("X8"));
                    //else
                    //    backgroundBrush = null; // Draw.GetBrush("#AAEEEEEE");
                }
                if (backgroundBrush is SolidBrush)
                    Draw.FillRectangle((SolidBrush)backgroundBrush, x, y, w, h);
            }



            //return;
            if (Icon != null)
            {
                if (Icon.ToLower().StartsWith("fa:"))
                {
                    List<string> data = Icon.Split(':').ToList();
                    string iconName = data.Count > 1 ? data[1] : "";
                    string colorname = data.Count > 2 ? data[2] : null;


                    Image icon = GetFaIcon(name: iconName, colorName:colorname);
                    Draw.Image(icon, x, y, x + w, y + h);
                }
                else if (Icon.ToLower().StartsWith("svg:"))
                {
                    Image icon = GetSvgIcon(Icon.Substring(4));
                    Draw.Image(icon, x, y, x + w, y + h);
                }
                else if (Icon.ToLower().StartsWith("data:"))
                {
                    string base64String = Icon.Substring("data:".Length);
                    Image icon = GetBase64Icon(base64String);
                    Draw.Image(icon, x, y, x + w, y + h);
                }
            }

            var font = Draw.GetFont(_Font.FontFamily.Name, _Font.Size * 0.30 * Draw.mmToPx, _Font.Style);
            string text = "";
            if (this.Text is Delegate)
            {
                text = ((Delegate)_Text).DynamicInvoke().ToString();
            }
            else
            {
                text = Text?.ToString() ?? "";
            }
            Draw.Text(text.Replace("\r", "").Replace("\\n", "\n"), x, y, w, h, font
                , (Hover && EditMask != null) ? System.Drawing.Color.DodgerBlue : TextColor, TextAlignment);

            Pen pen = null;
            if (BorderTWidth > 0.0)
            {
                pen = Draw.GetPen("#" + BorderTColor.ToArgb().ToString("X8"), BorderTWidth);
                Draw.Line(pen, x, y, x + w, y);
            }
            if (BorderRWidth > 0.0)
            {
                pen = Draw.GetPen("#" + BorderRColor.ToArgb().ToString("X8"), BorderRWidth);
                Draw.Line(pen, x + w, y, x + w, y + h);
            }
            if (BorderBWidth > 0.0)
            {
                pen = Draw.GetPen("#" + BorderBColor.ToArgb().ToString("X8"), BorderBWidth);
                Draw.Line(pen, x, y + h, x + w, y + h);
            }
            if (BorderLWidth > 0.0)
            {
                pen = Draw.GetPen("#" + BorderLColor.ToArgb().ToString("X8"), BorderLWidth);
                Draw.Line(pen, x, y, x, y + h);
            }



            if (Boxes == null)
            {
                //if (Bounds == null || !BoundsValid)
                //    CalculateAbsoluteBounds();
                //Console.WriteLine($"*{this.Name}: x:{Bounds?.X}, y:{Bounds?.Y}, w:{Bounds?.Width}, h:{Bounds?.Height}");
            }
            else
            {
                foreach (var box in Boxes)
                {
                    //Console.WriteLine($"- {box.Name}: x:{box.Bounds?.X}, y:{box.Bounds?.Y}, w:{box.Bounds?.Width}, h:{box.Bounds?.Height}");
                    box.Render(parent);
                }
            }

            if (false)
            {
                //draw background
                Brush backgroundBrush;
                if (Hover)
                {
                    if (BackgroundColorHover != null && BackgroundColorHover != System.Drawing.Color.Transparent)
                        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColorHover).ToArgb().ToString("X8"));
                    else
                        backgroundBrush = Draw.GetBrush("#AAEEEEEE");
                }
                else
                {
                    if (BackgroundColor != null && BackgroundColor != System.Drawing.Color.Transparent)
                        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColor).ToArgb().ToString("X8"));
                    else
                        backgroundBrush = null; // Draw.GetBrush("#AAEEEEEE");
                }
                if (backgroundBrush is SolidBrush)
                    Draw.FillRectangle((SolidBrush)backgroundBrush, x, y, w, h);
            }


        }







        //    ////text
        //    //double boundsX = Bounds.X;
        //    //double boundsY = Bounds.Y;
        //    //double boundsW = Bounds.W;
        //    //double boundsH = Bounds.H;
        //    //double x,y,w,h = 0;


        //    ////Bounds.W = w; Bounds.H = h;

        //    ////Draw.Rectangle(Draw.GetPen("#" + TextColor.ToArgb().ToString("X8"), 0.01), x, y, w, h);
        //    ////Draw.Rectangle(Draw.GetPen("pink", 0.01), x, y, w, h);

        //    //Brush backgroundBrush;
        //    //if (Hover)
        //    //{
        //    //    if (BackgroundColorHover != null && BackgroundColorHover != System.Drawing.Color.Transparent)
        //    //        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColorHover).ToArgb().ToString("X8"));
        //    //    else
        //    //        backgroundBrush = Draw.GetBrush("#AAEEEEEE");
        //    //}
        //    //else
        //    //{
        //    //    if (BackgroundColor != null && BackgroundColor != System.Drawing.Color.Transparent)
        //    //        backgroundBrush = Draw.GetBrush("#" + ((System.Drawing.Color)BackgroundColor).ToArgb().ToString("X8"));
        //    //    else
        //    //        backgroundBrush = null; // Draw.GetBrush("#AAEEEEEE");
        //    //}
        //    //if (backgroundBrush is SolidBrush)
        //    //    Draw.FillRectangle((SolidBrush)backgroundBrush, x, y, w, h);


        //    ////sub-boxes
        //    //if (Boxes != null)
        //    //{
        //    //    foreach (Box box in Boxes)
        //    //    {
        //    //        box.Render(this);
        //    //    }
        //    //}


        //    //if (this.Left != null && this.Top != null)
        //    //{
        //    //    //double x = (boundsX + ParsePosition(this.Left, boundsW)) ?? boundsX;
        //    //    //double y = (boundsY + ParsePosition(this.Top, boundsH)) ?? boundsY;
        //    //    //double w = ParsePosition(this.Width, boundsW) ?? boundsW;
        //    //    //double h = ParsePosition(this.Height, boundsH) ?? boundsH;
        //    //    //Draw.Rectangle(Draw.GetPen("#" + this.TextColor.ToArgb().ToString("X8"), 0.01), x, y, w, h);
        //    //    Draw.Text(this.Text.Replace("\r", "").Replace("\\n", "\n"), x, y, w, h, this.Font, Hover ? System.Drawing.Color.DodgerBlue : this.TextColor, this.TextAlignment);
        //    //}
        //    //else
        //    //{
        //    //    Draw.Text(this.Text.Replace("\r", "").Replace("\\n", "\n"), boundsX + 0.5, boundsY + 0.5, boundsW - 1, boundsH - 1, this.Font, Hover ? System.Drawing.Color.DodgerBlue : this.TextColor, this.TextAlignment);
        //    //}

        //    //Pen pen = null;
        //    //if (BorderTWidth > 0.0) 
        //    //{
        //    //    pen = Draw.GetPen("#"+BorderTColor.ToArgb().ToString("X8"), BorderTWidth);
        //    //    Draw.Line(pen, x, y, x+w, y);
        //    //}
        //    //if (BorderRWidth > 0.0)
        //    //{
        //    //    pen = Draw.GetPen("#" + BorderRColor.ToArgb().ToString("X8"), BorderRWidth);
        //    //    Draw.Line(pen, x + w, y, x + w, y + h);
        //    //}
        //    //if (BorderBWidth > 0.0)
        //    //{
        //    //    pen = Draw.GetPen("#" + BorderBColor.ToArgb().ToString("X8"), BorderBWidth);
        //    //    Draw.Line(pen, x, y + h, x + w, y + h);
        //    //}
        //    //if (BorderLWidth > 0.0)
        //    //{
        //    //    pen = Draw.GetPen("#" + BorderLColor.ToArgb().ToString("X8"), BorderLWidth);
        //    //    Draw.Line(pen, x, y, x, y + h);
        //    //}
        //}





        Rectangle GetEffectiveBounds(Box box)
        {
            if (box.Parent != null)
            {
                double x = (box.Parent.Bounds.X + ParsePosition(box.Left, box.Parent.Bounds.W)) ?? box.Parent.Bounds.X;
                double y = (box.Parent.Bounds.Y + ParsePosition(box.Top, box.Parent.Bounds.H)) ?? box.Parent.Bounds.Y;
                double w = ParsePosition(box.Width, box.Parent.Bounds.W) ?? box.Parent.Bounds.W;
                double h = ParsePosition(box.Height, box.Parent.Bounds.H) ?? box.Parent.Bounds.H;
                return GetEffectiveBounds(box.Parent as Box);
            }
            else
            {
                return box.Bounds; //page-level bounds
            }
        }

        private double? ParsePositionX(object position, Rectangle bounds)
        {
            if (position is null) return null;

            double pos = 0;
            if (position is string)
            {
                var left = (position as string).Trim();
                if (left.EndsWith("%"))
                    pos = Bounds.X + Bounds.W * double.Parse(left.Substring(0, left.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture) / 100.0;
                else
                    pos = Bounds.X + double.Parse(left, NumberStyles.Any, CultureInfo.InvariantCulture);

            }

            if (position is double)
                pos = Bounds.X + (double)position;

            if (position is float)
                pos = Bounds.X + (double)position;

            return pos;
        }

        private double? ParsePosition(object position, double coord)
        {
            if (position is null) return null;

            double pos = 0;
            if (position is string)
            {
                var left = (position as string).Trim();
                if (left.EndsWith("%"))
                    pos = coord * double.Parse(left.Substring(0, left.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture) / 100.0;
                else
                    pos = double.Parse(left, NumberStyles.Any, CultureInfo.InvariantCulture);

            }

            if (position is double)
                pos = (double)position;

            if (position is float)
                pos = Convert.ToDouble(position);

            if (position is int)
                pos = Convert.ToDouble(position);

            return pos;
        }


        //private double? ParsePositionY(object position, Rectangle bounds)
        //{
        //    if (position is null) return null;

        //    double pos = 0;
        //    if (position is string)
        //    {
        //        var top = (position as string).Trim();
        //        if (top.EndsWith("%"))
        //            pos = Bounds.Y + Bounds.H * double.Parse(top.Substring(0, top.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture) / 100.0;
        //        else
        //            pos = Bounds.Y + double.Parse(top.Substring(0, top.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture);

        //    }

        //    if (position is double)
        //        pos = Bounds.Y + (double)position;

        //    if (position is float)
        //        pos = Bounds.Y + (double)position;

        //    return pos;
        //}

        LimitedSizeDictionary<string, Image> _faIconDict = new QB.LimitedSizeDictionary<string, Image>(100);
        Image GetFaIcon(string name, int size = 256, object colorName = null)
        {
  
            if (colorName == null)
                colorName = "black";


          //  colorName = "blue";

            try
            {
                string key = name + ":" + size + ":" + colorName;
                if (_faIconDict.ContainsKey(key))
                    return _faIconDict[key];
                else
                {
                    try
                    {
                        FontAwesome.Sharp.IconChar fa_icon = FontAwesome.Sharp.IconChar.None;
                        bool ok = Enum.TryParse<FontAwesome.Sharp.IconChar>(name.Replace("-", ""), true, out fa_icon);
                        Bitmap bmp = fa_icon.ToBitmap(size, size, colorName.ToColor()); // System.Drawing.Color.Black);
                        //HACK: the returned bmp is not centered. so move it slightly:
                        Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                        using (Graphics graph = Graphics.FromImage(bmp2))
                        {
                            graph.DrawImageUnscaled(bmp, (int)(bmp.Width * 0.01), (int)(bmp.Height * 0.08));
                        }
                        _faIconDict.Add(key, bmp2);
                        return bmp2;
                    }
                    catch
                    {
                        _faIconDict.Add(key, null);
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        LimitedSizeDictionary<string, Image> _svgIconDict = new QB.LimitedSizeDictionary<string, Image>(100);
        Image GetSvgIcon(string svgString, int width = 256, int height = 256)
        {
            try
            {
                string key = svgString + ":" + width + ":" + height;
                if (_svgIconDict.ContainsKey(key))
                    return _svgIconDict[key];
                else
                {
                    try
                    {
                        // Create an SvgDocument from the SVG string
                        SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(svgString);

                        // Set the desired width and height
                        svgDocument.Width = new SvgUnit(SvgUnitType.Pixel, width);
                        svgDocument.Height = new SvgUnit(SvgUnitType.Pixel, height);

                        Bitmap bmp = svgDocument.Draw();

                        _svgIconDict.Add(key, bmp);
                        return bmp;
                    }
                    catch
                    {
                        _svgIconDict.Add(key, null);
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        LimitedSizeDictionary<string, Image> _base64IconDict = new QB.LimitedSizeDictionary<string, Image>(100);

        Image GetBase64Icon(string base64String)
        {
            try
            {
                string key = base64String;
                if (_svgIconDict.ContainsKey(key))
                    return _svgIconDict[key];
                else
                {
                    try
                    {
                        //if (base64String.StartsWith("data:"))
                        //    base64String = base64String.Substring("data:".Length);
                        int startPos = base64String.IndexOf(";base64,");
                        if (startPos >= 0)
                            base64String = base64String.Substring(startPos + ";base64,".Length);

                        byte[] imageBytes = Convert.FromBase64String(base64String);
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            Image image = Image.FromStream(ms);
                            return image;
                        }
                    }
                    catch
                    {
                        _svgIconDict.Add(key, null);
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

