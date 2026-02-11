using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB
{

    public class TAG
    {
        public static string Name = "Name";
        public static string Text = "Text";
        public static string Unit = "Unit";
        public static string Value = "Value";
        public static string Enabled = "Enabled";
        public static string Format = "Format";
        public static string Height = "Height";
        // public static string Indent = "Indent";
        public static string Signal = "Signal";

        public static string State = "State";
        public static string Mode = "Mode";
        public static string Timer = "Timer";
        public static string Alert = "Alert";

        public static string Set = "Set";
        public static string Out = "Out";

        public static string Aid = "AID";
        public static string Offset = "Offset";
        public static string Gain = "Gain";
        public static string OH = "OH";

        public static string Ks = "Ks";
        public static string Tu = "Tu";
        public static string Tg = "Tg";

        public static string Sn = "Sn";

        public static string Upload = "Upload";
        public static string Download = "Download";
        public static string Command = "Command";
        public static string Time = "Time";
        public static string X0 = "X0";
        public static string Y0 = "Y0";
        public static string X1 = "X1";
        public static string Y1 = "Y1";

    }


    public delegate void OnModifiedDelegate(string id, object value);

    public class TagDictionary
    {
        public Module Parent = null;

        public OnModifiedDelegate OnModified;
        Dictionary<string, object> Dictionary = new Dictionary<string, object>();

        public Dictionary<string, object> Dict
        {
            get { lock (Dictionary) { return Dictionary; } }
        }

        public List<object> Values
        {
            get { lock (Dictionary) { return Dictionary.Values.ToList(); } }
        }
        public bool Has(string id)
        {
            return Dictionary.ContainsKey(id);
        }


        public object this[string id]
        {
            get
            {
                lock (Dictionary)
                {
                    if (Has(id)) { return Dictionary[id]; }
                    else { Dictionary.Add(id, null); return null; }
                }
            }
            set
            {
                bool modified = false;
                if (value == null)
                {
                    return;
                }
                lock (Dictionary)
                {

                    if (Dictionary.ContainsKey(id))
                    {
                        if (Dictionary[id].ToString() != value.ToString())
                        {
                            Dictionary[id] = value;
                            modified = true;
                        }
                    }
                    else
                    {
                        Dictionary.Add(id, value);
                        modified = true;
                    }
                }

                if (modified && (OnModified != null))
                {
                    OnModified(id, value);
                    Console.WriteLine(Parent.Name + "." + id + ":" + value.ToString());
                }
            }
        }
    }
}
