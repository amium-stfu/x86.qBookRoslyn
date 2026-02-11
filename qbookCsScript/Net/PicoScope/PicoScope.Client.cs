
using PicoPinnedArray;
using PicoStatus;
using PS2000AImports;
using QB.Automation;
using System;
using System.Text;
using System.Threading;

namespace QB.Net
{
    public partial class PicoScope
    {
        struct ChannelSettings
        {
            public Imports.CouplingType couplingType;
            public Imports.Range range;
            public bool enabled;
        }

        class Pwq
        {
            public Imports.PwqConditions[] conditions;
            public short nConditions;
            public Imports.ThresholdDirection direction;
            public uint lower;
            public uint upper;
            public Imports.PulseWidthType type;

            public Pwq(Imports.PwqConditions[] conditions,
                short nConditions,
                Imports.ThresholdDirection direction,
                uint lower, uint upper,
                Imports.PulseWidthType type)
            {
                this.conditions = conditions;
                this.nConditions = nConditions;
                this.direction = direction;
                this.lower = lower;
                this.upper = upper;
                this.type = type;
            }
        }

        public class Client : Machine
        {
            public class PicoScopeClientMessageReceivedEventArgs : EventArgs
            {
                //    public char Dcb = ' ';
                //    public int Port = 0;
                public string Topic = "";
                public string Value = "";
                //     public string Channel = "K0";
                //     public string[] Parameters = null;
            }

