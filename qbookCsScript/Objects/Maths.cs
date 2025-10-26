using System;
using System.Collections.Generic;

namespace QB
{
    public static class Maths
    {
    }

    public class Buffer : Item
    {
        Dictionary<DateTime, double> values = new Dictionary<DateTime, double>();
        public Buffer(string name) : base(name, null)
        {
        }

        public void Clear()
        {
            values.Clear();
        }

        public void Add(double value)
        {
            values.Add(DateTime.Now, value);
        }

        public double Min()
        {
            double min = double.MaxValue;
            foreach (double v in values.Values)
                min = Math.Min(min, v);
            return min;
        }

        public double Max()
        {
            double max = double.MinValue;
            foreach (double v in values.Values)
                max = Math.Max(max, v);
            return max;
        }

        public double Avg()
        {
            double sum = 0;
            foreach (double v in values.Values)
                sum += v;
            if (values.Count >= 0)
                return (sum / values.Count);
            return 0;
        }

        public double Dstd()
        {
            double avg = Avg();
            double dstd = 0;
            foreach (double v in values.Values)
                dstd += Math.Pow(v - avg, 2);
            if (values.Count > 1)
                return Math.Sqrt(dstd / (values.Count - 1));
            return 0;
        }

        public double T90()
        {
            double min = Min();
            double max = Max();
            double l10 = min + (max - min) * 0.1;
            double l90 = min + (max - min) * 0.9;

            bool t10Set = false;

            DateTime start = DateTime.Now;
            foreach (DateTime dateTime in values.Keys)
            {
                if (!t10Set && (values[dateTime] > l10))
                {
                    t10Set = true;
                    start = dateTime;
                }

                if (values[dateTime] > l90)
                    return (dateTime - start).TotalMilliseconds;
            }
            return 0;
        }
    }

    public class Evaluation : Item
    {
        Dictionary<double, double> values = new Dictionary<double, double>();
        public Evaluation(string name) : base(name, null)
        {
        }

        public void Clear()
        {
            values.Clear();
        }

        public void Add(double x, double y)
        {
            values.Add(x, y);
        }


    }
}