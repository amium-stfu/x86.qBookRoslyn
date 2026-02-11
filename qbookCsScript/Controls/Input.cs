using QB.UI;
using System.Text;
using System.Web.UI.WebControls;

namespace QB.Controls
{
    public class Input : Control
    {

        public StringBuilder Value { get; set; }
        public string Unit { get; set; }

        public Input(string name, string text = null, StringBuilder value = null, string unit = "", double x = 0, double y = 0, double w = 30, double h = 15, ClickEventHandler onClick = null) : base(name, x: x, y: y, w: w, h: h)
        {
            if (text == null)
                Text = "#" + name;
            else
                Text = text;

            Value = value;  
            Unit = unit;

            if (onClick != null)
                this.OnClick += onClick;

            this.OnClick += Input_OnClick;
            this.Clickable = true;           
        }

        private void Input_OnClick(Control s)
        {
            InputDialog dialog = new InputDialog();
            dialog.ShowDialog();
            if (Value != null)
            {
                Value.Clear();
                Value.Append(dialog.Value);
            }
           
         //   throw new System.NotImplementedException();
        }

        internal override void Render(Control parent)
        {
            if ((ParentPanel != null) && !ParentPanel.Visible)
                return;


            Draw.Text(Text, Bounds.X+5, Bounds.Y + Bounds.H / 2, 0, Draw.fontHeader2, 
                System.Drawing.Color.Black, System.Drawing.ContentAlignment.MiddleLeft);
            if (Value != null)
            {
                Draw.Text(Value.ToString(), Bounds.X + Bounds.W / 5 * 4, Bounds.Y + Bounds.H / 2, 0, Draw.fontHeader1,
                System.Drawing.Color.Black, System.Drawing.ContentAlignment.MiddleRight);
            }

            Draw.Text(Unit, Bounds.X + Bounds.W / 5 * 4, Bounds.Y + Bounds.H / 2, 0, Draw.fontHeader3,
                System.Drawing.Color.Black, System.Drawing.ContentAlignment.MiddleLeft);

            // Draw.Text(Text, Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2, 0, this.Format.Font, Hover ? System.Drawing.Color.DodgerBlue : Format.ForeColor, align);

            base.Render(parent);
        }
    }
}
