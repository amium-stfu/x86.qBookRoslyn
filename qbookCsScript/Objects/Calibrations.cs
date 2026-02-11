using System;
using System.Collections.Generic;

namespace QB
{

    public static class Calibrations
    {
        public class o1066 //: CustomObject
        {
            public o1066()
            {
            }

            public void Process()
            {
                //  string name = "_1066_";
                Dictionary<double, double> values = new Dictionary<double, double>();

                // ref , read

                if (false)
                {
                    values.Add(160.704, 162);
                    values.Add(145.8, 145.8);
                    values.Add(130.41, 129.6);
                    values.Add(114.372, 113.4);
                    values.Add(98.658, 97.2);
                    values.Add(82.458, 81);
                    values.Add(65.934, 64.8);
                    values.Add(41.148, 40.5);
                    values.Add(32.886, 32.4);
                    values.Add(16.362, 16.2);
                    //     values.Add(0, 0);
                }
                else
                {

                    //     values.Add(0, 0);
                    values.Add(16.362, 16.2);
                    values.Add(32.886, 32.4);
                    values.Add(41.148, 40.5);
                    values.Add(65.934, 64.8);
                    values.Add(82.458, 81);
                    values.Add(98.658, 97.2);
                    values.Add(114.372, 113.4);
                    values.Add(130.41, 129.6);
                    values.Add(145.8, 145.8);
                    values.Add(160.704, 162);
                }


                /*
                values.Add(0, 0);
                values.Add(16.2, 16.362);
                values.Add(32.4, 32.886);
                values.Add(40.5, 41.148);
                values.Add(64.8, 65.934);
                values.Add(81, 82.458);
                values.Add(97.2, 98.658);
                values.Add(113.4, 114.372);
                values.Add(129.6, 130.41);
                values.Add(145.8, 145.8);
                values.Add(162, 160.704);
                */



                /*
                foreach (string vt in (Parent as TextWriter).Parameter.Value.GetItems())
                {
                    try
                    {
                        string[] vtt = vt.TrimStart('[').TrimEnd(']').Split(',');
                        if (vtt.Length == 2)
                        {
                            values.Add(vtt[0].ToDouble(), vtt[1].ToDouble());
                        }
                    }
                    catch
                    { }
                }
                */
                int count = values.Count;// Count.Parameter.Value = values.Count;
                double sumRef = 0;
                double sumRead = 0;
                foreach (double refKey in values.Keys)
                {
                    sumRef += refKey;
                    sumRead += values[refKey];
                }
                sumRef /= values.Count;
                sumRead /= values.Count;
                //  Console.WriteLine("" + sumRef + " " + sumRead);

                double ssdf = 0;
                double ssf = 0;
                double ssd = 0;
                foreach (double refKey in values.Keys)
                {
                    ssdf += (values[refKey] - sumRead) * (refKey - sumRef);
                    ssf += (refKey - sumRef) * (refKey - sumRef);
                    ssd += (values[refKey] - sumRead) * (values[refKey] - sumRead);
                }
                //  Qb.Set(Name + ".ssdf", ssdf);
                //  Qb.Set(Name + ".ssf", ssf);
                //  Qb.Set(Name + ".ssd", ssd);

                double a1_ = ssdf / ssf;
                double a0_ = sumRead - (a1_ * sumRef);

                //  Qb.Set(Name + ".sumRead", sumRead);
                //  Qb.Set(Name + ".sumRef", sumRef);

                double f = 0;


                double minRef = double.MinValue;
                double maxRef = double.MaxValue;
                foreach (double refKey in values.Keys)
                {
                    if (!double.IsNaN(refKey))
                        maxRef = Math.Max(maxRef, refKey);

                    f += Math.Pow(values[refKey] - a0_ - (a1_ * refKey), 2);

                    if (!double.IsNaN(refKey))
                        minRef = Math.Max(minRef, refKey);
                }

                foreach (double refKey in values.Keys)
                {
                    if (double.IsNaN(maxRef))
                        maxRef = refKey;
                    f += Math.Pow(values[refKey] - a0_ - (a1_ * refKey), 2);
                    minRef = refKey;
                }

                max = Math.Round(minRef, 3);
                max1perc = Math.Round(minRef / 100.0 * 1.0, 3);
                max2perc = Math.Round(minRef / 100.0 * 2.0, 3);

                double xmina1a0_ = Math.Abs(maxRef * (a1_ - 1) + a0_);
                xmina1a0 = Math.Round(xmina1a0_, 4);

                a0_ /= minRef;
                a0_ *= 100;
                a0 = Math.Round(a0_, 3);
                a1 = Math.Round(a1_, 3);

                double see_ = Math.Sqrt(f / (values.Count - 2));
                see = Math.Round(see_, 4);

                see_ /= minRef;
                see_ *= 100;
                see_x = Math.Round(see_, 4);

                double r2_ = 1 - (f / ssd);
                r2 = Math.Round(r2_, 4);
            }

            public double max;
            public double max1perc;
            public double max2perc;
            public double xmina1a0;
            public double a0;
            public double a1;
            public double see;
            public double see_x;
            public double r2;
        }
    }
}