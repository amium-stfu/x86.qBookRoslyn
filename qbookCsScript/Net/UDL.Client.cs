
using QB.Automation;
using System;
using System.Collections.Generic;
using System.Linq;


namespace QB.Net
{

    public class Udl
    {

        public class Client : Machine
        {

            System.Threading.Thread idleThread = null;
            public Client(string name, string uri = "127.0.0.1:9001") : base(name)
            {
                this.uri = uri;
                //    Timer = new Timer(name + ".Timer", 1);
                Open(uri);
                idleThread = new System.Threading.Thread(Idle);
                idleThread.IsBackground = true;
                idleThread.Start();
            }

            public override void Destroy()
            {
              
                if (CanClient != null)
                    CanClient.Close();
                  
               

                if (idleThread != null)
                    idleThread.Abort();

                base.Destroy();
            }

            public override void Run()
            {
                base.Run();
                Timer.OnElapsed -= _Elapsed;
                Timer.OnElapsed += _Elapsed;
            }

            public void Idle()
            {

                while (true)
                {
                    HbIdle();

                    foreach (Signal signal in Signals.Values)//.Values.ToList())//.items)
                                                             //     for (int i = 0; i < Signals.Count; i++)
                    {
                        try
                        {
                            if (signal is Module)
                            {
                                Module module = signal as Module;
                                lock (module)
                                {
                                    if (module.NetUploadQueue.Count > 0)
                                        PdoUpload((uint)module.NetId, 3, module.NetUploadQueue.Dequeue());

                                    if (!QB.Global.TempDisableUdlTimeout)
                                    {
                                        /*
                                        if (module.LastUpdate < (DateTime.UtcNow - TimeSpan.FromSeconds(5)))
                                        {
                                            module.OnAir = false;
                                            module.Value = double.NaN;
                                            module.Out.Value = double.NaN;
                                            module.State = 0xff;
                                            module.Alert = 1;
                                        }
                                        */
                                    }


                                    //   if (module.DoNetUpload)
                                    //       PdoUpload((uint)module.NetId, 3, module.Value);
                                    //   module.DoNetUpload = false;
                                }

                                //@STFU 2023-02-20
                                if (module.StreamWrite != null)
                                {
                                    QB.Logger.Debug("Udl: " + module.Text + " ID: " + module.NetId + " StreamWrite: '" + module.StreamWrite + "'");
                                    module.StreamRead = "";
                                    WriteStream((uint)module.NetId, module.StreamWrite);
                                    module.StreamWrite = null;
                                }



                                if (module.Command != -1)
                                    PdoUpload((uint)module.NetId, 1, module.Command);
                                module.Command = -1;


                                lock (module.Out)
                                {
                                    if (module.Out.NetUploadQueue.Count > 0)
                                        PdoUpload((uint)module.NetId, 5, module.Out.NetUploadQueue.Dequeue());

                                    //   if (module.Out.DoNetUpload)
                                    //       PdoUpload((uint)module.NetId, 5, module.Out.Value);
                                    //   module.Out.DoNetUpload = false;
                                }

                                /*
                                if (module.lastCommand != module.Command)
                                    PdoUpload((uint)module.NetId, 1, module.Command);
                                module.lastCommand = module.Command;
                                */
                                lock (module.Set)
                                {
                                    if (module.Set.NetUploadQueue.Count > 0)
                                        PdoUpload((uint)module.NetId, 4, module.Set.NetUploadQueue.Dequeue());

                                    //  if (module.Set.DoNetUpload)
                                    {
                                        //    PdoUpload((uint)module.NetId, 4, module.Set.Value);
                                        //  Console.Write(" " + module.Set.Value);
                                    }
                                    // module.Set.DoNetUpload = false;
                                }

                            }
                        }
                        catch { }
                    }
                    System.Threading.Thread.Sleep(5);
                }
            }

            private void _Elapsed(Timer t, TimerEventArgs ea)
            {
                //base.Elapsed(s);


            }

            public Module Module(int id)
            {
                string key = "m" + id.ToString("x3");

                //     if (!Signals.ContainsKey(key))
                //        Signals.TryAdd(key, new Signal(key, text: key));


                Module module = Signals[key] as Module;

                return module;
            }

            #region SignalHive: Semi-Automatic mamangement of Modules & Signals
            //Helpers.SignalHive shive = new Helpers.SignalHive();

            private Dictionary<string, string> _aliasDict = new Dictionary<string, string>();

            public void AddAlias(string alias, string key)
            {
                if (!_aliasDict.ContainsKey(alias))
                    _aliasDict.Add(alias, key);
                else
                    _aliasDict[alias] = key;
            }
            public delegate void OnNewModuleDelegate(Module module);
            public OnNewModuleDelegate OnNewModule;
            #endregion


            public Can.Link CanClient;
            public string uri = "127.0.0.1:9001";
            public bool RemoteTime = false;



            public void Open(string uri)
            {
                try
                {
                    if (CanClient != null)
                        CanClient.Close();

                    CanClient = new Can.Link(uri);
                    CanClient.MessageReceived += Can_MessageReived;
                }
                catch
                {
                }
            }


