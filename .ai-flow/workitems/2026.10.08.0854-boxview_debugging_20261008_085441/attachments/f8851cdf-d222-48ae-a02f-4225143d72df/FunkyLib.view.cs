namespace DefinitionFunkyLib
{
    //<CodeStart>
       using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Drawing;

    using System.Text.RegularExpressions;
    using QB;
    using QB.UI;
    using QB.Logging;
    using QB.Controls;
    using QB.Net;
    using QB.Helpers;
    using QB.Automation;
    using QB.Amium.Controls;
    using System.Threading.Tasks;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using DefinitionRessources;
    using DefinitionTestSystem;

    public class FunkyView : BoxView
    {
        public BoxButton ShowDeviceList;
        public BoxFlowPanel DataView;
        public BoxTable SignalView;

        public BoxFolderView ResultFolder;
        public BoxTable Config;
        public BoxTable ScriptEditor;
        public BoxTable ViewControl;
        public BoxTable ChartPanel;

        public BoxFlowPanel Setup;

        public BoxTable LoadConfig;

        public BoxTableView AlertsTable;
        public BoxTableView ViewResult;
        public BoxTable Control;
        public BoxTable ProcessData;
        public BoxLogView LogView;

        public BoxProcessLogView ProcessLogView;

        public Panel TestSchema;
        public Panel DeviceSchema;
       

        public CameraControl Camera;
        public TLiteHCameraControl Tcamera;
        public Panel FileExplorer;

        public Panel DevicePicture;
        public BoxButton Status;

        public BoxDialog Dialog;
        public BoxDialogWithParameters DialogWithParameters;
        public BoxFolderView DialogFile;


        public System.Drawing.Color SelectColor = "Orange".ToColor();
        public System.Drawing.Color UnselectColor = "Transparent".ToColor();
        public System.Drawing.Color BackColor = "#EEEEEE".ToColor();
        public System.Drawing.Color ButtonColor = "#DDDDDD".ToColor();
        public ChartExtended Chart;

        public ChartExtended LoadChart;
        int BodySelection;

        QB.Timer timer = new QB.Timer("viewUpdateTimer", 200);

        public DefinitionTestSystem.CellTest Test;

        public Table TblDutData = new Table("tblViewDutSpec");
        public Table TblRefData = new Table("tblViewRefSpec");

        public Table TblDataPoints = new Table("tblDataPoints");
        public Table TblResult = new Table("tblResult");

        double formatOffset = 0;

        public BoxFlowPanel AlertsPanel;

        public SystemAlerts Alerts;
        public FunkyView(string name, DefinitionTestSystem.CellTest test, string page, SystemAlerts alerts, string format = "A4") : base(name, page)
        {
            
            
            Test = test;
            Page = page;
            Alerts = alerts;
            timer.OnElapsed = (t, ea) => idle(t, ea);
            TblDutData.Add(",");
            TblRefData.Add(",");
            TblDataPoints.Add(",,,,,,");
            TblResult.Add(",,,,,,");

            switch (format)
            {
                case "A4":
                    formatOffset = 0;
                    break;
                case "16/9":
                    formatOffset = -43;
                    break;
                case "16/10":
                    formatOffset = -25;
                    break;

            }



            //Status
            Add(Status = new BoxButton("Status", id: "", x: 7, icon: "fa:question", description: "Status", y: 23, w: 75, h: 13, backColor: "transparent", iconSizeFactor: 0.8));

            //SignalView
            Add(SignalView = new BoxTable("SignalView", x: 10, y: 40, h: 120 + formatOffset, w: 64, columns: 9, rows: 40, gap: 0.2, backColor: "white", page: Page));

            //DevicePicture
            string uri;
            DevicePicture = new Panel("DevicePicture", x: 0, y: 145 + formatOffset, h: 35, w: 65)
            {
                Directory = Page,
                Visible = false
            };
            // uri = System.IO.Path.Combine(QB.Book.DataDirectory, test.Name, "DevicePicture.jpg");
            // if (System.IO.File.Exists(uri)) DevicePicture.BackgroundImage = Image.FromFile(uri);


            //ViewControl
            bool LedStyle = true;
            double X = 77.5;
            double Y = 25;
            double W = 195.5;
            double H = 135 + formatOffset;

            if (LedStyle)
                ButtonColor = BackColor;

            Add(ViewControl = new BoxTable("ViewControl", x: X + W + 2, y: 25, h: H, w: 15, columns: 1, rows: 10, gap: 0.2, backColor: BackColor, page: Page));
            Add(new BoxButton(name: "chart", icon: "fa:chart-line", id: "1", locked: false, onClick: (b) => SwitchView(1), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 1, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "info", icon: "fa:info", id: "2", locked: false, onClick: (b) => SwitchView(2), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 2, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "testSchema", icon: "fa:fill-drip", id: "3", locked: false, onClick: (b) => SwitchView(3), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 3, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "demo",icon: "fa:basketball", id: "4", locked: false, onClick: (b) => SwitchView(4), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 4, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "config", icon: "fa:gears", id: "5", locked: false, onClick: (b) => SwitchView(5), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 5, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "dataPoint", icon: "fa:triangle-exclamation", id: "6", locked: false, onClick: (b) => SwitchView(6), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 6, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "result", icon: "fa:list-check", id: "7", locked: false, onClick: (b) => SwitchView(7), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 7, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "camera", icon: "fa:camera", id: "8", locked: false, onClick: (b) => SwitchView(8), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 8, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "explorer", icon: "fa:folder-open", id: "9", locked: false, onClick: (b) => SwitchView(9), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 9, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);
            Add(new BoxButton(name: "free", icon: "fa:camera:red", id: "10", locked: true, onClick: (b) => SwitchView(10), ledStyle: LedStyle, backColor: ButtonColor), 
            column: 1, row: 10, columnSpan: 1, rowSpan: 1, boxTable: ViewControl);

            //Chart View
            Add(Chart = new ChartExtended("bodyChart ", x: X, y: Y, h: H, w: W, page: Page, useNormName: false));
            Add(LoadChart = new ChartExtended("loadChart ", x: X, y: Y, h: H - 60, w: W, page: Page, useNormName: false));
            LoadChart.Visible = false;

            Add(LoadConfig = new BoxTable("loadConfig", x: X, y: Y + H - 60, h: 60, w: W, columns: 20, rows: 20, gap: 0.2, backColor: "#F6F6F6", page: Page));
            LoadConfig.Visible = false;


            //Syntax Chart.Add(Singal,Axes)
            //Chart.Add(Client.Diameter, 1);
            //Chart.Add(Client.FiSample, 2);


            //Info
            // ViewRefSpec = new BoxTableView("ViewRefSpec", TblRefData, x: X, y: Y, h: H, w: W / 2 - 2.5, columnConfig: "60%,40%", headerText: "Ref Parameter,Value", page: Page, rows: 25, gap: 0.2, backColor: "#F6F6F6");
            // ViewDutSpec = new BoxTableView("ViewDutSpec", TblDutData, x: X + W / 2 + 2.5, y: Y, h: H, w: W / 2 - 2.5, columnConfig: "60%,40%", headerText: "Dut Parameter,Value", page: Page, rows: 25, gap: 0.2, backColor: "#F6F6F6");
            // ViewRefSpec.Visible = false;
            // ViewDutSpec.Visible = false;

            Add(ProcessLogView = new BoxProcessLogView("ProcessLog", x: X, y: Y, h: H, w: W, rows: 20, gap: 0.2,target:Test.Logger, page: Page, backColor: "#F6F6F6"));
            ProcessLogView.Visible = false;


            //  Add(LogView = new BoxLogView("Notes", x: X, y: Y, h: H, w: W, rows: 20, gap: 0.2, page: Page, backColor: "#F6F6F6"));
            // LogView.Uri = Test.Log.File;
            // LogView.ResetWatcher();
            // LogView.Visible = false;





      
            //Result
            ViewResult = new BoxTableView("result", TblDataPoints, x: X, y: Y, h: H, w: W, columnConfig: "30%,17.5%,17.5%,17.5%,17.5%", headerText: "Parameter,Read,Set,Limit,Result", page: Page, rows: 30, gap: 0.2, backColor: "white");
            ViewResult.Visible = false;

            // Testschema View
            TestSchema = new Panel("TestSchema", x: X - 10, y: 5, h: H, w: W)
            {
                Directory = Page,
                Visible = false
            };



            uri = System.IO.Path.Combine(QB.Book.DataDirectory, test.Name, "SchemaPicture.jpg");

            QB.Logger.Info("Picture Uri " + uri);

            if (System.IO.File.Exists(uri)) TestSchema.BackgroundImage = Image.FromFile(uri);
            

            //DeviceShema View
            DeviceSchema = new Panel("DeviceSchema", x: X - 10, y: 5, h: H, w: W)
            {
                Directory = Page,
                Visible = false
            };


            //Config View 
            Add(Config = new BoxTable("ConfigTable", x: X, y: Y, h: H + formatOffset, w: W, columns: 16, rows: 35, gap: 0.2, backColor: "#F6F6F6", page: Page));
            Config.SetBackColor("#EEEEEE");
            //		Add(new BoxStringSignal(name:"resultFolder", target:Cycler.LogFolder), columnSpan:4, boxFlowPanel:Config);
            //		Add(new BoxButton(name:"load", id:"", icon:"fa:download", description:"Load Battery from Folder", locked:false, onClick:(b) => LoadBatteryData(), iconSizeFactor:0.8), columnSpan:2, boxFlowPanel:Config);
            //	Add(new BoxButton(name:"new", id:"", icon:"fa:square-plus", description:"Add new csv", locked:false, onClick:(b) => Cycler.Battery.LoadCsvFilesBatch(), iconSizeFactor:0.8), columnSpan:2, boxFlowPanel:Config);
            //Add(new BoxButton(name:"saveDevices", id:"", icon:"fa:square-plus", description:"Save Devicelist", locked:false, onClick:(b) => SaveDeviceList(), iconSizeFactor:0.8), columnSpan:2, boxFlowPanel:Config);

                 //Config View 
            Add(ScriptEditor = new BoxTable("DemoTable", x: X, y: Y, h: H + formatOffset, w: W, columns: 10, rows: 12, gap: 0.2, backColor: "rgb(246, 246, 246)", page: Page));
          //  ScriptEditor.SetBackColor("#EEEEEE");
             ScriptEditor.SetBackColor("#969595");



            //Camera View
            Camera = new CameraControl("Camera", x: X - 10, y: 5 + formatOffset, h: H, w: W)
            {
                Directory = Page,
                Visible = false
            };
            Camera.SelectedDeviceId = 0;
            Camera.StartCamera();

            //FileExplorer View
            //Result Folder
            string ResultPath = System.IO.Path.Combine(Test.FolderResult);
            Add(ResultFolder = new BoxFolderView("results", "Results:", x: X, y: Y, h: H, w: W, rows: 10, page: Page, folder: ResultPath, extension: "*.*", backColor: ButtonColor, position: ""));
            ResultFolder.OnSelectClick((b) => System.Diagnostics.Process.Start("explorer.exe", ResultFolder.SelectedUri));
            ResultFolder.Visible = false;
            ResultFolder.ShowClose = false;
            ResultFolder.Select.Icon = "fa:file-pen";

            //Views end

            //DUT data
            Add(DataView = new BoxFlowPanel("DeviceView", x: 10, y: 165 + formatOffset, h: 35, w: 125, columns: 8, rows: 5, gap: 0.2, backColor: "#EEEEEE", page: Page));
            Add(new BoxLabel("label", text: "Battery Data", fontSize: 8, backColor: "transparent"), columnSpan: 7, boxFlowPanel: DataView);
            Add(new BoxButton(name: "", icon: "fa:ellipsis", locked: false, backColor: "transparent",onClick: (b) => Test.Dut.LoadFromJson(sender:"User")), columnSpan: 1, boxFlowPanel: DataView);

          

            //Control
            Add(Control = new BoxTable("control", x: 138, y: 165 + formatOffset, h: 35, w: 152, columns: 12, rows: 7, gap: 0.2, page: Page, backColor: ButtonColor));
            Add(new BoxLabelMultiLine("Testsetting", headline: "", content: "", backColor: BackColor, locked: true), column: 1, row: 1, columnSpan: 12, rowSpan: 3, boxTable: Control);
            //Add(new BoxButton(name:"menu", id:"", icon:"fa:ellipsis", locked:false, backColor:"transparent", onClick:(b) => Settings.Show()), column:8, row:1, columnSpan:1, rowSpan:1, boxTable:Control);
            Add(new BoxButton(name: "menu", id: "", icon: "fa:ellipsis", locked: false, backColor: "transparent", onClick: (b) => SequenceMenu.Visible = !SequenceMenu.Visible), column: 12, row: 1, columnSpan: 1, rowSpan: 1, boxTable: Control);

            CreateSequenceExplorerDialog();
            CreateSequenceMenu();
            SwitchView(1);


            //Dialogs
            Add(Dialog = new BoxDialog(name: "dialog", page: Page, backColor: ButtonColor, border: 1, borderColor: "Black", h: 50, w: 150, position: "center"));
            Add(DialogWithParameters = new BoxDialogWithParameters("dialogWithParameters", position: "center", w: 100, backColor: BackColor, page: Page, border: 1, borderColor: "Black"));
            Add(DialogFile = new BoxFolderView("dialogFile", "Select file:", h: 100, w: 120, rows: 10, page: Page, folder: @"C:", extension: "*.*", backColor: ButtonColor));

                         //DataPoints
            AlertsTable = new BoxTableView("dataPoint", Alerts.alertTable, x: X, y: Y, h: H-21, w: W, columnConfig: "20%,20%,60%", headerText: "Raised,Cleared,Description", page: Page, rows: 20, gap: 0.2, backColor: "white");
            AlertsTable.Visible = false;

            AlertsPanel = new BoxFlowPanel("alertsPanel", x: X, y: Y+H-20, h: 20, w: W,columns:4,rows:2, page: Page, gap: 0.2);
            Add(new BoxStringSignal(name:"lastAlert", target:Alerts.LastAlert, locked:false, backColor:"White"), columnSpan: 4, boxFlowPanel: AlertsPanel);
            Add(new BoxButton(name: "clearAlerts", icon: "fa:arrow-rotate-left", description: "Reset Alerts", id: "", locked: false, iconSizeFactor: 0.8, onClick: (b) => ResetAlerts(), backColor: "White"), columnSpan: 1, boxFlowPanel: AlertsPanel);
            Add(new BoxButton(name: "clearAllCleared", icon: "fa:broom", description: "Remove Cleared Alerts", id: "", locked: false, iconSizeFactor: 0.8, onClick: (b) => ClearAllClearedAlerts(), backColor: "White"), columnSpan: 1, boxFlowPanel: AlertsPanel);
            Add(new BoxButton(name: "onlyActive", icon: "fa:filter", description: "Show only active Alerts", id: "", locked: false, iconSizeFactor: 0.8, onClick: (b) => { Alerts.OnlyActive = !Alerts.OnlyActive;  AlertsTable.Update(); }), columnSpan: 1, boxFlowPanel: AlertsPanel);
           
            AlertsPanel.Visible = false;
            DialogFile.Visible = false;
        }

        public virtual void SelectSequenceFromFolder()
        {
            SequenceMenu.Visible = false;

            QB.Logger.Info("Load Sequencefile: " + DialogFile.SelectedUri);
            string json = System.IO.File.ReadAllText(DialogFile.SelectedUri);
            Dictionary<string, object> read = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            QB.Logger.Info("Sequence Name " + read["Name"]);
            Test.SequenceSelected = Test.Sequences[read["Name"].ToString()];
            Test.SequenceSelected.SetSettings(json);
            Test.SequenceSelected.UpateSequenceDisplay();
        }

        void ResetAlerts()
        {
         //   Alerts.SetAllAlertsCleared();
            AlertsTable.Update();
           
        }

        void ClearAllClearedAlerts()
        {
       //     Alerts.ClearAllClearedAlerts();
            AlertsTable.Update();
        }


        public virtual void Run()
        {
            timer.Run();
        }


        public void SwitchView(int s)
        {
            BodySelection = s;
            QB.Logger.Info(Name + "SwitchTo " + s);
            ViewControl.Boxes["chart"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["info"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["testSchema"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["demo"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["config"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["dataPoint"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["result"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["camera"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["explorer"].SetSelectColor(ButtonColor);
            ViewControl.Boxes["free"].SetSelectColor(ButtonColor);


         
            Chart.Visible = false;
            Config.Visible = false;
            ProcessLogView.Visible = false;
            ScriptEditor.Visible = false;

            LoadChart.Visible = false;
            LoadConfig.Visible = false;
           
            TestSchema.Visible = false;
            DeviceSchema.Visible = false;
            if(Camera != null) Camera.Visible = false;
            ResultFolder.Visible = false;

            

            if(AlertsTable != null)
            AlertsTable.Visible = false;
             if(AlertsPanel != null)
            AlertsPanel.Visible = false;
          

            ViewResult.Visible = false;
            if(Tcamera != null) Tcamera.Visible = false;

            if (s == 1)
            {
                Chart.Visible = true;
                ViewControl.Boxes["chart"].SetSelectColor(SelectColor);
            }
            if (s == 2)
            {
                ProcessLogView.Visible = true;
                ViewControl.Boxes["info"].SetSelectColor(SelectColor);
            }
            if (s == 3)
            {
                ViewControl.Boxes["testSchema"].SetSelectColor(SelectColor);
                LoadChart.Visible = true;
                LoadConfig.Visible = true;
               // TestSchema.Visible = true;
            }
            if (s == 4)
            {
                ViewControl.Boxes["demo"].SetSelectColor(SelectColor);
                ScriptEditor.Visible = true;
                ScriptNameBox.Frame.Clickable = true;
                ScriptEditor.Frame.Clickable = true;
                ScriptNameBox.ZOrder = 100;
                ScriptEditor.ZOrder = 100;
            }
            if (s == 5)
            {
                ViewControl.Boxes["config"].SetSelectColor(SelectColor);
                Config.Visible = true;
            }

           
            if (s == 6)
            {
                ViewControl.Boxes["dataPoint"].SetSelectColor(SelectColor);
                AlertsTable.Visible = true;
                AlertsPanel.Visible = true;
                AlertsTable.Update();

            }
            if (s == 7)
            {
                ViewControl.Boxes["result"].SetSelectColor(SelectColor);
                ViewResult.Visible = true;
                ViewResult.Update();

            }
            if (s == 8)
            {
                ViewControl.Boxes["camera"].SetSelectColor(SelectColor);
                Camera.Visible = true;

            }
            if (s == 9)
            {
                ResultFolder.Show();
            }
            if (s == 10)
            {
                ViewControl.Boxes["free"].SetSelectColor(SelectColor);
                Tcamera.Visible = true;
            }
        }

        public void idle(QB.Timer t, TimerEventArgs ea)
        {
            UpdateView();
        }



        public virtual void UpdateView()
        {
            UpdateButtons();
        }


        void UpdateButtons()
        {
            ((BoxButton)AlertsPanel.Boxes["onlyActive"]).SetBackColor(Alerts.OnlyActive ? SelectColor : "White");
            ((BoxButton)ViewControl.Boxes["dataPoint"]).SetBackColor(Alerts.HasActiveAlerts() ? "tomato" : ButtonColor);
           
           
            if (Test.SequenceSelected == null)
            {
                //    ((BoxButton)Control.Boxes["stop"]).SetBackColor("lightblue");
                return;
            }
            else
            {
                ((BoxLabelMultiLine)Control.Boxes["TestSetting"]).Headline.Text = Test.SequenceSelected.Display.Text;
                ((BoxLabelMultiLine)Control.Boxes["TestSetting"]).Content.Text = Test.SequenceSelected.Display.Value;
            }


            if(Test.SequenceSelected.Status == SequencerState.Running)
            {
                
                
                ((BoxButton)Control.Boxes["start"]).SetBackColor("Orange");
                ((BoxButton)Control.Boxes["stop"]).SetBackColor(ButtonColor.ChangeBrightness(60));
      
            }
          
            if (Test.SequenceSelected.Status == SequencerState.Paused)
            {
                ((BoxButton)Control.Boxes["start"]).SetBackColor(ButtonColor.ChangeBrightness(60));
                ((BoxButton)Control.Boxes["stop"]).SetBackColor(ButtonColor.ChangeBrightness(60));
    
            }

            if(Test.SequenceSelected.Status == SequencerState.Completed || Test.SequenceSelected.Status == SequencerState.Aborted)
            {
                ((BoxButton)Control.Boxes["start"]).SetBackColor(ButtonColor.ChangeBrightness(60));
                ((BoxButton)Control.Boxes["stop"]).SetBackColor("Orange");
           
            }


    
        }

         


        //Sequence Menu ...
        BoxFlowPanel SequenceMenu;
                void CreateSequenceMenu()
        {

            SequenceMenu = new BoxFlowPanel("SequenceMenu", x: 250, y: 155 + formatOffset, h: 10, w: 40, columns: 4, rows: 1, gap: 0.2, backColor: BackColor, page: Page);

            Add(new BoxButton(name: "save", id: "", icon: "fa:floppy-disk", locked: false, backColor: SelectColor.ChangeBrightness(0), onClick: (b) => Test.SequenceSelected.ExportSettings(Test.FolderSequences)), columnSpan: 1, boxFlowPanel: SequenceMenu);
            Add(new BoxButton(name: "load", id: "", icon: "fa:folder", locked: false, backColor: SelectColor.ChangeBrightness(0), onClick: (b) => loadSequenceFile()), columnSpan: 1, boxFlowPanel: SequenceMenu);
            Add(new BoxButton(name: "edit", id: "", icon: "fa:gear", locked: false, backColor: SelectColor.ChangeBrightness(0), onClick: (b) => EditSequence()), boxFlowPanel: SequenceMenu);
            Add(new BoxButton(name: "template", id: "", icon: "fa:list", locked: false, backColor: SelectColor.ChangeBrightness(0), onClick: (b) => selectTemplate()), columnSpan: 1, boxFlowPanel: SequenceMenu);

            SequenceMenu.Visible = false;
        }

        void loadSequenceFile()
        {

            DialogFile.Show("Select Sequence file:", Test.SequencesFolder(), "*.json", okAction: () => SelectSequenceFromFolder());
        }
        public void EditSequence()
        {
            if (Test.SequenceSelected == null) return;

            DialogWithParameters.Parameters = Test.SequenceSelected.Settings;
            DialogWithParameters.Frame.ZOrder = 20;
            DialogWithParameters.ZOrder = 100;
            DialogWithParameters.Show("Sequence edit:", okAction: () => ConfirmSequenceSettings());

        }

        void selectTemplate()
        {
            SequenceMenu.Visible = false;
            SequenceMenu.Visible = false;
            ShowSequenceExplorer();
            UpdateSequenceExplorer();
        }
        //----------------------------------------------

        //SequenceExplorer Dialog
        BoxFlowPanel SequenceExplorer;
        void CreateSequenceExplorerDialog()
        {

            int col = 16;
            SequenceExplorer = new BoxFlowPanel("sequences", x: 120, y: 20, h: 140, w: 80, columns: 2, rows: col, gap: 0.2, backColor: BackColor, page: Page);

            Add(new BoxLabel(name: "label", text: "Sequence Templates", locked: false, backColor: "transparent"), columnSpan: 2, boxFlowPanel: SequenceExplorer);
            for (int i = 0; i < col - 2; i++)
            {
                int p = i;

                Add(new BoxButton(name: "p" + i, id: i.ToString(), description: "", locked: false, backColor: BackColor.ChangeBrightness(-20), onClick: (b) => SelectSequence(p)), columnSpan: 2, boxFlowPanel: SequenceExplorer);
            }
            Add(new BoxButton(name: "close", id: "", icon: "fa:xmark", description: "Cancel", locked: false, backColor: BackColor.ChangeBrightness(-20), onClick: (b) => SequenceExplorer.Visible = false), columnSpan: 2, boxFlowPanel: SequenceExplorer);

            SequenceExplorer.ZOrder = -1;
            SequenceExplorer.Visible = false;
        }

        void UpdateSequenceExplorer()
        {
            int i = 0;
            foreach (UIBox b in SequenceExplorer.Boxes.Values)
            {
                if (b.Name.StartsWith("p"))
                {
                    ((BoxButton)b).Visible = false;
                }
            }
            i = 0;
            foreach (FunkySequencer seq in Test.Sequences.Values)
            {

                string key = "p" + i;
                ((BoxButton)SequenceExplorer.Boxes[key]).Description.Text = seq.Name;
                ((BoxButton)SequenceExplorer.Boxes[key]).Visible = true;
                i++;
            }
        }

        void SelectSequence(int index)
        {
            Test.SelectSequence(index);
          
            ((BoxLabelMultiLine)Control.Boxes["TestSetting"]).Headline.Text = Test.SequenceSelected.Name;

            SequenceExplorer.Visible = false;
            //   if (Test.SequenceSelected.Settings.Count == 0)
            // {
            //     ConfirmSequenceSettings();
            //     return;
            // }
            EditSequence();
            return;
            DialogWithParameters.Parameters = Test.SequenceSelected.Settings;

            DialogWithParameters.Show("Sequence settings:", width: 150, okAction: () => ConfirmSequenceSettings());
        }

        void ShowSequenceExplorer()
        {
            SequenceMenu.Visible = false;
            SequenceExplorer.Visible = true;
            SequenceExplorer.ZOrder = 100;
            UpdateSequenceExplorer();
        }
        void HideSequenceExplorer()
        {
            SequenceExplorer.ZOrder = -1;
            SequenceExplorer.Visible = false;
            UpdateSequenceExplorer();
        }


        void ConfirmSequenceSettings()
        {
            

            ((BoxLabelMultiLine)Control.Boxes["TestSetting"]).Headline.Text = Test.SequenceSelected.Display.Text;
            ((BoxLabelMultiLine)Control.Boxes["TestSetting"]).Content.Text = Test.SequenceSelected.Display.Value;

             Test.SequenceSelected.UpateSequenceDisplay();
        }


            BoxButton Charge;
        BoxButton Discharge;
        BoxButton MarkerStep;
        BoxButton DischargeToVoltageStep;
        BoxButton ChargeToVoltageStep;
        BoxButton RelaxationStep;
        BoxButton ChargeCCCVStep;
        BoxButton DischargeCCCVStep;
        BoxButton FastChargeStep;
        BoxButton FastDischargeStep;
        BoxTableView SequenceTable;

        BoxTableView ScriptTable;

        BoxStringSignal ScriptNameBox;
        BoxStringSignal SequenceNameBox;

        Table _sequenceTable;
        Table _scriptTable;

        StringSignal ScriptName = new StringSignal("scriptName", "Script Name",value: "<ENTER NAME>");
        StringSignal SequenceName = new StringSignal("sequenceName", "Sequence Name", value: "<ENTER NAME>");

        void resetScriptEditor()
        {
            _sequenceTable.Clear();
            _scriptTable.Clear();

            _sequenceTable.Add("Step");
            _scriptTable.Add("qty,Sequence");
        }

        public void InitializeScriptEditor()
        {
            Color backColor = Color.LightGray;
            _sequenceTable = new Table();
            _scriptTable = new Table();
            Add(ScriptNameBox = new BoxStringSignal(name: "scriptNameBox",target: ScriptName, backColor: backColor), column: 1, row: 1, columnSpan: 3, boxTable: ScriptEditor);
            Add(SequenceNameBox = new BoxStringSignal(name: "sequenceNameBox",target: SequenceName, backColor: backColor), column: 4, row: 1, columnSpan: 3, boxTable: ScriptEditor);
            Add(Charge = new BoxButton(name: "charge",id:"",description: "Charge Step", locked: false, backColor: backColor, onClick: (b) => {ScriptName.Value = "Charge"; }),column:8,row:2, columnSpan: 3, boxTable: ScriptEditor);
            Add(Discharge = new BoxButton(name: "discharge",id:"",description: "Discharge Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:3, columnSpan: 3, boxTable: ScriptEditor);
            Add(MarkerStep = new BoxButton(name: "markerStep",id:"",description: "Marker Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:4, columnSpan: 3, boxTable: ScriptEditor);
            Add(DischargeToVoltageStep = new BoxButton(name: "dischargeToVoltageStep",id:"",description: "Discharge to Voltage Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:5, columnSpan: 3, boxTable: ScriptEditor);
            Add(ChargeToVoltageStep = new BoxButton(name: "chargeToVoltageStep",id:"",description: "Charge to Voltage Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:6, columnSpan: 3, boxTable: ScriptEditor);
            Add(RelaxationStep = new BoxButton(name: "relaxationStep",id:"",description: "Relaxation Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:7, columnSpan: 3, boxTable: ScriptEditor);
            Add(ChargeCCCVStep = new BoxButton(name: "chargeCCCVStep",id:"",description: "Charge CCCV Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:8, columnSpan: 3, boxTable: ScriptEditor);
            Add(DischargeCCCVStep = new BoxButton(name: "dischargeCCCVStep",id:"",description: "Discharge CCCV Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:9, columnSpan: 3, boxTable: ScriptEditor);
            Add(FastChargeStep = new BoxButton(name: "fastChargeStep",id:"",description: "Fast Charge Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:10, columnSpan: 3, boxTable: ScriptEditor);
            Add(FastDischargeStep = new BoxButton(name: "fastDischargeStep",id:"",description: "Fast Discharge Step", locked: false, backColor: backColor, onClick: (b) => { }),column:8,row:11, columnSpan: 3, boxTable: ScriptEditor);


       
            // Add(SequenceTable = new BoxTableView(name: "sequenceTable", description: "Sequence Table", locked: false, backColor: Color.Yellow), column: 4, row: 1, columnSpan: 4, boxTable: ScriptEditor);
            // Add(ScriptTable = new BoxTableView(name: "scriptTable", id: "", description: "Script Table", locked: false, backColor: Color.Green), column: 1, row: 2, columnSpan: 4, boxTable: ScriptEditor);
       
       
        }

    }
   
    //<CodeEnd>
}
