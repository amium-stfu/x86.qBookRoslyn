using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB
{
    public class DbcValue
    {
        public DbcValue()
        {

        }

        //public ZedGraph.RollingPointPairList line = new ZedGraph.RollingPointPairList(5000);

        public DbcMessage Parent { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Color { get; set; }
        public string Panel { get; set; }
        /*
        public void SetValue(double value)
        {
            Value = value;
            if (!double.IsNaN(value))
                line.Add(new ZedGraph.XDate(DateTime.Now), value);
        }
        */
        public int Interval { get; set; }

        public string ValueUnit
        {
            get
            {
                if (ByteDecoder.Ieee)
                    return Value.ToString("0.00").PadLeft(7) + " " + Unit.PadRight(4);

                if (ByteDecoder.Gain < 0.001)
                    return Value.ToString("0.0000").PadLeft(7) + " " + Unit.PadRight(4);
                else if (ByteDecoder.Gain < 0.01)
                    return Value.ToString("0.000").PadLeft(7) + " " + Unit.PadRight(4);
                else if (ByteDecoder.Gain < 0.1)
                    return Value.ToString("0.00").PadLeft(7) + " " + Unit.PadRight(4);
                else if (ByteDecoder.Gain < 1)
                    return Value.ToString("0.0").PadLeft(7) + " " + Unit.PadRight(4);
                else
                    return Value.ToString("0").PadLeft(7) + " " + Unit.PadRight(4);

            }
        }

        public ByteDecoder ByteDecoder { get; set; }

        public override string ToString()
        {
            return ((ByteDecoder.Mux != null) ? "M" + ByteDecoder.MuxValue + ":" : "") + Name + " (" + ByteDecoder.MsgOffset + "/" + ByteDecoder.MsgBits + ")";
        }
    }
    public class DbcMessage
    {
        public DbcMessage()
        {

        }
        public string Name { get; set; }
        public UInt32 Id { get; set; }
        public int Dlc { get; set; }

        public Dictionary<string, DbcValue> DbcValues = new Dictionary<string, DbcValue>();

        public bool Has(string name)
        {
            return DbcValues.ContainsKey(name);
        }
        public double ValueOf(string name)
        {
            if (DbcValues.ContainsKey(name))
                return DbcValues[name].Value;
            return double.NaN;
        }
    }
}
