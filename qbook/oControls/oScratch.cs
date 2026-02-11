using QB.Controls;
using System;

namespace qbook
{
    [Serializable]
    public class oScratch : oControl
    {
        public oScratch()
        {
        }
        public oScratch(string name, string text) : base(name, text)
        {
        }

        public string Data;
        public override void Render()
        {
            Frame(true, true);
            if (Data == null)
                return;
            float x1 = float.NaN;
            float y1 = float.NaN;
            //Console.WriteLine($"\r\nscratch: --- render ---");
            foreach (string p in Data.Split(';'))
            {
                //Console.WriteLine($"scratch: {p}");
                string[] splits = p.Split(',');
                if (splits.Length == 2)
                {
                    float x = float.Parse(splits[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    float y = float.Parse(splits[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (!float.IsNaN(x1))
                    {
                        //Console.WriteLine($"scratch: {Draw.DrawPen}: {x1 + Bounds.X}/{y1 + Bounds.Y}-{x + Bounds.X}/{y + Bounds.Y}");
                        Draw.Line(Draw.DrawPen, x1 + Bounds.X, y1 + Bounds.Y, x + Bounds.X, y + Bounds.Y);
                    }
                    x1 = x;
                    y1 = y;
                }
                else
                {
                    x1 = float.NaN;
                    y1 = float.NaN;
                }
            }
        }
    }
}
