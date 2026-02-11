using Autofac.Features.Metadata;
using log4net;
using log4net.Appender;
using log4net.Core;
using QB.Controls;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
//using log4net.Layout;
//using log4net.Repository;

namespace QB
{
    public class Format
    {
        public bool AllowEdit = true;
        public string DisplayFormat = null; //eg: "F3" | "X2" | "+0.00;-0.00;zero"
        public string EditMask = null; //regex!? eg: "[\+\-]?/d/d./d/d/d"
        public System.Drawing.ContentAlignment? Alignment { get; set; } = null; // ContentAlignment.MiddleCenter;
        public string AlignmentConfig
        {
            get
            {
                return Alignment.ToString();
            }
            set
            {
                switch (value.ToLower())
                {
                    case "1":
                    case "tl":
                    case "lt":
                    case "topleft":
                    case "lefttopleft":
                        Alignment = ContentAlignment.TopLeft;
                        break;
                    case "2":
                    case "tc":
                    case "ct":
                    case "t":
                    case "topcenter":
                    case "centertop":
                        Alignment = ContentAlignment.TopCenter;
                        break;
                    case "3":
                    case "tr":
                    case "rt":
                    case "topright":
                    case "righttop":
                        Alignment = ContentAlignment.TopRight;
                        break;
                    case "4":
                    case "ml":
                    case "lm":
                    case "l":
                    case "middleleft":
                    case "leftmiddle":
                        Alignment = ContentAlignment.MiddleLeft;
                        break;
                    case "5":
                    case "mc":
                    case "cm":
                    case "c":
                    case "middlecenter":
                    case "centermiddle":
                        Alignment = ContentAlignment.MiddleCenter;
                        break;
                    case "6":
                    case "mr":
                    case "rm":
                    case "r":
                    case "middleright":
                    case "rightmiddle":
                        Alignment = ContentAlignment.MiddleRight;
                        break;
                    case "7":
                    case "bl":
                    case "lb":
                    case "bottomleft":
                    case "leftbottom":
                        Alignment = ContentAlignment.BottomLeft;
                        break;
                    case "8":
                    case "bc":
                    case "cb":
                    case "b":
                    case "bottomcenter":
                    case "centerbottom":
                        Alignment = ContentAlignment.BottomCenter;
                        break;
                    case "9":
                    case "br":
                    case "rb":
                    case "bottomright":
                    case "rightbottom":
                        Alignment = ContentAlignment.BottomRight;
                        break;
                }
            }
        }

        public Brush _BackBrush = null; //eg: "yellow"
        public Brush BackBrush
        {
            get
            {
                return _BackBrush;
            }
            set
            {
                _BackBrush = value;
            }
        }
        Color _BackColor = Color.Empty; //eg: "yellow"
        public Color BackColor
        {
            get
            {
                if (_BackBrush is SolidBrush)
                    return (_BackBrush as SolidBrush).Color;
                else if (_ForeBrush is TextureBrush)
                    return Color.Empty; // (_BackColorBrush as TextureBrush).Image? 
                else if (_ForeBrush is HatchBrush)
                    return (_BackBrush as HatchBrush).ForegroundColor;
                else if (_ForeBrush is LinearGradientBrush)
                    return (_BackBrush as LinearGradientBrush).LinearColors[0];
                else if (_ForeBrush is PathGradientBrush)
                    return (_BackBrush as PathGradientBrush).CenterColor;
                else return Color.Empty;
            }
            set
            {
                _BackBrush = new SolidBrush(value);
            }
        }
        string _BackColorName = null;
        public string BackColorName
        {
            get
            {
                if (_BackBrush is SolidBrush)
                    return (_BackBrush as SolidBrush).Color.Name;
                else if (_ForeBrush is TextureBrush)
                    return "TextureBrush#";
                else if (_ForeBrush is HatchBrush)
                    return $"HatchBrush:{(_BackBrush as HatchBrush).HatchStyle}:{(_BackBrush as HatchBrush).ForegroundColor.Name}-{(_BackBrush as HatchBrush).BackgroundColor}";
                else if (_ForeBrush is LinearGradientBrush)
                    return $"LinearGradientBrush:{(_BackBrush as LinearGradientBrush).LinearColors[0]}";
                else if (_ForeBrush is PathGradientBrush)
                    return $"PathGradientBrush:{(_BackBrush as PathGradientBrush).CenterColor}";
                else return null;
            }
            set
            {
                _BackBrush = new SolidBrush(Misc.ParseColor(value));
            }
        }

