using Autofac.Core;
using CefSharp.DevTools.Target;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenCvSharp;
using OpenCvSharp.Flann;
using PdfSharp.Pdf.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using static QB.Table;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QB.Controls
{
    public class BoxView : Item
    {
        public string Page;
        public BoxView(string name, string page) : base(name)
        {
            Page = page;
        }

        bool visible = true;
        
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

        UIBox Box;

        List<UIBox> Boxes = new List<UIBox>();
        List<BoxTable> BoxTables = new List<BoxTable>();
        List<BoxFlowPanel> BoxFlowPanels = new List<BoxFlowPanel>();
        List<BoxList> BoxLists = new List<BoxList>();
        List<ChartExtended> ChartChartExtendeds = new List<ChartExtended>();
        List<BoxLogView> boxLogViews = new List<BoxLogView>();

        public void Add(object box)
        {
            
            if(box is BoxButton)
            {
                Add((BoxButton)box);
            }
            else if (box is BoxTable)
            {
                BoxTables.Add((BoxTable)box);
            }
            else if (box is BoxFlowPanel)
            {
                BoxFlowPanels.Add((BoxFlowPanel)box);
            }
            else if (box is BoxList)
            {
                BoxLists.Add((BoxList)box);
            }
            else if (box is ChartExtended)
            {
                ChartChartExtendeds.Add((ChartExtended)box);
            }
            else if (box is BoxLogView)
            {
                boxLogViews.Add((BoxLogView)box);

            }
        }

        public void Add(UIBox box, BoxList boxList = null, BoxTable boxTable = null, BoxFlowPanel boxFlowPanel = null, int column = 1, int row = 1, int columnSpan = 1, int rowSpan = 1, string page = null)
        {
            Box = box;
        
            

            //		QB.Logger.Info("Add BoxItem" + Box.Name);
            if (boxList != null)
            {
                Box.Page = boxList.Page;
                //	QB.Logger.Info("BoxList item " + Box.Name);
                Box.X = boxList.X + boxList.Gap;
                Box.Y = boxList.Next + boxList.Gap;
                Box.W = boxList.Width - 2 * +boxList.Gap;

                Box.H = double.IsNaN(Box.H) ? boxList.RowHeight : Box.H;
                //Box.H = boxList.RowHeight;
                Box.H = Box.H - 2 * boxList.Gap;
                Box.Create();
                boxList.Boxes.Add(Box.Name, Box);
                boxList.Next += box.H + boxList.Gap * 2;
                Boxes.Add(Box);
                return;
            }

            if (boxTable != null)
            {
                Box.Page = boxTable.Page;
                Box.X = boxTable.X + (boxTable.cellW * (column - 1)) + boxTable.Gap;
                Box.Y = boxTable.Y + (boxTable.cellH * (row - 1)) + boxTable.Gap;
                Box.W = boxTable.cellW * columnSpan - 2 * boxTable.Gap;
                Box.H = boxTable.cellH * rowSpan - 2 * boxTable.Gap;

                Box.Create();
                boxTable.Boxes.Add(Box.Name, Box);


                return;
            }

            if (boxFlowPanel != null)
            {
                Box.Page = boxFlowPanel.Page;
                //		QB.Logger.Info("BoxFlowPanel item " + Box.Name);
                Box.X = boxFlowPanel.X + (boxFlowPanel.cellW * (boxFlowPanel.NextColumn)) + boxFlowPanel.Gap;
                Box.Y = boxFlowPanel.Y + (boxFlowPanel.cellH * (boxFlowPanel.NextRow)) + boxFlowPanel.Gap;
                Box.W = boxFlowPanel.cellW * columnSpan - 2 * boxFlowPanel.Gap;
                Box.H = boxFlowPanel.cellH - 2 * boxFlowPanel.Gap;

                boxFlowPanel.NextColumn += columnSpan;

                if (boxFlowPanel.NextColumn >= boxFlowPanel.Columns)
                {
                    boxFlowPanel.NextRow++;
                    boxFlowPanel.NextColumn = 0;
                }


                Box.Create();
                boxFlowPanel.Boxes.Add(Box.Name, Box);
                return;
            }

            Box.Page = Page;
         //   Debug.WriteLine(Box.Name + " " + Page);
            Box.Create();
            Boxes.Add(Box);

        }

        public void SetVisible()
        {
            foreach (BoxButton box in Boxes)
            {
                box.Visible = Visible;
               
            }
            foreach (BoxTable boxTable in BoxTables)
            {
                boxTable.Visible = Visible;
            }
            foreach (BoxFlowPanel boxFlowPanel in BoxFlowPanels)
            {
                boxFlowPanel.Visible = Visible;
            }
            foreach (BoxList boxList in BoxLists)
            {
                boxList.Visible = Visible;
            }
            foreach (ChartExtended charts in ChartChartExtendeds)
                charts.Visible = Visible;

            foreach (BoxLogView log in boxLogViews) 
            {
                log.Visible = Visible;
                log.Navigation.Visible = Visible;
            }




        }
    }
    public class UIBox : Item
    {

        public System.Drawing.Color Backcolor;
        public System.Drawing.Color Valuecolor;
        public System.Drawing.Color SelectColor = System.Drawing.Color.LightBlue;
        public System.Drawing.Color DefaultColor = System.Drawing.Color.Transparent;
        public QB.Controls.Box Frame;
        public QB.Controls.Box Description;
        public QB.Controls.Box State;
        public QB.Controls.Box Read;
        public QB.Controls.Box Led;



        public string Page;
        public string Icon;
        public double X;
        public double Y;
        public double W;
        public double H;

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
                if(Frame!= null)
                Frame.Visible = value;
                SetVisible();
            }
        }

        private int zorder = 1;
        public int ZOrder
        {
            get
            {
                return zorder;
            }
            set
            {
                zorder = value;
                SetZOrder();
            }
        }

        public UIBox(string name) : base(name)
        {

        }
        public virtual void SetVisible()
        {

        }
        public virtual void SetBackColor(object color)
        {

        }

        public virtual void Create()
        {

        }

        public virtual void SetZOrder()
        {
            Frame.ZOrder = zorder;

        }

        public virtual void SetTarget(object signal)
        {
        }

        public virtual void MouseWheel(Control c, int delta)
        {

        }
        public virtual void MoveUp()
        {

        }
        public virtual void MoveDown()
        {

        }
        public virtual void SetLedColor(object color) 
        {
            
        }
        public virtual void SetSelectColor(object color)
        {

        }

        public virtual void Click()
        {

            QB.UI.Toast.Show(Name + " Click", "", 1500);

        }
    }



    public class BoxFrame
    {
        public Box Frame;
        public BoxFrame(double x, double y, double h, double w, object backColor, string page)
        {
            Frame = new QB.Controls.Box("", x: x, y: y, w: w, h: h)
            {
                BackgroundColor = backColor.ToColor(),
                Directory = page
            };
        }

    }
    public class BoxList
    {

        public double X;
        public double Y;
        public double Width;
        public double RowHeight;
        public string Name;
        public string Text;
        public string Page;

        public double Next = 0;
        public double Gap = 0;

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

        public Dictionary<string, UIBox> Boxes = new Dictionary<string, UIBox>();
        public BoxList(string name, string text, double x, double y, double width, double rowHeight, string page, double gap = 0.2)
        {
            Page = page;
            Gap = gap;
            X = x;
            Y = y;
            Width = width;
            RowHeight = rowHeight;
            Next = Y;

        }

        public void SetVisible()
        {
            UIBox box;
            foreach (KeyValuePair<string, UIBox> entry in Boxes)
            {
                box = (UIBox)entry.Value;
                box.Visible = visible;
            }
        }

        public void AddSpace(int s)
        {
            Next += s;
        }
    }
    public class BoxTable
    {

        public QB.Controls.Box Frame;
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public string Name;
        public double Gap;

        public string Page;
        public int Columns;
        public int Rows;

        public double cellW;
        public double cellH;

       
        private bool visible = true;
        private int zorder = 1;
        public int ZOrder
        {
            get
            {
                return zorder;
            }
            set
            {
                zorder = value;
                SetZOrder();
            }
        }
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
        public System.Drawing.Color BackColor;
        double Border;
        System.Drawing.Color BorderColor;

        string Position = null;

        public Dictionary<string, UIBox> Boxes = new Dictionary<string, UIBox>();
        public BoxTable(string name, double h, double w, int columns, int rows, string page,string position = null, double x = 0, double y = 0, double gap = 0.2, object backColor = null, double border = 0, object borderColor = null)
        {
            Page = page;
            Gap = gap;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Columns = columns;
            Rows = rows;

            if (position != null)
            {
                Position = position.ToLower();
                switch (Position)
                {
                    case "center":
                        {
                          //  QB.Logger.Info("-----------Case center");
                            X = (300 - Width) / 2;
                            Y = (200 - Height) / 2;

                            //QB.Logger.Info("X: " + X);
                            //QB.Logger.Info("Y: " + Y);
                            break;
                        }
                }
            }

            Border = border;
            if (borderColor == null)
                borderColor = "gray";
            BorderColor = borderColor.ToColor();

            if (backColor == null)
                backColor = "transparent";
            BackColor = backColor.ToColor();

            cellW = Width / Columns;
            cellH = Height / Rows;
            create();
        }
        void create()
        {
            Frame = new QB.Controls.Box(Name,text:"", x: X, y: Y, w: Width, h: Height);
            Frame.Directory = Page;
            Frame.BorderBWidth = Border;
            Frame.BorderTWidth = Border;
            Frame.BorderLWidth = Border;
            Frame.BorderRWidth = Border;
            Frame.BorderBColor = BorderColor;
            Frame.BorderTColor = BorderColor;
            Frame.BorderLColor = BorderColor;
            Frame.BorderRColor = BorderColor;
            SetBackColor(BackColor);
            Frame.Clickable = false;
        }
        public void SetVisible()
        {
            Frame.Visible = visible;
            foreach (UIBox box in Boxes.Values)
            {
                box.Visible = visible;

                SetChildVisible();
            }
        }
        public virtual void SetChildVisible()
        {

        }
        public void SetZOrder()
        {
            //Frame.ZOrder = zorder-1;
            foreach (UIBox box in Boxes.Values)
            {
                box.ZOrder = zorder;
            }
        }
        public void SetBackColor(object color)
        {
            BackColor = color.ToColor();
            Frame.BackgroundColor = BackColor;
        }
        public virtual void Add(UIBox box, int column = 1, int row = 1, int columnSpan = 1, int rowSpan = 1)
        {
            box.Page = Page;
            box.X =X + (cellW * (column - 1)) +Gap;
            box.Y =Y + (cellH * (row - 1)) +Gap;
            box.W =cellW * columnSpan - 2 *Gap;
            box.H =cellH * rowSpan - 2 *Gap;
            box.Create();
            Boxes.Add(box.Name, box);
        }

    }
    public class BoxFlowPanel : BoxTable
    {
        public int NextColumn = 0;
        public int NextRow = 0;
        public BoxFlowPanel(string name, double x, double y, double h, double w, int columns, int rows, string page, string position = null, double gap = 0.2, object backColor = null) : 
            base(name:name, x:x, y:y, h:h, w:w, columns:columns, rows:rows, page:page, gap:gap, backColor:backColor,position:position)
        {
        }
        public override void Add(UIBox box, int column = 1, int row = 1, int columnSpan = 1, int rowSpan = 1)
        {
            Debug.WriteLine("Adding to FlowPanel");
            UIBox Box = box;

            Box.Page = Page;
            Box.X = X + (cellW * (NextColumn)) + Gap;
            Box.Y = Y + (cellH * (NextRow)) + Gap;
            Box.W = cellW * columnSpan - 2 * Gap;
            Box.H = cellH - 2 * Gap;

            NextColumn += columnSpan;

            if (NextColumn >= Columns)
            {
                NextRow++;
                NextColumn = 0;
            }

            Box.Create();
            Boxes.Add(Box.Name, Box);

        }
    }

    public class BoxDialog : UIBox
    {
        public string Page;
        string dialogText;
      
        string headerText;
        string OkText = "OK";
        string CancelText = "Cancel";

        //Box.ClickEventHandler OnOk;
        //Box.ClickEventHandler OnCancel;

        BoxButton okButton;
        BoxButton cancelButton;

        public System.Drawing.Color BackColor;
        double Border;
        System.Drawing.Color BorderColor;
        bool? dialogResult = null;
        public bool Cancel = false;
        public bool Ok
        {
            get
            {
                return dialogResult == true;
            }
        }

        string Position;

        public System.Action OkAction { get; set; }
        public System.Action CancelAction { get; set; }

        public BoxDialog(
            string name,
  
            string page, 
            double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, string position = null,
            object backColor = null, 
            double border = 0, 
            object borderColor = null,
            System.Action okAction = null,
            System.Action cancelAction = null
) : base(name)
        {
            OkAction = okAction;
            CancelAction = cancelAction;
            Position = position;
            Border = border;
            if (borderColor == null)
                borderColor = "gray";
            BorderColor = borderColor.ToColor();

            if (backColor == null)
                backColor = "transparent";
            BackColor = backColor.ToColor();

            Page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
            if (position != null)
            {
                switch (position.ToLower())
                {
                    case "center":
                        {
                            X = (300 - W) / 2;
                            Y = (200 - H) / 2;
                            break;
                        }
                    case "left":
                        {
                            X = 0;
                            break;
                        }
                    case "right":
                        {
                            X = 300 - W;
                            break;
                        }
                }
            }   
        }

     

        public override void Create()
        {
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H, style: $"font::{H * 0.3}:b,align:tl", boxes: new[] {

                new QB.Controls.Box("Header", textFunction:() => headerText,x:"1%", y:"2%", w:"98%", h:"14%", style:$"font::{H*0.3}:b,align:tl,bg:transparent"),
                new QB.Controls.Box("Text", textFunction:() => dialogText,x:2, y:"20%", w:W-4, h:"60%", style:$"font::{H*0.2}:b,align:tl,bg:White"),


            new QB.Controls.Box("ok", x:2, y:"82%", w:W/2-4, h:12, style:$"font::{H*0.3}:b,align:mc,bg:#9bcd9b",onClick: (b) => clickOk(), boxes: new[] {
                 new QB.Controls.Box("icon", icon: "fa:check", x:"3%", y:"5%", w:"20%", h:"90%", onClick: (b)=> clickOk()),
                 new QB.Controls.Box("description",  textFunction: () => OkText, x:"25%", y:"12%", w:"70%", h:"80%", onClick: (b)=> clickOk(), style:$"font::{H*0.3},align:ml,bg:Transparent"),
                }),
            new QB.Controls.Box("cancel", x: W/2+2, y:"82%", h:12,w:W/2-4, style:$"font::{H*0.3}:b,align:mc,bg:tomato",onClick: (b) => clickCancel(), boxes: new[] {
                 new QB.Controls.Box("icon", icon: "fa:xmark", x:"3%", y:"5%", w:"20%", h:"90%",onClick: (b) => clickCancel()),
                 new QB.Controls.Box("description",  textFunction: () => CancelText, x:"25%", y:"12%", w:"70%", h:"80%", onClick: (b)=> clickCancel(), style:$"font::{H*0.3},align:ml,bg:Transparent"),
                }),
                    }
                );
     //   okButton.Description.TextAlignment = ContentAlignment.MiddleCenter,
            Frame.Directory = Page;
            Frame.BorderBWidth = Border;
            Frame.BorderTWidth = Border;
            Frame.BorderLWidth = Border;
            Frame.BorderRWidth = Border;
            Frame.BorderBColor = BorderColor;
            Frame.BorderTColor = BorderColor;
            Frame.BorderLColor = BorderColor;
            Frame.BorderRColor = BorderColor;

            Frame.BackgroundColor = BackColor;
            Frame.Visible = false;

            Frame.Boxes[2].BackgroundColorHover = Frame.Boxes[2].BackgroundColor.ChangeBrightness(50);
            Frame.Boxes[3].BackgroundColorHover = Frame.Boxes[3].BackgroundColor.ChangeBrightness(50);

            // Frame.BackgroundColorHover = Frame.BackgroundColor.ChangeBrightness(50);
        }

        bool userDone = false;

        public bool UserDone { get { return userDone; } }

        void clickCancel()
        {
            userDone = true;
            Cancel = true;
            CancelAction?.Invoke();
        }

        void clickOk()
        {
            userDone = true;
            Cancel = false;
            OkAction?.Invoke();
        }


        public async void Show(string header, string text, string okText = "Ok", string cancelText = "Cancel", double h = 80, double w = 150, double fontSize = 12)
        {
            headerText = header;
            dialogText = text;

            int textLineBreaks = Math.Max(1, Regex.Matches(text, @"\r\n|\n|\r").Count + 1);
            int headerLineBreaks = Math.Max(1, Regex.Matches(headerText, @"\r\n|\n|\r").Count + 1);

           

            double headerH = (double)headerLineBreaks * (fontSize * 0.55);
            double textH = (double)textLineBreaks * (fontSize * 0.55);

            //QB.Logger.Info("header " + headerLineBreaks + " -> " + headerH);
            //QB.Logger.Info("text " + textLineBreaks + " -> " + textH);

            OkText = okText;
            CancelText = cancelText;
            dialogResult = null;
            userDone = false;
            
            Frame.ZOrder = 10;
            Visible = true;

          

            Frame.Boxes[0].Left = 2;
            Frame.Boxes[0].Top = 2;
            Frame.Boxes[0].Width = w -4;
            Frame.Boxes[0].Font = new Font("Tahoma", (float)(fontSize *1.2), FontStyle.Bold);
            Frame.Boxes[0].Height = headerH;
            Frame.Boxes[1].Top = headerH + 4;
            Frame.Boxes[1].Left = 2;
            Frame.Boxes[1].Width = w - 4;
            Frame.Boxes[1].Font = new Font("Tahoma", (float)fontSize, FontStyle.Regular);
            Frame.Boxes[1].Height = textH;

            Frame.Boxes[2].Top = 2 + headerH + 4 + textH + 1;
            Frame.Boxes[2].Left = 2;
            Frame.Boxes[2].Width = w / 2 - 4;

            Frame.Boxes[3].Top = 2 + headerH + 4 + textH + 1;
            Frame.Boxes[3].Left = 2 + w / 2;
            Frame.Boxes[3].Width = w / 2 - 4;

            h = 2 + headerH + 4 + textH + 1 + 15;
            Frame.Height = h;
            Frame.Width = w;


            if (Position != null)
            {
                switch (Position.ToLower())
                {
                    case "center":
                        {
                            QB.Logger.Info("Dialog is centered");
                            Frame.Left = (300 - w) / 2;
                            Frame.Top = (200 - h) / 2;
                            QB.Logger.Info("Dialog is centered: " + Frame.Bounds.X + " / " + Frame.Bounds.Y);
                            break;
                        }
                    case "left":
                        {
                            Frame.Left = 0;
                            break;
                        }
                    case "right":
                        {
                            Frame.Left = 300 - W;
                            break;
                        }
                }
            }
           
            if (cancelText == null)
            {
                Frame.Boxes[2].Visible = false;
                Frame.Boxes[2].Clickable = false;
            }

            await Task.Run(async () =>
            {
                while (!userDone)
                {
                    await Task.Delay(50);
                }

                Visible = false;
            });
        }

        public override void SetVisible()
        {
            Frame.Visible = Visible;
            Frame.Boxes[2].Visible = Visible;
            Frame.Boxes[2].Clickable = Visible;
            Frame.Boxes[1].Visible = Visible;
            Frame.Boxes[1].Clickable = Visible;
        }
    }


    public class BoxDialogWithParameters : BoxFlowPanel 
    {

        string headerText;
        Func<string> readHeader;
        string Position;

        double Border;
        Color BorderColor;

        public System.Action OkAction { get; set; }
        public System.Action CancelAction { get; set; }

        public BoxDialogWithParameters(string name, string page,  
            double w,
            double h = 80, 
            int columns = 4,
            int rows = 20,  
            double x = double.NaN, 
            double y = double.NaN, 
            string position = null, 
            double gap = 0.2,
            object backColor = null,
            double border = 0,
            object borderColor = null,
            System.Action okAction = null,
            System.Action cancelAction = null) :
                base(name: name, x: x, y: y, h: h, w: w, columns: columns, rows: rows, page: page, gap: gap, backColor: backColor, position: position)
        {
            Position = position;
            float fh = (float)(h * 0.2);

            OkAction = okAction;
            CancelAction = cancelAction;

            Frame.TextAlignment = ContentAlignment.TopLeft;
            Frame.Font =  new Font("Tahoma", fh, FontStyle.Regular);
            //readHeader = () => headerText;
            //headerText = header;
            //Frame.Text = readHeader();

            Border = border;
            if (borderColor == null)
                borderColor = "gray";
            BorderColor = borderColor.ToColor();

            Frame.BorderBWidth = Border;
            Frame.BorderTWidth = Border;
            Frame.BorderLWidth = Border;
            Frame.BorderRWidth = Border;
            Frame.BorderBColor = BorderColor;
            Frame.BorderTColor = BorderColor;
            Frame.BorderLColor = BorderColor;
            Frame.BorderRColor = BorderColor;


            create();
        }

        BoxButton okButton;
        BoxButton cancelButton;
     

        bool userDone = false;

        public bool UserDone { get { return userDone; } }
        void create()
        {
            Y = Y + cellH + Gap;

            for (int i = 0; i < Rows - 2; i++)
            {
                BoxParameterObject box = new BoxParameterObject(name: "p" + i, locked: false, backColor: BackColor.ChangeBrightness(-20));
                box.X = X + (cellW * (NextColumn)) + Gap + 2;
                box.Y = Y + (cellH * (NextRow)) + Gap;
                box.W = Width - 2 * Gap - 4;
                box.H = cellH - 2 * Gap;
                box.Page = Page;
                NextColumn += Columns;

                if (NextColumn >= Columns)
                {
                    NextRow++;
                    NextColumn = 0;
                }

                box.Create();
               
                int p = i;
            
                Boxes.Add("p" + i,box);
            }
            // NextRow++;

            double bw = Width / 2 - 2 * Gap - 4;
            okButton = new BoxButton("", description: "Übernehmen", icon: "fa:check", x: 2 + X , y: Y + (cellH * (NextRow)), w: bw , h: cellH , backColor: "#9bcd9b", onClick: (b) => clickOk(), page: Page);
            okButton.Description.TextAlignment = ContentAlignment.MiddleCenter;
            okButton.Frame.Visible = false;
            Boxes.Add("ok", okButton);
            cancelButton = new BoxButton("", description: "Abbruch", icon: "fa:xmark", x: X + Width - bw - 2  , y: Y + (cellH * (NextRow)) , w:bw , h: cellH , backColor: "tomato", onClick: (b) => clickCancel(), page: Page);
           
       
            
            cancelButton.Description.TextAlignment = ContentAlignment.MiddleCenter;
            cancelButton.Frame.Visible = false;
            Boxes.Add("cancel", cancelButton);

            Visible = false;
        }
        public bool Cancel = false;



        public override void SetChildVisible()
        {
            base.SetChildVisible();
          //  foreach (BoxParameterObject b in Boxes.Values) b.Visible = Visible;
       
        }
        void clickCancel()
        {
            userDone = true;
            Cancel = true;
            CancelAction?.Invoke();
        }

        void clickOk()
        {
            userDone = true;
            Cancel = false;
            OkAction?.Invoke();
        }


        public Dictionary<object, string> Parameters = new Dictionary<object, string>();


        double RowHeight = 10;
        void Update()
        {
            int rows = Parameters.Count + 3;
            double rowH = RowHeight;
            double h = rowH * rows + 2;
            double y = double.NaN;
            double w = Width;
            Frame.Width = w;

            double x = double.NaN;

            QB.Logger.Info("Postion " + Position);
            if (Position != null)
            {
                switch (Position.ToLower())
                {
                    case "center":
                        {
                            QB.Logger.Info("Dialog is centered");
                            y = (200 - h) / 2;
                            x = (300 - w) / 2;
                            Frame.Left = x;
                            Frame.Top = y;
                            QB.Logger.Info("Dialog is centered: " + Frame.Bounds.X + " / " + Frame.Bounds.Y);
                            break;
                        }
                    case "left":
                        {
                            Frame.Left = 0;
                            break;
                        }
                    case "right":
                        {
                            Frame.Left = 300 - Width;
                            break;
                        }
                }
            }

            int index = 2;

            Frame.Height = rowH * rows + 4;

            double rY = y;

            QB.Logger.Info("Frame Y: " + rY );

            foreach (UIBox b in Boxes.Values)
            {
                if (b.Name.StartsWith("p"))
                {
                //    QB.Logger.Log(Name + " hide Parameter " + b.Name);
                    BoxParameterObject box = b as BoxParameterObject;
                    box.Visible = false;
                }
      
            }
            int i = 0;

            float fh = (float)(RowHeight);

        
            foreach (var p in Parameters)
            {
                string key = "p" + i;
                ((BoxParameterObject)Boxes[key]).SetParameter(p.Key, p.Value);
                ((BoxParameterObject)Boxes[key]).Visible = true;
                ((BoxParameterObject)Boxes[key]).Frame.Height =rowH - 2 * Gap;
                ((BoxParameterObject)Boxes[key]).Frame.Width = w - 4;
                ((BoxParameterObject)Boxes[key]).Frame.Left = x + 2;
                ((BoxParameterObject)Boxes[key]).Frame.Top = rY + index * rowH + Gap;
                ((BoxParameterObject)Boxes[key]).ParameterName.Font = new Font("Tahoma", fh, FontStyle.Regular);
                ((BoxParameterObject)Boxes[key]).ParameterRead.Font = new Font("Tahoma", fh, FontStyle.Regular);
       //         ((BoxParameterObject)Boxes[key]).ParameterReadString.Font = new Font("Tahoma", fh, FontStyle.Regular);
                ((BoxParameterObject)Boxes[key]).ParameterText.Font = new Font("Tahoma", fh, FontStyle.Regular);
                ((BoxParameterObject)Boxes[key]).ParameterUnit.Font = new Font("Tahoma", fh, FontStyle.Regular);
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[3].Top = rowH * 0.25;
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[3].Height = rowH * 0.5;
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[3].Width = rowH * 0.5;
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[4].Top = rowH * 0.25;
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[4].Height = rowH * 0.5;

                ((BoxParameterObject)Boxes[key]).ParameterRead.Clickable = true;
               ((BoxParameterObject)Boxes[key]).Frame.Boxes[3].Clickable = true;
                ((BoxParameterObject)Boxes[key]).Frame.Boxes[4].Clickable = true;
                 index++;
                i++;
            }
            okButton.Frame.Top = rY + index * rowH + Gap + 2;
            okButton.Frame.Left = x + 2;
            okButton.Frame.Width = w/2 - 4;
            okButton.Frame.Height = rowH;
            okButton.State.Height = rowH * 0.8;
            okButton.State.Width  = rowH * 0.8;
            okButton.State.Top = rowH * 0.1;
            okButton.Description.Height = rowH * 0.8;
            okButton.Description.Left = rowH * 0.8;
            okButton.Description.Font = new Font("Tahoma", fh, FontStyle.Regular);
            okButton.Description.Top = rowH * 0.1;

            cancelButton.Frame.Top = rY + index * rowH + Gap + 2;
            cancelButton.Frame.Width = w/2 - 4;
            cancelButton.Frame.Left = x + w / 2 + 2;

            cancelButton.Frame.Height = rowH;
            cancelButton.State.Height = rowH * 0.8;
            cancelButton.State.Width = rowH * 0.8;
            cancelButton.State.Top = rowH * 0.1;
            cancelButton.Description.Height = rowH * 0.8;
            cancelButton.Description.Left = rowH * 0.8;
            cancelButton.Description.Font = new Font("Tahoma", fh, FontStyle.Regular);
            cancelButton.Description.Top = rowH * 0.1;
         //   cancelButton.Description.TextAlignment = ContentAlignment.MiddleLeft;

            //okButton.Frame.Height = rowH;
            //cancelButton.Frame.Top = rY + index * rowH + Gap + 2;
            //cancelButton.Frame.Height = rowH;
        }

        public async void Show(string header, double rowHeight = 10, double width = 100, System.Action okAction = null, System.Action cancelAction = null)
        {
            OkAction = okAction;
            CancelAction = cancelAction;

            
            userDone = false;
            RowHeight = rowHeight;

         
            Frame.Text = header;
            Width = width;

 
            Visible = true;
            okButton.ZOrder = 10;
            okButton.Frame.Clickable = true;
            cancelButton.ZOrder = 10;
            cancelButton.Frame.Clickable = true;
            Update();

            await Task.Run(async () =>
            {
                while (!userDone)
                {
                    await Task.Delay(50);
                }

                
                Visible = false;
                okButton.ZOrder = -1;
     
                okButton.Frame.Clickable = false;
                cancelButton.ZOrder = -1;
                cancelButton.Frame.Clickable = false;
            });
        }

    }


    public class BoxSignal : UIBox
    {

        QB.Controls.Box Read;
        string page;
        public Signal Target = new Signal("dummy", value: 1000, unit: "ddd");
        public double Min;
        public double Max;
        bool Locked = false;
        string Format;
        string TextPosition;

        public BoxSignal(string name, Signal target = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, object valueColor = null, double min = double.NaN, double max = double.NaN, bool locked = false, string format = "0.000",string textPosition = "tl") : base(name)
        {
            Min = min;
            Max = max;
            Format = format;
            if (target != null)
                Target = target;

            if (backColor == null)
                backColor = "lightgray";

            Valuecolor = System.Drawing.Color.Transparent;
            if (valueColor != null)
                Valuecolor = valueColor.ToColor();

            Backcolor = backColor.ToColor();
          
            Locked = locked;

            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
            TextPosition = textPosition;
        }

        public override void Create()
        {
         
            if (TextPosition.ToLower() == "tl" || TextPosition.ToLower() == "lt")
            {

                Frame = new Box(Name, text: "", x: X, y: Y, w: W, h: H
                    , boxes: new[] {

                Read = new QB.Controls.Box("read", textFunction:() => Target.Value.ToString(Format), y:"32%", w:"64%", h:"40%",
                    onClick:(e) => Target.ShowEditDialog(),
                    style:$"font::{H*1}:b,align:mr,bg:Transparent"),
                new QB.Controls.Box("unit", textFunction:() => Target.Unit, x:"65%", y:"35%", w:"35%", h:"40%", style:$"font::{H*0.8}:i,align:ml,bg:Transparent"),
                new QB.Controls.Box(Name, textFunction:() => Target.Text, x:0, y:0, h:"100%", style:$"font::{H*0.6}:,align:tl,bg:Transparent"),
                    }
                );
                Frame.Directory = Page;
                Read.BackgroundColor = Valuecolor;

            }

            if (TextPosition.ToLower() == "ml" || TextPosition.ToLower() == "lm")
            {

                Frame = new Box(Name, text: "", x: X, y: Y, w: W, h: H
                    , boxes: new[] {
                new QB.Controls.Box(Name, textFunction:() => Target.Text, x:1.5, y:"10%", w:"30%", h:"80%", style:$"font::{H}:,align:ml,bg:Transparent"),
                Read = new QB.Controls.Box("read", textFunction:() => Target.Value.ToString(Format), x:"30%", y:"10%", w:"50%", h:"80%",
                    onClick:(e) => Target.ShowEditDialog(),
                    style:$"font::{H*1}:b,align:mr,bg:Transparent"),
                
              
                new QB.Controls.Box("unit", textFunction:() => Target.Unit, x:"72%", y : "10%", w : "27%", h : "80%", style:$"font::{H}:i,align:ml,bg:Transparent")
                    }
                );
                Frame.Directory = Page;
                Read.BackgroundColor = Valuecolor;

            }

            if (Locked) Read.Clickable = false;
            SetBackColor(Backcolor);
        }

        public override void SetTarget(object signal)
        {
            if (signal == null)
            {
                QB.Logger.Error(Text + " set target failed is null");
                return;
            }
            try
            {
                Target = signal as Signal;
            }
            catch
            {
                QB.Logger.Error(Text + " set target failed set");
            }
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            //Read.BackgroundColorHover = Target.AllowEdit? ChangeColorBrightness(Backcolor, 50) : Backcolor;
        }

        public override void SetVisible()
        {
            Read.Clickable = Visible;

            
 
        }

    
    }
    public class BoxModule : UIBox
    {

        QB.Controls.Box Read;
        QB.Controls.Box Set;
        QB.Controls.Box Out;
        string page;

        Module Target = new Module("dummy", value: 1000, unit: "ddd");
        public double Min;
        public double Max;
        double FontSize;

        bool Locked = false;
        string Format;

        public BoxModule(string name, Module target = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, double min = double.NaN, double max = double.NaN, bool locked = false, string format = "0.000") : base(name)
        {
            Min = min;
            Max = max;
            Format = format;
            if (target != null)
                Target = target;

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Locked = locked;
            this.page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }

        public override void Create()
        {
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H
                , boxes: new[] {

                Read = new QB.Controls.Box("read", textFunction:() => Target.Value.ToString(Format), y:"32%", w:"64%", h:"40%",
                    onClick:(e) => Target.Set.ShowEditDialog(),
                    style:$"font::{H*1.1},align:mr,bg:Transparent"),
                 new QB.Controls.Box("unit", textFunction:() => Target.Unit, x:"65%", y:"32%", w:"35%", h:"40%", style:$"font::{H}:i,align:ml,bg:Transparent"),
                 new QB.Controls.Box(Name, textFunction:() => Target.Text, x:0, y:0, h:"100%", style:$"font::{H*0.8}:b:,align:tl,bg:Transparent"),
                // new QB.Controls.Box(Name, text:"Set: ", x:"66%", y:"58%", w:"15%", h:"40%", style:$"font::{H*0.5}:i:,align:bl,bg:Transparent"),
				// Set = new QB.Controls.Box(Name, text:() => Target.Set.Value.ToString(Format), editMask:"", x:"76%", y:"59%", w:"23%", h:"40%", onAccept:(e) => saveSet(), style:$"font::{H*0.5}:i:,align:bl,bg:Transparent"),
					Set = new QB.Controls.Box(Name, textFunction:() =>"Set: " + Target.Set.Value.ToString(Format), x:"50%", y:"75%", w:"50%", h:"25%", onClick:(e) => Target.Set.ShowEditDialog(min:Min, max:Max), style:$"font::{H*0.5}:i:,align:bl,bg:Transparent"),
              //   new QB.Controls.Box(Name, text:"Out: ", x:"0%", y:"58%", w:"10%", h:"40%", style:$"font::{H*0.5}:i:,align:bl"),
                 Out = new QB.Controls.Box(Name, textFunction:() =>"Out: " + Target.Out.Value.ToString("0.00"), x:"0%", y:"73%", w:"50%", h:"25%", style:$"font::{H*0.5}:i:,align:bl,bg:Transparent", onClick:(e) => Target.Out.ShowEditDialog()),
                }
            );

            Frame.Directory = Page;

            if (Locked) Read.Clickable = false;
            SetBackColor(Backcolor);
            //Read.EditMask = null;
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;

            //		Read.BackgroundColorHover = Target.AllowEdit? Backcolor.ChangeBrightness(50) : Backcolor;
            //		Set.BackgroundColorHover = Target.Set.AllowEdit? Backcolor.ChangeBrightness(50) : Backcolor;
            //		Out.BackgroundColorHover = Target.Out.AllowEdit? Backcolor.ChangeBrightness(50) : Backcolor;

        }

        public override void SetTarget(object signal)
        {
            if (signal == null)
            {
                QB.Logger.Error(Text + " set target failed is null");
                return;
            }
            try
            {
                Target = signal as Module;
            }
            catch
            {
                QB.Logger.Error(Text + " set target failed set");
            }
        }

        public override void SetVisible()
        {
            Read.Clickable = Visible;

        }
    }

    public class BoxLabel : UIBox
    {

        QB.Controls.Box Read;
        string page;
        public StringSignal Target = new StringSignal("demo", "Demo", "Value");
        bool Locked = false;

        string Label;
        double FontSize;
        string Alignment;
        public BoxLabel(string name, string text = null,double fontSize = double.NaN,string alignment = "tl", double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, bool locked = false) : base(name)
        {

            Label = text;

          
           

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Locked = locked;
            Page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;

            FontSize = double.IsNaN(fontSize) ? H : fontSize;
            Alignment = alignment;
        }

        public override void Create()
        {
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H
                , boxes: new[] {
                new QB.Controls.Box(Name, text:Label, x:0, y:0, w:"100%", h:"100%", format: $"bg:#e6e6e6,align:ml", style:$"font::{FontSize}:,align:tl,bg-hover:Transparent")
                {
                    BackgroundColorHover = Backcolor, BackgroundColor = Backcolor
                }
                }
             
            );
            Frame.Directory = Page;
            SetBackColor(Backcolor);

            switch (Alignment)
            {
                case "tl":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.TopLeft;
                    break;
                case "tr":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.TopRight;
                    break;
                case "ml":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.MiddleLeft;
                    break;
                case "mr":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.MiddleRight;
                    break;
                case "bl":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.BottomLeft;
                    break;
                case "br":
                    Frame.Boxes[0].TextAlignment = ContentAlignment.BottomRight;
                    break;

                default:
                    Frame.Boxes[0].TextAlignment = ContentAlignment.TopLeft;
                    break;
            }

            }
        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
        }



    }
    public class BoxStringSignal : UIBox
    {

        QB.Controls.Box Read;
        string page;
        public StringSignal Target = new StringSignal("demo", "Demo", "Value");
        bool Locked = false;
        public BoxStringSignal(string name, StringSignal target = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, bool locked = false) : base(name)
        {

            if (target != null)
                Target = target;

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Locked = locked;
            this.page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }

        public override void Create()
        {
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H
                , boxes: new[] {
                new QB.Controls.Box(Name, textFunction:() => Target.Text, x:0, y:0, w:"100%", h:"100%", format: $"bg:#e6e6e6,align:ml", style:$"font::{H*0.7}:,align:tl,bg-hover:Transparent") {
                    BackgroundColorHover = Backcolor, BackgroundColor = Backcolor
                },
                Read = new QB.Controls.Box("read", textFunction:() => Target.Value.ToString(), x:0, y:"27%", w:"90%", h:"73%",
                    onClick:(e) => edit(),
                    onAccept:(e) => saveValue(),
                    onCancel:(e) => undo(),
                    style:$"font::{H*0.9}:b:bg:Transparent,align:ml,bg-hover:Transparent"),


                }
            );
            Frame.Directory = Page;


            if (Locked)
            {
                Read.Clickable = false;
                Read.BackgroundColorHover = Backcolor;
                Read.BackgroundColor = Backcolor;

            }

            SetBackColor(Backcolor);

        }

        void edit()
        {
            if (Locked) return;
            string input = Target.Value.ToString();
            Extensions.ShowEditTextDialog(ref input, text: Target.Text);
            Target.Value = input;

        }

        void undo()
        {
            if (Locked) return;
            Read.Text = (Func<string>)(() => Target.Value);
        }

        void saveValue()
        {
            if (Locked) return;
            Target.Value = Read.Text.ToString();
            Read.Text = (Func<string>)(() => Target.Value);
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            Read.BackgroundColorHover = !Locked ? Backcolor.ChangeBrightness(50) : Backcolor;
        }


        public void EditText(string header, string value)
        {
            Target.Text = header;
            Target.Value = value;
        }

        public override void SetVisible()
        {
            Read.Clickable = Visible;
            Frame.Clickable = Visible;

        }

    }
    public class BoxLabelMultiLine : UIBox
    {

        public QB.Controls.Box Headline;
        public QB.Controls.Box Content;

        string BoxName;
        string HeadlineText;
        string ContentText;
        public BoxLabelMultiLine(string name, string headline, string content, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, bool locked = false) : base(name)
        {
            BoxName = name;
            HeadlineText = headline;
            ContentText  = content;

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Page = Page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }
        public override void Create()
        {
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H
                , boxes: new[] {
                new QB.Controls.Box(Name, text:BoxName, x:0, y:0, w:"100%", h:"100%", format: $"bg:#e6e6e6,align:ml", style:$"font::{H*0.66*0.7}:,align:tl,bg-hover:Transparent") {
                    BackgroundColorHover = Backcolor, BackgroundColor = Backcolor
                },
                Headline = new QB.Controls.Box("headline", text:HeadlineText, x:1.5, y:"22%", w:"97%", h:"35%", style:$"font::{H*0.66}:b:bg:Transparent,align:tl,bg-hover:Transparent"),
                Content = new QB.Controls.Box("content", text:ContentText, x:1.5, y:"60%", w:"97%", h:"90%", style:$"font::{H*0.35}:b:bg:Transparent,align:tl,bg-hover:Transparent"),
                }
            );
            Frame.Directory = Page;

            SetBackColor(Backcolor);
        }
        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
        }


        public override void SetVisible()
        {
            Headline.Clickable = Visible;
            Content.Clickable = Visible;
            Frame.Clickable = Visible;
        }


    }
    public class BoxSignalOnOff : UIBox
    {

        public QB.Controls.Box On;
        public QB.Controls.Box Off;
        bool Locked = false;
        string page;
        Signal Target;
        bool LedStyle = true;

        public BoxSignalOnOff(string name, Signal target, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, object defaultColor = null, object selectColor = null, bool locked = false, bool ledStyle = false) : base(name)
        {
            Target = target;

            if (backColor == null)
                backColor = "lightgray";

            if (defaultColor == null)
                defaultColor = "Transparent";

            if (selectColor == null)
                selectColor = "Lightblue";

            Backcolor = backColor.ToColor();
            DefaultColor = defaultColor.ToColor();
            SelectColor = selectColor.ToColor();
            Locked = locked;
            Page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
            LedStyle = ledStyle;
            //	Create();
        }

        public override void Create()
        {

            if (!LedStyle)
            {
                double bF = W * 0.12;
                Frame = new QB.Controls.Box(Name, text: Target.Text, x: X, y: Y, w: W, h: H, format: $"font::{H * 0.7}:,align:tl,bg-hover:Transparent"
                    , boxes: new[] {
                On = new QB.Controls.Box("read", text:"ON", x:"70%", y:"5%", w:"15%", h:"90%", style:$"font::{bF}:b:bg:#e6e6e6,border:1", onClick:(s) => set(1)),
                Off = new QB.Controls.Box("set", text:"OFF", x:"85%", y:"5%", w:"15%", h:"90%", style:$"font::{bF}:b:bg:#e6e6e6,border:1", onClick:(s) => set(0)),
                    }
                );
            }
            else
            {
                double ledSize = 2.5;
                double ly = (H - ledSize) / 2;
                
                Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H, 
                    boxes: new[] {
                    new QB.Controls.Box("text", x:1.5, text: Target.Text, w:W-5, style: $"font::{H}:,align:ml,bg-hover:Transparent"),
                    new QB.Controls.Box("led", x:W - 5, y:ly, w:ledSize, h:ledSize, style: $"bg:transparent,border:Black:1",onClick:(s) => toggle())
                    }
                 );
            }

            Frame.Directory = Page;
           
            Frame.BackgroundColor = Backcolor;
            Frame.BackgroundColorHover = Backcolor;
            SetBackColor(Backcolor);
            Update();
        }

        public override void SetVisible()
        {
            Frame.Boxes[1].Clickable = Visible;
            

        }

        void toggle()
        {
            Target.Value = Target.Value==1 ? 0 : 1;
            Update();
           

        }

        void set(double s)
        {
            Target.Value = s;
            Update();
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;

        }
        public void Update()
        {
            if (!LedStyle)
            {
                if (Locked)
                {
                    On.Clickable = false;
                    Off.Clickable = false;
                }

                if (Target.Value == 1)
                {
                    On.BackgroundColor = SelectColor;
                    On.BackgroundColorHover = SelectColor;
                    Off.BackgroundColor = DefaultColor;
                }
                else
                {
                    On.BackgroundColor = DefaultColor;
                    Off.BackgroundColor = SelectColor;
                    Off.BackgroundColorHover = SelectColor;
                }
            }
            else
            {

                if (Locked)
                {
                    Frame.Boxes[1].Clickable = false;
                }

                Frame.Boxes[1].BackgroundColor = Target.Value == 1 ? System.Drawing.Color.Orange : System.Drawing.Color.Transparent;

            }
        }
    }
    public class BoxRadioSignal : UIBox
    {
        Signal Target;

        public List<Signal> ChannelList = new List<Signal>();
        public QB.Controls.Box[] Channels;

        bool LedStyle = false;
        string LedPosition;
        public BoxRadioSignal(string name, Signal target, List<Signal> channelList, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, object selectColor = null, object defaultColor = null, bool ledStyle = false, string ledPosition = "tl") : base(name)
        {
           
            Target = target;
            if (backColor == null)
                backColor = "lightgray";

            if (selectColor == null)
                selectColor = "LightBlue";

            if (defaultColor == null)
                defaultColor = "gainsboro";
            DefaultColor = defaultColor.ToColor();
            SelectColor = selectColor.ToColor();
            Backcolor = backColor.ToColor();
            ChannelList = channelList;

            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;

            LedStyle = ledStyle;
            LedPosition = ledPosition;
        }

        public override void SetTarget(object signal)
        {
            if (signal == null)
            {
                QB.Logger.Error(Text + " set target failed is null");
                return;
            }
            try
            {
                Target = signal as Signal;
                Update();
            }
            catch
            {
                QB.Logger.Error(Text + " set target failed set");
            }
        }

        public override void Create()
        {
            Channels = new QB.Controls.Box[ChannelList.Count() + 1];
            double bw = (double)96 / ChannelList.Count().ToDouble();

            double sx = 2;  // Offset from Left

            Channels[0] = new QB.Controls.Box(Name, text: Target.Text, x: 0, y: 0, w: W, h: "40%", format: $"bg:#e6e6e6,align:ml", style: $"font::{H * 0.8}::,align:tl")
            {
                BackgroundColorHover = Backcolor,
                BackgroundColor = Backcolor
            };
            int c = 0;
            foreach (Signal channel in ChannelList)
            {
                c++;

                if (!LedStyle)
                {
                    Channels[c] = new QB.Controls.Box(channel.Name, text: channel.Text, x: sx + "%", y: "5%", w: bw + "%", h: "90%", style: $"font::{H * 0.7}:bg:#e6e6e6,border:1", onClick: (s) => SetValue(channel.Value))
                    {
                        Directory = Page,
                    };
                }
                else
                {

                    double lx = 0.5;            //led X
                    double ly = 0.5;            //led Y
                    double ledSize = 2.5;       //led size square
                    double ledOffset = 0.5;     //defaule offset

                    double cw = W * (bw / 100);   //channel button width
                    double ch = H * 0.9;        //channel button width

                    if (LedPosition.ToLower() == "tl" || LedPosition.ToLower() == "lt")
                    {
                        lx = ledOffset;
                        ly = ledOffset;
                    }

                    if (LedPosition.ToLower() == "tr" || LedPosition.ToLower() == "rt")
                    {
                        lx = cw - ledOffset - ledSize;
                        ly = ledOffset;
                    }

                    if (LedPosition.ToLower() == "ml" || LedPosition.ToLower() == "lm")
                    {
                        
                        lx =  1.5 ;
                        ly = (H * 0.6 / 2) -ledSize / 2;
                    }

                    if (LedPosition.ToLower() == "mr" || LedPosition.ToLower() == "rm")
                    {
                        lx = cw - 0.5 - ledSize;
                        ly = (H * 0.6 / 2) - ledSize / 2;
                    }


                    Channels[c] = new QB.Controls.Box(channel.Name, x: sx + "%", y: "40%", w: cw, h: "60%", style: $"bg:transparent", onClick: (s) => SetValue(channel.Value),
                         boxes: new[] {
                            new QB.Controls.Box("text", text: channel.Text, x:5,y:"2%", style: $"font::{H * 0.6}:b,bg:transparent,align:ml"),
                            new QB.Controls.Box("led", x:lx, y:ly, w:ledSize, h:ledSize,style: $"bg:transparent,border:Black:1")
                         })
                    {
                        Directory = Page,
                        
                    };
                }

                sx += bw;
            }
            Frame = new QB.Controls.Box(Name, text: "", x: X, y: Y, w: W, h: H, format: $"bg:#e6e6e6,align:ml,bg-hover:#e6e6e6",
                boxes: Channels)
            {
                BackgroundColor = Backcolor,
                BackgroundColorHover = Backcolor,

            };
            Update();
        }

        public void SetValue(double s)
        {
            Target.Value = s;
            Update();
        }

        public void Update()
        {

            List<string> buttons = new List<string>();
            foreach (Signal channel in ChannelList)
            {
                if (Target.Value == channel.Value)
                {
                    buttons.Add(channel.Name);
                }
            }
            int c = 0;
            foreach (QB.Controls.Box button in Channels)
            {

                if (!LedStyle)
                {
                    if (c != 0)
                    {
                        button.BackgroundColor = buttons.Contains(button.Name) ? SelectColor : DefaultColor;
                        button.BackgroundColorHover = buttons.Contains(button.Name) ? SelectColor.ChangeBrightness(50) : DefaultColor.ChangeBrightness(50);
                    }
                }
                else
                {
                    if (c != 0)
                    {
                           button.Boxes[1].BackgroundColor = buttons.Contains(button.Name) ? SelectColor : System.Drawing.Color.Transparent;
                    }

                }

                c++;
            }
        }

        public override void SetVisible()
        {
           Frame.Visible = Visible;
        }
    }
    public class BoxButton : UIBox
    {

        protected QB.Controls.Box Read;

        public string Icon;
        string page;
       // string DescriptionText;
        double IconSizeFactor;

        string Id;
        bool Locked = false;
        string Format;

        public Box.ClickEventHandler OnClick;
 
        bool LedStyle = false;

        Func<string> DescriptionText;

      
        public BoxButton(string name, string id = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object description = null, object backColor = null, bool locked = false, string icon = null, double iconSizeFactor = 0.6, QB.Controls.Box.ClickEventHandler onClick = null, string page = null, bool ledStyle = false) : base(name)
        {

            OnClick = onClick;
            Icon = icon;
            IconSizeFactor = iconSizeFactor;

            Id = id == null ? name : id;

            if(description is null)
                DescriptionText = null;
            else
            {
                if (description is Func<string>)
                    DescriptionText = description as Func<string>;
                else
                    DescriptionText = () => description.ToString();
            }

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Locked = locked;

            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;

            X = x;
            Y = y;
            W = w;
            H = h;

            if (page != null)
            {
                Page = page;
                Create();
            }

            LedStyle = ledStyle;
        }


        public override void SetZOrder()
        {
            Frame.ZOrder = ZOrder;
        }

        public override void Create()
        {

            double aligment = W < H ? W : H;
            double iconSize = aligment * IconSizeFactor;

            if (!LedStyle)
            {
                if (DescriptionText != null)
                {
                    Frame = new QB.Controls.Box(Name, text: Id, x: X, y: Y, w: W, h: H, style: $"font::{aligment * 0.5},align:tl,bg:Transparent",
                        onClick: OnClick,
                        boxes: new[] {
                    State = new QB.Controls.Box("icon", x:aligment * 0.2, y:(H - iconSize) / 2, w:iconSize, h:iconSize),
                    Description = new QB.Controls.Box("description", textFunction:DescriptionText, x:H * 0.9, y:(H - iconSize) / 2, w:W - H -2, h:H - (H - iconSize) / 2, onClick:OnClick, style:$"font::{H},align:ml,bg:Transparent"),
                        });
                    Description.OnWheel += (box, delta) => MouseWheel(box, delta);

                }

                if (DescriptionText == null)
                {
                    Frame = new QB.Controls.Box(Name, text: Id, x: X, y: Y, w: W, h: H, style: $"font::{H * 0.5},align:tl,bg:Transparent", onClick: OnClick, boxes: new[] {
                    State = new QB.Controls.Box("icon",text:"", x:0, y:H * 0.2, w:W, h:H * 0.6, onClick : OnClick),
                });
                }
            }
            else
            {
                if (DescriptionText != null)
                {
                    Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H, style: $"font::{aligment * 0.5},align:tl,bg:Transparent",
                        onClick: OnClick,
                        boxes: new[] {
                            Led = new QB.Controls.Box("led", x:0.5, y:0.5, w:2.5, h:2.5,style: $"bg:transparent,border::Black:1", onClick : OnClick),
                            State = new QB.Controls.Box("icon", x:aligment * 0.2, y:(H - iconSize) / 2, w:iconSize, h:iconSize,onClick: OnClick),
                            Description = new QB.Controls.Box("description", textFunction:DescriptionText, x:H * 0.9, y:(H - iconSize) / 2, w:W - H -2, h:H - (H - iconSize) / 2, onClick:OnClick, style:$"font::{H},align:ml,bg:Transparent"),
                        });
                    Description.OnWheel += (box, delta) => MouseWheel(box, delta);

                }

                if (DescriptionText == null)
                {
                    Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H, style: $"font::{H * 0.5},align:tl,bg:Transparent", 
                        onClick: OnClick, 
                        boxes: new[] {
                            Led = new QB.Controls.Box("led", x:0.5, y:0.5, w:2.5, h:2.5,style: $"bg:transparent,border:Black:1",onClick: OnClick),
                            State = new QB.Controls.Box("icon",text:"", x:0, y:H * 0.2, w:W, h:H * 0.6,onClick: OnClick),
                        });
                }

            }

            Frame.Directory = Page;
            State.Icon = Icon;
            Frame.OnClick += (e) => Click();

            if (Locked) Frame.Clickable = false;
            SetBackColor(Backcolor);
        }

        public override void SetSelectColor(object color)
        {
            if (LedStyle)
            {
                Led.BackgroundColor = color.ToColor();
            }

            else
            {
                Frame.BackgroundColor = color.ToColor();
                Frame.BackgroundColorHover = Frame.BackgroundColor.ChangeBrightness(50);
            }
        }

        public override void SetVisible()
        {
        
            if (Frame != null)
            {
                Frame.Visible = Visible;
                Frame.Clickable = Visible;
            }
            if (Description != null)
            {
                Description.Visible = Visible;
                Description.Clickable = Visible;
            }
            if (Led != null)
            {
                Led.Visible = Visible;
                Led.Clickable = Visible;
            }

            if (State != null)
            {
                State.Visible = Visible;
                State.Clickable = Visible;
            }
        }

        public override void SetLedColor(object color)
        {
            if(Led == null)
            {
                QB.Logger.Error("BoxButton '" + Name + "' Led is null");
                return;
            }
            Led.BackgroundColor = color.ToColor();
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            Frame.BackgroundColorHover = !Locked ? Backcolor.ChangeBrightness(50) : Backcolor;
            Frame.BackgroundColorHover = !Locked ? Backcolor.ChangeBrightness(50) : Backcolor;


        }

        public void Enable(bool set = true)
        {
            Frame.ZOrder = 10;
                Frame.Clickable = set;
            if(Description != null) 
                Description.Clickable = set;
               
        
        }

        

    }
    public class BoxTableView
    {

        public QB.Controls.Box Navigation;
        public QB.Controls.Box Up;
        public QB.Controls.Box Down;
        public QB.Controls.Box Left;
        public QB.Controls.Box Right;

        double X;
        double Y;
        double Width;
        double Height;
        public string Name;
        double Gap;
        string Page;
        int Rows;
        int NextColumn = 0;
        int NextRow = 0;
        int range;

        public System.Drawing.Color BackColor;

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


        double rowHeight;
        string page;
        string ColumnConfig;
        string HeaderText;
        public Table Target;
        int lastViewRow;
        int LastViewRow;
        int FirstViewRow;
        List<string> view = new List<string>();
        BoxTableCell readCell;
        public Dictionary<string, UIBox> Cell = new Dictionary<string, UIBox>();

        public BoxTableView(string name, Table target, double x, double y, double h, double w, string columnConfig, int rows, string page, double gap = 0.2, object backColor = null, string headerText = null)
        {

            Page = page;
            Gap = gap;
            X = x;
            Y = y + Gap;
            Width = w - 5;
            Height = h - 2 * Gap;

            Rows = rows;
            ColumnConfig = columnConfig;
            HeaderText = headerText;


            Target = target;
            range = ColumnConfig.Split(',').ToList().Count() * Rows;
            FirstViewRow = 1;
            LastViewRow = Rows;

            if (HeaderText != null)
                Rows++;

            rowHeight = Height / Rows;

            //Target = target;
            if (backColor == null)
                backColor = "lightgray";
            BackColor = backColor.ToColor();

            Navigation = new QB.Controls.Box("Navi",text:"", x: X + Width, y: Y, w: 5, h: Height, style: $"font::12:b,align:tl,bg:red,bg:lightgray,border:black:0.5", boxes: new[] {
                Up = new QB.Controls.Box("icon",text:"", h:"20%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-up", onClick:(b) => moveUp()),
                Down = new QB.Controls.Box("icon",text:"", y:"80%", h:"20%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-down", onClick:(b) => moveDown())
            }
            );

            Navigation.BackgroundColor = BackColor;
            Navigation.Directory = Page;
            Navigation.OnWheel += (box, delta) => MouseWheel(box, delta);

            Up.Directory = Page;
            Up.Clickable = true;
            Down.Directory = Page;
            Down.Clickable = true;
            Up.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);
            Down.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);

            create();
        }

        void create()
        {

            double cellH = rowHeight;
            int columns = ColumnConfig.Split(',').ToList().Count();
            ColumnConfig = ColumnConfig.Replace("%", "");

            List<string> cfg = ColumnConfig.Split(',').ToList();
            List<double> cellW = new List<double>();

            foreach (string i in cfg)
                cellW.Add(i.ToDouble() / 100 * Width);

            double x = X;
            double y = 0;
            double w = 0;
            double h = 0;


            if (HeaderText != null)
            {
                int htext = 0;
                y = Y + (cellH * NextRow);

                h = cellH;
                List<string> headerlist = HeaderText.Split(',').ToList();
                //QB.Logger.Info("headers: " + headerlist.Count());

                foreach (string t in headerlist)
                {
                    w = cellW[htext];
                    //QB.Logger.Info("Cell: " + NextRow + " / " + NextColumn);
                    Cell.Add($"0,{htext}", new BoxTableCell($"header", Target, x, y, w, h, page: Page, cellR: NextRow, cellC: NextColumn, view: this, backColor: BackColor.ChangeBrightness(-20), locked: true)
                    {
                        Page = Page
                    });
                    Cell[$"{NextRow},{NextColumn}"].Create();

                    Cell[$"{NextRow},{NextColumn}"].Read.Text = t;

                    htext++;
                    NextColumn++;
                    x = x + w;
                }
            }

            NextRow++;

            NextColumn = 0;
            x = X;

            for (int i = 0; i < range; i++)
            {

                y = Y + (cellH * NextRow);
                w = cellW[NextColumn];

                h = cellH;

                Cell.Add($"{NextRow},{NextColumn}", new BoxTableCell($"{NextRow},{NextColumn}", Target, x, y, w, h, page: Page, cellR: NextRow, cellC: NextColumn + 1, view: this)
                {
                    Page = Page
                });
                Cell[$"{NextRow},{NextColumn}"].Create();
                Cell[$"{NextRow},{NextColumn}"].SetBackColor(BackColor);

                ;
                NextColumn++;
                x = x + w;
                if (NextColumn >= columns)
                {
                    NextRow++;
                    NextColumn = 0;
                    x = X;
                }
            }
        }
        public void MouseWheel(Control c, int delta)
        {

            if (delta == 1)
                moveUp();
            if (delta == -1)
                moveDown();

        }
        public void Update()
        {
            foreach (KeyValuePair<string, UIBox> entry in Cell)
            {

                readCell = (BoxTableCell)entry.Value;
                if (readCell.Name != "header")
                {
                    readCell.Target = Target;
                    readCell.Update();
                }
            }
            
        }

        void moveUp()
        {


            if (FirstViewRow == 1)
                return;

            FirstViewRow--;
            LastViewRow--;

            foreach (KeyValuePair<string, UIBox> entry in Cell)
            {
                readCell = (BoxTableCell)entry.Value;
                if (readCell.Name != "header")
                {
                    readCell.CellR--;
                    readCell.Update();
                }
            }
        }
        void moveDown()
        {

            if (LastViewRow == Target.RowCount) return;

            FirstViewRow++;
            LastViewRow++;

            foreach (KeyValuePair<string, UIBox> entry in Cell)
            {
                readCell = (BoxTableCell)entry.Value;
                if (readCell.Name != "header")
                {
                    readCell.Read.Text = "";

                    if (readCell.CellR < Target.RowCount && readCell.CellC <= Target.ColCount)
                    {
                        readCell.CellR++;
                        readCell.Update();
                    }
                }
            }

            //		QB.Logger.Info("Move DOWN:       new    FirstViewRow " + FirstViewRow);
            //		QB.Logger.Info("Move DOWN:       new    LastViewRow  " + LastViewRow);


        }
        void moveLeft()
        {



        }
        void moveRight()
        {

        }

        public void SetVisible()
        {

            UIBox box;
            foreach (KeyValuePair<string, UIBox> entry in Cell)
            {
                box = (UIBox)entry.Value;
                box.Visible = visible;
                if (Navigation != null)
                {
                    Navigation.Visible = visible;
                    Navigation.Clickable = visible;
                }
                if (Up != null)
                {
                    Up.Visible = visible;
                    Up.Clickable = visible;
                }
                if (Down != null)
                {
                    Down.Visible = visible;
                    Down.Clickable = visible;
                }

                if (Left != null)
                {
                    Left.Visible = visible;
                    Left.Clickable = visible;
                }
                if (Right != null)
                {
                    Right.Visible = visible;
                    Right.Clickable = visible;
                }
            }
        }

        public void SetTarget(Table table)
        {
            if (table == null)
            {
                QB.Logger.Error(Name + " set target failed is null");
                return;
            }
            try
            {
                Target = table;
                QB.Logger.Info("Rows " + Target.RowCount);
                Update();
            }
            catch
            {
                QB.Logger.Error(Name + " set target failed set");
            }
        }


    }
    public class BoxTableCell : UIBox
    {
        public Table Target = new Table("");
        bool Locked = false;

        public int CellR;
        public int CellC;

        public BoxTableView View;

        public BoxTableCell(string name, Table target, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, bool locked = true, string page = null, int cellR = 1, int cellC = 1, BoxTableView view = null) : base(name)
        {

            View = view;
            if (target != null)
                Target = target;

            CellR = cellR;
            CellC = cellC;

            if (backColor == null)
                backColor = "lightgray";

            Backcolor = backColor.ToColor();
            Locked = locked;
            Page = page;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }

        public override void Create()
        {
            //	QB.Logger.Info($"Cell {CellR}:{CellC} = {Target[CellR,CellC].Value}");
            Frame = new QB.Controls.Box(Name, x: X, y: Y, w: W, h: H, style: $"font::{H * 1.3}:bg:Transparent,align:ml,bg-hover:Transparent,border:black:0.5"
                , boxes: new[] {

                Read = new QB.Controls.Box("read", text:"", editMask:"", x:"2%", y:"2%", w:"96%", h:"96%",
                    onClick:(e) => edit(),
                    onAccept:(e) => saveValue(),
                    onCancel:(e) => undo(),
                    style:$"font::{H*1.3}:bg:Transparent,align:ml"),
                }
            );
            Frame.Directory = Page;
            Read.Directory = Page;
            Read.OnWheel += (box, delta) => View.MouseWheel(box, delta);
            Frame.OnWheel += (box, delta) => View.MouseWheel(box, delta);
            Frame.Clickable = true;

            if (Target[CellR, CellC].Value != null)
                Read.Text = Target[CellR, CellC].Value;

            if (Locked)
            {
                Read.Clickable = false;
                Frame.Clickable = false;
            }

            SetBackColor(Backcolor);

        }

        void edit()
        {
            if (Locked) return;
            //Read.Text = Target.Value.ToString();
        }
        void undo()
        {
            if (Locked) return;
  
            Read.Text = (Func<string>)(() => Target[CellR, CellC].Value.ToString());

        }
        public void Update()
        {

            if (Target[CellR, CellC].Value == null)
                Read.Text = "";
            else
                Read.Text = Target[CellR, CellC].Value;
        }

        void saveValue()
        {
            if (Locked) return;
            Target[CellR, CellC].Value = Read.Text;
            Read.Text = (Func<string>)(() => Target[CellR, CellC].Value.ToString());

            //	QB.Logger.Info(Name + " New Value " + Target[CellR, CellC].Value);
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            Read.BackgroundColorHover = !Locked ? Backcolor.ChangeBrightness(50) : Backcolor;
        }
    }
    public class BoxLogView : BoxFlowPanel
    {

        public QB.Controls.Box Navigation;
        public QB.Controls.Box Up;
        public QB.Controls.Box Down;
        public QB.Controls.Box Last;

        public string Uri = null;

        int StartLine = -1;

        string[] AllLines;
        int TotalLines;

        FileSystemWatcher watcher;

        public BoxLogView(string name, double x, double y, double h, double w, int rows, string page, double gap = 0.2, object backColor = null) :
        base(name: name, position: null, x:x, y:y, h:h, w:w, columns: 1, rows:rows, page:page, gap:gap, backColor:backColor)
        {
    
            Navigation = new QB.Controls.Box("Navi", x: X + w - 5, y: Y, w: 5, h: h, style: $"font::12:b,align:tl,bg:red,bg:lightgray,border:black:0.5", boxes: new[] {
                Up = new QB.Controls.Box("up", h:"20%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-up", onClick:(b) => moveUp()),
                Down = new QB.Controls.Box("down", y:"60%", h:"20%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-down", onClick:(b) => moveDown()),
                Last = new QB.Controls.Box("last", y:"80%", h:"20%", style:$"font::10,align:tl,bg:transparent", icon:"fa:arrows-down-to-line", onClick:(b) => ToLast())
            }
            );

            Navigation.BackgroundColor = BackColor;
            Navigation.Directory = page;
            Navigation.OnWheel += (box, delta) => MouseWheel(box, delta);
            Navigation.Clickable = true;
            Up.Directory = page;
            Down.Directory = page;
            Last.Directory = page;

            Up.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);
            Down.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);


            create();
            //	Update();
            //	ResetWatcher();
        }

        public override void SetChildVisible()
        {
            Navigation.Visible = Visible;
        }


        public void ResetWatcher()
        {
            FileSystemEventHandler update = null;
            watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.GetDirectoryName(Uri);
            watcher.Filter = System.IO.Path.GetFileName(Uri);
            watcher.Changed -= update;
            update = new FileSystemEventHandler(OnChanged);
            watcher.Changed += update;
            watcher.EnableRaisingEvents = true;
            StartLine = -1;
            Update("ResetWatcher");

        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            StartLine = -1;
            //		WaitForFile(Uri);
            Update("Onchanged");
        }

        private void ToLast()
        {

            StartLine = -1;
            //		WaitForFile(Uri);
            Update("ToLast");

        }




        public void MouseWheel(Control c, int delta)
        {


            if (delta == 1)
                moveUp();
            if (delta == -1)
                moveDown();

        }

        public void moveUp()
        {
           // QB.Logger.Debug("LogView StartLine" + StartLine);
            if (StartLine > 0)
            {
                StartLine--;
                Update("moveUp");
            }
        }

        public void moveDown()
        {
            Read();
            if (StartLine + Rows < TotalLines)
            {
                StartLine++;
                Update("moveDown");
            }
        }


        void Read()
        {
            string workingFile = System.IO.Path.GetTempFileName();
            File.Copy(Uri, workingFile, true);
            string[] AllLines = File.ReadAllLines(workingFile);
            TotalLines = AllLines.Length;
            File.Delete(workingFile);


        }
        void create()
        {
            for (int i = 0; i < Rows; i++)
            {
                //		QB.Logger.Info("line add " + i);
                double x = X + (cellW * NextColumn) + Gap;
                double y = Y + (cellH * NextRow) + Gap;
                double w = cellW - 2 * Gap;
                double h = cellH - 2 * Gap;

                BoxLogLine line = new BoxLogLine("l" + i, this, 0, "", x, y, w - 5, h, "", "transparent", false, "", 0.6);
                line.Page = Page;

                line.Create();
                line.Description.Clickable = true;

                Boxes.Add("l" + i, line);
                NextRow++;
            }
        }

        public void Update(string sender)
        {
            try
            {
                // Create a working copy of the file
                string workingFile = System.IO.Path.GetTempFileName();
                File.Copy(Uri, workingFile, true);
                string[] allLines = File.ReadAllLines(workingFile);
                int totalLines = allLines.Length;
                int r = Rows - 1;
                if (StartLine == -1)
                    StartLine = Math.Max(totalLines - Rows, 0);

                int viewRow = 0;

                for (int i = 0; i < Rows - 1; i++)
                {
                    Boxes["l" + viewRow].State.Icon = "";
                    Boxes["l" + viewRow].Description.Text = "";
                    Boxes["l" + viewRow].SetBackColor("transparent");
                }


                //			if(allLines.Length < Rows)
                ////				Rows = allLines.Length;

                for (int i = StartLine; i < StartLine + Rows; i++)
                {
                    List<string> item = allLines[i].Split(',').ToList();
                    if (item == null) return;
                    string date = item[0];
                    string text = item[2];
                    string type = item[1];
                    string color = "#EEEEEE";
                    string icon = "fa:circle-info";

                    if (type == "ALRT")
                    {
                        icon = "fa:triangle-exclamation";
                        color = "#FF9A9A";
                    }

                    if (type == "WARN")
                    {
                        icon = "fa:circle-exclamation";
                        color = "lightyellow";
                    }

                    // QB.Logger.Info("Update " + viewRow);
                    Boxes["l" + viewRow].State.Icon = icon;
                    Boxes["l" + viewRow].Description.Text = $"{date}: {text}";
                    Boxes["l" + viewRow].SetBackColor(color);
                    // QB.Logger.Info(allLines[i]);
                    viewRow++;
                }

                // Delete the working copy
                File.Delete(workingFile);
            }
            catch
            {
                QB.Logger.Error(Name + " '" + sender + "' Failed to update Log-View");
            }
        }


    }
    public class BoxLogLine : BoxButton
    {
        public BoxLogView View;

        string Info = "";
        string Media = "";
        public BoxLogLine(string name, BoxLogView view, int line = 0, string id = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, string description = null, object backColor = null, bool locked = false, string icon = null, double iconSizeFactor = 0.6, QB.Controls.Box.ClickEventHandler onClick = null) :
        base(name, id, x, y, w, h, description, backColor, locked, icon, iconSizeFactor, onClick)
        {
            View = view;

        }

        public override void MouseWheel(Control c, int delta)
        {

            if (delta == 1)
                MoveDown();
            if (delta == -1)
                MoveUp();

        }

        public override void MoveUp()
        {
            View.moveUp();
        }

        public override void MoveDown()
        {
            View.moveDown();
        }

    }
    public class DeviceLog : IDisposable
    {
        private readonly string Id;
        public string Folder;
        public string File;
        private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource cts  = new CancellationTokenSource();
        private readonly Task logWriterTask;
        private static readonly TimeSpan flushInterval = TimeSpan.FromSeconds(2);
        private readonly ManualResetEventSlim flushEvent = new ManualResetEventSlim(false);

        public DeviceLog(string id, string folder)
        {
            Id = id;
            Folder = folder;

            if (!Directory.Exists(Folder))
            {
                QB.Logger.Error(Folder + " not exists");
                Directory.CreateDirectory(Folder);
            }

            File = GetLogFile();

            // Starte Hintergrund-Task
            logWriterTask = Task.Run(() => ProcessQueue(cts.Token));
        }

        public void Info(string text, string info = "", string media = "") =>
            Add(text, info, media);

        public void Warning(string text, string info = "", string media = "") =>
            Add(text, info, media, warning: true);

        public void Alert(string text, string info = "", string media = "") =>
            Add(text, info, media, alert: true);

        public void Add(string text = null, string info = "", string mediaId = "", bool alert = false, bool warning = false)
        {
            string type = alert ? "ALRT" : warning ? "WARN" : "INFO";
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{type},{text?.Replace("\r\n", " ")},{info},{mediaId}";
         
            logQueue.Enqueue(entry);
            flushEvent.Set(); // Signalisiert dem Thread, dass was zu tun ist
        }

        private static readonly object writeLock = new object();
        private string currentLogFile = null;

        private string GetLogFileCached()
        {
            try
            {
                if (string.IsNullOrEmpty(currentLogFile) || !System.IO.File.Exists(currentLogFile) || new FileInfo(currentLogFile).Length > 100_000)
                {
                    currentLogFile = GetLogFile();
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error("DeviceLog GetLogFileCached error: " + ex.Message);
                currentLogFile = GetLogFile(); // Fallback
            }

            return currentLogFile;
        }

        private async Task ProcessQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (logQueue.IsEmpty)
                        flushEvent.Wait(flushInterval, token); // warte auf Signal oder Timeout

                    flushEvent.Reset();

                    var lines = new List<string>();
                    while (logQueue.TryDequeue(out string logEntry))
                    {
                        lines.Add(logEntry);
                    }

                    if (lines.Count > 0)
                    {
                        string logFile = GetLogFileCached();

                        bool success = false;
                        int retries = 3;

                        for (int i = 0; i < retries && !success; i++)
                        {
                            try
                            {
                                lock (writeLock)
                                {
                                    using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                                    using (var writer = new StreamWriter(fs))
                                    {
                                        foreach (var line in lines)
                                        {
                                            writer.WriteLine(line);
                                        }
                                    }
                                }

                                success = true;
                            }
                            catch (IOException)
                            {
                                await Task.Delay(20, token); // Retry nach kurzem Warten
                            }
                        }

                        if (!success)
                        {
                            QB.Logger.Error("DeviceLog write failed after retries: " + logFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    QB.Logger.Error("DeviceLog background write error: " + ex.Message);
                }

                // kleine Pause zur Entkopplung der Schreibzyklen
                await Task.Delay(10, token);
            }
        }


        public string GetLogFile()
        {
            try
            {
                if (!Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);

                var files = Directory.GetFiles(Folder, "*.log");
                if (files.Length == 0)
                {
                    return System.IO.Path.Combine(Folder, NewLogFileName());
                }

                var newest = files
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .First();

                if (newest.Length > 100_000)
                    return System.IO.Path.Combine(Folder, NewLogFileName());

                return newest.FullName;
            }
            catch (Exception ex)
            {
                QB.Logger.Error("GetLogFile error: " + ex.Message);
                return System.IO.Path.Combine(Folder, NewLogFileName());
            }
        }

        private string NewLogFileName() =>
            DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".log";

        public void Dispose()
        {
            cts.Cancel();
            flushEvent.Set();
            logWriterTask.Wait();
            cts.Dispose();
            flushEvent.Dispose();
        }
    }




    public class BoxParameter : UIBox
    {

        public Signal Target = new Signal("dummy", value: 1000, unit: "ddd");
        public double Min;
        public double Max;

        Box ParameterName;
        Box ParameterRead;
        Box ParameterReadString;
        Box ParameterUnit;
        Box ParameterLed;
        Box ParameterText;


        bool Locked = false;
        string Format;

        string mode;

        public BoxParameter(string name, Signal target = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, object valueColor = null, double min = double.NaN, double max = double.NaN, bool locked = false, string format = "0.000",string page = null) : base(name)
        {
            Min = min;
            Max = max;

            if(Target.DefaultDisplayFormat != null)
                Format = Target.DefaultDisplayFormat;
            if (target != null)
                Target = target;

            if (backColor == null)
                backColor = "lightgray";

            Valuecolor = System.Drawing.Color.Transparent;
            if (valueColor != null)
                Valuecolor = valueColor.ToColor();

            Backcolor = backColor.ToColor();
            Locked = locked;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }

        public override void Create()
        {
           // QB.Logger.Info("BoxParameter " + Name + "Page " + Page);
            
            Frame = new Box(Name, text: "", x: X, y: Y, w: W, h: H
                    , boxes: new[] {
                ParameterName = new QB.Controls.Box(Name, textFunction:() => Target.Text, x:1.5, y:"10%", w:"50%", h:"80%", style:$"font::{H}:,align:ml,bg:Transparent"){Directory = Page },
                ParameterRead = new QB.Controls.Box("read", textFunction:() => Target.Value.ToString(Format), x:"30%", y:"10%", w:"50%", h:"80%",
                    onClick:(e) => editTarget(),
                    style:$"font::{H*1}:b,align:mr,bg:Transparent"){Directory = Page },

               ParameterUnit = new QB.Controls.Box("unit", textFunction:() => Target.Unit, x:"80%", y : "10%", w : "27%", h : "80%", style:$"font::{H}:i,align:ml,bg:Transparent") { Directory = Page },
               ParameterLed  = new QB.Controls.Box("led", x:"73%", y:H*0.25, w:H*0.5, h:H*0.5, style: $"bg:transparent,border:Black:1",onClick:(s) => editTarget()) { Directory = Page },
               ParameterText = new QB.Controls.Box("text", x:"80%",text:"",  w:20, h:H, style:$"font::{H}:i,align:ml,bg:Transparent"){Directory = Page }

                    });


                Frame.Directory = Page;

            ParameterLed.Clickable = false;
           
            if (Locked) Read.Clickable = false;
            SetBackColor(Backcolor);
        }

        void editTarget()
        {
            if (mode == "signal") Target.ShowEditDialog();
            if (mode == "bool") toggle();
        }

        void toggle()
        {
            Target.Value = Target.Value == 1 ? 0 : 1;
            Update();
        }

        public void BoolView()
        {
            mode = "bool";

            ParameterLed.Visible = true;
            ParameterLed.Clickable = true;

            Frame.Boxes[1].TextColor = Backcolor.ToColor();
   
            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Black;
            Update();
        }

        void reset()
        {
            ParameterLed.Visible = false;
            ParameterLed.Clickable = false;

            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BackgroundColor = System.Drawing.Color.Transparent;
            ParameterUnit.Text = "";
        }

        public void SignalView()
        {
            mode = "signal";
          
            Frame.Boxes[1].Visible = true;
            Frame.Boxes[1].TextColor = "Black".ToColor();
            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Transparent;
            Frame.Boxes[4].Text = "";
            Frame.Boxes[3].Clickable = false;
        }

        public void Update()
        {
            if (Locked)
            {
                Frame.Boxes[1].Clickable = false;
            }

            Frame.Boxes[3].BackgroundColor = Target.Value == 1 ? System.Drawing.Color.Orange : System.Drawing.Color.Transparent;
            Frame.Boxes[4].Text = Target.Value == 1 ? "Yes" : "No";
        }

        public void SetParameter(object signal, string viewType = "")
        {
            
            if (signal == null)
            {
                QB.Logger.Error(Text + " set target failed is null");
                return;
            }
            try
            {
                if (signal is Signal)
                {

                    Target = signal as Signal;

                    if (Target == null)
                    {
                        QB.Logger.Error(Text + " set target failed is null");
                        return;
                    }
                    if (Target.DefaultDisplayFormat != null)
                    {
                        Format = Target.DefaultDisplayFormat;
                    }

                    if (viewType.ToLower() == "bool")
                    {
                        reset();
                        BoolView();
                        ParameterLed.Visible = true;
                        ParameterLed.Clickable = true;

                    }
                    else if (viewType.ToLower() == "signal")
                    {
                        reset();
                        SignalView();
                        ParameterLed.Visible = false;
                        ParameterLed.Clickable = false;
                    }
                    else
                    {
                        reset();
                        SignalView();

                    }
                }
                if(signal is StringSignal)
                {
                    //Target = signal as StringSignal;
                    //reset();
                    //SignalView();
                }
            }
            catch
            {
                QB.Logger.Error(Text + " set target failed set");
            }
        }

        public void HideParameter()
        {
            Frame.Visible = false;
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            //Read.BackgroundColorHover = Target.AllowEdit? ChangeColorBrightness(Backcolor, 50) : Backcolor;
        }

        public override void SetVisible()
        {
          //  Debug.WriteLine(Name + "Set Visible " + Visible);
            Frame.Visible = Visible;
            ParameterName.Visible = Visible;
            ParameterRead.Visible = Visible;
            ParameterUnit.Visible = Visible;
            ParameterLed.Visible = Visible;

            ParameterName.Clickable = Visible;
            ParameterRead.Clickable = Visible;
            ParameterUnit.Clickable = Visible;
            ParameterLed.Clickable = Visible;

        }
    }
    public class BoxParameterObject : UIBox
    {
        public double Min;
        public double Max;

        public object Target = null;

       internal Box ParameterName;
        internal Box ParameterRead;
        internal Box ParameterReadString;
        internal Box ParameterUnit;
        internal Box ParameterLed;
        internal Box ParameterText;


        bool Locked = false;
        string Format;

        string mode;

        public BoxParameterObject(string name, double x = double.NaN, double y = double.NaN, double w = double.NaN, double h = double.NaN, object backColor = null, object valueColor = null, double min = double.NaN, double max = double.NaN, bool locked = false, string format = "0.000", string page = null) : base(name)
        {
            Min = min;
            Max = max;

            if (backColor == null)
                backColor = "lightgray";

            Valuecolor = System.Drawing.Color.Transparent;
            if (valueColor != null)
                Valuecolor = valueColor.ToColor();

            Backcolor = backColor.ToColor();
            Locked = locked;
            X = double.IsNaN(x) ? 0 : x;
            Y = double.IsNaN(y) ? 0 : y;
            W = double.IsNaN(w) ? 50 : w;
            H = double.IsNaN(h) ? 15 : h;
        }

        double readXDefault;
        double readWidthDefault;

        public override void Create()
        {
        //    QB.Logger.Info("BoxParameter " + Name + "Page " + Page);

            Frame = new Box(Name, text: "", x: X, y: Y, w: W, h: H
                    , boxes: new[] {
                ParameterName = new QB.Controls.Box(Name,  x:1.5, y:"10%", w:"50%", h:"80%", style:$"font::{H}:,align:ml,bg:Transparent"){Directory = Page },
                ParameterRead = new QB.Controls.Box("read",  x:"30%", y:"10%", w:"50%", h:"80%",
                    onClick:(e) => editTarget(),
                    style:$"font::{H*1}:b,align:mr,bg:Transparent"){Directory = Page },

               ParameterUnit = new QB.Controls.Box("unit", x:"80%", y : "10%", w : "27%", h : "80%", style:$"font::{H}:i,align:ml,bg:Transparent") { Directory = Page },
               ParameterLed  = new QB.Controls.Box("led", x:"73%", y:H*0.25, w:H*0.5, h:H*0.5, style: $"bg:transparent,border:Black:1",onClick:(s) => editTarget()) { Directory = Page },
               ParameterText = new QB.Controls.Box("text", x:"80%",text:"",  w:20, h:H, style:$"font::{H}:i,align:ml,bg:Transparent"){Directory = Page }

                    });

       
            Frame.Directory = Page;

            ParameterLed.Clickable = false;

            if (Locked) Read.Clickable = false;
            SetBackColor(Backcolor);
        }

        void editTarget()
        {

            Debug.WriteLine("Click");
            if (mode == "signal")
            {
                Signal read = Target as Signal;
                read.ShowEditDialog();
                Update();
            }
            if (mode == "bool") {
                Signal read = Target as Signal;
                read.Value = read.Value == 1 ? 0 : 1;
                Update();
            };

            if (mode == "text")
            {
                StringSignal read = Target as StringSignal;
                string input = "";
                Extensions.ShowEditTextDialog(ref input, read.Text);
                read.Value = input;
                Update();
            };

            if (mode == "divider")
            {
                StringSignal read = Target as StringSignal;
                string input = read.Value;
                read.Value.ShowEditDialog(ref input, unit1: "/",text: read.Text);
                read.Value = input;
                Update();
            };

            if (mode.Contains("uri"))
            {
                string folder = modeParameter == null ? System.IO.Path.Combine(QB.Book.SettingsDirectory, "csv") : modeParameter; 
      
                QB.Logger.Info("Open FileDialog '" + folder + "'");

                    StringSignal read = Target as StringSignal;
                string uri = "";
                using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = folder;
                    openFileDialog.ValidateNames = false;
                    openFileDialog.CheckFileExists = false;
                    openFileDialog.CheckPathExists = true;
             

                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        read.Value = System.IO.Path.GetFileName(openFileDialog.FileName);
                        Update();
                    }
                }
            }


            }

        public void BoolView()
        {
            mode = "bool";
            ParameterLed.Visible = true;
            ParameterLed.Clickable = true;

            Frame.Boxes[1].TextColor = Backcolor.ToColor();

            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Black;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Black;
            Update();
        }

        void reset()
        {
            ParameterUnit.Text = "";
            ParameterLed.Visible = false;
            ParameterLed.Clickable = false;
            ParameterUnit.Visible = false;
            ParameterUnit.Clickable = false;
            ParameterRead.Visible = false;
            ParameterRead.Clickable = false;

            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BackgroundColor = System.Drawing.Color.Transparent;

  
        }

        public void SignalView()
        {
            mode = "signal";

            Frame.Boxes[1].Visible = true;
            Frame.Boxes[1].TextColor = "Black".ToColor();
            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Transparent;
            Frame.Boxes[4].Text = "";
            Frame.Boxes[3].Clickable = false;
            ParameterUnit.Visible = true;
            ParameterUnit.Clickable = false;
            ParameterRead.Visible = true;
            ParameterRead.Clickable = true;
        }

        public void StringSignalView()
        {
            mode = "text";

            Frame.Boxes[1].Visible = true;
            Frame.Boxes[1].TextColor = "Black".ToColor();
            Frame.Boxes[3].BorderBColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderTColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderLColor = System.Drawing.Color.Transparent;
            Frame.Boxes[3].BorderRColor = System.Drawing.Color.Transparent;
            Frame.Boxes[4].Text = "";
            Frame.Boxes[3].Clickable = false;
            ParameterRead.Visible = true;
            ParameterRead.Clickable = true;
        }



        public void Update()
        {
            QB.Logger.Info("Update SettingsValue Mode: " + mode);
            
            if (Locked)
            {
                Frame.Boxes[1].Clickable = false;
            }

            if (mode == "bool")
            {
                Signal read = Target as Signal;
                Frame.Boxes[3].BackgroundColor = read.Value == 1 ? System.Drawing.Color.Orange : System.Drawing.Color.Transparent;
                Frame.Boxes[4].Text = read.Value == 1 ? "Yes" : "No";
            }
            if(mode == "signal")
            {
                Signal read = Target as Signal;
                ParameterName.Text = read.Text;
                ParameterUnit.Text = read.Unit;
                ParameterRead.Text = read.Value.ToString(Format);
            }
            if (mode == "text")
            {
                StringSignal read = Target as StringSignal;
                ParameterName.Text = read.Text;
                ParameterRead.Text = read.Value;
            }

            if (mode == "divider")
            {
                StringSignal read = Target as StringSignal;
                ParameterName.Text = read.Text;
                ParameterRead.Text = read.Value;
               
            }

            if (mode.Contains("uri"))
            {
                StringSignal read = Target as StringSignal;
                ParameterName.Text = read.Text;
                ParameterRead.Text = read.Value;
                ParameterRead.Bounds.X = 30;
                ParameterRead.Bounds.W = 180;
            }

            


        }
        string modeParameter = "";
        public void SetParameter(object target, string viewType = "")
        {
            modeParameter = null;
              Target = target;
            if (target == null)
            {
                QB.Logger.Error(Text + " set target failed is null");
                return;
            }
            try
            {
                if (target is Signal)
                {
                    Signal read = target as Signal;

                    if (read == null)
                    {
                        QB.Logger.Error(Text + " set target failed is null");
                        return;
                    }
                    if (read.DefaultDisplayFormat != null)
                    {
                        Format = read.DefaultDisplayFormat;
                    }

                    if (viewType.ToLower() == "bool")
                    {
                        reset();
                        BoolView();
                        ParameterName.Text = read.Text;
                        ParameterUnit.Text = read.Unit;
                        ParameterRead.Text = read.Value.ToString("0");



                    }
                    else if (viewType.ToLower() == "signal")
                    {
                        reset();
                        SignalView();
                        ParameterName.Text = read.Text;
                        ParameterUnit.Text = read.Unit;
                        ParameterRead.Text = read.Value.ToString(Format);
                    }
                    else
                    {
                        reset();
                        SignalView();
                    }
                    Debug.WriteLine("Parameter Mode = " + viewType);
                }

                if(target is StringSignal)
                {
                    StringSignal read = Target as StringSignal;
                    if (viewType.ToLower() == "text")
                    {
                        reset();
                        StringSignalView();
                        ParameterName.Text = read.Text;
                        ParameterRead.Text = read.Value;
                    }
                    if (viewType.ToLower() == "divider")
                    {
                        reset();
                        StringSignalView();
                        ParameterName.Text = read.Text;
                        ParameterRead.Text = read.Value;
                        ParameterUnit.Visible = true;
                        //ParameterUnit.Text = "C";
                        mode = "divider";
                    }

                    if (viewType.ToLower() == "text")
                    {
                        reset();
                        StringSignalView();
                        ParameterName.Text = read.Text;
                        ParameterRead.Text = read.Value;
                        mode = "text";
                    }

                    if (viewType.ToLower().Contains("uri"))
                    {
                        QB.Logger.Info("viewType '" + viewType + "'");
                        if (viewType.Contains("=")) {

                           List<string> parameter = viewType.Split('=').ToList();
                            QB.Logger.Info("Parameter Count '" + parameter.Count());

                            foreach (string p in parameter) QB.Logger.Info("Parameter: " + p);
                            modeParameter = parameter[1];

                            QB.Logger.Info("modeParameter '" + modeParameter);




                        }
                        ;
                        reset();
       
                        StringSignalView();
                        ParameterName.Text = read.Text;
                        ParameterRead.Text = read.Value;
                        
                        mode = "uri";
                    }
                }
         
            }
            catch
            {
                QB.Logger.Error(Text + " set target failed set");
            }
        }

        public void HideParameter()
        {
            Frame.Visible = false;
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            //Read.BackgroundColorHover = Target.AllowEdit? ChangeColorBrightness(Backcolor, 50) : Backcolor;
        }

        public override void SetVisible()
        {
            //  Debug.WriteLine(Name + "Set Visible " + Visible);
            Frame.Visible = Visible;
            ParameterName.Visible = Visible;
            ParameterRead.Visible = Visible;
            ParameterUnit.Visible = Visible;
            ParameterLed.Visible = Visible;

            ParameterName.Clickable = Visible;
            ParameterRead.Clickable = Visible;
            ParameterUnit.Clickable = Visible;
            ParameterLed.Clickable = Visible;

        }
    }

    public class BoxFile : UIBox
    {
        BoxFolderView Explorer;
        public string FileName = "";
        public string Uri = "";

        public BoxFile(string name, double x, double y, double h, double w, object backColor, string page, BoxFolderView explorer) : base(name)
        {
            Explorer = explorer;

            X = x;
            Y = y;
            W = w;
            H = h;
            Page = page;
            Backcolor = backColor.ToColor();
        }

        public override void Create()
        {
            double aligment = W < H ? W : H;
            double iconSize = aligment * 0.8;

            Frame = new QB.Controls.Box(Name, text: "", x: X, y: Y, w: W, h: H, style: $"font::{aligment * 0.5},align:tl,bg:Transparent",
                        onClick: (b) => onSelect(),
                        boxes: new[] {
                    State = new QB.Controls.Box("icon", x:aligment * 0.2, y:(H - iconSize) / 2, w:iconSize, h:iconSize,icon:"fa:file", onClick: (b) => onSelect()),
                    Description = new QB.Controls.Box("description", textFunction:() => FileName, x:H * 0.9, y:(H - iconSize) / 2, w:W - H -2, h:H - (H - iconSize) / 2, onClick:(b) => onSelect(), style:$"font::{H},align:ml,bg:Transparent"),
                        });
            Frame.Directory = Page;
            Frame.BackgroundColor = Backcolor;
            Frame.Clickable = true;

            State.Directory = Page;
            State.BackgroundColor = Color.Transparent;
            State.Clickable = true;

            Description.Directory = Page;
            Description.BackgroundColor = Color.Transparent;
            Description.Clickable = true;
            Description.OnWheel += (box, delta) => MouseWheel(box, delta);
            Frame.OnWheel += (box, delta) => MouseWheel(box, delta);
            State.OnWheel += (box, delta) => MouseWheel(box, delta);
        }

        void onSelect()
        {
            Explorer.SelectedFile = FileName;
            QB.Logger.Info("-------------------FIle:"+ FileName);
            Explorer.SelectedUri = Uri;
            Explorer.Update("selection");
        }

        public void MouseWheel(Control c, int delta)
        {
            Explorer.MouseWheel(c, delta);
        }

        public override void SetBackColor(object color)
        {
            Backcolor = color.ToColor();
            Frame.BackgroundColor = Backcolor;
            Frame.BackgroundColorHover =  Backcolor.ChangeBrightness(50);
            Frame.BackgroundColorHover =  Backcolor.ChangeBrightness(50);
        }
    }

    
    public class BoxFolderView : BoxFlowPanel
    {
        public QB.Controls.Box Navigation;
        public QB.Controls.Box Up;
        public QB.Controls.Box Down;
        public QB.Controls.Box Last;
        public QB.Controls.Box TargetFolder;
        public QB.Controls.Box TargetFile;
        public QB.Controls.Box Menu;
        public QB.Controls.Box Select;
        public QB.Controls.Box Close;
        public QB.Controls.Box OpenFolder;


        public string Uri = null;

        int FirstVisible = -1;

        string[] AllLines;
        int TotalLines;

        public string Folder = "";
        string Extension = "";
        string Text = "";
        public string SelectedFile = "";
        public string SelectedUri = "";

        public bool ShowClose = true;
        public bool ShowSelect = true;

        string Position;

        public System.Action OkAction { get; set; }
        public System.Action CancelAction { get; set; }


        public BoxFolderView(string name,string text, int rows, string page, string folder, string extension, double h = 0, double w = 0, double x = 0, double y = 0,string position = "center", double gap = 0.2, object backColor = null) :
        base(name:name, x:x, y:y, h:h, w:w, columns: 1,rows:rows, page:page, gap:gap, backColor:backColor,position:position)
        {
            Folder = folder;
            Extension = extension;
            Text = text;
            Position = position;

            create();
         	Update("Init");
        }

        public void OnSelectClick(Box.ClickEventHandler OnClick)
        {
            Select.Clickable = true;
            Select.OnBoxClick += OnClick;
        }

        


        public override void SetChildVisible()
        {
            Navigation.Visible = Visible;
            Menu.Visible = Visible;
            TargetFolder.Visible = Visible;
            Navigation.Visible = Visible;
        }

        public virtual void MouseWheel(Control c, int delta)
        {
            if (delta == 1)
                moveUp();
            if (delta == -1)
                moveDown();
        }
        public void moveUp()
        {
         //   QB.Logger.Debug("LogView StartLine" + StartLine);
            if (FirstVisible > 0)
            {
                FirstVisible--;
                Update("moveUp");
            }
        }
        public void moveDown()
        {

            Update("moveDown");
            if (FirstVisible + Rows <= TotalLines)
            {
                FirstVisible++;
                Update("moveDown");
            }
        }
        List<string> Files = new List<string>();
        List<string> Uris = new List<string>();
        void Read()
        {
            Files.Clear();
            Uris.Clear();
            if (Directory.Exists(Folder))
            {
                string[] jsonFiles = Directory.GetFiles(Folder, Extension);

                foreach (string file in jsonFiles)
                {
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                   // QB.Logger.Info(fileNameWithoutExtension);
                    Files.Add(fileNameWithoutExtension);
                    Uris.Add(file);
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
            }

            TotalLines = Files.Count;

            if (FirstVisible == -1) FirstVisible = 0;
         

            //QB.Logger.Info("FirstVisible " + FirstVisible);
            //QB.Logger.Info("Rows         " + Rows);


        }
        void create()
        {
            double h = Height - Height / Rows;
            double w = Width;
            double m = Height * 0.1;
            cellH = (Height - m * 2) / Rows;

            TargetFolder = new QB.Controls.Box("targetFolder", x: X, text: Text, y: Y, h: Height, w: w, style: $"font::{h / (Rows/2)}:b,align:tl,border:black:0.5", boxes: new[] {
                OpenFolder = new QB.Controls.Box("open", x:w-m-1,w:m,y:1,h: m, icon:"fa:folder-open", onClick:(b) => Process.Start("explorer.exe", Folder),
                style:$"bg:Transparent"),
            }
            );

            Navigation = new QB.Controls.Box("Navi", x: X + w - 6, y: Y + m, w: 5, h: Height - m * 2, style: $"font::12:b,align:tl,bg:red,bg:lightgray,border:black:0", boxes: new[] {
                Up = new QB.Controls.Box("up",y:0, h:"10%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-up", onClick:(b) => moveUp()),
                Down = new QB.Controls.Box("down", y:"90%", h:"10%", style:$"font::10,align:tl,bg:transparent", icon:"fa:angle-down", onClick:(b) => moveDown()),
            }
            );

            Menu = new QB.Controls.Box("menu", x: X, y: Y + Height-m , h: m, w: w, style: $"font::{h * 0.2}:b,align:tl,border:black:0.5,bg:red", boxes: new[] {
            Close = new QB.Controls.Box("close", x:0,w:"50%", icon:"fa:xmark", onClick:(b) => CancelClick(), style: $"border:black:0.5,bg:Red"),
            Select = new QB.Controls.Box("select", x:"50%",w:"50%", icon:"fa:check", onClick:(b) => OkClick(), style: $"border:black:0.5,bg:#9bcd9b"),
            }
            );

            TargetFolder.Directory = Page;
            Navigation.Directory = Page;
            Menu.Directory = Page;
            Close.Directory = Page;
            Select.Directory = Page;
            OpenFolder.Directory = Page;

            Close.BackgroundColor = Color.Tomato;

            OpenFolder.BackgroundColorHover = BackColor.ChangeBrightness(80);


            Menu.Boxes[0].BackgroundColor = BackColor;
            Menu.Boxes[1].BackgroundColor = BackColor;

            Navigation.BackgroundColor = BackColor;
            TargetFolder.BackgroundColor = BackColor;

            Navigation.Directory = Page;
            Navigation.OnWheel += (box, delta) => MouseWheel(box, delta);
            Navigation.Clickable = true;
            Up.Directory = Page;
            Down.Directory = Page;


            Up.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);
            Down.BackgroundColorHover = Navigation.BackgroundColor.ChangeBrightness(60);

            //cellH = Height * 0.9 / Rows;
            for (int i = 0; i < Rows; i++)
            {
                int index = i;
               
                double x = X + (cellW * NextColumn) + Gap;
                double y = (Y + m) + (cellH * NextRow) + Gap;
                w = cellW - 2 * Gap;
                h = cellH - 2 * Gap;
                BoxFile line = new BoxFile(name: "l" + index, x: x, y: y, h: h, w: w - 5, "Red", page: Page, this);
                line.Create();
                Boxes.Add("l" + index, line);
                NextRow++;

            }
        }

        public void CloseButtonVisible(bool set)
        {
            Menu.Boxes[0].Visible = set;
        }

        public void SelectButtonVisible(bool set)
        {
            Menu.Boxes[1].Visible = set;
        }


        public void Show(string text, string folder, string extension, System.Action okAction = null, System.Action cancelAction = null)
        {
            OkAction = okAction;
            CancelAction = cancelAction;

            
            
            Folder = folder;
            Extension = extension;
            TargetFolder.Text = text;
            FirstVisible = -1;
            SelectedFile = "";
            Update("show");

            OpenFolder.Clickable = true;
            OpenFolder.ZOrder = 10;

            Visible = true;
            Menu.Visible = true;
            TargetFolder.Visible = true;
            Navigation.Visible = true;
            if (!ShowClose)
            {
                Menu.Boxes[0].Icon = "";
                Close.Clickable = false;
            }
            Select.Visible = ShowSelect;

           
        }

        public void Update(string sender)
        {
            Read();
            try
            {
                int totalLines = Files.Count;
                for (int i = 0; i < Rows; i++)
                {
                    int index = i;
                    ((BoxFile)Boxes["l" + index]).FileName = "";
                    ((BoxFile)Boxes["l" + index]).Uri = "";
                    ((BoxFile)Boxes["l" + index]).State.Icon = "";
                    ((BoxFile)Boxes["l" + index]).SetBackColor(Color.Transparent);
                    //((BoxFile)Boxes["l" + index]).Description.OnWheel -= (box, delta) => MouseWheel(box, delta);
                    //((BoxFile)Boxes["l" + index]).Frame.OnWheel -= (box, delta) => MouseWheel(box, delta);
                    //((BoxFile)Boxes["l" + index]).State.OnWheel -= (box, delta) => MouseWheel(box, delta);
                }
      
                int viewRow = 0;
                //QB.Logger.Info("--------------------------Update");
                //QB.Logger.Info("Files        " + Files.Count);
                //QB.Logger.Info("Uris         " + Uris.Count);
                //QB.Logger.Info("FirstVisible " + FirstVisible);
                //QB.Logger.Info("Rows         " + Rows);

                for (int i = FirstVisible; i < Files.Count; i++)
                {
                  
                    int fileIndex = i;
                    //QB.Logger.Info("Index     " + fileIndex);
                    //QB.Logger.Info("viewRow   " + viewRow);
                    //QB.Logger.Info("URI   " + Uris[fileIndex]);

                    ((BoxFile)Boxes["l" + viewRow]).FileName = Files[fileIndex];
                    ((BoxFile)Boxes["l" + viewRow]).Uri = Uris[fileIndex];
                    ((BoxFile)Boxes["l" + viewRow]).State.Icon = "fa:file";
                    ((BoxFile)Boxes["l" + viewRow]).SetBackColor(BackColor.ChangeBrightness(50));
                    ((BoxFile)Boxes["l" + viewRow]).Description.Clickable = true;
                    //((BoxFile)Boxes["l" + viewRow]).Description.OnWheel += (box, delta) => MouseWheel(box, delta);
                    //((BoxFile)Boxes["l" + viewRow]).Frame.OnWheel += (box, delta) => MouseWheel(box, delta);
                    //((BoxFile)Boxes["l" + viewRow]).State.OnWheel += (box, delta) => MouseWheel(box, delta);

                    if (viewRow == Rows - 1) break;

                    viewRow++;
                }
                foreach (var box in Boxes.Values)
                {
                    if (SelectedFile == "") return;
                    if(box is BoxFile)
                    {
                        BoxFile b = box as BoxFile;
                        if (SelectedFile == b.FileName)
                        {
                            b.SetBackColor(Color.Orange);
                        }
                    }
                }

                double x;
                double y;

                switch (Position.ToLower())
                {
                    case "center":
                        {
                            QB.Logger.Info("Dialog is centered");
                            y = (200 - Height) / 2;
                            x = (300 - Width) / 2;
                            Frame.Left = x;
                            Frame.Top = y;
                            QB.Logger.Info("Dialog is centered: " + Frame.Bounds.X + " / " + Frame.Bounds.Y);
                            break;
                        }
                    case "left":
                        {
                            Frame.Left = 0;
                            break;
                        }
                    case "right":
                        {
                            Frame.Left = 300 - Width;
                            break;
                        }
                }





            }
            catch(Exception ex)
            {
                QB.Logger.Error(Name + " '" + sender + "' Failed to updateFolderView" + "\r\n" + ex.Message);
            }
        }

        
        void CancelClick()
        {
            
            Menu.Visible = false;
            TargetFolder.Visible = false;
            Navigation.Visible = false;
            Visible = false;

            CancelAction?.Invoke();

        }

        void OkClick()
        {
            Menu.Visible = false;
            TargetFolder.Visible = false;
            Navigation.Visible = false;
            Visible = false;

            OkAction?.Invoke();
        }

        public void Show()
        {
            FirstVisible = -1;
            SelectedFile = "";
            Update("show");

            Visible = true;
            Menu.Visible = true;
            TargetFolder.Visible = true;
            Navigation.Visible = true;
            if (!ShowClose)
            {
                Menu.Boxes[0].Icon = "";
                Close.Clickable = false;
            }
            Select.Visible = ShowSelect;


        }


    }
}
