using QB.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace qbook
{
    [
         XmlInclude(typeof(oControl)),
    ]
    [Serializable]
    public class oItem
    {
        internal static bool _IsDeserializing = false;

        public oItem()
        {
        }
        public oItem(string name, string text)
        {
            Name = name;
            Text = text;

            Selected = false;

        }

        [XmlIgnore]
        public string Marker = "";


        public virtual void Reset()
        {

        }

        public virtual QB.Controls.Control GetControlUnderCursor(PointF point, bool clickableOnly = false)
        {
            return null;
        }
        public virtual void Drag(PointF point)
        {

        }

        [XmlIgnore]
        public oItem Parent = null;

        public string Name = "";

        public string FullName
        {
            get
            {
                string name = Name;
                var parent = this.Parent;
                while (parent != null)
                {
                    while (parent != null && parent is oLayer)
                        parent = parent.Parent;
                    if (parent == qbook.Core.ThisBook.Main)
                    {
                        //name = "#" + "." + name;
                    }
                    else
                    {
                        if (name == "")
                            name = parent.Name;
                        else
                            name = parent.Name + "." + name;
                    }
                    parent = parent.Parent;
                }
                return name;
            }
        }

        string _Settings = "";
        //[XmlIgnore]
        public string Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                bool changed = _Settings != value;
                _Settings = value;
                if (changed && !_IsDeserializing)
                {
                    //var errors = new List<ObjectSettings.ErrorInfo>();
                    //ObjectSettings.Parse(value, out errors, this);
                    ReparseSettings();
                }
            }
        }

        public void ReparseSettings()
        {
            //SCAN - TODO
            //var errors = new List<ObjectSettings.ErrorInfo>();
            //ObjectSettings.Parse(Settings, out errors, this);
        }


        //[XmlElement(ElementName = "Settings")]
        //public string SettingsAsXml
        //{
        //    get
        //    {
        //        return this.Settings.Replace(">", "&gt;").Replace("<", "&lt;");
        //    }
        //    set
        //    {
        //        this.Settings = value.Replace("&gt;", ">").Replace("&lt;", "<");
        //    }
        //}

        [XmlIgnore]
      
      
        internal string CsCodeSourceFileKey = null;

        string _CsCode = //@"using @__default_usings__;
            qbook.Core.DefaultUsings + @"

public class @class_%name% {
    //common fields/properties/methods/classes/types go here

    public void Initialize() {
        //initialization code goes here
    }

    public void Run() {
        //run/work code goes here
    }