        //public string ForeColor = null; //eg: "blue"


        public Brush _ForeBrush = new SolidBrush(Color.Black); //eg: "yellow"
        public Brush ForeBrush
        {
            get
            {
                return _ForeBrush;
            }
            set
            {
                _ForeBrush = value;
            }
        }
        Color _ForeColor = Color.Empty; //eg: "yellow"
        public Color ForeColor
        {
            get
            {
                if (_ForeBrush is SolidBrush)
                    return (_ForeBrush as SolidBrush).Color;
                else if (_ForeBrush is TextureBrush)
                    return Color.Empty; // (_ForeColorBrush as TextureBrush).Image? 
                else if (_ForeBrush is HatchBrush)
                    return (_ForeBrush as HatchBrush).ForegroundColor;
                else if (_ForeBrush is LinearGradientBrush)
                    return (_ForeBrush as LinearGradientBrush).LinearColors[0];
                else if (_ForeBrush is PathGradientBrush)
                    return (_ForeBrush as PathGradientBrush).CenterColor;
                else return Color.Empty;
            }
            set
            {
                _ForeBrush = new SolidBrush(value);
            }
        }
        string _ForeColorName = null;
        public string ForeColorName
        {
            get
            {
                if (_ForeBrush is SolidBrush)
                    return (_ForeBrush as SolidBrush).Color.Name;
                else if (_ForeBrush is TextureBrush)
                    return "TextureBrush#";
                else if (_ForeBrush is HatchBrush)
                    return $"HatchBrush:{(_ForeBrush as HatchBrush).HatchStyle}:{(_ForeBrush as HatchBrush).ForegroundColor.Name}-{(_ForeBrush as HatchBrush).ForegroundColor}";
                else if (_ForeBrush is LinearGradientBrush)
                    return $"LinearGradientBrush:{(_ForeBrush as LinearGradientBrush).LinearColors[0]}";
                else if (_ForeBrush is PathGradientBrush)
                    return $"PathGradientBrush:{(_ForeBrush as PathGradientBrush).CenterColor}";
                else return null;
            }
            set
            {
                _ForeBrush = new SolidBrush(Misc.ParseColor(value));
            }
        }


        private static float DefaultBorderWidth = 1.0f;
        public Pen _BorderPen = new Pen(Color.Gray, DefaultBorderWidth);
        public Pen BorderPen
        {
            get
            {
                return _BorderPen;
            }
            set
            {
                _BorderPen = value;
            }
        }

        public Color _BorderColor = Color.Empty;
        public Color BorderColor
        {
            get
            {
                return _BorderPen.Color;
            }
            set
            {
                _BorderPen = new Pen(value);
            }
        }

        public string _Border = null; //eg: "red:2.5"
        public string Border
        {
            get
            {
                if (_BorderPen.Width == DefaultBorderWidth)
                    return _BorderPen.Color.Name;
                else
                    return _BorderPen.Color.Name + ":" + _BorderPen.Width.ToString("0.0");
            }
            set
            {
                var splits = value.Split(':');
                if (splits.Length > 1)
                    _BorderPen = new Pen(Misc.ParseColor(splits[0]), splits[1].ToFloat());
                else if (splits.Length > 0)
                    _BorderPen = new Pen(Misc.ParseColor(splits[0]));
            }
        }

        Font _Font = Draw.fontTextFixed;
        public Font Font
        {
            get
            {
                return _Font;
            }
            set
            {
                _Font = value;
            }
        }