            public delegate void OnMessageReceivedDelegate(Client client, PicoScopeClientMessageReceivedEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
            public OnMessageReceivedDelegate OnMessageReceived;


            public Client(string name, string uri = "127.0.0.1:1883", string user = "", string password = "") : base(name)
            {

            }


            public void Open()
            {
                short count = 0;
                short serialsLength = 40;
                StringBuilder serials = new StringBuilder(serialsLength);
                uint status = Imports.EnumerateUnits(out count, serials, ref serialsLength);

                if (status != StatusCodes.PICO_OK)
                {
                    Console.WriteLine("No devices found.\n");
                    Console.WriteLine("Error code : {0}", status);
                    // Console.WriteLine("Press any key to exit.\n");
                    // WaitForKey();
                    // Environment.Exit(0);
                }
                else
                {
                    if (count == 1)
                    {
                        Console.WriteLine("Found {0} device:", count);
                    }
                    else
                    {
                        Console.WriteLine("Found {0} devices", count);
                    }

                    Console.WriteLine("Serial(s) {0}", serials);

                }

                // Open unit and show splash screen
                Console.WriteLine("\n\nOpening the device...");
                short handle;

                status = Imports.OpenUnit(out handle, null);
                Console.WriteLine("Handle: {0}", handle);

                if (status != StatusCodes.PICO_OK)
                {
                    Console.WriteLine("Unable to open device");
                    Console.WriteLine("Error code : {0}", status);
                    //  WaitForKey();
                }
                else
                {
                    Console.WriteLine("Device opened successfully\n");

                    _handle = handle;
                    Run();
                    // PS2000ACSConsole consoleExample = new PS2000ACSConsole(handle);
                    //     consoleExample.Run();
                    //Imports.CloseUnit(handle);

                }
            }


            public void Close()
            {
                Imports.CloseUnit(_handle);
            }

            /****************************************************************************
        * Run - show menu and call user selected options
        ****************************************************************************/
            public void Run()
            {
                // setup devices
                GetDeviceInfo();
                _timebase = 1;

                _channelSettings = new ChannelSettings[MAX_CHANNELS];

                for (int i = 0; i < MAX_CHANNELS; i++)
                {
                    if (i < _channelCount)
                    {
                        _channelSettings[i].enabled = true;
                    }
                    else
                    {
                        _channelSettings[i].enabled = false;
                    }

                    _channelSettings[i].couplingType = Imports.CouplingType.PS2000A_DC;
                    _channelSettings[i].range = Imports.Range.Range_5V;
                }
            }





            /****************************************************************************
            * Initialise unit' structure with Variant specific defaults
            ****************************************************************************/
            private short _handle;
            public const int BUFFER_SIZE = 1024;
            public const int MAX_CHANNELS = 4;
            public const int QUAD_SCOPE = 4;
            public const int DUAL_SCOPE = 2;
            bool _ready = false;
            uint _timebase = 8;
            short _oversample = 1;
            bool _scaleVoltages = true;
            private int _channelCount;
            private int _digitalPorts;
            private short _maxValue;
            private ChannelSettings[] _channelSettings;
            private Imports.Range _firstRange;
            private Imports.Range _lastRange;
            ushort[] inputRanges = { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
            private Imports.ps2000aBlockReady _callbackDelegate;

            void GetDeviceInfo()
            {
                uint status = 0;

                string[] description = {
                                       "Driver Version    ",
                                       "USB Version       ",
                                       "Hardware Version  ",
                                       "Variant Info      ",
                                       "Serial            ",
                                       "Cal Date          ",
                                       "Kernel Ver        ",
                                       "Digital Hardware  ",
                                       "Analogue Hardware "
                                };

                System.Text.StringBuilder line = new System.Text.StringBuilder(80);

                // Default settings

                _firstRange = Imports.Range.Range_20MV; // This is for new 220X B, B MSO, 2405A and 2205A MSO models, older devices will have a first range of 50 mV
                _lastRange = Imports.Range.Range_20V;
                _digitalPorts = 0;


                if (_handle >= 0)
                {
                    for (int i = 0; i < description.Length; i++)
                    {
                        short requiredSize;
                        Imports.GetUnitInfo(_handle, line, 80, out requiredSize, i);

                        // Set properties according to the variant
                        if (i == 3)
                        {
                            _channelCount = Convert.ToInt32(line[1].ToString());

                            // Set first range for voltage if device is a 2206/7/8, 2206/7/8A or 2205 MSO
                            if (_channelCount == DUAL_SCOPE)
                            {
                                if (line.Length == 4 || (line.Length == 5 && line[4].Equals('A')) || line.ToString().Equals("2205MSO"))
                                {
                                    _firstRange = Imports.Range.Range_50MV;
                                }
                            }

                            if (line.ToString().EndsWith("MSO"))
                            {
                                _digitalPorts = 2;
                            }

                        }

                        Console.WriteLine("{0}: {1}", description[i], line);
                    }

                    // Find max ADC count
                    status = Imports.MaximumValue(_handle, out _maxValue);

                }
            }

            /****************************************************************************
             * Select input voltage ranges for analogue channels
             ****************************************************************************/
            void SetVoltages()
            {
                bool valid = false;

                /* See what ranges are available... */
                for (int i = (int)_firstRange; i <= (int)_lastRange; i++)
                {
                    Console.WriteLine("{0} . {1} mV", i, inputRanges[i]);
                }

                /* Ask the user to select a range */
                Console.WriteLine();
                Console.WriteLine("Specify voltage range ({0}..{1})", _firstRange, _lastRange);
                Console.WriteLine("99 - switches channel off.");

                for (int ch = 0; ch < _channelCount; ch++)
                {
                    Console.WriteLine("");
                    uint range = 8;

                    do
                    {
                        try
                        {
                            Console.WriteLine("Channel: {0}", (char)('A' + ch));
                            range = uint.Parse(Console.ReadLine());
                            valid = true;
                        }
                        catch (FormatException)
                        {
                            valid = false;
                            Console.WriteLine("\nEnter numeric values only");
                        }

                    } while ((range != 99 && (range < (uint)_firstRange || range > (uint)_lastRange) || !valid));


                    if (range != 99)
                    {
                        _channelSettings[ch].range = (Imports.Range)range;
                        Console.WriteLine(" = {0} mV", inputRanges[range]);
                        _channelSettings[ch].enabled = true;
                    }
                    else
                    {
                        Console.WriteLine("Channel Switched off");
                        _channelSettings[ch].enabled = false;
                    }
                }

                SetDefaults();  // Set defaults now, so that if all but 1 channels get switched off, timebase updates to timebase 0 will work
            }

            /****************************************************************************
             *
             * Select _timebase, set _oversample to on and time units as nano seconds
             *
             ****************************************************************************/
            void SetTimebase()
            {
                int timeInterval;
                int maxSamples;
                bool valid = false;

                Console.WriteLine("Specify timebase");

                do
                {
                    try
                    {
                        _timebase = uint.Parse(Console.ReadLine());
                        valid = true;
                    }
                    catch (FormatException)
                    {
                        valid = false;
                        Console.WriteLine("\nEnter numeric values only");
                    }
                } while (!valid);

                while (Imports.GetTimebase(_handle, _timebase, BUFFER_SIZE, out timeInterval, 1, out maxSamples, 0) != 0)
                {
                    Console.WriteLine("Selected timebase {0} could not be used", _timebase);
                    _timebase++;
                }

                Console.WriteLine("Timebase {0} - {1} ns", _timebase, timeInterval);
                _oversample = 1;
            }


            void SetDefaults()
            {
                for (int i = 0; i < _channelCount; i++) // reset channels to most recent settings
                {
                    uint status = Imports.SetChannel(_handle,
                                       Imports.Channel.ChannelA + i,
                                       (short)(_channelSettings[(int)(Imports.Channel.ChannelA + i)].enabled ? 1 : 0),
                                       _channelSettings[(int)(Imports.Channel.ChannelA + i)].couplingType,
                                       _channelSettings[(int)(Imports.Channel.ChannelA + i)].range,
                                       0);
                }
            }


            /****************************************************************************
            *  SetTrigger
            *  this function sets all the required trigger parameters, and calls the 
            *  triggering functions
            ****************************************************************************/
            uint SetTrigger(Imports.TriggerChannelProperties[] channelProperties,
                            short nChannelProperties,
                            Imports.TriggerConditions[] triggerConditions,
                            short nTriggerConditions,
                            Imports.ThresholdDirection[] directions,
                            Pwq pwq,
                            uint delay,
                            short auxOutputEnabled,
                            int autoTriggerMs,
                            Imports.DigitalChannelDirections[] digitalDirections,
                            short nDigitalDirections)
            {
                uint status;

                if ((status = Imports.SetTriggerChannelProperties(_handle, channelProperties, nChannelProperties, auxOutputEnabled, autoTriggerMs)) != 0)
                {
                    return status;
                }

                if ((status = Imports.SetTriggerChannelConditions(_handle, triggerConditions, nTriggerConditions)) != 0)
                {
                    return status;
                }

                if (directions == null) directions = new Imports.ThresholdDirection[] { Imports.ThresholdDirection.None,
                Imports.ThresholdDirection.None, Imports.ThresholdDirection.None, Imports.ThresholdDirection.None,
                Imports.ThresholdDirection.None, Imports.ThresholdDirection.None};

                if ((status = Imports.SetTriggerChannelDirections(_handle,
                                                                  directions[(int)Imports.Channel.ChannelA],
                                                                  directions[(int)Imports.Channel.ChannelB],
                                                                  directions[(int)Imports.Channel.ChannelC],
                                                                  directions[(int)Imports.Channel.ChannelD],
                                                                  directions[(int)Imports.Channel.External],
                                                                  directions[(int)Imports.Channel.Aux])) != 0)
                {
                    return status;
                }

                if ((status = Imports.SetTriggerDelay(_handle, delay)) != 0)
                {
                    return status;
                }

                if (pwq == null) pwq = new Pwq(null, 0, Imports.ThresholdDirection.None, 0, 0, Imports.PulseWidthType.None);

                status = Imports.SetPulseWidthQualifier(_handle, pwq.conditions,
                                                        pwq.nConditions, pwq.direction,
                                                        pwq.lower, pwq.upper, pwq.type);

                if (_digitalPorts > 0)
                {
                    if ((status = Imports.SetTriggerDigitalPort(_handle, digitalDirections, nDigitalDirections)) != 0)
                    {
                        return status;
                    }
                }

                return status;
            }


            /****************************************************************************
             * adc_to_mv
             *
             * Convert an 16-bit ADC count into millivolts
             ****************************************************************************/
            int adc_to_mv(int raw, int ch)
            {
                return (raw * inputRanges[ch]) / _maxValue;
            }

            /****************************************************************************
             * mv_to_adc
             *
             * Convert a millivolt value into a 16-bit ADC count
             *
             *  (useful for setting trigger thresholds)
             ****************************************************************************/
            short mv_to_adc(short mv, short ch)
            {
                return (short)((mv * _maxValue) / inputRanges[ch]);
            }

            /****************************************************************************
            * BlockCallback
            * used by data block collection calls, on receipt of data.
            * used to set global flags etc checked by user routines
            ****************************************************************************/
            void BlockCallback(short handle, uint status, IntPtr pVoid)
            {
                // flag to say done reading data
                _ready = true;
            }


            /// <summary>
            /// Print the block data capture to file 
            /// </summary>
            private void PrintBlockFile(uint sampleCount, int timeInterval, PinnedArray<short>[] minPinned, PinnedArray<short>[] maxPinned)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("For each of the {0} Channels, results shown are....", _channelCount);
                sb.AppendLine();
                sb.AppendLine("Time interval Maximum Aggregated value ADC Count & mV, Minimum Aggregated value ADC Count & mV");
                sb.AppendLine();

                // Build Header
                string[] heading = { "Time", "Channel", "Max ADC", "Max mV", "Min ADC", "Min mV" };
                sb.AppendFormat("{0,10}", heading[0]);

                for (int i = 0; i < _channelCount; i++)
                {
                    if (_channelSettings[i].enabled)
                    {
                        sb.AppendFormat("{0,10} {1,10} {2,10} {3,10} {4,10}", heading[1], heading[2], heading[3], heading[4], heading[5]);
                    }
                }

                sb.AppendLine();

                // Build Body
                for (int i = 0; i < sampleCount; i++)
                {
                    sb.AppendFormat("{0,10}", (i * timeInterval));

                    for (int ch = 0; ch < _channelCount; ch++)
                    {
                        if (_channelSettings[ch].enabled)
                        {
                            sb.AppendFormat("{0,10} {1,10} {2,10} {3,10} {4,10}",
                                            (char)('A' + ch),
                                            maxPinned[ch].Target[i],
                                            adc_to_mv(maxPinned[ch].Target[i], (int)_channelSettings[(int)(Imports.Channel.ChannelA + ch)].range),
                                            minPinned[ch].Target[i],
                                            adc_to_mv(minPinned[ch].Target[i], (int)_channelSettings[(int)(Imports.Channel.ChannelA + ch)].range));
                        }
                    }

                    sb.AppendLine();

                }

                Console.WriteLine(sb.ToString());

                // Print contents to file
                /*
                using (TextWriter writer = new StreamWriter(BlockFile, false))
                {
                    writer.Write(sb.ToString());
                    writer.Close();
                }
                */
            }

            /****************************************************************************
            * BlockDataHandler
            * - Used by all block data routines
            * - acquires data (user sets trigger mode before calling), displays 10 items
            *   and saves all to block.txt
            * Input :
            * - text : the text to display before the display of data slice
            * - offset : the offset into the data buffer to start the display's slice.
            ****************************************************************************/
            void BlockDataHandler(string text, int offset, Imports.Mode mode)
            {
                uint sampleCount = BUFFER_SIZE;
                PinnedArray<short>[] minPinned = new PinnedArray<short>[_channelCount];
                PinnedArray<short>[] maxPinned = new PinnedArray<short>[_channelCount];

                PinnedArray<short>[] digiPinned = new PinnedArray<short>[_digitalPorts];

                int timeIndisposed;
                uint status = 0; // PICO_OK


                if (mode == Imports.Mode.ANALOGUE || mode == Imports.Mode.MIXED)
                {
                    for (int i = 0; i < _channelCount; i++)
                    {
                        short[] minBuffers = new short[sampleCount];
                        short[] maxBuffers = new short[sampleCount];

                        minPinned[i] = new PinnedArray<short>(minBuffers);
                        maxPinned[i] = new PinnedArray<short>(maxBuffers);

                        status = Imports.SetDataBuffers(_handle, (Imports.Channel)i, maxBuffers, minBuffers, (int)sampleCount, 0, Imports.RatioMode.None);

                        if (status != StatusCodes.PICO_OK)
                        {
                            Console.WriteLine("BlockDataHandler:ps2000aSetDataBuffer Channel {0} Status = 0x{1:X6}", (char)('A' + i), status);
                        }
                    }
                }


                if (mode == Imports.Mode.DIGITAL || mode == Imports.Mode.MIXED)
                {
                    for (int i = 0; i < _digitalPorts; i++)
                    {
                        short[] digiBuffer = new short[sampleCount];
                        digiPinned[i] = new PinnedArray<short>(digiBuffer);

                        status = Imports.SetDataBuffer(_handle, i + Imports.Channel.PS2000A_DIGITAL_PORT0, digiBuffer, (int)sampleCount, 0, Imports.RatioMode.None);

                        if (status != StatusCodes.PICO_OK)
                        {
                            Console.WriteLine("BlockDataHandler:ps2000aSetDataBuffer {0} Status = 0x{1,0:X6}", i + Imports.Channel.PS2000A_DIGITAL_PORT0, status);
                        }
                    }
                }

                /* Find the maximum number of samples and time interval (in nanoseconds) at the current value of the timebase index.
                   If the timebase is invalid increment by one and try again.
                */
                int timeInterval;
                int maxSamples;

                while (Imports.GetTimebase(_handle, _timebase, (int)sampleCount, out timeInterval, _oversample, out maxSamples, 0) != 0)
                {
                    Console.WriteLine("Selected timebase {0} could not be used\n", _timebase);
                    _timebase++;

                }

                Console.WriteLine("Timebase: {0}\tTime interval:{1} ns\n\n", _timebase, timeInterval);

                /* Start it collecting, then wait for completion*/
                _ready = false;
                _callbackDelegate = BlockCallback;
                Imports.RunBlock(_handle, 0, (int)sampleCount, _timebase, _oversample, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);

                //Console.WriteLine("Waiting for data...Press a key to abort");

                //while (!_ready && !Console.KeyAvailable)
                {
                    //    Thread.Sleep(100);
                }

                //    if (Console.KeyAvailable)
                {
                    //      Console.ReadKey(true); // clear the key
                }

                DateTime start = DateTime.Now;
                while ((DateTime.Now < start.AddSeconds(1)) && !_ready)
                    Thread.Sleep(100);


                if (_ready)
                {
                    short overflow;
                    Imports.GetValues(_handle, 0, ref sampleCount, 1, Imports.DownSamplingMode.None, 0, out overflow);

                    /* Print out the first 10 readings, converting the readings to mV if required */
                    Console.WriteLine();
                    Console.WriteLine(text);
                    Console.WriteLine();

                    if (mode == Imports.Mode.ANALOGUE || mode == Imports.Mode.MIXED)
                    {
                        Console.WriteLine("Values are in {0}\n", (_scaleVoltages) ? ("mV") : ("ADC Counts"));

                        for (int ch = 0; ch < _channelCount; ch++)
                        {
                            Console.Write("Channel{0}                 ", (char)('A' + ch));
                        }

                        Console.WriteLine();
                    }

                    if (mode == Imports.Mode.DIGITAL || mode == Imports.Mode.MIXED)
                    {
                        Console.Write("DIGITAL VALUE");
                    }

                    Console.WriteLine();


                    for (int i = offset; i < offset + 10; i++)
                    {
                        if (mode == Imports.Mode.ANALOGUE || mode == Imports.Mode.MIXED)
                        {
                            for (int ch = 0; ch < _channelCount; ch++)
                            {
                                if (_channelSettings[ch].enabled)
                                {
                                    Console.Write("{0,8}                 ", _scaleVoltages ?
                                        adc_to_mv(maxPinned[ch].Target[i], (int)_channelSettings[(int)(Imports.Channel.ChannelA + ch)].range)  // If _scaleVoltages, show mV values
                                        : maxPinned[ch].Target[i]);                                                                           // else show ADC counts
                                }
                            }
                        }

                        if (mode == Imports.Mode.DIGITAL || mode == Imports.Mode.MIXED)
                        {
                            short digiValue = digiPinned[1].Target[i];
                            digiValue <<= 8;
                            digiValue |= digiPinned[0].Target[i];
                            Console.Write("0x{0,4:X}", digiValue.ToString("X4"));
                        }
                        Console.WriteLine();
                    }

                    if (mode == Imports.Mode.ANALOGUE || mode == Imports.Mode.MIXED)
                    {
                        PrintBlockFile(Math.Min(sampleCount, BUFFER_SIZE), timeInterval, minPinned, maxPinned);
                    }
                }
                else
                {
                    Console.WriteLine("data collection aborted");
                }

                Imports.Stop(_handle);

                if (mode == Imports.Mode.ANALOGUE || mode == Imports.Mode.MIXED)
                {
                    foreach (PinnedArray<short> p in minPinned)
                    {
                        if (p != null)
                        {
                            p.Dispose();
                        }
                    }

                    foreach (PinnedArray<short> p in maxPinned)
                    {
                        if (p != null)
                        {
                            p.Dispose();
                        }
                    }
                }

                if (mode == Imports.Mode.DIGITAL || mode == Imports.Mode.MIXED)
                {
                    foreach (PinnedArray<short> p in digiPinned)
                    {
                        if (p != null)
                        {
                            p.Dispose();
                        }
                    }
                }
            }

            /****************************************************************************
            * CollectBlockImmediate
            *  this function demonstrates how to collect a single block of data
            *  from the unit (start collecting immediately)
            ****************************************************************************/
            public void CollectBlockImmediate()
            {
                Console.WriteLine("Collect Block Immediate");
                //  Console.WriteLine("Data is written to disk file ({0})", BlockFile);
                //  Console.WriteLine("Press a key to start...");
                //  Console.WriteLine();
                //  WaitForKey();

                SetDefaults();

                /* Trigger disabled	*/
                SetTrigger(null, 0, null, 0, null, null, 0, 0, 0, null, 0);

                BlockDataHandler("First 10 readings", 0, Imports.Mode.ANALOGUE);
            }



            public override void Destroy()
            {
                base.Destroy();

                this.OnMessageReceived = null;  //removes all existing subscribers                
            }
        }
    }
}