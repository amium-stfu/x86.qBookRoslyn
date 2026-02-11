using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using static QB.Controls.Draw;

namespace QB.Net
{
    public class Serial
    {
        static Serial()
        {
            WqlEventQuery comPortAddQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            WqlEventQuery comPortRemoveQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");

            ManagementEventWatcher comPortAddWatcher = new ManagementEventWatcher(comPortAddQuery);
            ManagementEventWatcher comPortRemoveWatcher = new ManagementEventWatcher(comPortRemoveQuery);

            comPortAddWatcher.EventArrived += ComPortAdded;
            comPortRemoveWatcher.EventArrived += ComPortRemoved;

            // Start monitoring
            comPortAddWatcher.Start();
            comPortRemoveWatcher.Start();

            UpdateSerialPortList(); //initial list
        }


        private static void ComPortAdded(object sender, EventArrivedEventArgs e)
        {
            UpdateSerialPortList();
        }

        private static void ComPortRemoved(object sender, EventArrivedEventArgs e)
        {
            UpdateSerialPortList();
        }


        public class ComPort
        {
            public string PortName;
            public bool IsBusy;
            public bool Selected;
        }



        static Dictionary<string, ComPort> _ComPorts = new Dictionary<string, ComPort>();

        /// <summary>
        /// Holds every 
        /// </summary>

        public static List<ComPort> ComPorts
        {
            get
            {
                return _ComPorts.Values.ToList();
            }
        }

        static void UpdateSerialPortList()
        {

            string[] availablePorts = SerialPort.GetPortNames();
            string[] selectedPorts = _ComPorts.Values.Where(p => p.Selected).Select(p => p.PortName).ToArray();

            List<string> oldPortList;
            List<string> newPortList;
            lock (_ComPorts)
            {
                oldPortList = _ComPorts.Values.Select(p => p.PortName).ToList();
                oldPortList.Sort();
                _ComPorts.Clear();
                foreach (string portName in availablePorts)
                {
                    _ComPorts.Add(portName, new ComPort
                    {
                        PortName = portName,
                        IsBusy = IsPortInUse(portName),
                        Selected = selectedPorts.Contains(portName)
                    });
                }
                newPortList = _ComPorts.Values.Select(p => p.PortName).ToList();
                newPortList.Sort();
            }

            if (string.Join(",", newPortList) != string.Join(",", oldPortList))
            {
                RaiseComPortListChanged();
            }
        }


        public delegate void ComPortListChangedEventHandler(EventArgs e);
        public static event ComPortListChangedEventHandler ComPortListChangedEvent;
        static void RaiseComPortListChanged()
        {
            if (ComPortListChangedEvent != null)
            {
                MmToPxChangedEventArgs ea = new MmToPxChangedEventArgs() { };
                ComPortListChangedEvent(ea);
            }
        }

        // Function to check if a COM port is in use
        static bool IsPortInUse(string portName)
        {
            try
            {
                using (var port = new SerialPort(portName))
                {
                    port.Open();
                    return false; // Port is available
                }
            }
            catch (UnauthorizedAccessException)
            {
                return true; // Port is in use
            }
            catch (IOException)
            {
                return true; // Port is in use
            }
        }


        public static List<ComPort> SelectedComPorts
        {
            get
            {
                if (_ComPorts == null || _ComPorts.Values.Count == 0)
                    return new List<ComPort>();
                return (List<ComPort>)_ComPorts.Values.Where(p => p.Selected);
            }
        }


        /// <summary>
        /// Returns a list f available COM Ports
        /// </summary>
        /// <returns></returns>
        public static string[] GetComPortList()
        {
            string[] availablePorts = SerialPort.GetPortNames();
            return availablePorts;
        }
    }
}
