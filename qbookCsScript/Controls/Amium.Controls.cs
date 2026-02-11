
using QB.Controls;
using System;

namespace QB.Amium.Controls
{
    public class LuiView : View
    {
        public Panel StatusPanel = null;
        public Panel DataPanel = null;
        public Panel ControlPanel = null;
        public Panel NavPanel = null;
        public Panel MediaPanel = null;
        public Panel TextPanel = null;
        public Log LogPanel = null;
        public Chart Chart = null;

        public LuiView(string name) : base(name)
        {
            Buttons.luiView = this;
        }
    }

    public class DefaultLuiView : LuiView
    {

        public DefaultLuiView(string name) : base(name)
        {

            var stackTrace = new System.Diagnostics.StackTrace();
            var callerFrame = stackTrace.GetFrame(1); // 0 = aktuelle Methode, 1 = Aufrufer
            var callerType = callerFrame.GetMethod()?.DeclaringType;

            Directory = callerType?.Namespace.Replace("Definition", "");
 



            //    Buttons.luiView = this;
            double x = 0;
            double y = 0;
            double w = 280;
            double h = 180;
            this.Bounds = new Rectangle(x, y, w, h);
            StatusPanel = new Panel("Status", 0, 5, 90, 10) {Directory = Directory };
            DataPanel = new Panel("Data", 0, 20, 90, 125) { Directory = Directory };
            ControlPanel = new Panel("Control", 0, 150, 90, 30) { Directory = Directory };
            Chart = new Chart("Chart", 100, 5, 180, 140) { Directory = Directory };
            LogPanel = new Log("Log", 100, 150, 180, 30) { Directory = Directory };
        }
    }
}
