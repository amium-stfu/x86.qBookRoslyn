
using System;
using System.ComponentModel;

namespace QB.Automation
{
    public class Pid : TimerModule
    {

        /// <summary>
        /// Creates a new PID (proportional–integral–derivative controller)
        /// </summary>
        //    public Pid(string name) : base(name)
        //   {
        //      
        //  }

        /// <summary>
        /// Initializes the PID
        /// </summary>
        /// <param name="name">text/label of pid object</param>
        /// <param name="t">timer interval in milliseconds</param>
        /// <param name="ks">transfer coefficient [in dx/dy (eg. °C/%)]</param>
        /// <param name="tu">delay time [in milliseconds]</param>
        /// <param name="tg">compensation time [in milliseconds]</param>
        public Pid(string name, string text = null, string unit = "", double t = 1 * 1000, double ks = 10, double tu = 10 * 1000, double tg = 600 * 1000) : base(name, text: text)
        {
            Unit = unit;
            Color = System.Drawing.Color.Red;
            Set.Unit = unit;
            Set.Value = 0;
            Set.Name = name + ".Set";
            Set.Color = System.Drawing.Color.Green;
            Set.ValueRule = "0..20000";
            Out.Unit = "%";
            Out.Color = System.Drawing.Color.Blue;
            Out.Name = name + ".Out";
            Out.ValueRule = "0..100";



            T = t;


            KS = new Signal("KS", text: "loop factor", value: ks, unit: "1");
            Tu = new Signal("Tu", text: "dead time", value: tu, unit: "ms");
            Tg = new Signal("Tg", text: "rise time", value: tg, unit: "ms");

            KS.ValueRule = "0.001..10000";
            Tu.ValueRule = "0.001..10000";
            Tg.ValueRule = "0.001..10000";


            //      OutMin = new Signal("OutMin", text: "rise time", value: tg, unit: "ms");
            //      OutMax = new Signal("OutMax", text: "rise time", value: tg, unit: "ms");

            //    SetMin = new Signal("SetMin", text: "rise time", value: tg, unit: "ms");
            //   SetMax = new Signal("SetMax", text: "rise time", value: tg, unit: "ms");

            //     SetMin = new Signal("SetMin", text: " 2", value: tg, unit: "as");
            //    SetMax = new Signal("SetMax", text: " 1", value: tg, unit: "gh");

            //    Set.Min = new Signal("SetMin", text: " 2", value: tg, unit: "as"); //SetMin;
            //    Set.Max = new Signal("SetMax", text: " 2", value: tg, unit: "as"); //SetMax;

            //   SetMin = Set.Min;
            // KS = ks;
            // Tu = tu;
            // Tg = tg;

            Parameters["T"] = T;
            Parameters["KS"] = KS;
            Parameters["Tu"] = Tu;
            Parameters["Tg"] = Tg;

            //   Parameters.ad
            //     ParameterDict.Add("Ks", new Parameter("Ks", text: "loop factor", value: 10, unit: "1"));
            /*
            Parameters["T"] = new Signal("T", text: "period", value: t, unit: "ms");
            Parameters["KS"] = new Signal("KS", text: "loop factor", value: ks, unit: "1");
            Parameters["Tu"] = new Signal("Tu", text: "dead time", value: tu, unit: "ms");
            Parameters["Tg"] = new Signal("Tg", text: "rise time", value: tg, unit: "ms");
            
            Ksp = new Signal("T", text: "period", value: t, unit: "ms");
            Test = new Signal("T", text: "period", value: t, unit: "ms");
            */

            Run();
        }



        /// <summary>
        /// Timer Intervall [in milliseconds]
        /// </summary>
        /// r
        public double T
        {
            get
            {
                return _timer.Interval;
            }
            set
            {
                // if (_timer != null) 
                _timer.Interval = (int)value;
            }
        }

        /*
        [Browsable(true), CategoryAttribute("Settings"), DescriptionAttribute("Use KSP to....")]
        public Signal Ksp = null;
        
        [Browsable(true), CategoryAttribute("Settings"), DescriptionAttribute("Use Test to...."),
            Editor("Slider:0..500:5", "")
            ]
        public Signal Test = null;
        */



        [Browsable(true), CategoryAttribute("PID-Settings"), DescriptionAttribute("Use Test to....")
        ]
        public Signal KS = null;

        [Browsable(true), CategoryAttribute("PID-Settings"), DescriptionAttribute("Use Test to....")
        ]
        public Signal Tu = null;

