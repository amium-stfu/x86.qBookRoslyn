
using QB.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QB.Automation
{
    public class _Ui
    {
        public Panel ControlPanel = null;
        public Panel DataPanel = null;
        public Chart Chart = null;

        public void DefaultPanels(double dataviewHeight = 90)
        {
            ControlPanel = new Panel("ControlPanel", 10, 140, 90, 30);
            DataPanel = new Panel("DataPanel", 10, 10, 90, dataviewHeight);
            Chart = new Chart("Chart", 110, 10, 160, 160);
        }
    }

    public enum State { Reset, Resetted, Run, Runnning, Pause, Paused, Finish, Finished, Destroy, Destroyed };

    public class Machine : Item
    {
        protected Timer Timer;

        public delegate void FunctionDelegate(Machine m);

        public Machine(string name, FunctionDelegate function = null) : base(name)
        {
            Timer = new Timer(name + ".Timer", 100);
            State = State.Resetted;
            FunctionStart = DateTime.Now;
            resetFunction = function;
            this.function = function;
            Timer.Ticks.Value = 0;
        }

        //   public int Counter = 0;

        private FunctionDelegate function;
        public _Ui Ui = new _Ui();


        public DateTime FunctionStart = DateTime.Now;
        public State State;
        public string Status = "";

        private FunctionDelegate resetFunction;

        public FunctionDelegate Function//(FunctionDelegate function)
        {
            set
            {
                FunctionStart = DateTime.Now;
                this.function = value;
                Timer.Ticks.Value = 0;
            }
        }

        public void _Elapsed(Timer t, TimerEventArgs ea)
        {
            CallFunction();
        }


        void CallFunction()
        {
            //lock (this) //@SCAN removed 2023-07
            {
                if (function != null)
                {
                    function(this);
                }
            }
        }

        public void Reset()
        {
            lock (this)
            {
                Timer.Destroy();
                function = resetFunction;
                State = State.Reset;
                CallFunction();
                State = State.Resetted;
            }
        }

        public virtual void Run()
        {
            Timer.OnElapsed -= _Elapsed;
            Timer.OnElapsed += _Elapsed;
            lock (this)
            {
                State = State.Run;
                CallFunction();
                State = State.Runnning;
            }
            Timer.Run();
        }

        public void Pause()
        {
            lock (this)
            {
                State = State.Pause;
                CallFunction();
                State = State.Paused;
            }
        }
        public virtual void Destroy()
        {
            base.Destroy();
            lock (this)
            {
                State = State.Destroy;
                CallFunction();
                State = State.Destroyed;
            }
            Timer.Destroy();
           
        }

        public void Finish()
        {
            lock (this)
            {
                State = State.Finish;
                CallFunction();
                State = State.Finished;
            }
        }


        public class CSignal
        {
            Dictionary<string, Signal> Dict = new Dictionary<string, Signal>();

            public List<Signal> Values
            {
                get
                {
                    lock (Dict)
                    {
                        return Dict.Values.ToList();
                    }
                }
            }
            public Signal this[string name]
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
                            //return null;
                            var newItem = new Module(name);
                            if (name.StartsWith("m")) 
                            {
                               // string hexPart = name.Substring(1); // Entfernt das 'm'
                                newItem.NetId = Convert.ToInt32(name.Substring(1), 16);
                            }
                           
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
                            //    var newItem = new Module(key);
                            Dict.Add(name, value);
                        }
                    }
                }
            }
        }

        public CSignal Signals = new CSignal();

        // public ConcurrentDictionary<string, Signal> Signals = new ConcurrentDictionary<string, Signal>();
        // public ConcurrentDictionary<string, Control> Controls = new ConcurrentDictionary<string, Control>();
        public ConcurrentDictionary<string, object> Tags = new ConcurrentDictionary<string, object>();
    }



    public class Step : Item
    {
        public delegate void SteptDelegate(Step sender);
        public SteptDelegate Function;

        public State StepState = State.Resetted;
        public int Duration = 1000;
        public string Settings = "";

        public Step(string text, int duration, SteptDelegate function, string settings) : base(text)
        {
            Duration = duration;
            Function = function;
            Settings = settings;
        }

    }

    public class Sequencer : Timer
    {
        public ConcurrentQueue<Step> Steps = new ConcurrentQueue<Step>();

        public void Init()
        {
            while (Steps.Count > 0)
                Steps.TryDequeue(out Step step);
        }
        /*
        public void Add(Step step)
        {
            Steps.Enqueue(step);
        }
        */
        public void Add(string name, int duration = 1 * 1000, Step.SteptDelegate function = null, string config = null)
        {
            Steps.Enqueue(new Step(name, duration, function, config));
        }

        /*
        public void Add(Step.SteptDelegate function, string config = null)
        {
            Steps.Enqueue(new Step("", -1, function, config));
        }
        */
        public Sequencer(string name) : base(name, 100)
        {
        }

        public override void Run()
        {
            base.Run();
            this.OnElapsed -= _Elapsed;
            this.OnElapsed += _Elapsed;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        Step actualStep = null;
        DateTime actualStepStart = DateTime.Now;

        public void _Elapsed(Timer t, TimerEventArgs ea)
        {
            if (actualStep != null)
            {
                if (actualStep.Duration < 0)
                {
                    if (actualStep.StepState != State.Finished)
                        return;
                }
                else
                {
                    if (DateTime.Now < (actualStepStart.AddMilliseconds(actualStep.Duration)))
                        return;
                }

                if (actualStep.Function != null)
                {
                    actualStep.StepState = State.Destroyed;
                    actualStep.Function(actualStep);
                }
            }
            if (Steps.Count == 0)
            {
                Destroy();
                return;
            }

            Steps.TryDequeue(out actualStep);
            actualStepStart = DateTime.Now;

            if (actualStep.Function != null)
            {
                actualStep.StepState = State.Runnning;
                actualStep.Function(actualStep);
            }
        }
    }
}