namespace QB.Controls
{
    public class Icon : Control
    {
        public Icon(string name, string text = null, double x = 0, double y = 0, double w = 30, double h = 15) : base(name, x: x, y: y, w: w, h: h)
        {
            if (text == null)
                Text = "#" + name;
            else
                Text = text;

            this.Clickable = false;
        }

        internal override void Render(Control parent)
        {
            base.Render(parent);
        }
    }
}
