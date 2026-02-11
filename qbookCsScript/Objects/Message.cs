using System;
using System.Drawing;
//using System.Drawing;

namespace QB
{

    public static class Messages
    {

        public static Message Get(string id)
        {
            lock (Root.MessageDict)
            {
                if (Root.MessageDict.ContainsKey(id))
                    return Root.MessageDict[id];
                else
                    return null;
            }
        }

    }



    public class Message : Item
    {
        public Message(string name, string text = null, double value = double.NaN, string unit = null, string colorName = null, System.Drawing.Color? color = null) : base(name, null)
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::new Message({name})");

            Name = name;

            if (text == null)
                Text = "#" + name;
            else
                Text = text;

            Unit = unit;
            Color = color ?? Color.Black;
            if (colorName != null)
                Color = Misc.ParseColor(colorName); ;
        }

        public string Text;
        public int addIndex = 0;

        DateTime lastValueUpdate = DateTime.Now;
        private string _value = "*";
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                lock (this)
                {
                    _value = value;
                }
            }
        }

        public fformat Format { get; set; } = new fformat();

        public string Unit { get; set; }

        public System.Drawing.Color Color { get; set; } = Color.Black;
        public override string ToString()
        {
            return $"{Value:F5}{(string.IsNullOrEmpty(Unit) ? "" : " " + Unit)}";
        }

        ~Message()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::~Message({this.Name})");
        }

        public override void Destroy()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::destroy Message({this.Name})");
        }
    }

}