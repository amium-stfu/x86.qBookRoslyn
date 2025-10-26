using System;
using System.Xml.Serialization;

namespace qbook
{
    [Serializable]
    public class oHtml : oControl
    {

        public oHtml()
        {
        }

        public string CodeHtml { get; set; } = "";
        public string CodeCss { get; set; } = "";
        public string CodeSettings { get; set; } = "";
        //public RectangleF OrigBounds = new RectangleF(10, 10, 400, 300); //in mm


        [XmlIgnore]
        public cHtml MyControl;

        [XmlIgnore]
        public oPage MyPage;


        public override void Render()
        {
        }

        public override void Init()
        {
            base.Init();
        }

    }
}