    public void Destroy() {
        //destroy/cleanup code goes here
    }
}
";
        public string CsCode //the item's (page's) main code
        {
            get
            {
                if (_CsCode!=null && _CsCode.Contains("@class_%name%"))
                {
                    _CsCode = _CsCode.Replace("@class_%name%", "@class_" + this.Name);
                }
                return _CsCode;
            }
            set
            {
                _CsCode = value;
            }
        }

        public SerializableDictionary<string, string> CsCodeExtra
            = new SerializableDictionary<string, string>(); //Id, additional CsCode per page


        string _PyCode = null;
        public string PyCode
        {
            get
            {
                return _PyCode;
            }
            set
            {
                _PyCode = value;
            }
        }

        public SerializableDictionary<string, string> PyCodeExtra
            = new SerializableDictionary<string, string>(); //Id, additional CsCode per page


        public string CsCodeHeader { get; set; } = null;
        //= @"using System;
        //using System.Collections.Generic;
        //using System.Linq;
        //using QB;
        //using Main.Qb.Controls;
        //using System.Drawing;

        //public class @class_main
        //{
        //";
        public string CsCodeFooter { get; set; } = null;
        //= @"
        //}
        //";


        public string PyCodeHeader { get; set; } = null;
        //= @"
        //";

        public string PyCodeFooter { get; set; } = null;
        //= @"
        //";


        //[XmlIgnore]
        //public Helpers.ObjectSettings ObjectSettings = new Helpers.ObjectSettings();


        public bool SettingsIs(string name, string value)
        {
            return SettingsRead(name, null, 0, "").Trim().ToUpper() == value.Trim().ToUpper();
        }
        public string SettingsRead(string name, string valueListSeparator, int valueIndex, string def)
        {
            foreach (string setting in Settings.Split('\n'))
            {
                string s = setting.Replace("\r", "");//.Replace(" ", "");
                if (s.Split('=').Length == 2)
                {
                    string k = s.Split('=')[0];

                    if (k.Trim().ToUpper() == name.Trim().ToUpper())
                    {
                        string v = s.Split('=')[1];

                        if ((valueListSeparator != null) && (valueListSeparator.Length > 0))
                        {
                            if (v.Replace(valueListSeparator, "|").Split('|').Length > valueIndex)
                            {
                                return v.Replace(valueListSeparator, "|").Split('|')[valueIndex];
                            }
                        }
                        return v;
                    }
                }
            }
            return def;
        }


        public double SettingsReadValue(string name, string valueListSeparator, int valueIndex, double def)
        {
            try
            {
                foreach (string setting in Settings.Split('\n'))
                {
                    string s = setting.Replace("\r", "").Replace(" ", "");
                    if (s.Split('=').Length == 2)
                    {
                        string k = s.Split('=')[0];

                        if (k.Trim().ToUpper() == name.Trim().ToUpper())
                        {
                            string v = s.Split('=')[1];

                            if ((valueListSeparator != null) && (valueListSeparator.Length > 0))
                            {
                                if (v.Replace(valueListSeparator, "|").Split('|').Length > valueIndex)
                                {
                                    string vs1 = v.Replace(valueListSeparator, "|").Split('|')[valueIndex].Trim();
                                    if (vs1.Contains("0x"))
                                    {
                                        return int.Parse(vs1.ToLower().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo);
                                    }
                                    return double.Parse(vs1.ToLower(), System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                            }
                            string vs2 = v.Trim();
                            if (vs2.Contains("0x"))
                            {
                                return int.Parse(vs2.ToLower().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo);
                            }

                            return double.Parse(vs2.ToLower(), System.Globalization.NumberFormatInfo.InvariantInfo);
                        }
                    }
                }
            }
            catch { }
            return def;
        }

        public string TextDE;
        public string TextES;
        public string Text = "";

        [XmlIgnore]
        public string TextL
        {
            get
            {
                if (Text == "")
                    return Name;
                if (Text == null)
                    return Name;
                if (qbook.Core.ThisBook.Language == "de")
                {
                    if ((TextDE == null) || (TextDE.Length < 1))
                    {
                        try
                        {
                            TextDE = qbook.Helpers.TranslateText(Text, "en", "de");
                            return TextDE;
                        }
                        catch
                        {
                            TextDE = null;
                        }
                        return Text;
                    }
                    return TextDE;
                }
                else if (qbook.Core.ThisBook.Language == "es")
                {
                    if ((TextES == null) || (TextES.Length < 1))
                    {
                        try
                        {
                            TextES = qbook.Helpers.TranslateText(Text, "en", "es");
                            return TextES;
                        }
                        catch
                        {
                            TextES = null;
                        }
                        return Text;
                    }
                    return TextES;
                }
                return Text;
            }
            set
            {

                if (qbook.Core.ThisBook.Language == "de")
                    TextDE = value;
                else if (qbook.Core.ThisBook.Language == "es")
                    TextES = value;
                else
                    Text = value;
            }
        }


        public List<oItem> Objects = new List<oItem>();
        public bool Selected = false;
        public virtual void Init()
        {
            string a = Name;
            Selected = false;

            //MIGRATION-old
            //lock (Objects)
            //{
            //    foreach (oItem item in Objects)
            //    {
            //        item.Settings = item.Settings.Replace("&gt;", ">").Replace("&lt;", "<");
            //        var errors = new List<ObjectSettings.ErrorInfo>();
            //        item.ObjectSettings.Parse(1, item.Settings, out errors, item); //
            //        item.ObjectSettings.Parse(2, item.Settings, out errors, item); //
            //        item.Init();
            //    }
            //}
        }

        public void Add(oItem item)
        {
            lock (Objects)
            {
                Objects.Add(item);
            }
        }

        public void Remove(oItem item)
        {
            lock (Objects)
            {
                if (Objects.Contains(item))
                {
                    Objects.Remove(item);
                    qbook.Core.ThisBook.Modified = true;
                    return;
                }
            }

            lock (Objects)
            {
                foreach (oItem _item in Objects)
                    _item.Remove(item);
            }
        }

        /*
        virtual public void Idle()
        {
            lock (Objects)
            {
                try
                {
                    // return;
                    foreach (oItem _item in Objects)
                    {
                        _item.Idle();
                    }
                }
                catch { }
            }
        }*/

        public oItem GetParent(oItem item)
        {
            if (Objects.Contains(item))
                return this;
            lock (Objects)
            {
                foreach (oItem _item in Objects)
                {
                    oItem item2 = _item.GetParent(item);
                    if (item2 != null)
                        return item2;
                }
            }
            return null;
        }

        public oItem GetItemByName(string name)
        {
            if (Name.Trim().ToUpper() == name.Trim().ToUpper())
                return this;
            lock (Objects)
            {
                foreach (oItem _item in Objects)
                {
                    oItem item2 = _item.GetItemByName(name);
                    if (item2 != null)
                        return item2;
                }
            }
            return null;
        }
        public void Update()
        {
            lock (Objects)
            {
                foreach (oItem item in Objects.Where(item => (item is oLayer)))
                {
                    if (item.Text != "")
                    {
                        item.Text = "";
                        item.TextDE = null;
                        item.TextES = null;
                    }
                }
            }
            lock (Objects)
            {

                foreach (oItem item in Objects)
                    item.Update();
            }
        }

        public void UnSelectTag()
        {
            lock (Objects)
            {
                foreach (oItem tag in Objects.Where(item => item is oTag))
                    tag.Selected = false;
                foreach (oItem _item in Objects)
                    _item.UnSelectTag();
            }
        }



        public string[] SplitText(string text, Font font, double width)
        {
            List<string> lines = new List<string>();
            string l1 = "";
            bool startswitchnum = false;
            if (text.StartsWith("-"))
                startswitchnum = true;
            foreach (string t in text.Split())
            {
                SizeF s = Draw.g.MeasureString(" " + t, font);
                SizeF asw = Draw.g.MeasureString(l1, font);
                if (asw.Width + s.Width < width)
                    l1 += t + " ";
                else
                {
                    lines.Add(l1.TrimEnd());
                    l1 = (startswitchnum ? "  " : "") + t + " ";
                }
            }
            lines.Add(l1);
            return lines.ToArray();
        }
        public override string ToString()
        {
            return Name + "/" + Text + " #" + Objects.Count;
        }
        public string Serialize()
        {
            try
            {
                var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(oItem));
                StringBuilder stringBuilder = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='Item.xsl'");
                using (xmlWriter)
                {
                    xmlSerializer.Serialize(xmlWriter, this);
                    xmlWriter.Close();
                }
                return stringBuilder.ToString();
            }
            catch
            {
            }
            return "";
        }
        public static oItem Deserialize(string data)
        {
            oItem item = new oItem();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(oItem));
            using (TextReader textReader = new StringReader(data))
            {
                item = (oItem)xmlSerializer.Deserialize(textReader);
                if (item == null) item = new oItem();
                return item;
            }
        }


        ////HALE
        //public void DoAction(string action)
        //{
        //    if (action == null)
        //        return;

        //    try
        //    {
        //        //HALE: for now: "script:*" is default
        //        if (action.Trim().ToLower().StartsWith("script:"))
        //            action = action.Substring("script:".Length);

        //        string code = action.Trim();//SCAN.Substring(7);
        //        try
        //        {
        //            Main.Qb.ScriptingEngine.RunScript(code);
        //        }
        //        catch (Exception ex)
        //        {
        //            string info = "";
        //            if (ex is Mages.Core.ParseException)
        //            {
        //                var parseEx = ex as Mages.Core.ParseException;
        //                info += "---\r\n";
        //                info += "Code: " + code + "\r\n";
        //                info += "---\r\n";
        //                info += "Error: " + parseEx.Error.Code + "\r\n";
        //                info += $"at({parseEx.Error.Start.Row},{parseEx.Error.Start.Column})\r\n";
        //                ScriptingErrorForm.Show(parseEx.Error.Code.ToString(), code, parseEx.Error.Start.Index, parseEx.Error.End.Index);
        //            }
        //            else
        //            {
        //                info = ex.Message;
        //            }
        //            //MessageBox.Show("#EX in RunScript():\r\n" + info, "ERROR");

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.Forms.MessageBox.Show("#EX: " + ex.Message, "ERROR");
        //    }
        //}


    }
}
