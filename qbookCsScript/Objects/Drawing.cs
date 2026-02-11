namespace QB
{

    public static class Drawing
    {
        public class Axis
        {
            public Axis()
            {
            }
            /*
            public Axis(double min, double max, double majorticks, double minorticks)
            {
                this.Min = min;
                this.Max = max;
                this.MajorTicks = majorticks;
                this.MinorTicks = minorticks;
            }
            */
            public double Min = 0;
            public double Max = 100;

            public double Offset = 0;
            public double Range
            {
                get
                {
                    return this.Max - this.Min;
                }
                set
                {
                    this.Min = 0;
                    this.Max = value;
                }
            }
            public double MajorTicks = 10;
            public double MinorTicks = 5;

            public void Set(double min = double.NaN, double max = double.NaN, double majorTicks = double.NaN, double minorTicks = double.NaN)
            {
                if (!double.IsNaN(min))
                    this.Min = min;
                if (!double.IsNaN(max))
                    this.Max = max;
                if (!double.IsNaN(majorTicks))
                    this.MajorTicks = majorTicks;
                if (!double.IsNaN(minorTicks))
                    this.MinorTicks = minorTicks;
            }

            public string Format = "auto";
            public System.Drawing.Color Color = System.Drawing.Color.Black;

            public override string ToString()
            {
                return Min + " " + Max + " " + MajorTicks + " " + MinorTicks;
            }
            public Axis Clone()
            {
                return (Axis)this.MemberwiseClone();
            }

            //@STFU 2024-11-12
            public string GetFormat()
            {
                string pattern = "0.000";

                if (Max >= 10)
                    pattern = "0.000";

                if (Max >= 100)
                    pattern = "0.00";

                if (Max >= 1000)
                    pattern = "0.0";

                if (Max >= 10000)
                    pattern = "0";

                if (Format == "auto")
                    return pattern;
                else
                    return this.Format;
            }
        }
    }
}