        [Browsable(true), CategoryAttribute("PID-Settings"), DescriptionAttribute("Use Test to....")
        ]
        public Signal Tg = null;

        /*
        [Browsable(true), CategoryAttribute("OUT-Settings"), DescriptionAttribute("Use Test to....")
       ]
        public Signal OutMin = null;

        [Browsable(true), CategoryAttribute("OUT-Settings"), DescriptionAttribute("Use Test to....")
       ]
        public Signal OutMax = null;
        */
        //   [Browsable(true), CategoryAttribute("SET-Settings"), DescriptionAttribute("Use Test to....")
        // ]
        //  public Signal SetMin = null;

        //  [Browsable(true), CategoryAttribute("SET-Settings"), DescriptionAttribute("Use Test to....")
        //  ]
        //   public Signal SetMax = null;




        /// <summary>
        /// transfer coefficient [in dx/dy (eg. °C/%)]
        /// </summary>
        // [Browsable(true), CategoryAttribute("Params"), DescriptionAttribute("Use KS to....")]
        public double dKS { get; set; } = 10;
        //{
        //    //get
        //    //{
        //    //    return Parameters["KS"].ToDouble();
        //    //}
        //}
        /// <summary>
        /// delay time [in milliseconds]
        /// </summary>
       // [Browsable(true), CategoryAttribute("Params"), DescriptionAttribute("Use Tu to....")]
        public double dTu { get; set; } = 10000;
        //{
        //    //get
        //    //{
        //    //    return Parameters["Tu"].ToDouble();
        //    //}
        //}
        /// <summary>
        /// compensation time [in milliseconds]
        /// </summary>
        //[Browsable(true), CategoryAttribute("Params"), DescriptionAttribute("Use Tg to....")]
        public double dTg { get; set; } = 600000;
        //{
        //    //get
        //    //{
        //    //    return Parameters["Tg"].ToDouble();
        //    //}
        //}


        double read_1 = double.NaN;
        double read_2 = double.NaN;

        private void SetOut(double value)
        {
            value = Math.Max(value, 0);
            value = Math.Min(value, 100);
            Out.Value = value;
        }

        public override void Run()
        {
            base.Run();
            _timer.OnElapsed -= _Elapsed;
            _timer.OnElapsed += _Elapsed;
        }

        public override void Destroy()
        {
            _timer.OnElapsed -= _Elapsed;
            base.Destroy();

        }

        public void _Elapsed(Timer t, TimerEventArgs ea)
        {
            //   base.Elapsed(s);

            bool valid = true;

            // if (Read == null)
            //   valid = false;
            if (Set == null)
                valid = false;
            if (Out == null)
                valid = false;

            //     if (double.IsNaN(Read?.Value ?? double.NaN))
            //       valid = false;
            if (double.IsNaN(Set?.Value ?? double.NaN))
                valid = false;
            if (double.IsNaN(Out?.Value ?? double.NaN))
                Out.Value = 0;

            if (double.IsNaN(T) || (T == 0))
                valid = false;
            if (double.IsNaN(KS.Value) || (KS.Value == 0))
                valid = false;
            if (double.IsNaN(Tu.Value) || (Tu.Value == 0))
                valid = false;
            if (double.IsNaN(Tg.Value) || (Tg.Value == 0))
                valid = false;

            if (!valid)
            {
                _timer.Interval = 1000;
                SetOut(0);
            }
            else
            {
                _timer.Interval = (int)(T);
                if (double.IsNaN(read_1))
                    read_1 = Value;
                if (double.IsNaN(read_2))
                    read_2 = Value;


                double _t = T / 1000; // ms -> s
                double _tu = Tu.Value / 1000; // ms -> s
                double _tg = Tg.Value / 1000; // ms -> s

                double kr = 0.95 * _tg / (KS.Value * _tu);
                double tn = 2.4 * _tu;
                double tv = 0.42 * _tu;
                double b0 = kr * (1.0 + (_t / (2.0 * tn)) + (tv / _t));
                double b1 = -kr * (1.0 - (_t / (2.0 * tn)) + (2 * (tv / _t)));
                double b2 = kr * (tv / _t);

                double o = Out.Value;
                o += (Set.Value - Value) * b0;
                o += (Set.Value - read_1) * b1;
                o += (Set.Value - read_2) * b2;
                read_2 = read_1;
                read_1 = Value;
                SetOut(o);
            }
        }
    }
}