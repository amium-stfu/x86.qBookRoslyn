using CefSharp.DevTools.WebAudio;
using QB.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook
{
    public class oCode
    {
        public bool Active { get; set; } = true;
        public RoslynDocument RoslynDocument;
        public string Code;
        public string Filename;

        public oCode(string filename, bool active, RoslynDocument doc, string code)
        {
            this.Filename = filename;
            this.Active = active;
            this.RoslynDocument = doc;
            this.Code = code;
        }
    }
    
    
    
    [Serializable]
    public class oPage : oControl
    {
  

        public oPage()
        {
        }

        public int OrderIndex = 0;
        public bool Hidden { get; set; } = false; //don't show in tree

        public List<string> CodeOrder = new List<string>();

        public List<oHtml> HtmlItems = new List<oHtml>();

        //Roslyn
        [XmlIgnore]
        public RoslynDocument RoslynDoc;

        [XmlIgnore]
        public Dictionary<string,oCode> SubCodes = new Dictionary<string,oCode>();

        public string Filename { get; set; }
        //

        [XmlIgnore]
        public System.Threading.Thread IdleThread;


        //@stfu 2025-06-07
        public string Format = "A4"; //A4, "16/9", "16/10"
        public List<string> Includes { get; set; } = new List<string>();

        //@stfu 2025-10-21
        [XmlIgnore]
        public object DynInstance { get; set; } // Roslyn-Instanz z.B. class_Page1.@class_Page1

        [XmlIgnore]
        public bool DynInitialized { get; set; }

        public oPage(string name, string text) : base(name, text)
        {
           // return;
            oLayer layer = new oLayer("Main", "");
            layer.Selected = true;

            Add(layer);
        }

        public string Section = "";
        public double rItemWidth = Draw.DefaultItemWidth;
        public double lItemWidth = Draw.DefaultItemWidth;
        public string Url;

        public class Formula
        {
            public int Interval = 100; //10,100,1000
            public string Name;
            public string ParamStr;
            public string Code; //or script?!
        }

        //[XmlIgnore] //we serialize <Code>. from that we build functions, scripts, etc.
        //public List<Formula> Formulas; // = new List<Formula>();

        [XmlIgnore]
        public int FormulaCount = 0;
        public string Code { get; set; }

       

        public override void Reset()
        {
            //if (akcPage != null)
            //    akcPage.Reset();
            //if (udlPage != null)
            //    udlPage.Reset();
        }

        public void Idle()
        {
            while (true)
            {
                //if (akcPage != null)
                //    akcPage.Idle();
                //if (udlPage != null)
                //    udlPage.Idle();
                System.Threading.Thread.Sleep(1);
            }

           
        }

        public override void Init()
        {
            base.Init();

            //TODO: MIGRATION-new
            //var type = "ak"; // ObjectSettings.GetItem("source", "type", "*");

            //if (type.Trim('\"') == "ak")
            //    akcPage = new Main.Qb.AkcPage(FullName); //, ObjectSettings);
            //if (type.Trim('\"') == "udl")
            //    udlPage = new UdlPage(FullName); //, ObjectSettings);

            //if ((akcPage != null) ||
            //    (udlPage != null))
            //{
            //    if (IdleThread == null)
            //    {
            //        IdleThread = new System.Threading.Thread(Idle);
            //        IdleThread.IsBackground = true;
            //        IdleThread.Start();
            //    }
            //}

            //if (akcPage != null)
            //    akcPage.Init();
            //if (udlPage != null)
            //    udlPage.Init();
        }

        public override void Render()
        {
            Bounds = new Bounds(10, 20, Draw.Width - 17, Draw.Height - 30);
            base.Render();
        }


        static Regex fktNameRegex = new Regex(@"^fkt:(?<name>[a-zA-Z][a-zA-Z0-9]*)(\((?<params>[^\)]*)\))?(,(?<interval>\d+))?.*");
        static Regex scriptNameRegex = new Regex(@"^script:(?<name>[a-zA-Z][a-zA-Z0-9]*)(\((?<params>[^\)]*)\))?(,(?<interval>\d+))?.*");

        public static List<Formula> CodeToFunctions(string code)
        {
            var lines = code.Replace("\r", "").Split('\n').ToArray();
            int lineNr = 0;
            string fktName = null;
            string fktParams = null;
            int fktInterval = 100;
            string fktExpr = null;
            List<oPage.Formula> formulaList = new List<oPage.Formula>();
            while (lineNr < lines.Length)
            {
                if (lines[lineNr].TrimStart().StartsWith("//"))
                {
                    lineNr++;
                    continue;
                }

                Match m = fktNameRegex.Match(lines[lineNr]);
                if (m.Success)
                {
                    if (fktName != null)
                    {
                        formulaList.Add(new oPage.Formula() { Name = fktName, ParamStr = fktParams, Code = fktExpr.Trim(), Interval = fktInterval });
                    }
                    //fktName = lines[lineNr].TrimEnd(new char[] {'(', ')'});
                    fktName = m.Groups["name"].Value;
                    fktParams = m.Groups["params"].Value;
                    fktInterval = 100;
                    int.TryParse(m.Groups["interval"].Value, out fktInterval);
                    fktExpr = "";
                    lineNr++;
                }

                if (fktName != null)
                {
                    fktExpr += lines[lineNr] + "\r\n";
                }
                lineNr++;
            }
            if (fktName != null)
            {
                formulaList.Add(new oPage.Formula() { Name = fktName, ParamStr = fktParams, Code = fktExpr.Trim(), Interval = fktInterval });
            }

            return formulaList;
        }






    }
}
