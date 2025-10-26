using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB
{
    public static class Util
    {
        public static string GetApplicationPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        }

        public static string Prefix(this string text, string separator)
        {
            if (!text.Contains(separator)) return text;
            int index = text.IndexOf(separator);
            if (index < 0) return text;
            return text.Substring(0, index);
        }

        public static string Postfix(this string text, string separator)
        {
            if (!text.Contains(separator)) return "";
            int index = text.IndexOf(separator);
            if (index < 0) return "";
            return text.Substring(index + separator.Length);
        }
    }

    public class DbcDecoder
    {
        public SortedDictionary<UInt32, DbcMessage> DbcMessages = new SortedDictionary<uint, DbcMessage>();

        static char comma = ',';

        public void Decode(UInt32 id, byte dlc, byte[] data)//, ref canMessageDecode cmd)
        {
            int mux;

        //    string value;
            id &= 0x3fffffff;
            if (DbcMessages.ContainsKey(id))
            {
                DbcMessage dm = DbcMessages[id];
                foreach (DbcValue canValue in DbcMessages[id].DbcValues.Values)
                {
                    if (canValue.ByteDecoder.Mux != null)
                    {
                        if (canValue.ByteDecoder.MuxValue == canValue.ByteDecoder.Mux.ValueDoubleFromBytes(data))
                        {
                            // double v = canValue.ByteDecoder.ValueDoubleFromBytes(data);
                            // canValue.SetValue(v);
                            mux = (int)(canValue.ByteDecoder.MsgOffset << 8) + canValue.ByteDecoder.MuxValue;
                            canValue.Value = canValue.ByteDecoder.ValueDoubleFromBytes(data);
                            //value =  mux.ToString("X4") + " " + canValue.Name.PadLeft(30) + " " + canValue.ValueUnit;
                     //       value = canValue.Name.PadLeft(30) + " " + canValue.ValueUnit;

                      //      cmd.SetDecoded(mux, value);

                            //return true;
                        }
                    }
                    else
                    {
                        mux = (int)(canValue.ByteDecoder.MsgOffset << 8);
                        canValue.Value = canValue.ByteDecoder.ValueDoubleFromBytes(data);
                        //value = mux.ToString("X4") +  " " + canValue.Name.PadLeft(30) + " " +  canValue.ValueUnit;
                      //  value = canValue.Name.PadLeft(30) + " " + canValue.ValueUnit;
                    //    cmd.SetDecoded(mux, value);
                        //     return true;
                    }
                }
            }
            //   return false;
        }

        double TextToDouble(string text)
        {
            return double.Parse(text.Replace('.', comma));
        }

        public DbcDecoder(string fileName)
        {
            comma = ("" + 1.2)[1];
            Dictionary<uint, ByteDecoder> Multiplexer = new Dictionary<uint, ByteDecoder>();
            DbcMessages.Clear();
            if (System.IO.File.Exists(fileName))
            {
                System.IO.StreamReader swfile = new System.IO.StreamReader(fileName, Encoding.Default);
                uint actualId = uint.MaxValue;
                do
                {

                    try
                    {

                        if (!swfile.EndOfStream)
                        {
                            string line = swfile.ReadLine();
                            if (line.StartsWith("BO_"))
                            {
                                line = line.Replace(" :", ":");
                                actualId = uint.Parse(line.Split()[1]);
                                byte dlc = byte.Parse(line.Split()[3]);
                                actualId &= 0x1fffffff;
                                if (!DbcMessages.ContainsKey(actualId))
                                {
                                    DbcMessage canMessage = new DbcMessage();
                                    canMessage.Id = actualId;
                                    canMessage.Name = line.Trim().Split()[2].TrimEnd(':');
                                    canMessage.Dlc = dlc;
                                    DbcMessages.Add(actualId, canMessage);
                                }
                            }
                            if (line.Trim().StartsWith("SG_"))
                            {
                                line = line.Replace("SG_", "");
                                line = line.Replace(" ", " ");
                                string name = line.Prefix(":");
                                string multiplexer = "";
                                int muxValue = -1;
                                string[] nameSplit = name.Trim().Split();
                                if (nameSplit.Length == 2) // multiplexer
                                {
                                    name = nameSplit[0];
                                    multiplexer = nameSplit[1];
                                    try
                                    {
                                        if (multiplexer.Length > 1)
                                        {
                                            muxValue = int.Parse(multiplexer.Substring(1));
                                        }
                                    }
                                    catch
                                    {
                                        muxValue = -1;
                                    }
                                }
                                name = name.Trim();
                                line = line.Postfix(":");
                                string msgOffset = line.Prefix("|");
                                line = line.Postfix("|");
                                string bits = line.Prefix("@");
                                line = line.Postfix("@");
                                string byteOrder = "INTEL";
                                bool signed = false;
                                if (line[0] == '0') byteOrder = "MOTOROLA";
                                if (line[1] == '-') signed = true;
                                double gain = 1;
                                double offset = 0;
                                double min = 0;
                                double max = 0;
                                if (line.Contains("(") && line.Contains(")"))
                                {
                                    line = line.Postfix("(");
                                    gain = TextToDouble(line.Prefix(","));
                                    line = line.Postfix(",");
                                    offset = TextToDouble(line.Prefix(")"));
                                }
                                if (line.Contains("[") && line.Contains("]"))
                                {
                                    line = line.Postfix("[");
                                    min = TextToDouble(line.Prefix("|"));
                                    line = line.Postfix("|");
                                    max = TextToDouble(line.Prefix("]"));
                                }
                                line = line.Postfix("\"");
                                string unit = line.Prefix("\"");
                                line = line.Postfix("\"");
                                string suf = line.Trim();
                                string color = "Black";
                                string panel = "Px";
                                if (suf.Contains("_"))
                                {
                                    try
                                    {
                                        panel = suf.Split('_')[0];
                                        color = suf.Split('_')[1];
                                    }
                                    catch
                                    { }
                                }


                                if (multiplexer.Trim() == "M")
                                {
                                    ByteDecoder byteDecoder = new ByteDecoder(msgOffset, bits, byteOrder, signed);
                                    if (!Multiplexer.ContainsKey(actualId))
                                        Multiplexer.Add(actualId, byteDecoder);
                                }
                                else
                                {
                                    DbcValue canValue = new DbcValue();

                                    //name = DbcMessages[actualId].Name.Trim() + "." + name;
                                    name = name;
                                    canValue.Name = name;
                                    canValue.Unit = unit;
                                    canValue.ByteDecoder = new ByteDecoder(msgOffset, bits, byteOrder, signed);
                                    canValue.ByteDecoder.MuxValue = muxValue;
                                    canValue.ByteDecoder.Offset = offset;
                                    canValue.ByteDecoder.Gain = gain;
                                    canValue.Min = min;
                                    canValue.Max = max;
                                    canValue.Interval = 0;
                                    canValue.Panel = panel;
                                    canValue.Color = color;
                                    if (DbcMessages[actualId].DbcValues.ContainsKey(name))
                                    {
                                        //      log.Error("EX_CanMap ImportFromDbc " + "CanMAp - " + name + " bereits vorhanden");
                                    }
                                    else
                                    {
                                        DbcMessages[actualId].DbcValues.Add(name, canValue);
                                        canValue.Parent = DbcMessages[actualId];
                                    }
                                }
                            }
                            if (line.Trim().StartsWith("SIG_VALTYPE_"))
                            {
                                line = line.Trim();
                                line = line.Postfix(" ");
                                string v1 = line.Prefix(" ");
                                line = line.Postfix(" ");
                                string name = line.Prefix(":").Replace(" ", "");
                                line = line.Postfix(":");
                                string type = line.Prefix(";").Replace(" ", "");
                                foreach (DbcMessage message in DbcMessages.Values)
                                {
                                    foreach (DbcValue link in message.DbcValues.Values)
                                    {
                                        if (link.Name.ToUpper().Trim() == name.ToUpper().Trim())//message.Name.ToUpper() + "." + name.ToUpper().Trim())
                                        {
                                            if (type == "1")
                                                link.ByteDecoder.Ieee = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                while (!swfile.EndOfStream);
                swfile.Close();

                foreach (uint messageId in DbcMessages.Keys)
                {
                    if (Multiplexer.ContainsKey(messageId))
                    {
                        ByteDecoder muxBitmask = Multiplexer[messageId];
                        foreach (DbcValue link in DbcMessages[messageId].DbcValues.Values)
                        {
                            if (link.ByteDecoder.MuxValue >= 0)
                            {
                                link.ByteDecoder.Mux = new ByteDecoder();
                                link.ByteDecoder.Mux.MsgOffset = muxBitmask.MsgOffset;
                                link.ByteDecoder.Mux.MsgBits = muxBitmask.MsgBits;
                            }
                        }
                    }
                }
            }
        }
    }
}
