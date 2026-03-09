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
            Destroyers.Add(destroyer);
        }

        public static void DestroyAll()
        {
            foreach (System.Action dest in Destroyers)
            {
                try
                {
                    dest();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing destroyer: {ex.Message}");
                }
            }
            Destroyers.Clear();
        }
    }
}
