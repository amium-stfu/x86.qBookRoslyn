using QB.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace QB
{
    public class Item
    {
        public string Directory { get; set; }
        public string Name;
        public string Description;
        public string Text;
        public string Id = null;

        public string CreatedBy = null;

        public static dynamic MyQbook = null;

        public List<string> LogItems = new List<string>();
        public int LogItemCount = 10;
        public void Log(string message)
        {
            lock (LogItems)
            {
                LogItems.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + Name + " " + message);
                while (LogItems.Count > LogItemCount)
                {
                    LogItems.RemoveAt(0);
                }
            }
        }

        //   [XmlIgnore]
        //   public object Tag = null;

        public static string GetCallingClassPath(ref int skipFrames)
        {
            Regex cssRootPageRegex = new Regex(@"css_root\+class_[_0-9a-zA-Z]+$");
            string fullName = null;
            Type declaringType = null;
            //int skipFrames = 4;
            int version = 0;

            //2025-10-23 STFU
            if (version == 0)
            {
                StackTrace stackTrace = new StackTrace();
                int maxSkipFrames = Math.Min(stackTrace.FrameCount, 25);
                StackFrame frame;
                string assemblyName;
                do
                {
                    frame = stackTrace.GetFrame(skipFrames);
                    assemblyName = frame?.GetMethod()?.Module?.Assembly?.FullName;
                    skipFrames++;
                } while ((skipFrames < maxSkipFrames) && (!assemblyName.StartsWith("ℛ*")));
                declaringType = frame?.GetMethod()?.DeclaringType;
                fullName = declaringType?.FullName ?? declaringType?.DeclaringType?.FullName;


                string namespaceName = declaringType?.Namespace;

                if (namespaceName != null)
                    return namespaceName.Replace("Definition", "");
                version = 2;

            }




            if (version == 1)
            {
                do
                {
                    MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                    declaringType = method?.DeclaringType;
                    if (declaringType == null)
                    {
                        return method?.Name;
                    }
                    skipFrames++;
                    fullName = declaringType.FullName;
                }
                //while (!fullName.StartsWith("css_root+@class_") && skipFrames < 7); //TODO: limit to 7 for performance. needs improvement!!!
                while (!cssRootPageRegex.IsMatch(fullName) && skipFrames < /*9*/25); //TODO: limit to 9 for performance. needs improvement!!!
            }                                                                   // && declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)); 

            if (version == 2)
            {
                //skipFrames = 4;
                StackTrace stackTrace = new StackTrace();
                int maxSkipFrames = Math.Min(stackTrace.FrameCount, 25);
                StackFrame frame;
                string assemblyName;
                do
                {
                    frame = stackTrace.GetFrame(skipFrames);
                    assemblyName = frame?.GetMethod()?.Module?.Assembly?.FullName;
                    skipFrames++;
                } while ((skipFrames < maxSkipFrames) && (!assemblyName.StartsWith("ℛ*")));
                declaringType = frame?.GetMethod()?.DeclaringType;
                fullName = declaringType?.FullName ?? declaringType?.DeclaringType?.FullName;


                string namespaceName = declaringType?.Namespace;

            }

 




            //if "static string _classpath_" is defined, the use it; otherwise try to get the name from the class-name
            //string classpath = declaringType.GetField("_classpath_", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null).ToString();
            string classpath = declaringType.GetFields(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(f => f.Name == "_classpath_")?.GetValue(null).ToString();
            if (!string.IsNullOrEmpty(classpath))
                return classpath;
            else
            {
                if (fullName.StartsWith("css_root+class_"))
                    return fullName.Substring("css_root+class_".Length).Split('+')[0];
                else if (fullName.StartsWith("css_root+"))
                    return fullName.Substring("css_root+".Length);

                else
                    return null;
            }
        }

        static Regex getNewObjectAssignee = new Regex(@"\b(?<name>[_a-zA-Z\.][_0-9a-zA-Z\.]+)\s+=\s+new\s+.*");


        public static string GetDirectoryFromCaller(int skipFrames = 2)
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var maxFrames = Math.Min(stackTrace.FrameCount, 25);

            for (int i = skipFrames; i < maxFrames; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var type = frame?.GetMethod()?.DeclaringType;
                var ns = type?.Namespace;

                if (ns != null)
                {
                    var parts = ns.Split('.');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("Definition"))
                        {
                            return part.Replace("Definition", "");
                        }
                    }
                }
            }

            return null;
        }


        private void TrySetMyQbook()
        {
            int skipFrames = 1;
            Type declaringType;
            var stackTrace = new StackTrace();
            do
            {
                MethodBase method = stackTrace.GetFrame(skipFrames).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType != null && declaringType.FullName.StartsWith("qbook.Core"))
                {
                    MyQbook = stackTrace.GetFrame(skipFrames).GetMethod()?.DeclaringType?.GetField("ThisBook")?.GetValue(null);
                    QB.Root.ActiveQbook = MyQbook;
                }
                skipFrames++;
            }
            while (MyQbook == null && skipFrames < stackTrace.FrameCount);
        }

        public virtual void Destroy()
        {
            //--> override in descendants!
            //System.Windows.Forms.MessageBox.Show("Destroy");
        }

        public Item(string name, string id = null)
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::new Item({name})");

            MyQbook = null;

            //TEST
            /*
            QB.Table tbl = new Table();
            tbl.Clear();
            tbl.Add("A1,B1,C1");
            tbl["A1"].Value = () => DateTime.Now.Second;
            */


            Directory = GetDirectoryFromCaller();

            Name = name;
            if (!(this is Controls.Control))
            {
                if (id == null)
                    id = Guid.NewGuid().ToString();

                while (Root.ObjectDict.ContainsKey(id))
                    id += "_";

                this.Id = id;
                //this.CreatedBy = createdBy;

                Root.ObjectDict.Add(id, this);
            }

            //try to set .directory
            int skipFrames = 3;
            try
            {
                string callingClassName = null;
                if (this is QB.Module)
                {
                    callingClassName = ""; //TODO //HACK
                    lock (Root.ModuleDict)
                    {
                        if (!Root.ModuleDict.ContainsKey(id))
                            Root.ModuleDict.Add(id, this as QB.Module);
                    }
                }
                else if (this is QB.Signal)
                {
                    callingClassName = ""; //TODO //HACK
                    lock (Root.SignalDict)
                    {
                        if (!Root.SignalDict.ContainsKey(id))
                            Root.SignalDict.Add(id, this as QB.Signal);
                    }
                }
                else
                {
                    if (this.Directory == null)
                    {
                        callingClassName = GetCallingClassPath(ref skipFrames);
                        /*
                        if (callingClassName.StartsWith("css_root+@class_"))
                        {
                            this.directory = callingClassName.Substring("css_root+@class_".Length);
                        }
                        else if (callingClassName.StartsWith("css_root+"))
                        {
                            this.directory = callingClassName.Substring("css_root+".Length);
                        }
                        else
                        {
                            //TODO
                        }
                        */
                        if (callingClassName != null)
                        {
                            this.Directory = callingClassName;
                        }
                        else
                        {
                            this.Directory = null;
                        }
                    }
                    
                    ////HACK
                    //if (MyQbook == null)
                        TrySetMyQbook();
                }
            }
            catch
            {
                //this.directory = null;
            }

            return;

            //try to set .name
            try
            {
                var srcFile = new StackFrame(skipFrames - 2, true).GetFileName();
                var srcLineNr = new StackFrame(skipFrames - 2, true).GetFileLineNumber();
                string line = System.IO.File.ReadLines(srcFile).Skip(srcLineNr - 1).Take(1).First();
                Match m = getNewObjectAssignee.Match(line);
                if (m.Success)
                {
                    if (this is QB.Signal)
                    {
                        this.Name = m.Groups["name"].Value + ".Signal";
                    }
                    else
                    {
                        this.Name = m.Groups["name"].Value;
                    }
                    //this.text = m.Groups["name"].Value;

                }
                else
                {
                    //TODO
                }
            }
            catch
            {
                //this.name = null;
            }
        }


        public void Edit()
        {
            new ParametersDialog(this); //.Dialog() -> HALE?
        }

        //public qbObject(string key)
        //{
        //    if (!(this is qbWidget))
        //    {
        //        while (Root.ObjectDict.ContainsKey(key))
        //            key += "_";
        //        Root.ObjectDict.Add(key, this);
        //    }
        //}


        public class CTag : DynamicObject
        {
            public Dictionary<string, object> Dict { private set; get; } = new Dictionary<string, object>();
            //object Dummy;// = new Button("dummy");

            public List<object> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public List<string> Keys
            {
                get
                {
                    return Dict.Keys.ToList();
                }

            }

            public int Count
            {
                get
                {
                    return Dict.Keys.Count;
                }
            }

            public object this[string name]
            {
                get
                {
                    lock (Dict)
                    {
                        if (Dict.ContainsKey(name))
                        {
                            return Dict[name];
                        }
                        else
                        {
                            var newItem = new Module(name);
                            Dict.Add(name, newItem);
                            return newItem;
                        }
                    }
                }
                set
                {
                    lock (Dict)
                    {
                        if (Dict.ContainsKey(name))
                        {
                            Dict[name] = value;
                        }
                        else
                        {
                            Dict.Add(name, value);
                        }
                    }
                }
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var name = binder.Name;
                return Dict.TryGetValue(name, out result);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Dict[binder.Name] = value;
                return true;
            }
        }

        public dynamic Tags = new CTag();


        public class CParameter : DynamicObject
        {
            public Dictionary<string, object> Dict { private set; get; } = new Dictionary<string, object>();
            //object Dummy;// = new Button("dummy");

            public List<object> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public List<string> Keys
            {
                get
                {
                    return Dict.Keys.ToList();
                }

            }

            public int Count
            {
                get
                {
                    return Dict.Keys.Count;
                }
            }

            public object this[string name]
            {
                get
                {
                    lock (Dict)
                    {
                        if (Dict.ContainsKey(name))
                        {
                            return Dict[name];
                        }
                        else
                        {
                            var newItem = new Signal(name);
                            Dict.Add(name, newItem);
                            return newItem;
                        }
                    }
                }
                set
                {
                    lock (Dict)
                    {
                        if (Dict.ContainsKey(name))
                        {
                            Dict[name] = value;
                        }
                        else
                        {
                            Dict.Add(name, value);
                        }
                    }
                }
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var name = binder.Name;
                return Dict.TryGetValue(name, out result);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Dict[binder.Name] = value;
                return true;
            }

            public void Edit()
            {
                new ParametersDialog(this); //.Dialog() -> HALE?
            }
        }

        public dynamic Parameters = new CParameter();

        //SCANN virtual internal void Cleanup()
        //{
        //}
    }

    public class TimerEventArgs : EventArgs
    {
        public string[] Parameters = null;
    }

    public class Timer : Item
    {
        System.Timers.Timer _timer = null;
        public int Interval = 1000;



        //public delegate string OnMessageReceivedDelegate(Timer t, TimerEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
        //public OnMessageReceivedDelegate OnMessageReceived;


        public Timer(string name, int interval, string id = null) : base(name, id)
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::new Timer({name}, {interval})");
            Interval = interval;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        /// <param name="interval">Interval in milliseconds</param>
        /// <param name="repeatCount">If specified, the counter only fires *repeatCount* times</param>
        public virtual void Run()
        {
            //@SCAN return;
            //this.Interval = interval;
            // this.RepeatCount = repeatCount;
            if (_timer == null)
            {
                _timer = new System.Timers.Timer();
                _timer.Elapsed += _Elapsed;
                _timer.AutoReset = true;
            }

            if (Interval < 1)
                Destroy();
            else
            {
                if (_timer.Interval != Interval)
                    _timer.Interval = Interval;
                if (!_timer.Enabled)
                    _timer.Start();
            }
        }

        public virtual void Stop()
        {
            _timer.Stop();
        }

        public virtual void Reset()
        {
            _timer.Stop();
            _timer.Start();
        }

        public override void Destroy()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::destroy Timer({this.Name})");

            if (_timer != null)
            {
                _timer.Elapsed -= _Elapsed;
                _timer.Stop();
                //    _timer = null;
            }

            OnElapsed = null;
        }

        public int RepeatCount = -1; //auto-restart
        public double Min = 0;
        public double Max = 100;
        public double Step = 1;

        public Signal Ticks = new Signal("Ticks", value: 0, unit: "", colorName: "brown");

        static int _ElapsedCount = 0;
        void _Elapsed(object s, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _ElapsedCount++;

                if (Interval < 1)
                {
                    Destroy();
                    return;
                }

                double newValue = Ticks.Value;
                if (newValue < (Max - Step))
                    newValue = Ticks.Value + Step;
                else
                    newValue = Min;

                if (newValue < Min)
                    newValue = Min;
                if (newValue > Max)
                    newValue = Max;

                Ticks.Value = newValue;

                if (_timer.Interval != Interval)
                    _timer.Interval = Interval;

                if (RepeatCount >= 0)
                {
                    RepeatCount--;
                    if (RepeatCount == 0)
                    {
                        Destroy();
                    }
                }

                // Elapsed(this);

              
                if (OnElapsed != null)
                {
                    //try
                    //{
                    //    OnElapsed(this, new TimerEventArgs());
                    //}
                    //catch (Exception ex)
                    //{
                    //    GlobalExceptions.Handle(ex, "OnElapsed delegate");
                    //}
                    _doOnElapsed();
                }
         

            }
            catch (Exception ex)
            {
                //QB.Logger.Error($"#ex in timer.elapsed({this.Name},{this.Interval}): " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                //throw;
                GlobalExceptions.Handle(ex, $"Timer.Elapsed({Name})");
            }
}

        public delegate void OnElapsedDelegate(Timer t, TimerEventArgs ea);

        public OnElapsedDelegate OnElapsed;

        // System.Action OnElapsed;

        void _doOnElapsed()
        {

            foreach (var handler in OnElapsed.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, new TimerEventArgs());
                }
                catch (Exception ex)
                {
                    GlobalExceptions.Handle(ex, $"Timer.Elapsed({Name})");

                }
             
            }
        }

        public OnElapsedDelegate _onElapsed;

        /*
        public virtual void Elapsed(Timer s)
        {
        }*/
    }


    public class ExactTimer : Item
    {
        //System.Timers.Timer _timer = null;
        public int Interval = 1000;

        //public delegate string OnMessageReceivedDelegate(Timer t, TimerEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
        //public OnMessageReceivedDelegate OnMessageReceived;
        CancellationTokenSource TimerCancellationTokenSource = new CancellationTokenSource(); // TimeSpan.FromSeconds(300));
        CancellationToken Timer10msCancellationToken = CancellationToken.None;

        public ExactTimer(string name, int interval, string id = null) : base(name, id)
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::new AccurateTimer({name}, {interval})");
            Interval = interval;

            TimerCancellationTokenSource = new CancellationTokenSource(); // TimeSpan.FromSeconds(300));
            Timer10msCancellationToken = TimerCancellationTokenSource.Token;
            //_ = AccurateTimer.PrecisionRepeatActionOnIntervalAsync(TimerAction(), TimeSpan.FromMilliseconds(interval), Timer10msCancellationToken);
        }

        public System.Action TimerAction() => () =>
        {
            if (OnElapsed != null)
                OnElapsed(this, new TimerEventArgs());
        };

        /// <summary>
        /// Starts the timer
        /// </summary>
        /// <param name="interval">Interval in milliseconds</param>
        /// <param name="repeatCount">If specified, the counter only fires *repeatCount* times</param>
        public virtual void Run()
        {
            //this.Interval = interval;
            // this.RepeatCount = repeatCount;

            //if (_timer == null)
            //{
            //    _timer = new System.Timers.Timer();
            //    _timer.Elapsed += _Elapsed;
            //    _timer.AutoReset = true;
            //}

            //if (Interval < 1)
            //    Destroy();
            //else
            //{
            //    if (_timer.Interval != Interval)
            //        _timer.Interval = Interval;
            //    if (!_timer.Enabled)
            //        _timer.Start();
            //}

            if (Interval < 1)
                Destroy();
            else
                _ = AccurateTimer.PrecisionRepeatActionOnIntervalAsync(TimerAction(), TimeSpan.FromMilliseconds(Interval), Timer10msCancellationToken);
        }


        public override void Destroy()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::destroy AccurateTimer({this.Name})");

            TimerCancellationTokenSource?.Cancel();
            OnElapsed = null;
        }

        public int RepeatCount = -1; //auto-restart
        public double Min = 0;
        public double Max = 100;
        public double Step = 1;

        public Signal Ticks = new Signal("Ticks", value: 0, unit: "", colorName: "brown");

        static int _ElapsedCount = 0;
        void _Elapsed(object s, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _ElapsedCount++;

                if (Interval < 1)
                {
                    Destroy();
                    return;
                }

                double newValue = Ticks.Value;
                if (newValue < (Max - Step))
                    newValue = Ticks.Value + Step;
                else
                    newValue = Min;

                if (newValue < Min)
                    newValue = Min;
                if (newValue > Max)
                    newValue = Max;

                Ticks.Value = newValue;

                //TODO: ExactTimer: allow changing the interval?!
                //if (_timer.Interval != Interval)
                //    _timer.Interval = Interval;

                if (RepeatCount >= 0)
                {
                    RepeatCount--;
                    if (RepeatCount == 0)
                    {
                        Destroy();
                    }
                }

                // Elapsed(this);

                if (OnElapsed != null)
                    OnElapsed(this, new TimerEventArgs());
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"#EX in AccurateTimer.Elapsed({this.Name},{this.Interval}): " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
            }
        }

        public delegate void OnElapsedDelegate(ExactTimer t, TimerEventArgs ea);
        public OnElapsedDelegate OnElapsed;

        /*
        public virtual void Elapsed(Timer s)
        {
        }*/
    }



    // under construction
    public class ThreadObject : Item
    {
        System.Threading.Thread idleThread = null;
        public int Interval = 1000;

        public ThreadObject(string text) : base(text)
        { }

        /// <summary>
        /// Starts the timer
        /// </summary>
        /// <param name="interval">Interval in milliseconds</param>
        /// <param name="repeatCount">If specified, the counter only fires *repeatCount* times</param>
        public virtual void Resume()
        {


            if (Interval < 1)
                Stop();
            else
            {
                if (idleThread != null)
                    idleThread.Abort();

                idleThread = new System.Threading.Thread(_idle);
                idleThread.IsBackground = true;
                idleThread.Start();
            }
        }

        public virtual void Stop()
        {
            if (idleThread != null)
                idleThread.Abort();
        }


        protected virtual void _idle()
        {
            while (true)
            {
                Idle(this);
            }
        }

        public virtual void Idle(ThreadObject s)
        {

        }
    }
}