        public string FontConfig
        {
            get
            {
                if (_Font.Style == FontStyle.Regular)
                    return $"{_Font.Name}:{_Font.Size}{_Font.Unit}";
                else
                {
                    string style = "";
                    if ((_Font.Style | FontStyle.Bold) > 0)
                        style += "B";
                    if ((_Font.Style | FontStyle.Italic) > 0)
                        style += "I";
                    if ((_Font.Style | FontStyle.Underline) > 0)
                        style += "U";
                    if ((_Font.Style | FontStyle.Strikeout) > 0)
                        style += "S";
                    return $"{_Font.Name}:{_Font.Size}{_Font.Unit}:{style}";
                }
            }
            set
            {
                var splits = value.Split(':');
                FontFamily family = Draw.fontTextFixed.FontFamily;
                string familyName = Draw.fontTextFixed.Name;
                FontStyle style = Draw.fontTextFixed.Style;
                float size = Draw.fontTextFixed.Size;
                GraphicsUnit unit = GraphicsUnit.Point;
                Regex sizeRegex = new Regex(@"^(?<size>[\d\.]+)(?<unit>(mm|px|pt))?$");
                Regex styleRegex = new Regex(@"^(?<style>[ibusIBUS]{1,4})$");
                foreach (var conf in splits)
                {
                    Match sizeMatch = sizeRegex.Match(conf);
                    Match styleMatch = styleRegex.Match(conf);
                    if (sizeMatch.Success)
                    {
                        size = (float)sizeMatch.Groups["size"].Value.ToDouble();
                        string unitStr = sizeMatch.Groups["unit"].Value.ToLower();
                        switch (unitStr)
                        {
                            case "pt":
                                unit = GraphicsUnit.Point;
                                break;
                            case "px":
                                unit = GraphicsUnit.Pixel;
                                break;
                            case "mm":
                                unit = GraphicsUnit.Millimeter;
                                break;
                        }
                    }

                    else if (styleMatch.Success)
                    {
                        var styleString = styleMatch.Groups["style"].Value;
                        if (styleString.Contains("B"))
                            style |= FontStyle.Bold;
                        if (styleString.Contains("I"))
                            style |= FontStyle.Italic;
                        if (styleString.Contains("U"))
                            style |= FontStyle.Underline;
                        if (styleString.Contains("S"))
                            style |= FontStyle.Strikeout;
                    }

                    else
                    {
                        familyName = conf;
                    }
                }
                _Font = new Font(familyName, size, style, unit);
            }
        }
        public double Width = double.NaN; //eg: 40 //in mm
        public double Height = double.NaN; //eg: 10 //in mm

        public override string ToString()
        {
            string s = "";
            s += ((DisplayFormat != null) ? ",DisplayFormat=" + DisplayFormat : "");
            s += ((EditMask != null) ? ",EditMask=" + EditMask : "");
            s += ((Alignment != null) ? ",Alignment=" + Alignment : "");
            s += ((ForeColor != null) ? ",ForeColor=" + ForeColor : "");
            s += ((BackColor != null) ? ",BackColor=" + BackColor : "");
            s += ((Border != null) ? ",Border=" + Border : "");
            s += ((Font != null) ? ",Font=" + Font : "");
            s += ((AllowEdit) ? ",AllowEdit=1" : "");
            s += ((!double.IsNaN(Width)) ? ",Width=" + Width : "");
            return s.TrimStart(',');
        }


