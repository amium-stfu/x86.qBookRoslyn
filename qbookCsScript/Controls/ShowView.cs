using QB.Amium.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;
//using static QB.Controls.Projects;
//using QB.Amium.Controls;

namespace QB.Controls
{
    public class ShowView : LuiView
    {
        public TableView ActivityTableView = new TableView("ActivityTableView", 105, 5, 175, 160);

        public Panel ProjectPanel = new Panel("Projects", 5, 10, 25, 155);
        public Panel JobPanel = new Panel("Jobs", 35, 10, 65, 155);
        public Panel StepPanel = new Panel("Steps", 105, 10, 65, 155);
        public Panel ActivityPanel = new Panel("Activity", 70, 10, 30, 155);

        public ShowView(string name) : base(name)//, x:x, y:y, w:w, h:h)
        {
            // Buttons.luiView = this;
            double x = 0;
            double y = 0;
            double w = 280;
            double h = 180;
            this.Bounds = new Rectangle(x, y, w, h);
            StatusPanel = new Panel("Status", 105, 5, 30, 160);
            DataPanel = new Panel("Data", 0, 60, 30, 85);
            ControlPanel = new Panel("Control", 5, 168, 105, 10);
            Chart = new Chart("Chart", 70, 5, 210, 160);
            LogPanel = new Log("Log", 115, 168, 160, 10);

            StatusPanel.Visible = false;
            DataPanel.Visible = false;
            Chart.Visible = false;

            ProjectPanel.BackColor = "AliceBlue";
            JobPanel.BackColor = "AliceBlue";
            StepPanel.BackColor = "AliceBlue";
            ActivityTableView.Visible = false;            

            NavPanel = new Panel("Nav", 175, 135, 100, 30);
            TextPanel = new Panel("Text", 175, 10, 100, 120);
            MediaPanel = new Panel("Media", 10, 10, 160, 125);
            MediaPanel.Visible = false;            
        }

        public void HandleClick(PointF point)
        {
            try
            {
                double deltaX = point.X - Bounds.X;
                double deltaY = point.Y - Bounds.Y;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"#EX: " + ex.Message, "ERROR");
            }
        }


        internal override void Render(Control parent)
        {
            System.Drawing.Pen pen = Draw.GetPen2(System.Drawing.Color.DarkGray, 0.2);
            Draw.Rectangle(pen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            base.Render(parent);
        }
    }
}
