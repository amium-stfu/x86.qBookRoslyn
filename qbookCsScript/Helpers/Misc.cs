using System;
using System.Collections.Generic;
using System.Linq;

namespace QB
{
    public class Misc
    {
        static Misc()
        {
            AppStopwatch = new System.Diagnostics.Stopwatch();
            AppStopwatch.Start();
        }

        internal static System.Diagnostics.Stopwatch AppStopwatch = null;



        public static Int32 ParseColorInt(string color)
        {
            if (color == null)
                return 0x000000;

            if (color.StartsWith("#"))
            {
                if (color.Length > 1)
                    return Int32.Parse(color.Substring(1), System.Globalization.NumberStyles.HexNumber);
                else
                    return 0x000000;
            }
            else if ((color.StartsWith("0x") || color.StartsWith("0X")) && ((color.Length == 8) || (color.Length == 10)))
                return Int32.Parse(color.Substring(2), System.Globalization.NumberStyles.HexNumber);
            else
            {
                //return System.Drawing.Color.FromName(color).ToInt32();

                //TODO: this must be easier?!? problem: color names are case sensitive
                var allColours = Enum.GetNames(typeof(System.Drawing.KnownColor));
                var nameOfColour = allColours.FirstOrDefault(c => String.Compare(c, color, StringComparison.OrdinalIgnoreCase) == 0);
                if (nameOfColour != null)
                {
                    var col = System.Drawing.Color.FromName(nameOfColour);
                    return col.ToArgb();
                }
                else
                    return 0x000000;
            }
        }

        public static System.Drawing.Color ParseColor(string color)
        {
            if (color!=null && color.Length == 9)
                return System.Drawing.Color.FromArgb((int)(ParseColorInt(color)));
            else
                return System.Drawing.Color.FromArgb((int)(ParseColorInt(color) | 0xFF000000));
        }

        public static int RemoveWidgetsByName(string prefix)
        {
            int count = 0;
            int errCount = 0;
            lock (QB.Root.ControlDict)
            {
                try
                {
                    foreach (var o in QB.Root.ControlDict.Where(w => w.Value.Name.StartsWith(prefix)).ToList())
                    {
                        QB.Root.ControlDict.Remove(o.Key);
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    errCount++;
                }
            }

            return count;
        }

        public static int RemoveObjectsByName(string prefix)
        {
            int count = 0;
            int errCount = 0;
            lock (QB.Root.ObjectDict)
            {
                try
                {
                    foreach (var o in QB.Root.ObjectDict.Where(w => w.Value.Name.StartsWith(prefix)).ToList())
                    {
                        QB.Root.ObjectDict.Remove(o.Key);
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    errCount++;
                }
            }

            return count;
        }

        public static void RunThrottled(int delay, System.Action task)
        {
            ThrottledAction throttledAction = null;
            if (!DelayTaskDict.ContainsKey(task))
            {
                //Console.WriteLine("new");
                throttledAction = new ThrottledAction();
                DelayTaskDict.Add(task, throttledAction);
            }
            else
            {
                //Console.WriteLine("reuse");
                throttledAction = DelayTaskDict[task];
            }
            throttledAction.Throttle(delay, task);
        }

        static Dictionary<System.Action, ThrottledAction> DelayTaskDict = new Dictionary<System.Action, ThrottledAction>();

    }

    class ThrottledAction
    {
        System.Timers.Timer timer = null;

        System.Action myAction = null;
        public void Throttle(double delay, System.Action action)
        {
            myAction = action;
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += timer_Elapsed;
            }
            //timer.Stop();
            if (timer.Interval != delay)
                timer.Interval = delay;
            timer.Start();
        }

        void timer_Elapsed(System.Object source, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            myAction.DynamicInvoke();
        }
    }

    public class LimitedSizeDictionary<TKey, TValue>
    {
        private readonly int maxSize;
        private readonly Queue<TKey> keyQueue;
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public LimitedSizeDictionary(int maxSize)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("maxSize must be greater than zero.");
            }
            this.maxSize = maxSize;
            keyQueue = new Queue<TKey>(maxSize);
        }

        public void Add(TKey key, TValue value)
        {
            lock (dictionary)
            {
                if (dictionary.Count >= maxSize)
                {
                    TKey oldestKey = keyQueue.Dequeue();
                    dictionary.Remove(oldestKey);
                }

                keyQueue.Enqueue(key);
                dictionary[key] = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (dictionary)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (dictionary.TryGetValue(key, out TValue value))
                {
                    return value;
                }
                else
                    return default(TValue);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (dictionary)
            {
                return dictionary.ContainsKey(key);
            }
        }

    }
}