        public static Format FromString(string str)
        {
            Format f = new Format();
            var splits = str.SplitOutsideSingleQuotes(); //(new [] {','}); //outside quotes!
            foreach (string split in splits)
            {
                string token = split.Trim();
                if (token.StartsWith("'") && token.EndsWith("'"))
                    token = token.Substring(1, token.Length - 2);
                var kvp = token.Split(new[] { ':' }, 2);
                if (kvp.Length == 2)
                {
                    string key = kvp[0].ToLower();
                    string value = kvp[1];
                    switch (key)
                    {
                        case "edit":
                            f.AllowEdit = value.ToBool();
                            break;
                        case "f":
                        case "format":
                            f.DisplayFormat = value;
                            break;
                        case "mask":
                        case "editmask":
                            f.EditMask = value;
                            break;
                        case "a":
                        case "align":
                        case "alignment":
                            f.AlignmentConfig = value;
                            break;
                        case "fc":
                        case "forecolor":
                        case "color":
                            f.ForeColorName = value;
                            break;
                        case "bc":
                        case "backcolor":
                            f.BackColorName = value;
                            break;
                        case "border":
                            f.Border = value;
                            break;
                        case "font":
                            f.FontConfig = value;
                            break;
                        case "w":
                        case "width":
                            f.Width = value.ToDouble();
                            break;
                        case "h":
                        case "height":
                            f.Height = value.ToDouble();
                            break;
                    }
                }
            }

            return f;
        }
    }


    public class Action
    {
        public Action()
        {

        }

        public string name = "";
        public string code = "";
        public string description = "";
        public bool done = false;
        public string duration = "1";
        public void set(string code)
        {
            this.code = code;
        }
    }

    //public class String_
    //{
    //    public String_()
    //    {

    //    }

    //    public string value = "";
    //    public void set(string value)
    //    {
    //        this.value = value;
    //    }
    //}

    public class Rectangle
    {
        public Rectangle()
        {

        }

        public Rectangle(double x, double y, double w, double h)
        {
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;
        }

        private double _X = 0;
        public double X
        {
            get => _X;
            set
            {
                double delta = value - _X;
                _X = value;

                if (OwnerControl != null)
                {
                    foreach (var child in OwnerControl.Children)
                    {
                        child.Bounds.X += delta; 
                    }
                }
            }
        }
        
        private double _Y = 0;
        public double Y
        {
            get => _Y;
            set
            {
                double delta = value - _Y;
                _Y = value;

                if (OwnerControl != null)
                {
                    foreach (var child in OwnerControl.Children)
                    {
                        child.Bounds.Y += delta;
                    }
                }
            }
        }
        
        private double _W = 10;
        public double W
        {
            get => _W;
            set
            { _W = value; }
        }
        
        private double _H = 10;
        public double H
        {
            get => _H;
            set
            { _H = value; }
        }


        public double Size
        {
            get
            {
                return W * H;
            }
        }


        public void Set(double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, string unit = null)
        {

            if (!double.IsNaN(x))
                X = x + 10;
            if (!double.IsNaN(y))
                Y = y + 20;
            if (!double.IsNaN(w))
                W = w;
            if (!double.IsNaN(h))
                H = h;
        }

        public bool Contains(System.Drawing.PointF p)
        {
            return Contains(p.X, p.Y);
        }
        public bool Contains(double x, double y)
        {
            return ((x >= this.X) && (x <= (this.X + this.W)) && (y >= this.Y) && (y <= (this.Y + this.H)));
        }

        internal Control OwnerControl = null;
    }




    public class fformat
    {
        public string f { get; set; }

    }

    public class Logger
    {
        //private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ILog log = null;
        static bool logInvalid = false;
        //internal static void addLog(string text)
        //{
        //    addLog('L', text);
        //}


        internal static void SetILog(ILog log1)
        {
            log = log1;
            log.Info("qbLogger setup: OK");
        }

        static Logger()
        {

        }

