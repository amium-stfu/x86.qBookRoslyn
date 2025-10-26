using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Helpers
{
    public class SignalFilter : Module
    {
        string name;
        public Module raw = new Module("Raw");
        public QB.Timer timer;

        //adjust
        public (Signal offset, Signal gain, Signal set, Signal raw, double[] buffer, int bufferCounter, double bufferAvg) Adjust;

        //global
        public (
            Module interval,
            Module zeroClipping,
            Module recordInterval,
            Module filterTime, Signal mode,
            Module lowLevelThreshold,
            Module lowLevelFilterTime,
            List<double> values,
            int valuesMax,
            int valuesQty) Filter;


        //PeakFilter
        public (
            Signal active,
            Module threshold,
            Module length,
            double[] buffer,
            int counter,
            double bufferAvg,
            int skipCounter,
            bool peakDetected,
            double dif) PeakFilter;


        //dynamicMode
        public (
            Signal active,
            Module thresholdAbs,
            Module thresholdRel,
            Module filterTime,
            Module overrunTime) DynamicFilter;

        //statistics
        public (
            Signal active,
            Signal min,
            Signal max,
            Signal average,
            Signal stdDev,
            Module valueInterval,
            Module intValue,
            DateTime initTime,
            int avgCounter,
            double avgSum,
            List<DateTime> timeHistory,
            List<double> valueHistory,
            DateTime start,
            DateTime stop) Statistics;



        public List<double> filterValues = new List<double>();
        double sum;
        int max = 0;
        double result = 0;

        public SignalFilter(string name, Module raw, int Interval, int Filtertime) : base(name)
        {

            this.raw = raw;
            Adjust.offset = new Signal("offset", text: "Offset");
            Adjust.gain = new Signal("gain", text: "Gain");
            Adjust.set = new Signal("set", text: "Set");
            Adjust.raw = new Signal("raw", text: "AdjustRaw");
            Adjust.buffer = new double[10];
            Adjust.bufferCounter = 0;

            Adjust.offset.Value = 0;
            Adjust.gain.Value = 1;
            Adjust.set.Value = 100;


            Filter.interval = new Module("interval", text: "ScanInterval", unit: "ms");
            Filter.recordInterval = new Module("recordInterval", text: "RecordInterval", unit: "Scans");
            Filter.filterTime = new Module("filtertime", text: "Filtertime", unit: "ms");
            Filter.mode = new Signal("mode", text: "Mode");
            Filter.lowLevelThreshold = new Module("lowLevelThreshold", text: "LowLevelThreshold");
            Filter.lowLevelFilterTime = new Module("lowLevelFilterTime", text: "LowLevelFilterTime");
            Filter.values = new List<double>();
            Filter.lowLevelThreshold.Value = 0;
            Filter.lowLevelFilterTime.Value = 0;

            Filter.interval.Value = Interval;
            Filter.recordInterval.Value = 1;
            Filter.filterTime.Value = Filtertime;
            Filter.mode.Value = 1;

            Filter.zeroClipping = new Module("zeroClipping") { Text = "Zeroclipping" };


            PeakFilter.active = new Signal("on", text: "Peakfilter");
            PeakFilter.threshold = new Module("threshold", text: "Threshold");
            PeakFilter.length = new Module("peakLength", text: "max. length", unit: "ms");
            PeakFilter.buffer = new double[3];
            PeakFilter.counter = 0;
            PeakFilter.bufferAvg = 0;
            PeakFilter.dif = 0;
            PeakFilter.skipCounter = 0;
            PeakFilter.peakDetected = false;

            PeakFilter.active.Value = 0;
            PeakFilter.threshold.Value = 10;
            PeakFilter.length.Value = 200;

            PeakFilter.active.Parameters["OnOff"] = PeakFilter.active;
            PeakFilter.active.Parameters["peakThreshold"] = PeakFilter.threshold;
            PeakFilter.active.Parameters["peakLength"] = PeakFilter.length;



            DynamicFilter.active = new Signal("on", text: "DynamicFilter");
            DynamicFilter.thresholdAbs = new Module("dynamicAbsThreshold", text: "Theshold abs.");
            DynamicFilter.thresholdRel = new Module("dynamicRelThreshold", text: "Theshold rel.", unit: "%");
            DynamicFilter.filterTime = new Module("dynamicFiltertime", text: "Filtertime", unit: "ms");
            DynamicFilter.overrunTime = new Module("dynamicOverruntime", text: "Overruntime", unit: "ms");
            DynamicFilter.active.Value = 1;
            DynamicFilter.thresholdAbs.Value = 2;
            DynamicFilter.thresholdRel.Value = 5;
            DynamicFilter.filterTime.Value = 1000;
            DynamicFilter.overrunTime.Value = 1500;

            DynamicFilter.active.Parameters["OnOff"] = DynamicFilter.active;
            DynamicFilter.active.Parameters["dynamicAbsThreshold"] = DynamicFilter.thresholdAbs;
            DynamicFilter.active.Parameters["dynamicRelThreshold"] = DynamicFilter.thresholdRel;
            DynamicFilter.active.Parameters["dynamicFiltertime"] = DynamicFilter.filterTime;
            DynamicFilter.active.Parameters["dynamicOverruntime"] = DynamicFilter.overrunTime;


            Statistics.active = new Signal("statistic", text: "Statistics");
            Statistics.min = new Signal("statsMin", text: "min");
            Statistics.max = new Signal("statsMax", text: "max");
            Statistics.average = new Signal("average", text: "average");
            Statistics.stdDev = new Signal("stdDev", text: "std.Deviation");
            Statistics.valueInterval = new Module("valueInterval", text: "valueInterval", unit: "ms");
            Statistics.intValue = new Module("intValue", text: "int. Value");
            Statistics.timeHistory = new List<DateTime>();
            Statistics.valueHistory = new List<double>();

            Statistics.active.Value = 0;
            Statistics.min.Value = 0;
            Statistics.max.Value = 0;
            Statistics.average.Value = 0;
            Statistics.intValue.Value = 0;
            Statistics.stdDev.Value = 0;
            Statistics.valueInterval.Value = 60000;


            timer = new Timer("filter", Interval);


            //defaults
            Parameters["mode"] = Filter.mode;
            Parameters["scanInterval"] = Filter.interval;
            Parameters["recordInterval"] = Filter.recordInterval;
            Parameters["filtertime"] = Filter.filterTime;



            timer.OnElapsed = (t, ea) => Idle(t, ea);
            timer.Run();
        }

        void recordValues()
        {

            if (double.IsNaN(raw.Value))
                return;

            double v = Adjust.raw.Value;

            Adjust.raw.Value = raw.Value + Adjust.offset.Value * Adjust.gain.Value;

            if (Filter.zeroClipping.Value == 1)
                Adjust.raw.Value = Adjust.raw.Value < 0 ? 0 : Adjust.raw.Value;




            PeakFilter.peakDetected = false;
            Filter.valuesMax = ((int)Filter.filterTime.Value / timer.Interval / (int)Filter.recordInterval.Value);


            //		if (lowLevel.Value == 1) {
            //			if (raw.Value < 10)
            //				max = max * 2;
            //		}

            Filter.valuesQty = 0;
            Filter.recordInterval.Set.Value++;


            if (PeakFilter.active.Value == 1)
            {
                PeakFilter.bufferAvg = (PeakFilter.buffer[0] + PeakFilter.buffer[1] + PeakFilter.buffer[2]) / 3;

                PeakFilter.dif = Adjust.raw.Value - Value;

                if (PeakFilter.dif < 0) PeakFilter.dif = PeakFilter.dif * -1;

                if (PeakFilter.dif > PeakFilter.threshold.Value)
                {
                    PeakFilter.length.Set.Value += Filter.interval.Value;
                    QB.Logger.Info("peak peakDetected length = " + PeakFilter.length.Set.Value);
                    if (PeakFilter.length.Set.Value < PeakFilter.length.Value)
                    {
                        Adjust.raw.Value = Value;
                    }
                    else
                    {
                        QB.Logger.Info("peakReset");
                        PeakFilter.length.Set.Value = 0;
                    }
                }
                else
                {
                    PeakFilter.buffer[PeakFilter.counter] = Adjust.raw.Value;
                    PeakFilter.skipCounter = 0;
                    PeakFilter.length.Set.Value = 0;
                    PeakFilter.counter++;
                    if (PeakFilter.counter == 3)
                        PeakFilter.counter = 0;
                }
            }



            if (Filter.filterTime.Value > 0)
            {

                //DynamicMode
                if (DynamicFilter.active.Value == 1)
                {
                    double dynDif = Value - v;
                    double min = Value * (DynamicFilter.thresholdRel.Value / 100);
                    if (min < 0)
                        min = min * -1;

                    if (min < DynamicFilter.thresholdAbs.Value)
                        min = DynamicFilter.thresholdAbs.Value;


                    if (dynDif < 0)
                        dynDif = dynDif * -1;

                    if (dynDif > min)
                        DynamicFilter.overrunTime.Set.Value = 0;

                    if (DynamicFilter.overrunTime.Set.Value < DynamicFilter.overrunTime.Value)
                    {
                        DynamicFilter.overrunTime.Set.Value += Filter.interval.Value;
                        Filter.valuesMax = 1;
                        if (DynamicFilter.filterTime.Value > 0)
                            Filter.valuesMax = (DynamicFilter.filterTime.Value / Filter.interval.Value).ToInt32();
                    }

                    if (DynamicFilter.overrunTime.Set.Value < DynamicFilter.overrunTime.Value)
                    {
                        Filter.values.Add(v);
                        Filter.recordInterval.Set.Value = 0;
                        Filter.valuesMax = (DynamicFilter.filterTime.Value / Filter.interval.Value).ToInt32();
                    }
                    else
                    {
                        int d = Filter.valuesMax - Filter.valuesQty;
                        if (d > 0)
                        {
                            int mA = 0;
                            while (mA < d)
                            {
                                Filter.values.Add(Value);
                                mA++;
                            }
                        }

                    }
                }

                if (Filter.recordInterval.Set.Value >= Filter.recordInterval.Value)
                {
                    Filter.values.Add(v);
                    Filter.recordInterval.Set.Value = 0;
                    Adjust.buffer[Adjust.bufferCounter] = raw.Value;
                    Adjust.bufferCounter++;
                    if (Adjust.bufferCounter == 10)
                        Adjust.bufferCounter = 0;
                }

                Filter.valuesQty = Filter.values.Count();
                int dif = Filter.valuesQty - Filter.valuesMax;

                if (Filter.valuesQty > Filter.valuesMax)
                {
                    Filter.values.RemoveAt(0);
                }

                Filter.valuesQty = Filter.values.Count();
                dif = Filter.valuesQty - Filter.valuesMax;
                if (Filter.valuesQty > Filter.valuesMax)
                {
                    Filter.values.RemoveRange(0, dif);

                }


                int mDif = Filter.valuesMax - Filter.valuesQty;
                if (mDif > 0)
                {
                    int mA = 0;
                    while (mA < mDif)
                    {
                        Filter.values.Add(v);
                        mA++;
                    }
                }

            }
        }
        double avg()
        {
            result = 0;
            double sum = 0;
            foreach (double v in Filter.values)
            {
                sum = sum + v;
            }
            result = sum / Filter.valuesMax;
            return result;
        }
        double ema()
        {
            if (Filter.values.Count == 0) return Adjust.raw.Value;
            double sf = 2.0 / (Filter.values.Count + 1.0);
            double ema = Filter.values[0];
            for (int i = 1; i < Filter.values.Count; i++) ema = Filter.values[i] * sf + ema * (1 - sf);
            return ema;
        }
        double wma()
        {
            double sumC = 0;
            double c = 1;
            sum = 0;
            foreach (double v in Filter.values)
            {
                c++;
                sum += v * c;
                sumC += c;
            }
            result = sum / sumC;
            return result;
        }
        double emawma()
        {
            double sumC = 0;
            double c = 1;
            double sf = 2.0 / ((double)Filter.valuesMax + 1.0) * 10.0;
            double ema = 0;
            List<double> emaList = new List<double>();
            //emaList.Add(0);
            ema = Filter.values[0];
            foreach (double v in Filter.values)
            {
                ema = v * sf + ema * (1 - sf);
                emaList.Add(ema);
            }
            sum = 0;
            foreach (double v in emaList)
            {
                c++;
                sum += v * c;
                sumC += c;
            }

            result = sum / sumC;
            return result;
        }

        double standardDeviation(IEnumerable<double> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                double average = sequence.Average();
                double sum = sequence.Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / sequence.Count());
            }
            return result;
        }


        void stats()
        {
            if (Statistics.active.Value == 0)
                return;

            Statistics.timeHistory.Add(DateTime.Now);
            Statistics.valueHistory.Add(Value);

            Statistics.min.Value = Value < Statistics.min.Value ? Value : Statistics.min.Value;
            Statistics.max.Value = Value > Statistics.max.Value ? Value : Statistics.max.Value;

            Statistics.avgSum += Value;
            Statistics.avgCounter++;
            Statistics.average.Value = Statistics.avgSum / Statistics.avgCounter;

            //			//			stdDevList.Add(Value);
            //			//			stdDev.Value = standardDeviation(stdDevList);

            DateTime now = DateTime.Now;
            TimeSpan t = (now - Statistics.initTime);
            Statistics.initTime = now;
            Statistics.intValue.Value += (Value / Statistics.valueInterval.Value) * t.TotalMilliseconds;
            Statistics.intValue.Out.Value = t.TotalMilliseconds;
            TimeSpan total = (now - Statistics.start);
            Statistics.intValue.Set.Value = total.TotalSeconds;


        }

        public void startStatistic()
        {
            Statistics.avgCounter = 0;
            Statistics.avgSum = 0;
            Statistics.min.Value = Value;
            Statistics.max.Value = Value;
            Statistics.average.Value = 0;
            Statistics.timeHistory.Clear();
            Statistics.valueHistory.Clear();
            Statistics.intValue.Value = 0;
            Statistics.start = DateTime.Now;
            Statistics.initTime = Statistics.start;
            Statistics.active.Value = 1;
        }

        public void stopStatistic()
        {
            Statistics.stop = DateTime.Now;
            Statistics.active.Value = 0;
        }


        private void Idle(QB.Timer t, TimerEventArgs ea)
        {
            raw.Set.Value = (Set.Value + Adjust.offset.Value * -1) / Adjust.gain.Value;

            Out.Value = raw.Out.Value;
            timer.Interval = Filter.interval.Value.ToInt32();
            if (Filter.filterTime.Value == 0)
            {
                Value = Adjust.raw.Value;
                return;
            }
            recordValues();
            if (Filter.mode.Value == 0) Value = Adjust.raw.Value;
            if (Filter.mode.Value == 1) Value = avg();
            if (Filter.mode.Value == 2) Value = wma();
            if (Filter.mode.Value == 3) Value = ema();
            if (Filter.mode.Value == 4) Value = emawma();
            stats();
        }


        public void adjustZero()
        {

            Adjust.bufferAvg = 0;
            foreach (double n in Adjust.buffer)
                Adjust.bufferAvg += n;

            Adjust.bufferAvg = Adjust.bufferAvg / 10;
            Adjust.offset.Value = Adjust.bufferAvg * -1;
            Set.Value = 0;
        }

        public void adjustSpan()
        {
            Adjust.bufferAvg = 0;
            foreach (double n in Adjust.buffer)
                Adjust.bufferAvg += n;

            Adjust.bufferAvg = Adjust.bufferAvg / 10;
            Adjust.gain.Value = Adjust.set.Value / (Adjust.bufferAvg + Adjust.offset.Value);
            Set.Value = Adjust.set.Value;
        }

    }
}
