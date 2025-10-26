using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Net
{
    public class SignalMeasurementData
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public double Epoch { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
