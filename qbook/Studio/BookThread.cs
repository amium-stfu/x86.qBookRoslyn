using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace qbook.Studio
{
    public class BookThread
    {
        public Thread Thread { get; }

        public BookThread(ThreadStart start, string? name = null)
        {
            Thread = new Thread(() =>
            {
                try
                {
                    start();
                }
                catch (ThreadInterruptedException)
                {
                    // sauber beendet
                }
                catch (Exception ex)
                {
                    QB.Logger.Warn($"BookThread exception: {ex.Message}");
                }
                finally
                {
                 //   qbook.Core.UnregisterThread(Thread);
                }
            });

            if (name != null)
                Thread.Name = name;

       //     qbook.Core.RegisterThread(Thread);
        }

        public void Start() => Thread.Start();

        public void Stop()
        {
            try
            {
                if (Thread.IsAlive)
                    Thread.Interrupt(); // optional: Soft stop
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"Stop failed: {ex.Message}");
            }
        }
    }
}
