using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


class AccurateTimer
{
    static Stopwatch MyStopwatch = new Stopwatch();

    static AccurateTimer()
    {
        MyStopwatch.Start();
    }

    /// <summary>
    /// A timer that will fire an action at a regular interval. The timer will aline itself.
    /// </summary>
    /// <param name="action">The action to run Asyncrinasally</param>
    /// <param name="interval">The interval to fire at.</param>
    /// <param name="ct">(optional)A CancellationToken to cancel.</param>
    /// <returns>The Task.</returns>
    public static async Task PrecisionRepeatActionOnIntervalAsync(Action action, TimeSpan interval, CancellationToken? ct = null)
    {
        long intervalMillis = (long)interval.TotalMilliseconds;
        long stage1Delay = 16;
        long stage2Delay = 8 * TimeSpan.TicksPerMillisecond;
        bool USE_SLEEP0 = false;

        long targetMillis = MyStopwatch.ElapsedMilliseconds + ((int)stage1Delay + 2);
        bool warmup = true;
        while (true)
        {
            if (ct != null && ((CancellationToken)ct).IsCancellationRequested)
                return;

            // Getting closer to 'target' - Lets do the less precise but least cpu intensive wait
            var timeLeftMillis = targetMillis - MyStopwatch.ElapsedMilliseconds;
            if (timeLeftMillis >= stage1Delay)
            {
                try
                {
                    await Task.Delay((int)(timeLeftMillis - stage1Delay), ct ?? CancellationToken.None);
                }
                catch (TaskCanceledException ex) when (ct != null)
                {
                    return;
                }
            }

            // Getting closer to 'target' - Lets do the semi-precise but mild cpu intesive wait - Task.Yield()
            while (MyStopwatch.ElapsedMilliseconds < targetMillis - stage2Delay)
            {
                await Task.Yield();
            }

            // Getting closer to 'target' - Lets do the semi-precise but mild cpu intesive wait - Thread.Sleep(0)
            // Note: Thread.Sleep(0) is removed below because it is sometimes looked down on and also said not good to mix 'Thread.Sleep(0)' with Tasks.
            //       However, Thread.Sleep(0) does have a quicker and more reliable turn around time then Task.Yield() so to 
            //       make up for this a longer (and more expensive) Thread.SpinWait(1) would be needed.
            if (USE_SLEEP0)
            {
                while (MyStopwatch.ElapsedMilliseconds < targetMillis - stage2Delay / 8)
                {
                    Thread.Sleep(0);
                }
            }

            // Extreamlly close to 'target' - Lets do the most precise but very cpu/battery intensive 
            while (MyStopwatch.ElapsedMilliseconds < targetMillis)
            {
                Thread.SpinWait(64);
            }

            if (!warmup)
            {
                await Task.Run(action); // or your code here
                targetMillis += intervalMillis;
            }
            else
            {
                //long start1 = DateTime.Now.Ticks + ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond);
                //long alignVal = start1 - (start1 % ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond));
                //target = new DateTime(alignVal);

                long start1 = MyStopwatch.ElapsedMilliseconds + intervalMillis;
                long alignVal = start1 - (start1 % (intervalMillis));
                targetMillis = alignVal;

                warmup = false;
            }
        }
    }
}