using System.Drawing;
//using System.Drawing;

namespace QB.Amium
{

    public class State : Item
    {
        public static State Ready = new State("Ready", "Ready", udlValue: 0x22, akValue: "STBY", color: Color.Gray);
        public static State Sample = new State("Sample", "Sample", udlValue: 0x23, akValue: "SMGA", color: Color.Blue);

        public static State FromUdlValue(int udlValue)
        {
            if (udlValue == 0x22)
                return Ready;
            if (udlValue == 0x23)
                return Sample;
            return Ready;
        }

        public static State FromAkValue(string akValue)
        {
            if (akValue == "STBY")
                return Ready;
            if (akValue == "SMGA")
                return Sample;
            return Ready;
        }



        public State(string name, string text = null, int udlValue = -1, string akValue = "????", string colorName = null, System.Drawing.Color? color = null) : base(name, null)
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::new State({name})");

            Name = name;

            if (text == null)
                Text = "#" + name;
            else
                Text = text;

            Color = color ?? Color.Black;
            if (colorName != null)
                Color = Misc.ParseColor(colorName);

            UdlValue = udlValue;
            AkValue = akValue;
        }

        public string Text;
        public int UdlValue;
        public string AkValue;


        public System.Drawing.Color Color { get; set; } = Color.Black;
        public override string ToString()
        {
            return $"{UdlValue:F5}";
        }

        ~State()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::~State({this.Name})");
        }

        public override void Destroy()
        {
            if (QB.Logger.LibDebugLevel >= 2) QB.Logger.Debug($"::destroy State({this.Name})");
        }
    }

}