            public Dictionary<uint, string> label = new Dictionary<uint, string>();

            private void Can_MessageReived(object sender, uint id, byte dlc, byte[] data)
            {

                if ((id >= 0x180) && (id <= 0x4ff))
                    PdoOnCanMessageReceived(id, dlc, data);
                else if ((id >= 0x580) && (id <= 0x5ff))
                    SdoOnCanMessageReceived(id, dlc, data);
                else if ((id >= 0x700) && (id <= 0x7ff))
                    HbOnCanMessageReceived(id, dlc, data);
                //  else if ((id >= 0x7c) && (id <= 0x7f))
                //    UdlProgrammer.OnCanMessageReceived(id, dlc, data);
            }

            void SetStatus(Module module, uint moduleId, uint state)
            {
                module.State = state;

                if (state == 0x0) module.Status = "Off";
                if (state == 0x1) module.Status = "Sp1";
                if (state == 0x2) module.Status = "Sp2";
                if (state == 0x21) module.Status = "Ready";
                if (state == 0x22) module.Status = "Sample";
                if (state == 0x31) module.Status = "SampleZero";
                if (state == 0x32) module.Status = "SampleSpan";
                /*
                CmdOff = 0x20, Pause = 0x21, Ready = 0x22, Sample = 0x23, Clean = 0x25, Diagnose = 0x29, Service = 0x2a, Initialize = 0x2b,  
	 BootStart = 0x2d, Shutdown = 0x2e, Busy = 0x2f, Adjust = 0x31, SampleZero = 0x32, SampleSpan = 0x33, RequestAdjustZero = 0x37,  
	 SampleLin = 0x39, SampleLch = 0x38, SampleCuvette = 0x3a, SampleAutoAdjustZero = 0x42, SampleAutoAdjustSpan = 0x43, 
	 AdjustReset = 0x50, AutoAdjust = 0x51,  AdjustZero = 0x52, AdjustSpan = 0x53, Force_AdjustSpan = 0x54, Clear_AdjustErrors = 0x5f, 
	 CalibrateCuvette = 0x5d, Leakcheck = 0x61, DriftCompensation_On = 0x71, DriftCompensation_Off = 0x70, FidIgnition = 0xd1, ShedInjection = 0xd4,
	 TestLeak = 0xb1, TestDrift = 0xb2, TestZeroSpan = 0xb5, TestT90 = 0xb7, TestAnr = 0xba, TestSystemResponse = 0xbb, TestEu7 = 0xbe,
*/

                /*
                SetValueInternal(moduleId, "state", state.ToString("X2"), "Udl:" + this.Name + ".SetStatus");
                if (state == 0x0) SetValueInternal(moduleId, "status", "Off", "Udl:" + this.Name + ".SetStatus.Off");
                if (state == 0x1) SetValueInternal(moduleId, "status", "Sp1", "Udl:" + this.Name + ".SetStatus.Sp1");
                if (state == 0x2) SetValueInternal(moduleId, "status", "Sp2", "Udl:" + this.Name + ".SetStatus.Sp2");
                if (state == 0x21) SetValueInternal(moduleId, "status", "Ready", "Udl:" + this.Name + ".SetStatus.Ready");
                if (state == 0x22) SetValueInternal(moduleId, "status", "Sample", "Udl:" + this.Name + ".SetStatus.Sample");
                if (state == 0x31) SetValueInternal(moduleId, "status", "SampleZero", "Udl:" + this.Name + ".SetStatus.SampleZero");
                if (state == 0x32) SetValueInternal(moduleId, "status", "SampleSpan", "Udl:" + this.Name + ".SetStatus.SampleSpan");
                */
            }

            bool init = false;

