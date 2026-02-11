using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEXhubCommon
{
    static public class Licensing
    {
        //TODO: proprieatry licensing!!!
        //only temporary! must be replaced by "LicenseTool (XERO) or simmilar!!!

        static public bool EnableLicense2 = false;

        public static DateTime? DemoEndTimestamp = null;
        //public static DateTime? DemoEndTimestamp = new DateTime(2021, 03, 27, 22, 09, 00);

        static public bool AuthorizedHardwareOnly = false;
        static public bool IsAuthorizedHardware
        {
            get
            {
                return AuthorizedHardwareIds.Contains(HardwareId);
            }
        }
        static string[] AuthorizedHardwareIds = new string[] { 
            "L1HF09M09K0178BFBFF00860F01", //APC0218 HALE-Laptop
            "GEWY41800A09BFEBFBFF00040651",//APC0001 Fa. FAKT TestPC
        };
        public static string _HardwareId = null;
        public static string HardwareId
        {
            get
            {
                if (_HardwareId == null)
                {
                    _HardwareId = GetHardwareId();
                    //System.IO.File.WriteAllText("c:\\temp\\hw.txt", _HardwareId);
                }
                return _HardwareId;
            }
        }
        public static string GetHardwareId()
        {
            string usbSerial = Amium.Helpers.HardwareInfo.GetUsbDiskSerial();
            if (usbSerial != null)
            {
                //Xero started from USB drive?! use the Stick's serial as the hardware id
                return usbSerial;
            }
            else
            {
                return ""
                    + Amium.Helpers.HardwareInfo.GetBoardSerial().Replace(" ", "")
                    + Amium.Helpers.HardwareInfo.GetProcessorId();
            }
        }
    }
}
