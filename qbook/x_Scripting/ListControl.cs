using QB.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace qbook
{
    public partial class ListControl : UserControl
    {
        public ListControl()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            this.MouseWheel += ListControl_MouseWheel;

            //this.SuspendLayout();
            //// 
            //// ListControl
            //// 
            //this.Name = "ListControl";
            //this.DoubleClick += new System.EventHandler(this.ListControl_DoubleClick);
            //this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListControl_MouseDown);
            //this.ResumeLayout(false);
        }

        private void ListControl_MouseWheel(object sender, MouseEventArgs e)
        {
            int itemCount = this.Height / itemHeight;

            TopItemIndex -= e.Delta / 20;
            if (TopItemIndex < 0)
                TopItemIndex = 0;
            if (TopItemIndex > Items.Count - itemCount)
                TopItemIndex = Items.Count - itemCount - 1;
            this.Invalidate();
        }

        public List<object> Items = new List<object>();

        int TopItemIndex = 0;
        //int ItemCount = 10;

        System.Drawing.Font ItemFont = new System.Drawing.Font("Consolas", 8);
        System.Drawing.Font ItemFontBold = new System.Drawing.Font("Consolas", 8, FontStyle.Bold);
        System.Drawing.Font ItemFontItalic = new System.Drawing.Font("Consolas", 8, FontStyle.Italic);
        System.Drawing.Font ItemFontUnderline = new System.Drawing.Font("Consolas", 8, FontStyle.Underline);
        System.Drawing.Font ItemFontStrikeout = new System.Drawing.Font("Consolas", 8, FontStyle.Strikeout);
        int itemHeight = 12;

        public bool ShowExtendedInfo = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            try
            {
                int itemCount = this.Height / itemHeight;

                vScrollBar1.Minimum = 0;
                vScrollBar1.Maximum = Math.Max(0, Items.Count); // - itemCount * 5 / 6); //TODO: !!!
                if (TopItemIndex < 0)
                    TopItemIndex = 0;
                vScrollBar1.Value = Math.Min(TopItemIndex, vScrollBar1.Maximum);

                if (Items != null && Items.Count > 0)
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        if (Items.Count > TopItemIndex + i)
                        {
                            var item = Items[TopItemIndex + i];
                            if (item is LogEntry)
                            {
                                var logEntry = item as LogEntry;
                                Brush brush = Brushes.Black;
                                switch (logEntry.Type)
                                {
                                    case 'L': brush = Brushes.Black; break;
                                    case 'E': brush = Brushes.Red; break;
                                    case 'W': brush = Brushes.DarkOrange; break;
                                    case 'I': brush = Brushes.Black; break;
                                    case 'D': brush = Brushes.SteelBlue; break;
                                }
                                System.Drawing.Font itemFont = ItemFont;
                                if (logEntry.Style != null)
                                {
                                    string[] splits = logEntry.Style.Split(':');
                                    if (splits.Length > 0)
                                        brush = Draw.GetBrush(splits[0]);
                                    if (splits.Length > 1)
                                    {
                                        switch (splits[1].ToUpper())
                                        {
                                            case "B": itemFont = ItemFontBold; break;
                                            case "I": itemFont = ItemFontItalic; break;
                                            case "U": itemFont = ItemFontUnderline; break;
                                            case "S": itemFont = ItemFontStrikeout; break;
                                        }
                                    }
                                }
                                string text = (ShowExtendedInfo ? logEntry.ToStringEx() : logEntry.ToString());
                                bool showSpecialChars = true;
                                if (showSpecialChars)
                                {
                                    string pattern = @"([\r\n\t\u0000\u0002\u0003\u0007\u0008\u000C\u001B\u007F])";
                                    // Split the input string using the regular expression pattern
                                    string[] splits = Regex.Split(text, pattern);

                                    // Display the split texts and characters
                                    float x = 1.0f;
                                    string sumText = "";
                                    foreach (string split in splits)
                                    {
                                        float sumTextWidth = e.Graphics.MeasureString(sumText, itemFont).Width;
                                        x = 1.0f + sumTextWidth - 3.4f;
                                        if (x < 1.0f)
                                            x = 1.0f;
                                        string token = split;
                                        if (Regex.IsMatch(split, pattern))
                                        {
                                            switch (split)
                                            {
                                                case "\r": token = "R"; break;
                                                case "\n": token = "N"; break;

                                                case "\t": token = "T"; break;
                                                case "\u0000": token = "0"; break; //null 0
                                                case "\u0002": token = "S"; break; //Stx
                                                case "\u0003": token = "E"; break; //Etx
                                                case "\u0007": token = "L"; break; //belL
                                                case "\u0008": token = "B"; break; //Backspace
                                                case "\u000C": token = "F"; break; //Form feed
                                                case "\u001B": token = "X"; break; //escape X
                                                case "\u007F": token = "D"; break; //Del
                                            }
                                            float textWidth = e.Graphics.MeasureString(token, itemFont).Width;
                                            e.Graphics.FillRectangle(brush, new RectangleF(x + 1.0f, 1 + i * itemHeight + 1.0f, textWidth - 2.6f, itemHeight - 1.0f));
                                            e.Graphics.DrawString(token, itemFont, Brushes.White, new PointF(x, 1 + i * itemHeight));
                                            //x += textWidth - 4.0f;
                                            sumText += token;
                                        }
                                        else
                                        {
                                            //float textWidth = e.Graphics.MeasureString(token, itemFont).Width;
                                            e.Graphics.DrawString(token, itemFont, brush, new PointF(x, 1 + i * itemHeight));
                                            //x += textWidth - 3.0f;
                                            sumText += token;
                                        }
                                    }
                                }
                                else
                                {
                                    e.Graphics.DrawString(text, itemFont, brush, new System.Drawing.Rectangle(1, 1 + i * itemHeight, this.Width - 1, itemHeight));
                                }
                            }
                            else
                            {
                                e.Graphics.DrawString(item.ToString(), ItemFont, Brushes.Black, new System.Drawing.Rectangle(1, 1 + i * itemHeight, this.Width - 1, itemHeight));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                e.Graphics.DrawLine(Pens.Red, 0, 0, this.Width, this.Height);
                e.Graphics.DrawLine(Pens.Red, 0, this.Width, 0, this.Height);
            }

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            TopItemIndex = e.NewValue;
            this.Invalidate();
            base.OnScroll(e);
        }

        public void ScrollToEnd()
        {
            TopItemIndex = Items.Count - (this.Height / itemHeight);
            this.Invalidate();
        }

        //private void InitializeComponent()
        //{
        //    this.SuspendLayout();
        //    // 
        //    // ListControl
        //    // 
        //    this.Name = "ListControl";
        //    this.DoubleClick += new System.EventHandler(this.ListControl_DoubleClick);
        //    this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListControl_MouseDown);
        //    this.ResumeLayout(false);

        //}

        private void ListControl_Click(object sender, EventArgs e)
        {
            try
            {
                int rowId = TopItemIndex + LastMousePos.Y / itemHeight;
                var item = Items[rowId];
                if (item != null)
                    OnItemClicked(rowId, item);
            }
            catch (Exception ex)
            {

            }
        }


        private void ListControl_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                int rowId = TopItemIndex + LastMousePos.Y / itemHeight;
                var item = Items[rowId];
                if (item != null)
                    OnItemDoubleClicked(rowId, item);
            }
            catch (Exception ex)
            {

            }

        }

        Point LastMousePos = Point.Empty;
        private void ListControl_MouseDown(object sender, MouseEventArgs e)
        {
            LastMousePos = e.Location;
        }

        public class ItemClickedEventArgs : EventArgs
        {
            public int Index;
            public object Item;
        }
        public delegate void ItemClickedEventHandler(ItemClickedEventArgs e);
        public event ItemClickedEventHandler ItemClicked;
        void OnItemClicked(int index, object item)
        {
            if (ItemClicked != null)
            {
                ItemClickedEventArgs ea = new ItemClickedEventArgs() { Index = index, Item = item };
                ItemClicked(ea);
            }
        }
        public delegate void ItemDoubleClickedEventHandler(ItemClickedEventArgs e);
        public event ItemDoubleClickedEventHandler ItemDoubleClicked;
        void OnItemDoubleClicked(int index, object item)
        {
            if (ItemDoubleClicked != null)
            {
                ItemClickedEventArgs ea = new ItemClickedEventArgs() { Index = index, Item = item };
                ItemDoubleClicked(ea);
            }
        }

    }
}
