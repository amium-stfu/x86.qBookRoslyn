using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amium.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management;

    public static class HardwareInfo
    {
        /// <summary>
        /// Retrieving Processor Id.
        /// </summary>
        /// <returns></returns>
        /// 
        public static String GetProcessorId()
        {

            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            String Id = String.Empty;
            foreach (ManagementObject mo in moc)
            {

                Id = mo.Properties["processorID"].Value.ToString();
                break;
            }
            return Id;

        }
        /// <summary>
        /// Retrieving HDD Serial No.
        /// </summary>
        /// <returns></returns>
        public static String GetHDDSerialNo()
        {
            ManagementClass mangnmt = new ManagementClass("Win32_LogicalDisk");
            ManagementObjectCollection mcol = mangnmt.GetInstances();
            string result = "";
            foreach (ManagementObject strt in mcol)
            {
                result += Convert.ToString(strt["VolumeSerialNumber"]);
            }
            return result;
        }
        /// <summary>
        /// Retrieving System MAC Address.
        /// </summary>
        /// <returns></returns>
        public static string GetMACAddress()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            string MACAddress = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                if (MACAddress == String.Empty)
                {
                    if ((bool)mo["IPEnabled"] == true) MACAddress = mo["MacAddress"].ToString();
                }
                mo.Dispose();
            }

            MACAddress = MACAddress.Replace(":", "");
            return MACAddress;
        }
        /// <summary>
        /// Retrieving Motherboard Manufacturer.
        /// </summary>
        /// <returns></returns>
        public static string GetBoardMaker()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Manufacturer").ToString();
                }

                catch { }

            }

            return "Board Maker:Unknown";

        }
        /// <summary>
        /// Retrieving Motherboard Product Id.
        /// </summary>
        /// <returns></returns>
        public static string GetBoardProductId()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Product").ToString();

                }

                catch { }

            }

            return "Product:Unknown";
        }

        public static string GetBoardSerial()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("SerialNumber").ToString();

                }

                catch { }

            }

            return "BordSerial:Unknown";
        }

        /// <summary>
        /// Retrieving CD-DVD Drive Path.
        /// </summary>
        /// <returns></returns>
        public static string GetCdRomDrive()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_CDROMDrive");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Drive").ToString();

                }

                catch { }

            }

            return "CD ROM Drive Letter:Unknown";

        }
        /// <summary>
        /// Retrieving BIOS Maker.
        /// </summary>
        /// <returns></returns>
        public static string GetBIOSmaker()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Manufacturer").ToString();

                }

                catch { }

            }

            return "BIOS Maker:Unknown";

        }
        /// <summary>
        /// Retrieving BIOS Serial No.
        /// </summary>
        /// <returns></returns>
        public static string GetBIOSserNo()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("SerialNumber").ToString();

                }

                catch { }

            }

            return "BIOS Serial Number:Unknown";

        }
        /// <summary>
        /// Retrieving BIOS Caption.
        /// </summary>
        /// <returns></returns>
        public static string GetBIOScaption()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Caption").ToString();

                }
                catch { }
            }
            return "BIOS Caption:Unknown";
        }
        /// <summary>
        /// Retrieving System Account Name.
        /// </summary>
        /// <returns></returns>
        public static string GetAccountName()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_UserAccount");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {

                    return wmi.GetPropertyValue("Name").ToString();
                }
                catch { }
            }
            return "User Account Name:Unknown";

        }
        /// <summary>
        /// Retrieving Physical Ram Memory.
        /// </summary>
        /// <returns></returns>
        public static string GetPhysicalMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
        }
        /// <summary>
        /// Retrieving No of Ram Slot on Motherboard.
        /// </summary>
        /// <returns></returns>
        public static string GetNoRamSlots()
        {

            int MemSlots = 0;
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery2 = new ObjectQuery("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            ManagementObjectSearcher oSearcher2 = new ManagementObjectSearcher(oMs, oQuery2);
            ManagementObjectCollection oCollection2 = oSearcher2.Get();
            foreach (ManagementObject obj in oCollection2)
            {
                MemSlots = Convert.ToInt32(obj["MemoryDevices"]);

            }
            return MemSlots.ToString();
        }
        //Get CPU Temprature.
        /// <summary>
        /// method for retrieving the CPU Manufacturer
        /// using the WMI class
        /// </summary>
        /// <returns>CPU Manufacturer</returns>
        public static string GetCPUManufacturer()
        {
            string cpuMan = String.Empty;
            //create an instance of the Managemnet class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuMan == String.Empty)
                {
                    // only return manufacturer from first CPU
                    cpuMan = obj.Properties["Manufacturer"].Value.ToString();
                }
            }
            return cpuMan;
        }
        /// <summary>
        /// method to retrieve the CPU's current
        /// clock speed using the WMI class
        /// </summary>
        /// <returns>Clock speed</returns>
        public static int GetCPUCurrentClockSpeed()
        {
            int cpuClockSpeed = 0;
            //create an instance of the Managemnet class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuClockSpeed == 0)
                {
                    // only return cpuStatus from first CPU
                    cpuClockSpeed = Convert.ToInt32(obj.Properties["CurrentClockSpeed"].Value.ToString());
                }
            }
            //return the status
            return cpuClockSpeed;
        }
        /// <summary>
        /// method to retrieve the network adapters
        /// default IP gateway using WMI
        /// </summary>
        /// <returns>adapters default IP gateway</returns>
        public static string GetDefaultIPGateway()
        {
            //create out management class object using the
            //Win32_NetworkAdapterConfiguration class to get the attributes
            //of the network adapter
            ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //create our ManagementObjectCollection to get the attributes with
            ManagementObjectCollection objCol = mgmt.GetInstances();
            string gateway = String.Empty;
            //loop through all the objects we find
            foreach (ManagementObject obj in objCol)
            {
                if (gateway == String.Empty)  // only return MAC Address from first card
                {
                    //grab the value from the first network adapter we find
                    //you can change the string to an array and get all
                    //network adapters found as well
                    //check to see if the adapter's IPEnabled
                    //equals true
                    if ((bool)obj["IPEnabled"] == true)
                    {
                        gateway = obj["DefaultIPGateway"].ToString();
                    }
                }
                //dispose of our object
                obj.Dispose();
            }
            //replace the ":" with an empty space, this could also
            //be removed if you wish
            gateway = gateway.Replace(":", "");
            //return the mac address
            return gateway;
        }
        /// <summary>
        /// Retrieve CPU Speed.
        /// </summary>
        /// <returns></returns>
        public static double? GetCpuSpeedInGHz()
        {
            double? GHz = null;
            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
            {
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                    break;
                }
            }
            return GHz;
        }
        /// <summary>
        /// Retrieving Current Language
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentLanguage()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("CurrentLanguage").ToString();

                }

                catch { }

            }

            return "BIOS Maker:Unknown";

        }
        /// <summary>
        /// Retrieving Current Language.
        /// </summary>
        /// <returns></returns>
        public static string GetOSInformation()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
                }
                catch { }
            }
            return "BIOS Maker:Unknown";
        }
        /// <summary>
        /// Retrieving Processor Information.
        /// </summary>
        /// <returns></returns>
        public static String GetProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            String info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                string name = (string)mo["Name"];
                name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

                info = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];
                //mo.Properties["Name"].Value.ToString();
                //break;
            }
            return info;
        }
        /// <summary>
        /// Retrieving Computer Name.
        /// </summary>
        /// <returns></returns>
        public static String GetComputerName()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            String info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                info = (string)mo["Name"];
                //mo.Properties["Name"].Value.ToString();
                //break;
            }
            return info;
        }



        public static String GetUsbDiskSerial(string drive)
        {
            //string drive = "F:";

            USBSerialNumber usb = new USBSerialNumber();
            string serial = usb.getSerialNumberFromDriveLetter(drive);
            //MessageBox.Show(drive + " -> " + serial);

            return serial;
        }


        public static String GetUsbDiskSerial()
        {
            //use ExecutingAssemblie's drive-letter
            string drive = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Substring(0, 2);
            if (drive[1] == ':')
            {
                return GetUsbDiskSerial(drive);
            }
            else
            {
                return null;
            }
        }


        public static List<USBSerialNumber.USBDeviceInfo> GetUsbDeviceList()
        {
            USBSerialNumber usb = new USBSerialNumber();
            return usb.getUsbDriveList();
        }

        public static string GetDefaultHardwareId()
        {
            string hwid = "";
            string type = "";
            string usbSerial = HardwareInfo.GetUsbDiskSerial();
            if (usbSerial != null)
            {
                //Xero started from USB drive?! use the Stick's serial as the hardware id
                string enc = CipherAndReorder(usbSerial);
                hwid = enc;

                string drive = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Substring(0, 2);
                if (drive[1] == ':')
                {
                    //OK
                }
                else
                {
                    drive = "?";
                }

                type = $"USB:{drive.ToUpper()[0]}";  // $"USB-Serial, Disk '{drive}'";
            }
            else
            {
                string hwId = ""
                    + HardwareInfo.GetBoardSerial().Replace(" ", "")
                    + HardwareInfo.GetProcessorId();
                string enc = CipherAndReorder(hwId);
                hwid = enc;

                type = "PC"; //  "[board.serial] + [processor.id]";
            }

            return hwid + "," + type;
        }


        // Define other methods and classes here
        static string AlphaNumTable = "XHMWZNQ781DVOTKE0C3Y4R6GJSPU9L52FIBA";
        static private string CipherAndReorder(string text)
        {
            text = text.ToUpper();
            text = text.Length.ToString("X2") + text;
            text = text.PadRight(7, '0');
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                int index;
                if (char.IsDigit(c))
                    sb.Append(AlphaNumTable[(byte)c - 48 + 26]);
                else if (char.IsLetter(c))
                    sb.Append(AlphaNumTable[(byte)c - 65]);
                else
                    sb.Append(c);
            }

            //reorder

            StringBuilder sb2 = new StringBuilder();
            string s1 = sb.ToString();
            s1 = s1.PadRight((int)(((s1.Length / 7) + 1) * 7), '0');
            int len = s1.Length;
            for (int i = 0; i < len; i += 7)
            {
                if (i + 4 < len) sb2.Append(s1[i + 4]);
                if (i + 1 < len) sb2.Append(s1[i + 1]);
                if (i + 0 < len) sb2.Append(s1[i + 0]);
                if (i + 2 < len) sb2.Append(s1[i + 2]);
                if (i + 6 < len) sb2.Append(s1[i + 6]);
                if (i + 3 < len) sb2.Append(s1[i + 3]);
                if (i + 5 < len) sb2.Append(s1[i + 5]);
            }

            return sb2.ToString();
        }

        static internal string DecipherAndReorder(string text)
        {
            text = text.ToUpper();
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                int index = AlphaNumTable.IndexOf(c);
                if (index >= 0 && index < 26)
                    sb.Append((char)(index + 65));
                else if (index >= 26 && index < 36)
                    sb.Append((char)(index - 26 + 48));
                else
                    sb.Append(c);
            }

            //reorder
            StringBuilder sb2 = new StringBuilder();
            string s1 = sb.ToString();
            int len = s1.Length;
            int origLen = -1;
            for (int i = 0; i < len; i += 7)
            {
                if (i + 2 < len) sb2.Append(s1[i + 2]);
                if (i + 1 < len) sb2.Append(s1[i + 1]);
                if (i + 3 < len) sb2.Append(s1[i + 3]);
                if (i + 5 < len) sb2.Append(s1[i + 5]);
                if (i + 0 < len) sb2.Append(s1[i + 0]);
                if (i + 6 < len) sb2.Append(s1[i + 6]);
                if (i + 4 < len) sb2.Append(s1[i + 4]);

                if (i == 0) //first
                {
                    origLen = Convert.ToInt16(sb2.ToString().Substring(0, 2), 16);
                }
            }

            string s2 = sb2.ToString();
            if (origLen >= 0)
                return s2.Substring(2, origLen);
            else
                return s2.Substring(2);
        }




        #region USB Device Serial

        public class USBSerialNumber
        {

            string _serialNumber;
            string _driveLetter;

            public string getSerialNumberFromDriveLetter(string driveLetter)
            {
                this._driveLetter = driveLetter.ToUpper();

                if (!this._driveLetter.Contains(":"))
                {
                    this._driveLetter += ":";
                }

                matchDriveLetterWithSerial();

                return this._serialNumber;
            }

            private void matchDriveLetterWithSerial()
            {

                string[] diskArray;
                string driveNumber;
                string driveLetter;

                ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDiskToPartition");
                foreach (ManagementObject dm in searcher1.Get())
                {
                    diskArray = null;
                    driveLetter = getValueInQuotes(dm["Dependent"].ToString());
                    diskArray = getValueInQuotes(dm["Antecedent"].ToString()).Split(',');
                    driveNumber = diskArray[0].Remove(0, 6).Trim();
                    //Console.WriteLine($"disk '{driveLetter}'");
                    if (driveLetter == this._driveLetter)
                    {
                        /* This is where we get the drive serial */
                        ManagementObjectSearcher disks = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                        foreach (ManagementObject disk in disks.Get())
                        {
                            //Console.WriteLine($"{(disk["Name"].ToString())} - {("\\\\.\\PHYSICALDRIVE" + driveNumber)}");
                            if ((disk["Name"].ToString() == ("\\\\.\\PHYSICALDRIVE" + driveNumber))
                                && (disk["InterfaceType"].ToString() == "USB")
                            )
                            {
                                this._serialNumber = parseSerialFromDeviceID(disk["PNPDeviceID"].ToString());
                                //Console.WriteLine($"found {this._serialNumber}");
                            }
                        }
                    }
                }
            }

            private string parseSerialFromDeviceID(string deviceId)
            {
                string[] splitDeviceId = deviceId.Split('\\');
                string[] serialArray;
                string serial;
                int arrayLen = splitDeviceId.Length - 1;

                serialArray = splitDeviceId[arrayLen].Split('&');
                serial = serialArray[0];

                return serial;
            }

            private string getValueInQuotes(string inValue)
            {
                string parsedValue = "";

                int posFoundStart = 0;
                int posFoundEnd = 0;

                posFoundStart = inValue.IndexOf("\"");
                posFoundEnd = inValue.IndexOf("\"", posFoundStart + 1);

                parsedValue = inValue.Substring(posFoundStart + 1, (posFoundEnd - posFoundStart) - 1);

                return parsedValue;
            }


            public class USBDeviceInfo
            {
                //public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
                //{
                //    this.DeviceID = deviceID;
                //    this.PnpDeviceID = pnpDeviceID;
                //    this.Description = description;
                //}
                public string DriveLetter { get; set; }
                public string Name { get; set; }
                public string DeviceID { get; set; }
                public string PnpDeviceID { get; set; }
                public string Description { get; set; }
            }

            public List<USBDeviceInfo> getUsbDriveList()
            {
                List<string> UsbDrives = new List<string>();

                List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

                ManagementObjectCollection collection;
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity where DeviceID Like ""USBSTOR%"""))
                    collection = searcher.Get();

                ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDiskToPartition");

                string[] diskArray;
                string driveNumber;
                string driveLetter;
                foreach (ManagementObject dm in searcher1.Get())
                {
                    diskArray = null;
                    driveLetter = getValueInQuotes(dm["Dependent"].ToString());
                    diskArray = getValueInQuotes(dm["Antecedent"].ToString()).Split(',');
                    driveNumber = diskArray[0].Remove(0, 6).Trim();

                    //TODO...
                }

                foreach (var device in collection)
                {
                    devices.Add(new USBDeviceInfo
                    {
                        DriveLetter = "?:",
                        Name = (string)device.GetPropertyValue("Name"),
                        DeviceID = (string)device.GetPropertyValue("DeviceID"),
                        PnpDeviceID = (string)device.GetPropertyValue("PNPDeviceID"),
                        Description = (string)device.GetPropertyValue("Description")
                    }
                    );
                }

                collection.Dispose();
                return devices;

                //return UsbDrives;
            }


        }


        #endregion

    }
}
