//@SCAN using jnsoft.Comm.CAN;
//@SCAN using jnsoft.DBC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;

namespace QB
{
    //public class Can
    //{
    public class CanDbc
    {
        string BusId = "";
        public CanDbc(string id)
        {
            BusId = "CAN." + id;
        }

        //@SCAN   DBCFile dbcFile;

        DbcDecoder dbcDecoder;

        private Net.Can.Client _CanClient = null;
        public Net.Can.Client CanClient
        {
            get
            {
                return _CanClient;
            }
            set
            {
                _CanClient = value;
                BusId = "CAN." + _CanClient.Name;
                _CanClient.OnMessageReceived -= OnCanClientMessageReceived;
                _CanClient.OnMessageReceived += OnCanClientMessageReceived;
            }
        }


        //public DBCFile OpenDbcFile(string filename)
        public object OpenDbcFile(string filename)
        {
            try
            {
                if ((QB.Root.ActiveQbook) != null && (filename.StartsWith("./") || filename.StartsWith(@".\")))
                {
                    //use the qbook's directory if a relative path is given
                    filename = Path.GetFullPath(Path.Combine(QB.Root.ActiveQbook.Directory, filename));
                }


                dbcDecoder = new DbcDecoder(filename);
              
                //dbcFile = DBCFile.open(filename);
                //if (dbcFile != null)
                //    BuildSignalDict();

                return null;// dbcFile;
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"#EX opening DBC file '{filename}': " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                return null;
            }
        }

        //Dictionary<string, DBCFile.SignalType> SignalDict = new Dictionary<string, DBCFile.SignalType>();
        //public void BuildSignalDict()
        //{
        //    lock (SignalDict)
        //    {
        //        SignalDict.Clear();
        //        foreach (var source in dbcFile.Sources.Values)
        //        {
        //            foreach (var message in source.Messages)
        //            {
        //                foreach (var signal in message.Signals)
        //                {
        //                    //full name (device.message.signal)
        //                    string key = source.Name.ToLower() + "." + message.Name.ToLower() + "." + signal.Name.ToLower();
        //                    if (!SignalDict.ContainsKey(key))
        //                        SignalDict.Add(key, signal);
        //                    else
        //                        QB.Logger.Warn("DBC: duplicate key: " + key);

        //                    //medium name (message.signal)
        //                    key = /*source.Name.ToLower() + "." +*/ message.Name.ToLower() + "." + signal.Name.ToLower();
        //                    if (!SignalDict.ContainsKey(key))
        //                        SignalDict.Add(key, signal);
        //                    else
        //                        QB.Logger.Warn("DBC: duplicate key: " + key);

        //                    //short name (signal)
        //                    key = /*source.Name.ToLower() + "." + message.Name.ToLower() + "." + */signal.Name.ToLower();
        //                    if (!SignalDict.ContainsKey(key))
        //                        SignalDict.Add(key, signal);
        //                    //else
        //                    //    QB.Logger.Warn("DBC: duplicate key: " + key);
        //                }
        //            }
        //        }
        //    }
        //}


        void OnCanClientMessageReceived(Net.Can.Client can, Net.Can.Message cm)
        {
            if (dbcDecoder != null)
            {
                dbcDecoder.Decode(cm.Id, (byte)cm.Data.Length, cm.Data);//, ref cmd);
            }
            //if (dbcFile != null)
            //{
            //    dbcFile.onCANReceived(new CANFrame(ref BusId, cm.Id, cm.Data, false));
            //}
        }

        //public DBCFile.SignalType GetDbcSignal(string signalName)
        //{
        //    if (dbcFile == null || string.IsNullOrEmpty(signalName))
        //        return null;

        //    signalName = signalName.ToLower();
        //    lock (SignalDict)
        //    {
        //        if (SignalDict.ContainsKey(signalName))
        //            return SignalDict[signalName];
        //    }

        //    return null;
        //}

        public double GetDbcSignalValue(string signalName)
        {
            if (dbcDecoder != null)
            {
                foreach(DbcMessage dm in dbcDecoder.DbcMessages.Values)
                {
                    if (dm.Has(signalName))
                        return dm.ValueOf(signalName);    
                }
            }

            //if (dbcFile == null || string.IsNullOrEmpty(signalName))
            //    return double.NaN;

            //signalName = signalName.ToLower();
            //lock (SignalDict)
            //{
            //    DBCFile.SignalType signal = null;
            //    if (SignalDict.ContainsKey(signalName))
            //    {
            //        string value = SignalDict[signalName].toStringValue(jnsoft.Helpers.ValueObjectFormat.Physical, 9).Replace(',', '.');
            //        double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d);
            //        return d;
            //    }
            //}

            return double.NaN;
        }

        //public string GetDbcSignalText(string signalName)
        //{
        //    if (dbcFile == null || string.IsNullOrEmpty(signalName))
        //        return "?";

        //    signalName = signalName.ToLower();
        //    lock (SignalDict)
        //    {
        //        DBCFile.SignalType signal = null;
        //        if (SignalDict.ContainsKey(signalName))
        //        {
        //            string value = SignalDict[signalName].toStringValue(jnsoft.Helpers.ValueObjectFormat.Physical);
        //            return value;
        //        }
        //    }
        //    return "-";
        //}


    }

    //}
}