        public static void InitalizeLogger()
        {
            // Configure log4net programmatically
            log4net.Repository.ILoggerRepository repository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            // Create a RollingFileAppender
            var rollingFileAppender = new RollingFileAppender(); //.MinimalLock();
            rollingFileAppender.File = "myLogFile.log";
            rollingFileAppender.AppendToFile = true;

            // Create a PatternLayout for the RollingFileAppender
            var patternLayout = new log4net.Layout.PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();
            rollingFileAppender.Layout = patternLayout;

            // Set rolling properties for the RollingFileAppender
            rollingFileAppender.MaxSizeRollBackups = 5;
            rollingFileAppender.MaximumFileSize = "10MB";
            rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            rollingFileAppender.StaticLogFileName = true;

            // Activate options for the RollingFileAppender
            rollingFileAppender.ActivateOptions();

            // Create a UdpAppender
            var udpAppender = new UdpAppender();
            udpAppender.RemoteAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            udpAppender.RemotePort = 39999;

            // Create a PatternLayout for the UdpAppender
            var udpPatternLayout = new log4net.Layout.PatternLayout();
            udpPatternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            udpPatternLayout.ActivateOptions();
            udpAppender.Layout = udpPatternLayout;

            // Activate options for the UdpAppender
            udpAppender.ActivateOptions();

            // Create a logger and attach the appenders
            //var logger = (log4net.Repository.Hierarchy.Logger)repository.GetLogger("qbLogger");
            //logger.AddAppender(rollingFileAppender);
            //logger.AddAppender(udpAppender);
            //logger.Level = Level.All;

            ILog logger = LogManager.GetLogger(repository.Name, "qbLogger");
            ((log4net.Repository.Hierarchy.Logger)logger.Logger).AddAppender(rollingFileAppender);
            ((log4net.Repository.Hierarchy.Logger)logger.Logger).AddAppender(udpAppender);
            ((log4net.Repository.Hierarchy.Logger)logger.Logger).Level = Level.All;

            // Log some messages
            logger.Info("LOGGER: This is an info message.");
            logger.Error("LOGGER: This is an error message.");
        }


        public static int LibDebugLevel = 0; //0=off, 1=few, 2=normal, 3=noisy, 4=verbose
        public static bool ShowStackTrace = false;

        public static void Log(string text, string style = null)
        {
            addLog('L', text, style);
        }
        public static void Error(string text, string style = null)
        {
            addLog('E', text, style);
        }
        public static void Warn(string text, string style = null)
        {
            addLog('W', text, style);
        }
        public static void Info(string text, string style = null)
        {
            addLog('I', text, style);
        }
        public static void Debug(string text, string style = null)
        {
            addLog('D', text, style);
        }


        internal static void addLog(char type, string text, string style = null)
        {
            //if (log == null || logInvalid)
            //{
            //    var udpAppender = CreateUdpLogAppender();
            //    log = LogManager.GetLogger("qbLog"); // System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);            
            //    logInvalid = false;
            //}

            if (log == null || logInvalid)
            {
                //var udpAppender = CreateUdpLogAppender();
                log = LogManager.GetLogger("qbLog"); // System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);            
                //logInvalid = false;
            }

            var h = log.GetHashCode();
            if (style != null)
                text = text + "{style=" + style + "}";

            try { 
            switch (type)
            {
                case 'E':
                    log.Error(text);
                    break;
                case 'W':
                    log.Warn(text);
                    break;
                case 'I':
                    log.Info(text);
                    break;
                case 'D':
                    log.Debug(text);
                    break;
                default:
                    log.Info(text);
                    break;
            }
        }
            catch
            {

            }
            finally
            {
                if (ShowStackTrace)
                {
                    var stackTrace = new System.Diagnostics.StackTrace();
                    log.Info(stackTrace.ToString());
                }
            }
        }



        public static void InvalidateLog()
        {
            logInvalid = true;
        }

        private static log4net.Appender.UdpAppender CreateUdpLogAppender()
        {
            var patternLayout = new log4net.Layout.PatternLayout
            {
                //ConversionPattern = "%date %level: %message%newline"
                ConversionPattern = "%date [%property{pid}/%3thread] %-5level %logger: %message%newline"
            };
            patternLayout.ActivateOptions();

            var newAppender = new log4net.Appender.UdpAppender
            {
                RemoteAddress = IPAddress.Parse("127.0.0.1"),
                RemotePort = 39999,
                Layout = patternLayout
            };
            newAppender.ActivateOptions();

            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.AddAppender(newAppender);
            hierarchy.Root.Level = log4net.Core.Level.All; // Default is Debug

            log4net.Config.BasicConfigurator.Configure(hierarchy);

            return newAppender;
        }

        //HALE //TEMP //TEST
        public static double add(double a, double b)
        {
            return a + b;
        }
    }



}
