//using PdfSharp.Charting;
using QB.Net;
using QB.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Controls
{
    public class ChartSeries
    {
        public QB.Controls.ChartXY.Series series = new QB.Controls.ChartXY.Series();
        public QB.Controls.ChartXY.Series temp = new QB.Controls.ChartXY.Series();
        public Signal Read;
        public List<double> time = new List<double>();
        public List<double> values = new List<double>();
        public string Name;
        public int History;
        public int LegendIndex;

        Chart chart;
        public QB.Controls.Box Frame;
        QB.Controls.Box Axis;
        QB.Controls.Box Legend;


        QB.Controls.Box Line;
        private ChartExtended parent;

        public double Range = 1000;
        public double Values = 1000;

        public double RangeMin;
        public double RangeMax;

        public double ValueMin;
        public double ValueMax;

        string Page;
        public bool AutoPlay = true;
        public int StartIndex;

        double X;
        double Y;
        public int Yaxis;

        public ChartSeries(string name, Signal Read, Chart chart, int history, int legendIndex, ChartExtended parent, double x, double y, string page, int yAxis = 1, bool useNormName = true)
        {
            this.Name = name;
            this.Read = Read;
            this.chart = chart;
            this.LegendIndex = legendIndex;
            this.parent = parent;
            this.History = history;
            Yaxis = yAxis;
            Page = page;
            X = x;
            Y = y;

            if (yAxis == 1)
            {
                chart.Add(Read, chart.AxisY1);
            }
            if (yAxis == 2)
            {
                chart.Add(Read, chart.AxisY2);
            }
            if (yAxis == 3)
            {
                chart.Add(Read, chart.AxisY3);
            }
            if (yAxis == 4)
            {
                chart.Add(Read, chart.AxisY4);
            }



            string axisText = "Y" + yAxis;

            y = Y + LegendIndex * 4;

            if (Read.Unit == null)
                Read.Unit = "";

            string label = Read.Text;
            if(useNormName)
                label = Read.Name;


            Frame = new QB.Controls.Box("legend", x: X + 60, y: y + 0.1, w: 50, h: 4, style: $"font:Calibri:8:b,bg:#FAFAFA,align:ml,bg-hover:white"
                , boxes: new[] {
                Axis = new QB.Controls.Box("axis", x:7, y:0, w:5, h:4, text:axisText, style: $"font:Calibri:8:b,bg:transparent,align:ml,bg-hover:transparent"),
                Legend = new QB.Controls.Box("text", x:12, y:0, w:30, h:4, text:label, style: $"font:Calibri:8:b,bg:transparent,align:ml,bg-hover:transparent"),
                Line = new QB.Controls.Box("line", x:0, y:1.5, w:5, h:1) {
                    BackgroundColor = Read.Color
                },
                }
            );

            Frame.BackgroundColor = Frame.BackgroundColor.ChangeTransparency(80);
            Frame.Directory = Page;
            Frame.BackgroundColorHover = Frame.BackgroundColor;
            Frame.Clickable = false;
            Legend.Clickable = false;
            Line.Clickable = false;
            Axis.Clickable = false;
            Frame.ZOrder = -1;
            Legend.Directory = Page;
            Line.Directory = Page;
            chart.Directory = Page;
        }



        public void Update(double second)
        {
            double v = Read.Value;
            int q = time.Count();

            //if (q == 0) return;


            if (double.IsNaN(v))
            {
                return;
            }
            else
            {
                if (second > History)
                {
                    if (time.Count > 0)
                    {
                        time.RemoveAt(0);
                        values.RemoveAt(0);
                    }
                }
                time.Add(second);
                values.Add(v);
            }
        }

        public void Reset()
        {
            time.Clear();
            values.Clear();
        }

    }

    public class ChartExtended
    {

        public (DateTime now, string date, string time) startTime;
        public (DateTime now, string date, string time) endTime;
        public (DateTime now, DateTime startTime, TimeSpan duration, double read) timer;

        public int History;
        public int Buffer;


        private Dictionary<string, QB.Drawing.Axis> Axes = new Dictionary<string, QB.Drawing.Axis>();
        private QB.Timer updateTimer;

        public (double min, double max, double limitMin, double limitMax) Y1;
        public (double min, double max, double limitMin, double limitMax) Y2;
        public (double min, double max, double limitMin, double limitMax) Y3;
        public (double min, double max, double limitMin, double limitMax) Y4;

        public bool Record = true;

        public bool Y1autoRange = false;
        public bool Y2autoRange = false;
        public bool Y3autoRange = false;
        public bool Y4autoRange = false;


        public Drawing.Axis AxisY1;
        public Drawing.Axis AxisY2;
        public Drawing.Axis AxisY3;
        public Drawing.Axis AxisY4;


        private double acutalSecond;
        private Chart chart;
        private double xrange = 3600;

        private QB.Controls.Box yFrame;

        private QB.Controls.Box xControl;

        private QB.Controls.Box RecordControl;
        private QB.Controls.Box record;
        private QB.Controls.Box UdpStream;
        private QB.Controls.Box UdpPort;
        private QB.Controls.Box f1Hz;
        private QB.Controls.Box f10Hz;
        private QB.Controls.Box f100Hz;
        private QB.Controls.Box f1kHz;
        private QB.Controls.Box hSet;
        private QB.Controls.Box folder;
        private QB.Controls.Box MenuButton;

        private QB.Controls.Box y1Control;
        private QB.Controls.Box y2Control;
        private QB.Controls.Box y3Control;
        private QB.Controls.Box y4Control;

        BoxTable AxisControl;

        public BoxButton Execute;
        public BoxSignal yMin;
        public BoxSignal yMax;
        public BoxSignalOnOff yAuto;


        private Box Recording;


        private bool recording = false;

        private string Page;

        private bool visible = true;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                SetVisible();
            }
        }

        private double X;
        private double Y;
        private double H;
        private double W;

        public List<ChartSeries> signals;

        int legendIndex = 0;

        string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                chart.Name = value;
            }
        }

        StringSignal SelectedAxis = new StringSignal("SelectedAxis", "Axis", "Y");
        Signal SelectedAxisMin = new Signal("SelectedAxisMin", "Min",unit:"",value:0);
        Signal SelectedAxisMax = new Signal("SelectedAxisMax", "Max", unit: "", value: 10);
        Signal SelectedAxisAuto = new Signal("SelectedAxisAuto", "Autorange", unit: "");


        bool UserNormName = true;

        StreamerWriterUDP Streamer;

        public ChartExtended(string name, double? x = null, double? y = null, double? h = null, double? w = null, string page = null, bool useNormName = true)
        {
            Streamer = new StreamerWriterUDP(name: $"{name}.Streamer", sendDirect: false, valueInterval: 100, descriptorInterval: 2000);
            UserNormName = useNormName;
            
            updateTimer = new QB.Timer("chartExtended updateTimer", 200);
            Y1.min = double.NaN;
            Y1.max = double.NaN;
            Y1.limitMin = -5;
            Y1.limitMax = 5;

            Y2.min = double.NaN;
            Y2.max = double.NaN;
            Y2.limitMin = -5;
            Y2.limitMax = 5;

            Y3.min = double.NaN;
            Y3.max = double.NaN;
            Y3.limitMin = -5;
            Y3.limitMax = 5;

            Y4.min = double.NaN;
            Y4.max = double.NaN;
            Y4.limitMin = -5;
            Y4.limitMax = 5;

            Page = page;

            X = x == null ? 100 : x.ToDouble();
            Y = y == null ? 5 : y.ToDouble();
            H = h == null ? 130 : h.ToDouble();
            W = w == null ? 170 : w.ToDouble();

            double sizeIcon = 5;
            double sizeBar = 6;
            double offsetY = (sizeBar - sizeIcon) / 2;
            signals = new List<ChartSeries>();

            double cX = X - 10;  // Chart Offset
            double cY = Y - 20;  // Chart Offset

            chart = new Chart(name, x: cX, y: cY + sizeBar, w: W, h: H - sizeBar);
            Name = name;
            chart.Directory = page;
            chart.SignalLegendVisible = false;
            chart.Clickable = false;

            AxisY1 = chart.AxisY1;
            AxisY2 = chart.AxisY2;
            AxisY3 = chart.AxisY3;
            AxisY4 = chart.AxisY4;

            updateTimer.OnElapsed = (t, ea) => idle(t, ea);
            updateTimer.Run();
            timerReset();

            xControl = new QB.Controls.Box(name, x: X + W - 35, y: Y, w: W - sizeBar, h: sizeBar, style: $"bg:#e6e6e6,align:ml,bg-hover:#e6e6e6"
                    , boxes: new[] {


                new QB.Controls.Box("htext", x:0,text:"XRange:", y:0, w:13, h:sizeBar, style:"font::7:,align:ml"),
                hSet = new QB.Controls.Box("history", text: "100s", x:12.5, y:0, w:12, h:sizeBar, style:"font::7:b,bg:transparent,align:mr",
                 onClick:(e) => EditHistory(),
                onAccept:(e) => reset()) {
                    Directory = Page
                },
                 MenuButton = new QB.Controls.Box("htext", x:30, icon: "fa:ellipsis", y: 0, w: 5, h: sizeBar, style: "font::7:,align:ml",onClick: (s) => RecordControl.Visible = !RecordControl.Visible)
                }
                
            );
            
            RecordControl = new QB.Controls.Box("record", x: X + W - 30, y: Y+ sizeBar, w: 30, h: 60, style: $"bg:#F0F0F0,align:ml,bg-hover:#e6e6e6"
                    , boxes: new[] {

                    f1Hz = new QB.Controls.Box("recordInterval", x:3, y:0, w:40, h:sizeBar, style:"font::8,align:mc", onClick:(s) => chart.Log.Interval = 1000,
                    boxes: new[] {
                        new QB.Controls.Box("led", x:0.5, y:sizeBar / 4, w:sizeBar / 2, h:sizeBar / 2,style: $"bg:transparent,border:1")
                    {
                        BorderBColor = "Black".ToColor(),
                        BorderTColor = "Black".ToColor(),
                        BorderLColor = "Black".ToColor(),
                        BorderRColor = "Black".ToColor(),
                        Directory = Page
                    },

                    new QB.Controls.Box("text", text: "1Hz",x:4, y:sizeBar / 4 + 0.2, w:10, h:sizeBar / 2,style:"font::7.5,align:ml")
                    }
                    ),


                    f10Hz = new QB.Controls.Box("recordInterval", x:3, y:sizeBar * 1, h:sizeBar, style:"font::7.5,align:mc", onClick:(s) => chart.Log.Interval = 100,
                    boxes: new[] {
                        new QB.Controls.Box("led", x:0.5, y:sizeBar / 4, w:sizeBar / 2, h:sizeBar / 2,style: $"bg:transparent,border:1")
                    {
                        BorderBColor = "Black".ToColor(),
                        BorderTColor = "Black".ToColor(),
                        BorderLColor = "Black".ToColor(),
                        BorderRColor = "Black".ToColor(),
                        Directory = Page
                    },

                    new QB.Controls.Box("text", text: "10Hz", x:4, y:sizeBar / 4 + 0.2, w:10, h:sizeBar / 2,style:"font::8,align:ml")
                    }
                    ),

                    f100Hz = new QB.Controls.Box("recordInterval", x:3, y:sizeBar * 2, h:sizeBar, style:"font::7.5,align:mc", onClick:(s) => chart.Log.Interval = 10,
                    boxes: new[] {
                        new QB.Controls.Box("led", x:0.5, y:sizeBar / 4, w:sizeBar / 2, h:sizeBar / 2,style: $"bg:transparent,border:1")
                    {
                        BorderBColor = "Black".ToColor(),
                        BorderTColor = "Black".ToColor(),
                        BorderLColor = "Black".ToColor(),
                        BorderRColor = "Black".ToColor(),
                        Directory = Page
                    },

                    new QB.Controls.Box("text", text: "100Hz", x:4, y:sizeBar / 4 + 0.2, h:sizeBar / 2,style:"font::8,align:ml")
                    }
                    ),

                    f1kHz = new QB.Controls.Box("recordInterval", x:3, y:sizeBar * 3, w:40, h:sizeBar, style:"font::7.5,align:mc", onClick:(s) => chart.Log.Interval = 1,
                    boxes: new[] {
                        new QB.Controls.Box("led", x:0.5, y:sizeBar / 4, w:sizeBar / 2, h:sizeBar / 2,style: $"bg:transparent,border:1")
                    {
                        BorderBColor = "Black".ToColor(),
                        BorderTColor = "Black".ToColor(),
                        BorderLColor = "Black".ToColor(),
                        BorderRColor = "Black".ToColor(),
                        Directory = Page
                    },

                    new QB.Controls.Box("text", text: "1kHz", x:4, y:sizeBar / 4 + 0.2, h:sizeBar / 2,style:"font::8,align:ml")
                    }
                    ),

                           record = new QB.Controls.Box("csv", x:2, y:sizeBar * 4 + 1,w:26, h:6, onClick:(s) => toggleRecording(), style:"font::7:b,align:ml",
                           boxes: new[] {
                        new QB.Controls.Box("led",text:"Record", x:6,style:"font::7.5:b,align:ml"),
                        new QB.Controls.Box("led",icon:"fa:circle-dot", x:1,y:0.25,h:5,w:5)
                           }
                           )
                           {
                    Directory = Page
                },

                    folder = new QB.Controls.Box("folder", x:2, y:sizeBar * 5 + 1, w:26, h:6,style:"font::7.5:b,align:mc", onClick:(s) => chart.Log.OpenFolder(),
                           boxes: new[] {
                        new QB.Controls.Box("led",text:"Folder", x:6,style:"font::7.5:b,align:ml"),
                        new QB.Controls.Box("led",icon:"fa:folder", x:1,y:0.25,h:5,w:5)
                           }
                           )
                           {
                    Directory = Page
                },
                     UdpStream = new QB.Controls.Box("udpStream", x:2, y:sizeBar * 6 + 1, w:26, h:6,style:"font::7.5:b,align:mc", onClick:(s) => toggleStream(),
                           boxes: new[] {
                        new QB.Controls.Box("led",text:"UDP Stream", x:6,style:"font::7.5:b,align:ml"),
                        new QB.Controls.Box("led",icon:"fa:share", x:1,y:0.25,h:5,w:5)
                           }
                           )
                           {
                    Directory = Page
                },
                    UdpPort = new QB.Controls.Box("udpStream", x:2, y:sizeBar * 7 + 1, w:26, h:6,style:"font::7.5:b,align:mc", onClick:(s) => Streamer.StartSteam(),
                           boxes: new[] {
                        new QB.Controls.Box("led",textFunction:() => Streamer.Port.ToString(), x:6,style:"font::7.5:b,align:ml"),
                        new QB.Controls.Box("led",icon:"fa:at", x:1,y:0.25,h:5,w:5)
                           }
                           )
                           {
                    Directory = Page
                },
                }
            );


            Recording = new QB.Controls.Box("rec", icon: "fa:circle-dot", x: X + 60, y: Y-0.2, w: 50, h: sizeBar, style: "bg:tranparent,font::12:b|i,align:ml,textcolor:Red",
                
                boxes: new[]
                {
                new QB.Controls.Box("record",  x:28,text:"",y:0.2 , style:"font::10:b|i:red,textcolor:Red,bg:transparent,align:ml")
               
                }
                )
            { Visible = false}
                ;




          
            RecordControl.Clickable = false;
            RecordControl.BackgroundColorHover = RecordControl.BackgroundColor;
            RecordControl.Visible = false;


            y1Control = new QB.Controls.Box(name, x: X, y: Y, w: sizeBar * 2, h: sizeBar, onClick:(e) => EditAxis(1)
                    , boxes: new[] {
                    new QB.Controls.Box("axis", text:"Y1", x:0, y:0, w:sizeBar, h:sizeBar, style: $"bg:transparent,align:ml,font::7:b"),
                           new QB.Controls.Box("led", x:5, y:1.8, w:sizeBar / 3, h:sizeBar / 3,style: $"bg:transparent,border:Black:1")
                    }
                )
            { Directory = Page, BackgroundColor = System.Drawing.Color.Transparent };
            
            y2Control = new QB.Controls.Box(name, x: X + 12, y: Y, w: sizeBar * 2, h: sizeBar, onClick: (e) => EditAxis(2)
                , boxes: new[] {
                    new QB.Controls.Box("axis", text:"Y2", x:0, y:0, w:sizeBar, h:sizeBar, style: $"bg:transparent,align:ml,font::7:b"),
                     new QB.Controls.Box("led", x:5, y:1.8, w:sizeBar / 3, h:sizeBar / 3,style: $"bg:transparent,border:Black:1")
                    }
                )
            { Directory = Page, BackgroundColor = System.Drawing.Color.Transparent, Visible = false };

            y3Control = new QB.Controls.Box(name, x: X + 24, y: Y, w: sizeBar * 2, h: sizeBar, onClick: (e) => EditAxis(3)
                , boxes: new[] {
                    new QB.Controls.Box("axis", text:"Y3", x:0, y:0, w:sizeBar, h:sizeBar, style: $"bg:transparent,align:ml,font::7:b"),
                    new QB.Controls.Box("led", x:5, y:1.8, w:sizeBar / 3, h:sizeBar / 3,style: $"bg:transparent,border:Black:1")
                    }
                )
            { Directory = Page, BackgroundColor = System.Drawing.Color.Transparent,Visible = false };

            y4Control = new QB.Controls.Box(name, x: X + 36, y: Y, w: sizeBar * 2, h: sizeBar, onClick: (e) => EditAxis(4)
                , boxes: new[] {
                    new QB.Controls.Box("axis", text:"Y4", x:0, y:0, w:sizeBar, h:sizeBar, style: $"bg:transparent,align:ml,font::7:b"),
                    new QB.Controls.Box("led", x:5, y:1.8, w:sizeBar / 3, h:sizeBar / 3,style: $"bg:transparent,border:Black:1")
                    }
                )
            { Directory = Page, BackgroundColor = System.Drawing.Color.Transparent, Visible = false };

            System.Drawing.Color col = "#F0F0F0".ToColor();

           

            AxisControl = new BoxTable("AxisControl", x: X, y: Y + sizeBar, h: 35, w: 30, columns: 1, rows: 4, gap: 0.2, backColor: col, page: Page);
            AxisControl.Add(yMin = new BoxSignal("min", target: this.SelectedAxisMin, locked: false, backColor: col, valueColor: "white", textPosition:"ml"), column: 1, row: 1);
            AxisControl.Add(yMax = new BoxSignal("max", target: this.SelectedAxisMax, locked: false, backColor: col,valueColor:"white", textPosition: "ml"), column: 1, row: 2);
            AxisControl.Add(yAuto = new BoxSignalOnOff("auto", target: this.SelectedAxisAuto, locked: false, backColor: col,ledStyle:true), column: 1, row: 3);
            AxisControl.Add(Execute = new BoxButton("play", icon: "fa:play-circle", locked: false, backColor: col, ledStyle: false,iconSizeFactor:1,onClick:(e) => this.SetAxis()), column: 1, row: 4,rowSpan:1);

            AxisControl.Visible = false;
            AxisControl.ZOrder = 10;
        //    Debug.WriteLine("AxisControl: " + AxisControl.GetHashCode());

            y2Control.Visible = false;
            y3Control.Visible = false;
            y4Control.Visible = false;

            chart.Directory = Page;
            y1Control.Directory = Page;
            xControl.Directory = Page;
          
            reset();

            xControl.BackgroundColor = System.Drawing.Color.Transparent;
            xControl.BackgroundColorHover = System.Drawing.Color.Transparent;
        }


        void toggleStream()
        {
          //  QB.Logger.Info("click");
            if (Streamer.IsRunning)
            {
          //      QB.Logger.Info("stop");
                Streamer.StopStream();
                UdpStream.BackgroundColor = Color.Transparent;
                Streamer.Port = 0;
              
            }
            else
            {
        //        QB.Logger.Info("start");
                Streamer.StartSteam();
                UdpStream.BackgroundColor = Color.Orange;
             
            }
        }

        void EditHistory()
        {
            string input = "";
            input.ShowEditDialog(ref input, text:"New X-Range",unit1:"s", unit2: "m", unit3: "h", unit4: "");
            if (input == "") return;
            hSet.Text = input;
            reset();
        }
        public void Update()
        {
            chart.RemoveAll();
            foreach (ChartSeries series in signals)
            {
                chart.Add(series.Read);
            }

        }
        public void EditAxis(int axis)
        {
      //      Debug.WriteLine(Name + " Edit Axis " + axis);

            if (AxisControl.Visible)
            {
                AxisControl.Visible = false;
                AxisControl.ZOrder = -1 ;
                y1Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y2Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y3Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y4Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                return;
            }

            AxisControl.Visible = true;
            AxisControl.ZOrder = 10;
        
      //      Debug.WriteLine("AxisControl: " + AxisControl.GetHashCode());

            double min = double.NaN;
            double max = double.NaN;

            if (axis == 1)
            {
                y1Control.Boxes[1].BackgroundColor = "orange".ToColor();
                SelectedAxisAuto.Value = Y1autoRange ? 1 : 0;
                ((BoxSignalOnOff)AxisControl.Boxes["auto"]).Update();
                ((BoxButton)AxisControl.Boxes["play"]).Enable();
                min = chart.AxisY1.Min;
               max = chart.AxisY1.Max;
            }
            if (axis == 2)
            {
           //     Debug.WriteLine(Name + "2");
                y2Control.Boxes[1].BackgroundColor = "orange".ToColor();
                SelectedAxisAuto.Value = Y2autoRange ? 1 : 0;
                ((BoxSignalOnOff)AxisControl.Boxes["auto"]).Update();
                ((BoxButton)AxisControl.Boxes["play"]).Enable();
                min = chart.AxisY2.Min;
                max = chart.AxisY2.Max;
            }
            if (axis == 3)
            {
                y3Control.Boxes[1].BackgroundColor = "orange".ToColor();
                SelectedAxisAuto.Value = Y3autoRange ? 1 : 0;
                ((BoxSignalOnOff)AxisControl.Boxes["auto"]).Update();
                ((BoxButton)AxisControl.Boxes["play"]).Enable();
                min = chart.AxisY3.Min;
                max = chart.AxisY3.Max;
            }
            if (axis == 4)
            {
                y4Control.Boxes[1].BackgroundColor = "orange".ToColor();
                SelectedAxisAuto.Value = Y4autoRange ? 1 : 0;
                ((BoxSignalOnOff)AxisControl.Boxes["auto"]).Update();
                ((BoxButton)AxisControl.Boxes["play"]).Enable();
                min = chart.AxisY4.Min;
                max = chart.AxisY4.Max;
            }

            SelectedAxis.Value = "Y" + axis;
            SelectedAxisMin.Value = min;
            SelectedAxisMax.Value = max;
 
        }
        public void SetAxis()
        {
        //    Debug.WriteLine(Name + "Set Click");

            AxisControl.ZOrder = -1;
            AxisControl.Visible = false;
            if(SelectedAxis.Value == "Y1")
            {
             //   Debug.WriteLine("Set Y1");
                if (SelectedAxisAuto.Value == 0)
                {
                    Y1autoRange = false;
                    chart.AxisY1.Min = SelectedAxisMin.Value;
                    chart.AxisY1.Max = SelectedAxisMax.Value;
                    scaleY(10, 20, 1);
                }
                else
                {
                    Y1autoRange = true;
                }

            }
            if (SelectedAxis.Value == "Y2")
            {
          //      Debug.WriteLine("Set Y2");
                if (SelectedAxisAuto.Value == 0)
                {
                    Y2autoRange = false;
                    chart.AxisY2.Min = SelectedAxisMin.Value;
                    chart.AxisY2.Max = SelectedAxisMax.Value;
                    scaleY(10, 20, 2);
                }
                else
                {
                    Y2autoRange = true;
                }

            }
            if (SelectedAxis.Value == "Y3")
            {
           //     Debug.WriteLine("Set Y3");
                if (SelectedAxisAuto.Value == 0)
                {
                    Y3autoRange = false;
                    chart.AxisY3.Min = SelectedAxisMin.Value;
                    chart.AxisY3.Max = SelectedAxisMax.Value;
                    scaleY(10, 20, 3);
                }
                else
                {
                    Y3autoRange = true;
                }

            }
            if (SelectedAxis.Value == "Y4")
            {
           //     Debug.WriteLine("Set Y4");
                if (SelectedAxisAuto.Value == 0)
                {
                    Y4autoRange = false;
                    chart.AxisY4.Min = SelectedAxisMin.Value;
                    chart.AxisY4.Max = SelectedAxisMax.Value;
                    scaleY(10, 20, 4);
                }
                else
                {
                    Y4autoRange = true;
                }

            }
            y1Control.Boxes[1].BackgroundColor = "transparent".ToColor();
            y2Control.Boxes[1].BackgroundColor = "transparent".ToColor();
            y3Control.Boxes[1].BackgroundColor = "transparent".ToColor();
            y4Control.Boxes[1].BackgroundColor = "transparent".ToColor();



        }

        int lastAxis = 1;

        bool y2Active = false;
        bool y3Active = false;
        bool y4Active = false;


        ColorGenerator ChartColors = new ColorGenerator();


        public bool AutoColor = false;
        public void Add(Signal s, int yAxis = 1)
        {
            if (AutoColor) s.Color = ChartColors.GetNextColor();

            signals.Add(new ChartSeries(s.Text, s, chart, History, legendIndex++, this, X, Y + 6, Page, yAxis,UserNormName));
            if (yAxis == 2) { y2Control.Visible = true; y2Active = true; };
            if (yAxis == 3) { y3Control.Visible = true; y3Active = true; };
            if (yAxis == 4) { y4Control.Visible = true; y4Active = true; } ;

            Streamer.Add(s);
        }

        void toggleRecording()
        {
            // recording = !recording;
            if (!chart.Log.Running)
            {
                chart.Log.Start();  
            }
            else { 
            _ = chart.Log.Stop();
            }
            RecordControl.Visible = false;
            record.BackgroundColor = recording ? System.Drawing.Color.Tomato : System.Drawing.Color.Transparent;
        }
        void timerReset()
        {
            timer.read = 0;
            timer.startTime = DateTime.Now;
        }
        void reset()
        {
            try
            {
                string t = hSet.Text.ToString();
                int i = t.Length - 1;

              
                string u = t.Substring(t.Length - 1, 1);
                if (u.ToLower() != "s" && u.ToLower() != "m" && u.ToLower() != "h")
                {
                    History = t.ToInt32();
                    hSet.Text = t + "s";
                }
                else
                {
                    History = t.Substring(0, t.Length - 1).ToInt32();
                    if (u.ToLower() == "h")
                        History = History * 3600;
                    if (u.ToLower() == "m")
                        History = History * 60;

                    //if (u.ToLower() == "s")
                    //    History = History;
                }

                chart.AxisX.Range = History * 1000;

                foreach (ChartSeries v in signals)
                    v.History = History;
            }
            catch
            {
                QB.UI.Toast.Show($"Wrong Format\n History: Int ", "", 1500);
                History = 3600;
                hSet.Text = "3600";
                return;
            }
            foreach (ChartSeries i in signals)
                i.Reset();
            timerReset();

        }
        double seconds()
        {
            timer.now = DateTime.Now;
            timer.duration = (timer.now - timer.startTime);
            return timer.duration.TotalSeconds;
        }

        int ledCounter = -1;
        void idle(Timer t, TimerEventArgs ea)
        {

            if (Visible)
            {
                if (Recording == null) return;
                if (!chart.Log.Running)
                {
                    ledCounter = -1;
                    Recording.Visible = false;
                    Recording.Icon = "fa:circle-pause";
                    Recording.Boxes[0].Text = "";

                }
                else
                {
                    Recording.Visible = true;
                    Recording.Icon = "fa:circle-dot:red";
                    ledCounter++;

                    if (Recording.Boxes.Count == 0) return;

                    if (ledCounter == 10)
                    {
                        string freq = "1 Hz";

                        if (chart.Log.Interval == 1)
                            freq = "1 kHz";
                        if (chart.Log.Interval == 10)
                            freq = "100 Hz";
                        if (chart.Log.Interval == 100)
                            freq = "10 Hz";

                        Recording.Boxes[0].Text = "REC " + freq;
                    }
                    else if (ledCounter == 20)
                    {
                        Recording.Boxes[0].Text = "";
                        ledCounter = 0;
                    }

                }
            }

          
            record.BackgroundColor = chart.Log.Running ? System.Drawing.Color.Tomato : System.Drawing.Color.Transparent;

          
            acutalSecond = seconds();
            List<ChartSeries> signalsWork;
            lock (signals)
            {
                signalsWork = signals;
            }

            foreach (ChartSeries s in signalsWork)
                s.Update(seconds());

           

            if (chart.Log.Interval == 1000)
            {
                f1Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Orange;
                f10Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f100Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f1kHz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
            }
            if (chart.Log.Interval == 100)
            {
                f1Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f10Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Orange;
                f100Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f1kHz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
            }
            if (chart.Log.Interval == 10)
            {
                f1Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f10Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f100Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Orange;
                f1kHz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
            }
            if (chart.Log.Interval == 1)
            {
                f1Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f10Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f100Hz.Boxes[0].BackgroundColor = System.Drawing.Color.Transparent;
                f1kHz.Boxes[0].BackgroundColor = System.Drawing.Color.Orange;
            }


            if (hSet.Text.ToInt32() != History)
            {
                QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "History -> " + History);
                History = hSet.Text.ToInt32();
            }

            int sc = signals.Count();
            if (sc == 0)
                return;
            UpdateScaling();

        }
        void UpdateScaling()
        {

            int sc = signals.Count();

            if (sc == 0)
                return;

            

            if (Y1autoRange)
                scaleAutoRange(axis: 1);

            if (Y2autoRange)
                scaleAutoRange(axis: 2);

            if (Y3autoRange)
                scaleAutoRange(axis: 3);

            if (Y4autoRange)
                scaleAutoRange(axis: 4);
        }

        void scaleAutoRange(int axis = 1)
        {
     
            List<double> minList = new List<double>();
            List<double> maxList = new List<double>();
            double min;
            double max;

            try
            {
                foreach (var s in signals)
                {
                    if (s.Yaxis != axis) continue;

                    if (s.values != null && s.values.Count > 0)
                    {
                        double smin = s.values.Min();
                        double smax = s.values.Max();

                        if (!double.IsNaN(smin)) minList.Add(smin);
                        if (!double.IsNaN(smax)) maxList.Add(smax);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Error " + e.Message);
            }

            if (minList.Count == 0 || maxList.Count == 0)
            {
                Debug.WriteLine("AutoRange: Keine gültigen Werte für Achse " + axis);
                return;
            }

            min = minList.Min();
            max = maxList.Max();


           // Debug.WriteLine(Name + " AutoRange " + axis + " Min " + min + " Max " + max);

            if (axis == 1)
            {
                Y1.min = min;
                Y1.max = max;
                chart.AxisY1.Min = min - (max - min) * 0.05;
                chart.AxisY1.Max = max + (max - min) * 0.05;
                scaleY(10, 20, 1);
            }

            if (axis == 2)
            {
                Y2.min = min;
                Y2.max = max;
                chart.AxisY2.Min = min - (max - min) * 0.05;
                chart.AxisY2.Max = max + (max - min) * 0.05;
                scaleY(10, 20, 2);
            }

            if (axis == 3)
            {
                Y3.min = min;
                Y3.max = max;
                chart.AxisY3.Min = min - (max - min) * 0.05;
                chart.AxisY3.Max = max + (max - min) * 0.05;
                scaleY(10, 20, 3);

            }

            if (axis == 4)
            {
                Y4.min = min;
                Y4.max = max;
                chart.AxisY4.Min = min - (max - min) * 0.05;
                chart.AxisY4.Max = max + (max - min) * 0.05;
                scaleY(10, 20, 4);

            }
        }


        public void ScaleAxisY1(double min, double max, double major = 10, double minor = 20)
        {
            chart.AxisY1.Min = min;
            chart.AxisY1.Max = max;
            scaleY(10, 20, 1);
        }
        public void ScaleAxisY2(double min, double max, double major = 10, double minor = 20)
        {
            chart.AxisY2.Min = min;
            chart.AxisY2.Max = max;
            scaleY(10, 20, 2);
        }
        public void ScaleAxisY3(double min, double max, double major = 10, double minor = 20)
        {
            chart.AxisY3.Min = min;
            chart.AxisY3.Max = max;
            scaleY(10, 20, 3);
        }
        public void ScaleAxisY4(double min, double max, double major = 10, double minor = 20)
        {
            chart.AxisY4.Min = min;
            chart.AxisY4.Max = max;
            scaleY(10, 20, 4);
        }

        void scaleY(double major, double minor, int axis = 1)
        {

            if (axis == 1)
            {
                chart.AxisY1.MajorTicks = (chart.AxisY1.Max - chart.AxisY1.Min) / major;
                chart.AxisY1.MinorTicks = (chart.AxisY1.Max - chart.AxisY1.Min) / minor;
            }

            if (axis == 2)
            {
                chart.AxisY2.MajorTicks = (chart.AxisY2.Max - chart.AxisY2.Min) / major;
                chart.AxisY2.MinorTicks = (chart.AxisY2.Max - chart.AxisY2.Min) / minor;
            }

            if (axis == 3)
            {
                chart.AxisY3.MajorTicks = (chart.AxisY3.Max - chart.AxisY3.Min) / major;
                chart.AxisY3.MinorTicks = (chart.AxisY3.Max - chart.AxisY3.Min) / minor;
            }

            if (axis == 4)
            {
                chart.AxisY4.MajorTicks = (chart.AxisY4.Max - chart.AxisY4.Min) / major;
                chart.AxisY4.MinorTicks = (chart.AxisY4.Max - chart.AxisY4.Min) / minor;
            }
        }

        void scaleX(double major, double minor)
        {

            chart.AxisX.MajorTicks = (xrange / major) < 1 ? 1 : (xrange / major);
            //chart.AxisX.MinorTicks = (xrange / minor) <1? 1:(xrange / minor);

            chart.AxisX.MinorTicks = (xrange / minor);

        }


        void openFolder()
        {
            try
            {
                Process.Start(QB.Book.DataDirectory);
            }
            catch
            {

            }
        }

        public void SetVisible()
        {
            if (!Visible && AxisControl.Visible)
            {
                AxisControl.ZOrder = -1;
                AxisControl.Visible = false;
                y1Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y2Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y3Control.Boxes[1].BackgroundColor = "transparent".ToColor();
                y4Control.Boxes[1].BackgroundColor = "transparent".ToColor();
            }


            chart.Visible = Visible;
            xControl.Visible = Visible;
            
            y1Control.Visible = Visible;
            y2Control.Visible = y2Active && Visible ? true : false;
            y3Control.Visible = y3Active && Visible ? true : false;
            y4Control.Visible = y4Active && Visible ? true : false;
            Recording.Visible = visible && Visible ? true : false;

            y1Control.Clickable = Visible;
            y2Control.Clickable = y2Active && Visible ? true : false;
            y3Control.Clickable = y3Active && Visible ? true : false;
            y4Control.Clickable = y4Active && Visible ? true : false;
            MenuButton.Clickable = Visible;
            


            record.Clickable = Visible;
            f1Hz.Clickable = Visible;
            f10Hz.Clickable =  Visible;
            f100Hz.Clickable = Visible;
            f1kHz.Clickable = Visible;
            hSet.Clickable = Visible;
            folder.Clickable = Visible;



            foreach (ChartSeries i in signals)
                i.Frame.Visible = Visible;
        }

    }

    public class ColorGenerator
    {
        private readonly List<(int Hue, double Lightness)> _preferredColors = new List<(int Hue, double Lightness)>()
    {
        (0, 0.35),     // dark red
        (240, 0.30),   // dark blue
        (120, 0.35),   // dark green
        (270, 0.45),   // violet
        (20, 0.30),    // brown
        (45, 0.50),    // orange (bright, distinct from brown)
        (180, 0.40),   // cyan
        (285, 0.40),   // purple
        (90, 0.45),    // yellow-green
        (210, 0.40),   // petrol

        (0, 0.50),     // full red
        (30, 0.50),    // full orange
        (240, 0.50),   // full blue
        (120, 0.50),   // full green
        (300, 0.50),   // full magenta
    };

        private int _nextIndex = 0;
        private readonly HashSet<Color> _usedColors = new HashSet<Color>();

        public Color GetNextColor()
        {
            const double saturation = 0.9;

            while (_nextIndex < _preferredColors.Count)
            {
                var (hue, lightness) = _preferredColors[_nextIndex++];
                var color = FromHSL(hue, saturation, lightness);
                if (_usedColors.Add(color))
                    return color;
            }

            // Fallback: auto-generated colors using golden angle
            int i = _nextIndex++;
            int hueAuto = (i * 137 + 23) % 360;
            double lightnessAuto = 0.35 + ((i / 12) % 3) * 0.1;
            var fallback = FromHSL(hueAuto, saturation, lightnessAuto);
            _usedColors.Add(fallback);
            return fallback;
        }

        public static Color FromHSL(double h, double s, double l)
        {
            h = h / 360.0;

            double r = l, g = l, b = l;
            if (s != 0)
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRGB(p, q, h + 1.0 / 3);
                g = HueToRGB(p, q, h);
                b = HueToRGB(p, q, h - 1.0 / 3);
            }

            return Color.FromArgb(
                255,
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255)
            );
        }

        private static double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }


}
