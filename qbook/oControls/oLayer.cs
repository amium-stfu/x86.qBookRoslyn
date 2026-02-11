using System;
using System.Xml.Serialization;

namespace qbook
{
    [Serializable]
    public class oLayer : oControl
    {
        public oLayer()
        {
        }
        public oLayer(string name, string text) : base(name, text)
        {
        }

        [XmlIgnore]
        public int nr;
    }
}
