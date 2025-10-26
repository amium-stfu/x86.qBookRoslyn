using System;
using System.Diagnostics;

namespace qbook
{
    public class LogEntry
    {
        public int Count; //incemental counter
        public DateTime Timestamp;
        public DateTime TimestampFirst; //in case Repeatcount > 1
        public char Type = 'L'; //L)og, E)rror, W)arn, I)nfo, D)ebug
        public string Text { get; set; }
        public string Style { get; set; } = null; //e.g. red:B, blue:I
        public int RepeatCount = 1; //same message(Text+Type+Style) multiple times
        public int Pid = 0;
        public int ThreadId = 0;

        public LogEntry()
        {
            if (Pid == 0)
                Pid = Process.GetCurrentProcess().Id; //only set once
            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public override string ToString()
        {
            //return $"{Timestamp:HH:mm:ss.fff} [{Type}]: {Text}";
            if (RepeatCount > 1)
                return $"{(Count.ToString().PadLeft(5))} [{Type}] {Timestamp.ToString("HH:mm:ss.fff")} ({RepeatCount}): {Text}";
            else
                return $"{(Count.ToString().PadLeft(5))} [{Type}] {Timestamp.ToString("HH:mm:ss.fff")}: {Text}";
        }

        public string ToStringEx()
        {
            //return $"{Timestamp:HH:mm:ss.fff} [{Type}]: {Text}";
            if (RepeatCount > 1)
                return $"{(Count.ToString().PadLeft(5))} [{Type}] {Timestamp.ToString("HH:mm:ss.fff")} [{Pid.ToString().PadLeft(5)}/{ThreadId.ToString().PadLeft(3)}]({RepeatCount}): {Text}";
            else
                return $"{(Count.ToString().PadLeft(5))} [{Type}] {Timestamp.ToString("HH:mm:ss.fff")} [{Pid.ToString().PadLeft(5)}/{ThreadId.ToString().PadLeft(3)}]: {Text}";
        }

    }

}
