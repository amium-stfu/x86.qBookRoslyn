using QB.Controls;
using System;
using System.Drawing;

namespace qbook
{
    [Serializable]
    public class oText : oControl
    {
        public oText()
        {
        }
        public oText(string name, string text) : base(name, text)
        {
        }
        int angle = 0;
        public override void Render()
        {
            if (qbook.Core.ThisBook.DesignMode)
            {
                Draw.FillRectangle(Selected ? Draw.WhiteBrush : Draw.WhiteBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            }
            Frame(true, true);
            string[] lines = TextL.Replace("\r\n", "\n").Trim('\n').Split('\n');

            float y = 1;
            bool header = true;
            foreach (string line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    if (line.Trim().ToUpper() == "#WARNING")
                    {
                        for (int i = 0; i < 20; i++)
                            Draw.Polygon(Pens.Red, 3, Bounds.X + 5 + 10 * i, Bounds.Y - 5, 10, angle);
                        angle += 5;
                    }
                    if (line.Trim().ToUpper() == "#DANGER")
                    {

                    }
                    if (line.Trim().ToUpper() == "#INFO")
                    {

                    }

                    if (header)
                    {
                        foreach (string line2 in SplitText(line, Draw.fontHeader3, Bounds.W * Draw.mmToPx))
                        {
                            Draw.Text(line2, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontHeader3, Selected ? Color.Orange : Color.Black, ContentAlignment.TopLeft);
                            y += 7f;
                        }
                    }
                    else
                    {
                        foreach (string line2 in SplitText(line, Draw.fontText, Bounds.W * Draw.mmToPx))
                        {
                            Draw.Text(line2, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, ContentAlignment.TopLeft);
                            y += 6f;
                        }
                    }
                    header = false;
                }
            }
            // Draw.Text(Text, Bounds.X, Bounds.Y, Draw.fontHeader2, Color.Black);
        }
    }
}