            public bool PdoOnCanMessageReceived(UInt32 id, byte dlc, byte[] data)
            {
                try
                {
                    UInt16 moduleId = (UInt16)(((id & 0x7f) << 4) | (UInt16)(data[7] & 0x0f));
                    if (id == 0x100)
                    {
                        RemoteTime = true;
                    }
                    if ((id >= 0x480) && (id <= 0x4ff))
                    {
                        int type = data[6];

                        Module module = Module(moduleId); //add or get
                        if (module == null)
                            return false;
                        module.NetId = moduleId;
                        module.OnAir = true;

                        //  module.SetParameter("updated", DateTime.UtcNow.ToString("HH:mm:ss.fff")); //HALE: obsolete?!
                        module.LastUpdate = DateTime.UtcNow;

                        //2023-02-20 @STFU
                        if (type == 0xe1)
                        {
                            lock (module.StreamRead)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    char c = Convert.ToChar(data[i]);
                                    module.StreamRead += c;
                                }
                            }
                        }


                        if (type > 0x80) // label
                        {
                            if (type == 0xa1)
                            {
                                if (module.ModuleSettings == null)
                                    module.ModuleSettings = new ModuleSettings();
                                module.ModuleSettings.Ks = BitConverter.ToSingle(data, 0);
                                module.Tags["Ks"] = BitConverter.ToSingle(data, 0);

                            }
                            if (type == 0xa2)
                            {
                                if (module.ModuleSettings == null)
                                    module.ModuleSettings = new ModuleSettings();
                                module.ModuleSettings.Tu = BitConverter.ToSingle(data, 0);
                                module.Tags["Tu"] = BitConverter.ToSingle(data, 0);
                            }
                            if (type == 0xa3)
                            {
                                if (module.ModuleSettings == null)
                                    module.ModuleSettings = new ModuleSettings();
                                module.ModuleSettings.Tg = BitConverter.ToSingle(data, 0);
                                module.Tags["Tg"] = BitConverter.ToSingle(data, 0);
                            }
                        }

                        //UdlTypeState_ = 0x81, UdlTypeMode_ = 0x82,
                        //UdlTypeSet1 = 0x91, UdlTypeSet2 = 0x92, UdlTypeSetMin = 0x98, UdlTypeSetMax = 0x99,
                        //UdlTypeKs = 0xa1, UdlTypeTu = 0xa2, UdlTypeTg = 0xa3, UdlTypeOutMin = 0xa8, UdlTypeOutMax = 0xa9,

                        else if (type > 0x70) // label
                        {
                            if (!label.ContainsKey(moduleId))
                                label.Add(moduleId, "");
                            char[] chars = new char[100];
                            label[moduleId].ToCharArray().CopyTo(chars, 0);
                            for (int i = 0; i < 6; i++)
                            {
                                char c = Convert.ToChar(data[i]);
                                int blockIndex = (type & 0x0f) - 1;
                                chars[blockIndex * 6 + i] = c;
                            }
                            for (int i = 0; i < 50; i++)
                            {
                                if (chars[i] == 0)
                                {
                                    label[moduleId] = new string(chars, 0, i);
                                    module.Text = label[moduleId];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            /*
                            if ((index == 0) && (type == 1)) // state
                            {
                                State_48_8 = (int)value;
                            }
                            if ((index == 0) && (type == 2)) // alert
                            {
                                Alert_56_8 = (int)value;
                            }*/
                            if (type == 1)
                            {
                                uint value = (uint)BitConverter.ToSingle(data, 0);
                                SetStatus(module, moduleId, (value >> 0) & 0xff);
                                module.Mode = (uint)(value >> 8) & 0xff;
                                module.Tags["Timer"] = (uint)(value >> 16) & 0xff;
                            }
                            if (type == 0x41)
                            {
                                uint value = (uint)data[0] << 0 | (uint)data[1] << 8 | (uint)data[2] << 16 | (uint)data[3] << 32 | (uint)data[4] << 40 | (uint)data[5] << 48;
                                SetStatus(module, moduleId, (value >> 0) & 0xff);
                                module.Mode = (uint)(value >> 8) & 0xff;
                                module.Tags["Timer"] = (uint)(value >> 16) & 0xff;
                            }
                            if (type == 2)
                            {
                                module.Alert = (uint)BitConverter.ToSingle(data, 0);
                            }
                            if (type == 0x42)
                            {
                                uint value = (uint)data[0] << 0 | (uint)data[1] << 8 | (uint)data[2] << 16 | (uint)data[3] << 32 | (uint)data[4] << 40 | (uint)data[5] << 48;
                                module.Alert = (uint)value;
                            }
                            if (type == 3) // read
                            {
                                module.ValueFromNet = BitConverter.ToSingle(data, 0);
                                PdoUpdateUnit(module, moduleId,
                                     (UInt16)((((UInt16)data[5]) << 8) | (UInt16)data[4]),
                                      (data[7] & 0x10) > 0);
                            }
                            if (type == 4) // set
                                module.Set.ValueFromNet = BitConverter.ToSingle(data, 0);




                            if (type == 5) //output
                                module.Out.ValueFromNet = BitConverter.ToSingle(data, 0);

                            if (type == 0x0f)
                                module.Tags["OH"] = BitConverter.ToSingle(data, 0);

                            //@STFU 2025-02-20 
                            if (type == 0x98)
                                module.Tags["SetMin"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x99)
                                module.Tags["SetMax"] = BitConverter.ToSingle(data, 0);
                            //

                            if (type == 0x10)
                                module.Tags["Offset"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x11)
                                module.Tags["Gain"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x12)
                                module.Tags["X0"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x13)
                                module.Tags["Y0"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x14)
                                module.Tags["X1"] = BitConverter.ToSingle(data, 0);

                            if (type == 0x15)
                                module.Tags["Y1"] = BitConverter.ToSingle(data, 0);


                            if (type == 0x4e)
                            {
                                uint value = (uint)data[0] << 0 | (uint)data[1] << 8 | (uint)data[2] << 16 | (uint)data[3] << 32 | (uint)data[4] << 40 | (uint)data[5] << 48;
                                module.Tags["AId"] = value;
                            }
                            if (type == 0x4f)
                            {
                                uint value = (uint)data[0] << 0 | (uint)data[1] << 8 | (uint)data[2] << 16 | (uint)data[3] << 32 | (uint)data[4] << 40 | (uint)data[5] << 48;
                                module.Tags["Sn"] = value;
                            }

                      

                        }
                    }
                }
                catch (Exception ex)
                {
                    string e = ex.ToString();
                }
                return false;
            }
            public void PdoUpdateUnit(Module module, uint moduleId, UInt16 metricUnit, bool isController)
            {
                string unit = "";
                string label = "#";
                if ((metricUnit & 0x0ff0) == 0x0120)
                {
                    label = "f" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "Hz";
                }
                if ((metricUnit & 0x0ff0) == 0x0130)
                {
                    label = "l";
                    unit = PdoMetric(metricUnit) + "m";
                }
                if ((metricUnit & 0x0ff0) == 0x0140)
                {
                    label = "W" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g";
                }
                if ((metricUnit & 0x0ff0) == 0x0150)
                {
                    label = "t" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "s";
                }
                if ((metricUnit & 0x0ff0) == 0x0160)
                {
                    label = "U" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "V";
                }
                if ((metricUnit & 0x0ff0) == 0x0170)
                {
                    label = "I" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "A";
                }
                if ((metricUnit & 0x0ff0) == 0x01a0)
                {
                    label = "Time";
                    unit = PdoMetric(metricUnit) + "min";
                }
                if ((metricUnit & 0x0ff0) == 0x01b0)
                {
                    label = "Time";
                    unit = PdoMetric(metricUnit) + "h";
                }
                if ((metricUnit & 0x0ff0) == 0x01f0)
                {
                    label = (isController ? "DO" : "DI");
                    unit = "";
                }
                if ((metricUnit & 0x0ff0) == 0x0110) // parts per
                {
                    if ((metricUnit & 0x0f) == 0x0a)
                        unit = "%";
                    if ((metricUnit & 0x0f) == 0x0b)
                        unit = "Promille";
                    if ((metricUnit & 0x0f) == 0x0c)
                        unit = "ppm";
                }
                if ((metricUnit & 0x0ff0) == 0x01d0)
                {
                    label = "Degree" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°";
                }
                if ((metricUnit & 0x0ff0) == 0x0200)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "K";
                }
                if ((metricUnit & 0x0ff0) == 0x0210)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°C";
                }
                if ((metricUnit & 0x0ff0) == 0x0220)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°F";
                }
                if ((metricUnit & 0x0ff0) == 0x0230)
                {
                    label = "AH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g/m³";
                }
                if ((metricUnit & 0x0ff0) == 0x0240)
                {
                    label = "AH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g/kg";
                }
                if ((metricUnit & 0x0ff0) == 0x0250)
                {
                    label = "RH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "%";
                }

