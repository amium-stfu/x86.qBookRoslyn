using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace QB.Controls
{
    public class Log : Panel
    {
        public Log(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
        }

        public void Add(Item item)
        {
            Items[item.Name] = item;
        }

        public void Remove(Item item)
        {
            lock (Items.Values)
            {
                if (Items.Values.Contains(item))
                    Items.Values.Remove(item);
            }
        }
        public void Remove(string itemName) //by Name
        {
            lock (Items.Values)
            {
                var item = Items.Values.FirstOrDefault(x => x.Name == itemName);
                if (item != null)
                    Items.Values.Remove(item);
            }
        }
        public void RemoveAll()
        {
            Items.Values.Clear();
        }



        public class CItem
        {
            Dictionary<string, Item> Dict = new Dictionary<string, Item>();

            public List<Item> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public Item this[string name]
            {
                get
                {
                    if (Dict.ContainsKey(name))
                    {
                        return Dict[name];
                    }
                    else
                    {
                        //return null;
                        var newItem = new Module(name);
                        Dict.Add(name, newItem);
                        return newItem;
                    }
                }
                set
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

        public CItem Items = new CItem();

        /*
        public class CControl
        {
            Dictionary<string, Control> Dict = new Dictionary<string, Control>();


            public bool Contains(string name)
            {
                return Dict.ContainsKey(name);
            }

            public List<Control> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public Control this[string name]
            {
                get
                {
                    if (Dict.ContainsKey(name))
                    {
                        return Dict[name];
                    }
                    else
                    {
                        //return null;
                        var newItem = new Control(name);
                        Dict.Add(name, newItem);
                        return newItem;
                    }
                }
                set
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

        public CControl Control = new CControl();
        */

        internal override void Render(Control parent)
        {

            base.Render(parent);
            try
            {
                double yPos = 0;
                foreach (Item item in Items.Values.ToList())
                {
                    Pen pen = new Pen(System.Drawing.Color.Black);
                    lock (item.LogItems)
                    {
                        foreach (string le in item.LogItems)
                        {
                            Draw.Text(le, Bounds.X + 1, Bounds.Y + 0.5 + yPos, Bounds.W, Draw.fontTerminalFixed, System.Drawing.Color.Black, ContentAlignment.MiddleLeft);
                            yPos += 3.0;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
