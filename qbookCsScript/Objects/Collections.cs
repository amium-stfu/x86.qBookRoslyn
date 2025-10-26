using System;
using System.Collections.Concurrent;

namespace QB
{

    public static class Collections
    {
        //scan
        public static int RECORDERSIZE_ = 100000; //HALE = 100000;
        public static int MINISIZE = 600;
        public class RecorderItem
        {
            public DateTime Timestamp;
            public double Value;
            public override string ToString()
            {
                return Timestamp.ToString("HH:mm:ss") + " " + Value;
            }
        }
        public class Recorder
        {
            private readonly int capacity;

       
            public int Capacity => capacity;

            private readonly ConcurrentQueue<RecorderItem> items = new ConcurrentQueue<RecorderItem>();
            private readonly ConcurrentQueue<RecorderItem> itemsMini = new ConcurrentQueue<RecorderItem>();
            private readonly ConcurrentQueue<RecorderItem> itemsMiniMin = new ConcurrentQueue<RecorderItem>();
            private readonly ConcurrentQueue<RecorderItem> itemsMiniMax = new ConcurrentQueue<RecorderItem>();

            private double min = double.MaxValue;
            private double max = double.MinValue;
            private double avg = 0;
            private int avgCount = 0;

            private DateTime lastMiniDate = DateTime.Now;
            private double lastValue = 0;

            public static bool triggered = false;
            public static DateTime triggeredDate;

            public Recorder(int capacity)
            {
                this.capacity = Math.Max(1, capacity);
            }

            public void Add(double value)
            {
                var now = DateTime.Now;

                if (!triggered && value > 100)
                {
                    triggered = true;
                    triggeredDate = now.AddMilliseconds(-100);
                }

                EnqueueFixedSize(items, new RecorderItem { Timestamp = now, Value = value }, capacity);

                if (double.IsNaN(value))
                    return;

                min = Math.Min(min, value);
                max = Math.Max(max, value);

                avg += value;
                avgCount++;

                if (now < lastMiniDate.AddMilliseconds(1000))
                    return;

                while (now > lastMiniDate.AddMilliseconds(-1000))
                {
                    AddMiniPoint(lastMiniDate, lastValue);
                    lastMiniDate = lastMiniDate.AddMilliseconds(1000);
                }

                double avgValue = avgCount > 0 ? avg / avgCount : value;
                AddMiniPoint(now, avgValue);

                lastMiniDate = lastMiniDate.AddMilliseconds(1000);
                lastValue = avgValue;
                avg = 0;
                avgCount = 0;
                min = double.MaxValue;
                max = double.MinValue;
            }

            private void AddMiniPoint(DateTime timestamp, double value)
            {
                EnqueueFixedSize(itemsMini, new RecorderItem { Timestamp = timestamp, Value = value }, Collections.MINISIZE);
                EnqueueFixedSize(itemsMiniMin, new RecorderItem { Timestamp = timestamp, Value = min }, Collections.MINISIZE);
                EnqueueFixedSize(itemsMiniMax, new RecorderItem { Timestamp = timestamp, Value = max }, Collections.MINISIZE);
            }

            private void EnqueueFixedSize(ConcurrentQueue<RecorderItem> queue, RecorderItem item, int maxSize)
            {
                queue.Enqueue(item);
                while (queue.Count > maxSize)
                    queue.TryDequeue(out _); // überschüssige Daten verwerfen
            }

            // 📤 UI-Zugriff: konsistente Snapshot-Erzeugung
            public RecorderItem[] GetItemsSnapshot()
            {
                return items.ToArray(); // sicherer Snapshot für Anzeige oder Export
            }

            public RecorderItem[] GetMiniSnapshot() => itemsMini.ToArray();
            public RecorderItem[] GetMiniMinSnapshot() => itemsMiniMin.ToArray();
            public RecorderItem[] GetMiniMaxSnapshot() => itemsMiniMax.ToArray();
        }
    }
}