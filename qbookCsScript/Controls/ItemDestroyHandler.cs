using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Controls
{
    public static class ItemDestroyHandler
    {
        public static List<System.Action> Destroyers { get; set; } = new List<System.Action>();


        public static void AddDestroyer(System.Action destroyer)
        {
            lock (Destroyers) Destroyers.Add(destroyer);
        }

        internal static void RemoveDestroyer(System.Action destroyer)
        {
            lock (Destroyers) Destroyers.Remove(destroyer);
        }

        public static void DestroyAll()
        {
            var errors = new List<Exception>();
            System.Action[] callbacks;
            lock (Destroyers)
            {
                callbacks = Destroyers.ToArray();
            }
            foreach (System.Action dest in callbacks)
            {
                // Earlier callbacks can unregister owned controls from this snapshot.
                lock (Destroyers)
                    if (!Destroyers.Remove(dest)) continue;
                try
                {
                    dest();
                    if (dest.Target is Item item)
                    {
                        item.UnregisterObject();
                        lock (Root.ModuleDict)
                            foreach (var key in Root.ModuleDict.Where(pair => ReferenceEquals(pair.Value, item)).Select(pair => pair.Key).ToArray())
                                Root.ModuleDict.Remove(key);
                        lock (Root.SignalDict)
                            foreach (var key in Root.SignalDict.Where(pair => ReferenceEquals(pair.Value, item)).Select(pair => pair.Key).ToArray())
                                Root.SignalDict.Remove(key);
                        lock (Root.MessageDict)
                            foreach (var key in Root.MessageDict.Where(pair => ReferenceEquals(pair.Value, item)).Select(pair => pair.Key).ToArray())
                                Root.MessageDict.Remove(key);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing destroyer: {ex.Message}");
                    errors.Add(ex);
                }
            }
            if (errors.Count != 0)
                throw new AggregateException("Book workers could not be stopped. Reload has been cancelled.", errors);
        }
    }
}