                if ((metricUnit & 0x0ff0) == 0x0260) //Pascal
                {
                    label = "P" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "Pa";
                }
                if ((metricUnit & 0x0ff0) == 0x0270) //bar
                {
                    label = "P" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "bar";
                }

                if ((metricUnit & 0x0ff0) == 0x0280) //m/s
                {
                    label = "Speed" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "m/s";
                }

                if ((metricUnit & 0x0ff0) == 0x0290) //m/h
                {
                    label = "Speed" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "m/h";
                }

                if ((metricUnit & 0x0ff0) == 0x02c0) //l/min
                {
                    label = "Flow" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "l/min";
                }
                if ((metricUnit & 0x0ff0) == 0x02d0) //l/h
                {
                    label = "Flow" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "l/h";
                }
                if ((metricUnit & 0x0ff0) == 0x02e0) //m³/h
                {
                    label = "Flow" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "m³/h";
                }
                if ((metricUnit & 0x0ff0) == 0x02f0) //g/h
                {
                    label = "Flow" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g/h";
                }
                //      if (GetValue(moduleId, "label")?.ToString().Length ==0)

                //        if (module.Label == null)
                {
                    //        module.Text = label;
                    //SCAN obsolete?   Values[(int)moduleId].Name = label;
                }
                //SCAN obsolete?  Values[(int)moduleId].Unit = unit;
                //    module.Unit = unit;
            }

            public static string PdoGetUnit(UInt16 metricUnit, bool isController)
            {
                string unit = "";
                string label = "#";
                if ((metricUnit & 0x0ff0) == 0x0120)
                {
                    label = "f" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "Hz";
                }
                if ((metricUnit & 0x0ff0) == 0x0130)
                {
                    label = "l";
                    unit = PdoMetric(metricUnit) + "m";
                }
                if ((metricUnit & 0x0ff0) == 0x0140)
                {
                    label = "W" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g";
                }
                if ((metricUnit & 0x0ff0) == 0x0150)
                {
                    label = "t" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "s";
                }
                if ((metricUnit & 0x0ff0) == 0x0160)
                {
                    label = "U" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "V";
                }
                if ((metricUnit & 0x0ff0) == 0x0170)
                {
                    label = "I" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "A";
                }
                if ((metricUnit & 0x0ff0) == 0x01f0)
                {
                    label = (isController ? "DO" : "DI");
                    unit = "";
                }
                if ((metricUnit & 0x0ff0) == 0x0110) // parts per
                {
                    if ((metricUnit & 0x0f) == 0x0a)
                        unit = "%";
                    if ((metricUnit & 0x0f) == 0x0b)
                        unit = "Promille";
                    if ((metricUnit & 0x0f) == 0x0c)
                        unit = "ppm";
                    if ((metricUnit & 0x0f) == 0x0d)
                        unit = "ppb";
                }
                if ((metricUnit & 0x0ff0) == 0x01d0)
                {
                    label = "Degree" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°";
                }
                if ((metricUnit & 0x0ff0) == 0x0200)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "K";
                }
                if ((metricUnit & 0x0ff0) == 0x0210)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°C";
                }
                if ((metricUnit & 0x0ff0) == 0x0220)
                {
                    label = "T" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "°F";
                }
                if ((metricUnit & 0x0ff0) == 0x0230)
                {
                    label = "AH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g/m³";
                }
                if ((metricUnit & 0x0ff0) == 0x0240)
                {
                    label = "AH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "g/kg";
                }
                if ((metricUnit & 0x0ff0) == 0x0250)
                {
                    label = "RH" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "%";
                }

                if ((metricUnit & 0x0ff0) == 0x0260) //Pascal
                {
                    label = "P" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "Pa";
                }
                if ((metricUnit & 0x0ff0) == 0x0270) //bar
                {
                    label = "P" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "bar";
                }

                if ((metricUnit & 0x0ff0) == 0x0280) //m/s
                {
                    label = "Speed" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "m/s";
                }

                if ((metricUnit & 0x0ff0) == 0x0290) //m/h
                {
                    label = "Speed" + (isController ? "C" : "I");
                    unit = PdoMetric(metricUnit) + "m/h";
                }
                return unit;
            }

            static string PdoMetric(UInt16 type)
            {
                if ((type & 0x0f) == 0x01)
                    return "f";
                if ((type & 0x0f) == 0x02)
                    return "p";
                if ((type & 0x0f) == 0x03)
                    return "n";
                if ((type & 0x0f) == 0x04)
                    return "µ";
                if ((type & 0x0f) == 0x05)
                    return "m";
                if ((type & 0x0f) == 0x06)
                    return "c";
                if ((type & 0x0f) == 0x07)
                    return "d";
                if ((type & 0x0f) == 0x08)
                    return "";
                if ((type & 0x0f) == 0x09)
                    return "D";
                if ((type & 0x0f) == 0x0a)
                    return "h";
                if ((type & 0x0f) == 0x0b)
                    return "k";
                if ((type & 0x0f) == 0x0c)
                    return "M";
                if ((type & 0x0f) == 0x0d)
                    return "G";
                if ((type & 0x0f) == 0x0e)
                    return "T";
                if ((type & 0x0f) == 0x0f)
                    return "P";
                return "#";
            }

            public void SdoOnCanMessageReceived(UInt32 id, byte dlc, byte[] data)
            {
                UInt16 index = (UInt16)((UInt16)data[1] | ((UInt16)data[2] << 8));
                UInt16 subIndex = (UInt16)((UInt16)data[3]);
                UInt16 mId = (UInt16)((id & 0x7f) << 4);


                //SCAN: QbookObjects.Module module = Qb.GetModule(Name + "»" + "m" + mId.ToString("x3"));
                UInt16 moduleId = (UInt16)(((id & 0x7f) << 4) | (UInt16)(data[7] & 0x0f));
                //string idi = Name + "." + "m" + moduleId.ToString("x3");
                //qb.Modules.UdlModule module = qb.Root.GetObject(idi) as qb.Modules.UdlModule;

                Module module = Module(moduleId); //, "udl.sdo"); //get only; no autocreate
                if (module == null)
                    return;

                if ((index == 0x1010) && (subIndex == 0x10))
                {
                    module.Tags["CardType"] = ((UInt16)((UInt16)data[7] << 8 | (UInt16)data[6]));
                    module.Tags["SensorType"] = ((UInt16)((UInt16)data[5] << 8 | (UInt16)data[4]));
                }

                if (index == 0x1018)
                {

                    UInt32 v = (UInt32)((UInt32)data[4] | ((UInt32)data[5] << 8) | ((UInt32)data[6] << 16) | ((UInt32)data[7] << 24));
                    DateTime date = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(v);
                    string type = subIndex.ToString("X2");
                    if (subIndex == 0x52) type = "CZ";
                    if (subIndex == 0x53) type = "CS";
                    //SetValueInternal(mId, "dates." + type, date.ToString("yyyy-MM-dd HH:mm"), "Udl:" + this.Name + ".onSdoMsg");


                    //type += " " + date.ToString("yy-MM-dd HH:mm");
                    //    SetValue(mId, "dates." , type);
                    /*
                    if (!Module[_id].dates.Contains(type))
                    {
                        Module[_id].dates.Add(type);
                        Module[_id].Log.Add(type);
                    }
                    */
                }

                if (index == 0x1019)
                {
                    if (data[3] == 0x01)
                        module.Tags["OperatingHours"] = BitConverter.ToSingle(data, 4);
                }

                if ((index == 0x1010) && (subIndex == 6))
                {
                    UInt32 v = (UInt32)((UInt32)data[4] | ((UInt32)data[5] << 8) | ((UInt32)data[6] << 16) | ((UInt32)data[7] << 24));
                    DateTime date = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(v);
                    module.Tags["Version"] = date.ToString("yyyy-MM-dd");
                }
            }

            public void SdoUpload(uint id, string Value, UInt16 index, byte subIndex, string dataType)
            {
                try
                {
                    Can.Message canMessage = new Can.Message(0x600 + id, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

                    if (dataType.Contains("A4"))
                        canMessage.Data[0] = 0x23;
                    if (dataType.Contains("SN"))
                        canMessage.Data[0] = 0x23;
                    if (dataType.Contains("DT"))
                        canMessage.Data[0] = 0x23;
                    if (dataType.Contains("FLOAT"))
                        canMessage.Data[0] = 0x23;
                    if (dataType.Contains("32"))
                        canMessage.Data[0] = 0x23;
                    if (dataType.Contains("16"))
                        canMessage.Data[0] = 0x2b;
                    if (dataType.Contains("8"))
                        canMessage.Data[0] = 0x2f;

                    canMessage.Data[1] = (byte)index;
                    canMessage.Data[2] = (byte)(index >> 8);
                    canMessage.Data[3] = subIndex;
                    double value = Value.ToDouble();
                    if (dataType.Contains("SN"))
                    {
                        value = Value.Replace("-", "").ToDouble();
                    }

                    if (dataType.Contains("TEXT"))
                    {
                        string v = Value;
                        if (v.Length > 4)
                            v = v.Substring(0, 4);
                        while (v.Length < 4)
                            v = v + " ";

                        canMessage.Data[4] = (byte)v[3];
                        canMessage.Data[5] = (byte)v[2];
                        canMessage.Data[6] = (byte)v[1];
                        canMessage.Data[7] = (byte)v[0];
                    }
                    else if (dataType.Contains("FLOAT"))
                    {
                        byte[] v = BitConverter.GetBytes(Convert.ToSingle(value));
                        canMessage.Data[4] = v[0];// (byte)v;
                        canMessage.Data[5] = v[1];//(byte)(v >> 8);
                        canMessage.Data[6] = v[2];//(byte)(v >> 16);
                        canMessage.Data[7] = v[3];//(byte)(v >> 24);
                    }
                    else
                    {
                        if (dataType.Contains("DT:"))
                        {
                            if (Value.Tolerant() == "N")
                                Value = DateTime.Now.ToString("yyyy-MM-dd");
                            else if (Value.Split('-').Length == 2)
                                Value = DateTime.Now.Year + "-" + Value;
                            value = (DateTime.Parse(Value) - new DateTime(1970, 1, 1)).TotalSeconds;
                        }
                        UInt32 v = (UInt32)value;
                        canMessage.Data[4] = (byte)v;
                        canMessage.Data[5] = (byte)(v >> 8);
                        canMessage.Data[6] = (byte)(v >> 16);
                        canMessage.Data[7] = (byte)(v >> 24);
                    }
                    CanClient.Transmit(canMessage);
                    //*         RequestDownload = DateTime.Now + TimeSpan.FromMilliseconds(500);
                }
                catch
                { }
            }

            public void SdoUploadModuleIds(UInt32 serialNumber, string[] moduleIds)
            {
                if (moduleIds.Length > 8)
                    return;

                List<uint> moduleIdList = new List<uint>();

                foreach (string moduleId in moduleIds)
                {

                    double id = moduleId.ToDouble();
                    if (!double.IsNaN(id) && (id > 0) && (id < 128))
                        moduleIdList.Add((uint)id);
                }
                if (moduleIdList.Count == 0)
                    moduleIdList.Add(0x7f);
                moduleIdList.Add(0);

                byte i = 0;
                UInt32 v = (UInt32)serialNumber;
                foreach (uint moduleId in moduleIdList)
                {
                    CanClient.Transmit(new Can.Message(0, new byte[] { 0xf1, (byte)(i + 1), (byte)moduleId, 0,
                    (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)(v >> 0) }));
                    i++;
                    //    System.Threading.Thread.Sleep(20);
                }
                //* Rebuild = true;
                //*  Dispose();
            }

            void Remove(UInt16 id)
            {
                //    if (Module.ContainsKey(id))
                //      Module.Remove(id);

            }

            public void HbOnCanMessageReceived(UInt32 id, byte dlc, byte[] data)
            {
                UInt16 mId = (UInt16)((id & 0x7f) << 4);
                //SCAN: QbookObjects.Module module = Qb.GetModule(Name + "»" + "m" + mId.ToString("x3"));

                UInt16 moduleId = (UInt16)(((id & 0x7f) << 4) | (UInt16)(data[7] & 0x0f));
                //string idi = Name + "." + "m" + moduleId.ToString("x3");
                //qb.Modules.UdlModule module = qb.Root.GetObject(idi) as qb.Modules.UdlModule;
                Module module = Module(moduleId); //, "udl.hb"); //get only; no autocreate
                if (module == null)
                    return;

                //   foreach (UInt16 _id in Module.Keys)
                {
                    //     if ((_id & 0x0ff0) == mId)
                    {
                        if (dlc == 6)
                        {
                            UInt32 csn =
                                (UInt32)data[2] << 24 |
                                (UInt32)data[3] << 16 |
                                (UInt32)data[4] << 8 |
                                (UInt32)data[5];
                            //      if ((Module[_id].CardSerialNumber != 0) && (Module[_id].CardSerialNumber != csn))
                            {
                                //        Module[_id].UdlError = true;
                            }

                            module.Tags["CSn"] = csn.ToString("X8");
                            //    SetValueInternal(mId, "csn", csn.ToString("X8"), "Udl:" + this.Name + ".onHbMsg");

                            //Module[_id].CardSerialNumber = csn;

                            //  Module[_id].DateUpdate = DateTime.Now;

                            //    if (Module[_id].Version == "")
                            {
                                //        Can.Transmit(new CanMessage(0x600 + (id & 0x7f), new byte[] { 0x40, 0x10, 0x10, 6, 0, 0, 0, 0 }));
                            }

                        }
                        if (dlc == 8)
                        {
                            //      Module[_id].LogText(System.Text.Encoding.UTF7.GetString(data));
                        }
                    }
                }

            }

            ////TODO:obsolete? public Dictionary<UInt32, oModule> Modules = new Dictionary<UInt32, oModule>();    
            //public Dictionary<string, string> aliasDict = new Dictionary<string, string>();
            //public string Name = "";
            //void SetValueInternal(UInt32 id, string name, object value, object sender = null)
            //{
            //    string idi = Name + "." + "m" + id.ToString("x3");
            //    //HALE:20220920

            //    qb.Modules.UdlModule module = qb.Root.GetObject(idi) as qb.Modules.UdlModule;
            //    if (true) //HALE
            //    {
            //        if (module != null)
            //        {
            //            switch (name)
            //            {
            //                case "read":
            //                    module.read.value = value.ToDouble();
            //                    break;
            //                case "set":
            //                    module.set.value = value.ToDouble();
            //                    break;
            //                case "out":
            //                    module.output.value = value.ToDouble();
            //                    break;
            //                case "label":
            //                    module.text = value.ToString();
            //                    break;
            //                case "state":
            //                    module.state = (uint)value.ToInt32();
            //                    break;
            //                case "status":
            //                    module.status = value.ToString();
            //                    break;
            //                case "updated":
            //                    module.updated = value.ToString();
            //                    break;
            //                case "timer":
            //                    module.timer = (uint)value.ToInt32();
            //                    break;
            //                case "mode":
            //                    module.mode = (uint)value.ToInt32();
            //                    break;
            //                case "alert":
            //                    module.alert = (uint)value.ToInt32();
            //                    break;
            //                case "offset":
            //                    module.offset = value.ToDouble();
            //                    break;
            //                case "gain":
            //                    module.gain = value.ToDouble();
            //                    break;
            //                case "aid":
            //                    module.aid = (uint)value.ToInt32();
            //                    break;
            //                case "sn":
            //                    module.sn = (uint)value.ToInt32();
            //                    break;
            //                case "csn":
            //                    module.csn = value.ToString();
            //                    break;
            //                case "version":
            //                    module.version = value.ToString();
            //                    break;
            //                case "cardType":
            //                    module.cardtype = (uint)value.ToInt32();
            //                    break;
            //                case "sensorType":
            //                    module.sensortype = (uint)value.ToInt32();
            //                    break;
            //                case "operatingHours":
            //                    module.operatinghours = value.ToDouble();
            //                    break;
            //                //case "unit":
            //                //    module.read.unit = value.ToString();
            //                //    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //    //return; //HALE:20220920



            //    //if (false) //SCAN
            //    //{
            //    //    if (!Qb.ScriptingEngine.MagesEngine.Scope.ContainsKey(idi))
            //    //    {
            //    //        object wo_ = Qb.ScriptingEngine.MagesEngine.Interpret(idi + " = oModule();");
            //    //    }

            //    //    object wo = Qb.ScriptingEngine.MagesEngine.Scope[idi];
            //    //    if (!(wo is Mages.Core.Runtime.WrapperObject))
            //    //        return;

            //    //    qbModules.Module module1 = (wo as Mages.Core.Runtime.WrapperObject).Content as qbModules.Module;
            //    //    if (module1 == null)
            //    //        return;

            //    //    if (!Modules.ContainsKey(id))
            //    //    {
            //    //        Modules.Add(id, new oModule());
            //    //        object o = Qb.ScriptingEngine.MagesEngine.Interpret(Name + "»" + idi + " = oModule();");
            //    //    }

            //    //}

            //    if (name == "read")
            //        module.read.value = value.ToDouble();
            //    if (name == "set")
            //        module.set.value = value.ToDouble();

            //    return;


            //    //  oModule module = Qb.ScriptingEngine.MagesEngine.Scope["asdf"]; //

            //    //MIGRATE
            //    /*
            //    if (aliasDict.ContainsKey(idi))
            //        Qb.SetInternal(Name , aliasDict[idi] + "." + name, value, sender);
            //    else
            //        Qb.SetInternal(Name , idi + "." + name, value, sender);
            //    */
            //    //\MIGRATE
            //}

            //object GetValue(UInt32 id, string name)
            //{
            //    //MIGRATE
            //    /*
            //    string idi = "m" + id.ToString("x3");
            //    if (aliasDict.ContainsKey(idi))
            //        return Qb.Get(Name ,aliasDict[idi] + "»" + name);
            //    else
            //        return Qb.Get(Name, idi + "»" + name);
            //    */
            //    return null;
            //    //\MIGRATE
            //}

            public DateTime NextHeartbeat = DateTime.Now;
            public void HbIdle()
            {
                if (CanClient == null)
                    return;
                if (DateTime.Now > NextHeartbeat)
                {
                    NextHeartbeat = NextHeartbeat.AddSeconds(1);
                    CanClient.Transmit(new Can.Message(0x70e, new byte[] { 5, 4 }));

                    if (!RemoteTime)
                    {
                        long currentMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        var b = BitConverter.GetBytes(currentMillis);
                        CanClient.Transmit(new Can.Message(0x100, new byte[] { b[0], b[1], b[2], b[3], b[4], b[5], 0x00, 0x08 }));
                    }

                }
            }

            public void NmtReset(uint id)
            {
                CanClient.Transmit(new Can.Message(0, new byte[] { 0xf4, (byte)id }));
            }

            public void NmtCommandF3(uint id)
            {
                CanClient.Transmit(new Can.Message(0, new byte[] { 0xf3, (byte)id }));
            }
            public void NmtCommandF5_On()
            {
                CanClient.Transmit(new Can.Message(0, new byte[] { 0xf5, 0 }));
            }
            public void NmtCommandF6_Off()
            {
                CanClient.Transmit(new Can.Message(0, new byte[] { 0xf6, 0 }));
            }
            public void NmtFactoryDefault(uint id)
            {
                CanClient.Transmit(new Can.Message(0, new byte[] { 0xf7, (byte)id }));
            }
            public void PdoUpload(uint moduleId, byte type, double value)
            {
           if(CanClient == null) return;
                //    if (moduleId == 1360)
            //        return;
            //    if (QB.Global.TempDisablePdoUploadForId0)
            //        return;
                byte[] v = BitConverter.GetBytes(Convert.ToSingle((float)value));
                CanClient.Transmit(new Can.Message(0x500 + (moduleId >> 4), new byte[] { v[0], v[1], v[2], v[3],
                0,0,type, (byte)(moduleId & 0x0f)  }));
            }

            //@STFU 2023-02-20 
            public void WriteStream(uint moduleId, string msg)
            {
                QB.Logger.Debug("StreamWirte ID: " + moduleId + ": '" +  msg + "'");
                char[] txt = msg.ToCharArray();
                while ((txt.Length % 6) != 0)
                {
                    txt = txt.Append((char)0).ToArray();
                }
                int i = 0;
                int l = txt.Length;


                while (l > 0)
                {
                    CanClient.Transmit(new QB.Net.Can.Message(0x500 + (moduleId >> 4),
                        new byte[] { (byte)txt[0 + i * 6],(byte)txt[1+ i * 6],(byte)txt[2+ i * 6],
                            (byte)txt[3+ i * 6], (byte)txt[4+ i * 6],(byte)txt[5+ i * 6],
                        (byte)0xe1, (byte)(moduleId & 0x0f)  }));

                    i += 6;
                    l -= 6;
                }

            }
            public void SdoUpload(uint moduleId, byte type, double value)
            {
                byte[] v = BitConverter.GetBytes(Convert.ToSingle((float)value));
                CanClient.Transmit(new Can.Message(0x600 + (moduleId >> 4), new byte[] { v[0], v[1], v[2], v[3],
                0,0,type, (byte)(moduleId & 0x0f)  }));
            }
            public void PdoUpload48(uint moduleId, byte type, UInt32 v)
            {
                CanClient.Transmit(new Can.Message(0x500 + (moduleId >> 4), new byte[] { (byte)v,(byte)(v >> 8),(byte)(v >> 16), (byte)(v >> 24),
                (byte)(v >> 32),(byte)(v >> 40),type, (byte)(moduleId & 0x0f)  }));
            }
            public void PdoUploadText(uint moduleId, byte type, string text)
            {
                byte[] txt = new byte[100];
                for (int i = 0; i < 100; i++)
                    txt[i] = 0;

                for (int i = 0; i < text.Length; i++)
                    txt[i] = (byte)text[i];

                for (int i = 0; i < 3; i++)
                    CanClient.Transmit(new Can.Message(0x500 + (moduleId >> 4), new byte[] { (byte)txt[0 + i * 6],(byte)txt[1+ i * 6],(byte)txt[2+ i * 6], (byte)txt[3+ i * 6],
                (byte)txt[4+ i * 6],(byte)txt[5+ i * 6], (byte)( type + i), (byte)(moduleId & 0x0f)  }));
            }
        }
    }

}