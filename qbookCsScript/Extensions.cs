using Microsoft.CodeAnalysis.CSharp.Syntax;
using QB.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.Json;
using static QB.Extensions;

namespace QB
{
    public static class Extensions
    {
        public static string Tolerant(this string text)
        {
            if (text == null)
                return null;

            return text.ToUpper().Replace("\009", "").Replace(" ", "").Replace("Ä", "AE").Replace("Ö", "OE").Replace("Ü", "UE").Replace("ß", "SS");
        }

        public static DateTime Truncate(this DateTime date, long resolution)
        {
            return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
        }

        public static string Formatted(this float value)
        {
            if (value > 10000)
                return value.ToString("0");
            else if (value > 1000)
                return value.ToString("0");
            else if (value > 100)
                return value.ToString("0.0");
            else if (value > 10)
                return value.ToString("0.00");
            else if (value > 1)
                return value.ToString("0.000");
            else if (value > 0.1)
                return value.ToString("0.0000");


            return value.ToString("0.00000");
        }
        public static float GetFloat(this Net.Can.Message canMessage, int byteOffset, int decimals)
        {
            byte[] bytes = new byte[8];
            bytes[0] = canMessage.Data[byteOffset + 0];
            bytes[1] = canMessage.Data[byteOffset + 1];
            bytes[2] = canMessage.Data[byteOffset + 2];
            bytes[3] = canMessage.Data[byteOffset + 3];
            return (float)Math.Round(BitConverter.ToSingle(bytes, 0), decimals);
        }

        public static float GetInt16(this Net.Can.Message canMessage, int byteOffset, float vGain, float vOffset, int decimalPlaces, bool signed)
        {
            if (signed)
            {
                Int16 v = (Int16)(((UInt16)canMessage.Data[byteOffset + 1] << 8) | (UInt16)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
            else
            {
                UInt16 v = (UInt16)(((UInt16)canMessage.Data[byteOffset + 1] << 8) | (UInt16)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
        }

        public static float GetInt32(this Net.Can.Message canMessage, int byteOffset, float vGain, float vOffset, int decimalPlaces, bool signed)
        {
            if (signed)
            {
                Int32 v = (Int32)(((UInt32)canMessage.Data[byteOffset + 3] << 24) | ((UInt32)canMessage.Data[byteOffset + 2] << 16) | ((UInt32)canMessage.Data[byteOffset + 1] << 8) | (UInt32)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
            else
            {
                UInt32 v = (UInt32)(((UInt32)canMessage.Data[byteOffset + 3] << 24) | ((UInt32)canMessage.Data[byteOffset + 2] << 16) | ((UInt32)canMessage.Data[byteOffset + 1] << 8) | (UInt32)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
        }

        public static float GetInt24(this Net.Can.Message canMessage, int byteOffset, float vGain, float vOffset, int decimalPlaces, bool signed)
        {
            if (signed)
            {
                Int32 v = (Int32)(((UInt32)canMessage.Data[byteOffset + 2] << 16) | ((UInt32)canMessage.Data[byteOffset + 1] << 8) | (UInt32)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
            else
            {
                UInt32 v = (UInt32)(((UInt32)canMessage.Data[byteOffset + 2] << 16) | ((UInt32)canMessage.Data[byteOffset + 1] << 8) | (UInt32)canMessage.Data[byteOffset]);
                return (float)Math.Round(v * vGain + vOffset, decimalPlaces);
            }
        }

        public static string RemoveInvalidCharsFromFilename(this string filename)
        {
            string regex = String.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())));
            System.Text.RegularExpressions.Regex removeInvalidChars
                = new System.Text.RegularExpressions.Regex(regex
                    , System.Text.RegularExpressions.RegexOptions.Singleline
                    | System.Text.RegularExpressions.RegexOptions.Compiled
                    | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            return removeInvalidChars.Replace(filename, ".");
        }

        /*
        public static double ToDouble(this string text)
        {
            return Convert.ToDouble(text);
        }
        */
        public static int ToInt(this string text)
        {
            return (int)Convert.ToDouble(text);
        }
        static int FindMatchingBracket(string str, int bracketPos)
        {
            if (bracketPos >= str.Length)
                return -1;

            if (str[bracketPos] != '[')
                return -1;

            bool inQuotes = false;
            int bracketCount = 0;
            for (int i = bracketPos; i < str.Length; i++)
            {
                if (str[i] == '\"')
                    inQuotes = !inQuotes;

                if (!inQuotes)
                {
                    if (str[i] == '[')
                        bracketCount++;
                    if (str[i] == ']')
                        bracketCount--;
                }
                if (str[i] == ']' && i > bracketPos && bracketCount == 0)
                    return i;
            }
            return -1;
        }
        public static string RemoveRedundantBrackets(string orig)
        {
            if (string.IsNullOrEmpty(orig))
                return orig;

            while (FindMatchingBracket(orig, 0) == orig.Length - 1)
                orig = orig.Substring(1, orig.Length - 2).Trim();

            return orig;
        }
        /*
        public static string[] GetItems(this string orig, char seperator)
        {
            //Console.WriteLine($"GetItems >> <{orig}>");

            //if (string.IsNullOrEmpty(orig))
            if (orig == null)
                return null;

            orig = RemoveRedundantBrackets(orig);

            List<String> splitted = new List<String>();
            int skipCommas = 0;
            int skipCommas2 = 0;
            String s = "";
            for (int i = 0; i < orig.Length; i++)
            {
                char c = orig[i];
                if (c == seperator && skipCommas == 0 && skipCommas2 == 0)
                {
                    s = s.Trim();
                    if (s.StartsWith("[") && s.EndsWith("]") && s.Count(c1 => c1 == '[') == 1 && s.Count(c1 => c1 == ']') == 1)
                        s = s.Substring(1, s.Length - 2);
                    splitted.Add(s);
                    s = "";
                }
                else
                {
                    //brackets can be nested!
                    if (c == '[')
                        skipCommas2++;
                    if (c == ']')
                        if (skipCommas2 > 0)
                            skipCommas2--;

                    //quotes cannot be nested!
                    if (c == '\"')
                        if (skipCommas > 0)
                            skipCommas--;
                        else
                            skipCommas++;
                    s += c;
                }
            }
            if (s.Length > 0)
            {
                s = s.Trim();
                if (s.StartsWith("[") && s.EndsWith("]") && s.Count(c1 => c1 == '[') == 1 && s.Count(c1 => c1 == ']') == 1)
                    s = s.Substring(1, s.Length - 2);
                splitted.Add(s); //.TrimStart('[').TrimEnd(']'));
            }

            //Console.WriteLine($"GetItems >> <{string.Join(" * ", splitted)}>");
            return splitted.ToArray();
        }

        public static string GetItemValue(this string str, string requestKey, string defaultValue)
        {
            //long ticks = DateTime.Now.Ticks;
            if (string.IsNullOrEmpty(str) || str == "#")
                return defaultValue;

            string[] items = GetItems(str, ',');
            if (items == null)
                return defaultValue;

            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                string key = "";
                string value = null;
                //string[] kvp = Regex.Split(item, "=(?=(?:[^'\"]*['\"][^'\"]*['\"])*[^'\"]*$)"); //split all commas outside quotes (single and double)
                string[] kvp = item.Split(new char[] { '=' }, 2); // Split(item, "=',(?=(?:[^'\"]*['\"][^'\"]*['\"])*[^'\"]*$)"); //split all commas outside quotes (single and double)
                if (kvp.Length == 1)
                {
                    key = kvp[0].Trim();
                    value = "";
                }
                else if (kvp.Length > 1)
                {
                    key = kvp[0].Trim();
                    value = kvp[1].Trim();
                    while ((value.StartsWith("[") && value.EndsWith("]"))
                        || (value.StartsWith("\"") && value.EndsWith("\"")))
                        value = value.Substring(1, value.Length - 2);
                }

                if (key.ToUpper() == requestKey.ToUpper())
                {
                    //return value.Trim();
                    value = value.Trim();
                    if (value.StartsWith("[") && !value.EndsWith("]"))
                        value = value.Substring(1);
                    return value;
                }
            }

            return defaultValue; //not found
        }
        */

        public static string[] GetItems(this string orig)
        {
            //orig = orig.Trim();
            //if (orig.EndsWith(","))
            //    orig = orig.Substring(0, orig.Length - 1);
            //if (orig.StartsWith("[") && orig.EndsWith("]"))
            //    orig = orig.Substring(1, orig.Length - 2);
            return GetItems(orig, ',');
        }

        /// <summary>
        /// splits the string at the seperator (default=',')
        /// 1) does not split inside quotes and inside [...]-terms
        /// 2) trims whitespaces around each item 
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string[] GetItems(this string orig, char seperator)
        {
            //split at all seperators (default=comma) outside quotes and brackets

            if (orig == null)
                return null;

            List<String> splitted = new List<String>();
            int skipCommas = 0;
            int skipCommas2 = 0;
            String s = "";
            for (int i = 0; i < orig.Length; i++)
            {
                char c = orig[i];
                if (c == seperator && skipCommas == 0 && skipCommas2 == 0)
                {
                    s = s.Trim();
                    if (s.StartsWith("[") && s.EndsWith("]") && s.Count(c1 => c1 == '[') == 1 && s.Count(c1 => c1 == ']') == 1)
                        s = s.Substring(1, s.Length - 2);
                    splitted.Add(s);
                    s = "";
                }
                else
                {
                    //brackets can be nested!
                    if (c == '[')
                        skipCommas2++;
                    if (c == ']')
                        if (skipCommas2 > 0)
                            skipCommas2--;

                    //quotes cannot be nested!
                    if (c == '\"')
                        if (skipCommas > 0)
                            skipCommas--;
                        else
                            skipCommas++;
                    s += c;
                }
            }
            if (s.Length > 0)
            {
                s = s.Trim();
                if (s.StartsWith("[") && s.EndsWith("]") && s.Count(c1 => c1 == '[') == 1 && s.Count(c1 => c1 == ']') == 1)
                    s = s.Substring(1, s.Length - 2);
                splitted.Add(s); //.TrimStart('[').TrimEnd(']'));
            }

            return splitted.ToArray();
        }

        public static string GetItemValue(this string str, string requestKey)
        {
            return GetItemValue(str, requestKey, null);
        }
        public static string GetItemValue(this string str, string requestKey, string defaultValue)
        {
            long ticks = DateTime.Now.Ticks;
            string[] items = GetItems(str);
            if (items == null)
                return null;

            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                string key = "";
                string value = null;
                //string[] kvp = Regex.Split(item, "=(?=(?:[^'\"]*['\"][^'\"]*['\"])*[^'\"]*$)"); //split all commas outside quotes (single and double)
                string[] kvp = item.Split(new char[] { '=' }, 2); // Split(item, "=',(?=(?:[^'\"]*['\"][^'\"]*['\"])*[^'\"]*$)"); //split all commas outside quotes (single and double)
                if (kvp.Length == 1)
                {
                    key = kvp[0].Trim();
                    value = "";
                }
                else if (kvp.Length > 1)
                {
                    key = kvp[0].Trim();
                    value = kvp[1].Trim();
                    while ((value.StartsWith("[") && value.EndsWith("]"))
                        || (value.StartsWith("\"") && value.EndsWith("\"")))
                        value = value.Substring(1, value.Length - 2);
                }

                if (key.ToUpper() == requestKey.ToUpper())
                {
                    //return value.Trim();
                    //SCAN  value = value.Trim();
                    if (value.StartsWith("[") && !value.EndsWith("]"))
                        value = value.Substring(1);
                    return value;
                }
            }

            return defaultValue; //not found
        }


        public static bool GetItemBoolValue(this string str, string requestKey)
        {
            return (str.GetItemValue(requestKey, "0").Tolerant() != "0");
        }

        public static bool HasItemBoolValue(this string str, string requestKey, bool defaultValue)
        {
            string v = str.GetItemValue(requestKey, "x");

            if (v == "x") //not present, default value
                return defaultValue;

            if (v == "") //present, but no value
                return true;

            if (v == "0")
                return false;

            return true;
        }


        public static double GetItemDoubleValue(this string str, string requestKey, double defaultValue)
        {
            string v = GetItemValue(str, requestKey);
            if (v == null)
                return defaultValue;

            double d = double.NaN;
            if (double.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                return d;
            else
                return defaultValue;
        }


        public static string GetItemByIndex(this string str, int index)
        {
            string[] items = GetItems(str);
            if (index < items.Length)
                return items[index];
            else
                return null;
        }

        public static bool HasItemKey(this string str, string requestKey)
        {
            if (GetItemValue(str, requestKey) != null)
                return true;
            else
                return false;
        }

        public static double ToDouble(this string text)
        {
            if (text == null) return double.NaN;
            if (text == "") return double.NaN;
            text = text.Tolerant();
            if (text.Contains("#")) return double.NaN;
            if (text.Contains("INF")) return double.NaN;
            if (text.Contains("DEF")) return double.NaN;
            if (text.Contains("?")) return double.NaN;
            try
            {


                if (text.Contains("LONG"))
                {
                    return 32;
                }
                if (text.Contains("INT32"))
                {
                    return 32;
                }
                if (text.Contains("INT8"))
                {
                    return 8;
                }
                if (text.Contains("INT"))
                {
                    return 16;
                }
                if (text.Contains("BYTE"))
                {
                    return 8;
                }
                if (text.Contains("CHAR"))
                {
                    return 8;
                }
                if (text.Contains("H"))
                {
                    text = text.Replace("H", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("0X"))
                {
                    text = text.Replace("0X", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("X"))
                {
                    text = text.Replace("X", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("B")) // muss nach h stehen sonst wird b falsch interpretierr
                {
                    text = text.Replace("B", "");
                    return Convert.ToUInt32(text, 2);
                }
            }
            catch
            {
                //    log.Fatal("%ToDouble1% " + text + " " + sender + " " + ex.Message);
            }


            string comma = "" + (("" + 1.1)[1]);
            text = text.Replace(" ", "").Replace("#", "").Replace(",", comma).Replace(".", comma);
            if (text.Trim() == "NAN") return double.NaN;
            try
            {
                double d = 0;
                if (double.TryParse(text, out d))
                    return d;

                return double.NaN;//.Parse(text);
            }
            catch
            {
                //   log.Fatal("%ToDouble2% " + text + " " + sender + " " + ex.Message);
            }
            return double.NaN;
        }


        public static string SplitStringAt(this string str, int index, string defaultValue = null, char splitChar = ' ')
        {
            var splits = str.Split(splitChar);
            if (index < 0 || index >= splits.Length)
            {
                return defaultValue;
            }
            return splits[index];
        }

        public static double SplitDoubleAt(this string str, int index, double defaultValue = double.NaN, char splitChar = ' ')
        {
            var splits = str.Split(splitChar);
            if (index < 0 || index >= splits.Length)
            {
                return defaultValue;
            }
            return splits[index].ToDouble();
        }


        public static string ToString(this double value, int maxPlaces, int decimalPlaces)
        {
            if (double.IsNaN(value)) return "#";
            string text = "";
            if (maxPlaces < 3) maxPlaces = 3;
            try
            {

                switch (decimalPlaces)
                {
                    case 1: text = String.Format("{0:0.0}", value).Replace(',', '.'); break;
                    case 2: text = String.Format("{0:0.00}", value).Replace(',', '.'); break;
                    case 3: text = String.Format("{0:0.000}", value).Replace(',', '.'); break;
                    case 4: text = String.Format("{0:0.0000}", value).Replace(',', '.'); break;
                    case 5: text = String.Format("{0:0.00000}", value).Replace(',', '.'); break;
                    case 6: text = String.Format("{0:0.000000}", value).Replace(',', '.'); break;
                    case 7: text = String.Format("{0:0.0000000}", value).Replace(',', '.'); break;
                    case 8: text = String.Format("{0:0.00000000}", value).Replace(',', '.'); break;
                    case 9: text = String.Format("{0:0.000000000}", value).Replace(',', '.'); break;
                    case 10: text = String.Format("{0:0.0000000000}", value).Replace(',', '.'); break;
                    default: text = String.Format("{0:0}", value).Replace(',', '.'); break;
                }
                if (text.Length > maxPlaces)
                {
                    text = text.Substring(0, maxPlaces);
                    if (text[maxPlaces - 1] == '.') text = text.Substring(0, maxPlaces - 1);
                }
            }
            catch
            {
                //MessageBox.Show(new System.Windows.Forms.Form { TopMost = true }, "ToString " + value, "XERO Error");
            }
            return text;
        }

        public class SignalJson
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public string Unit { get; set; }
            public double Value { get; set; }
        }


        public static string ToJson(this Signal signal)
        {
            return JsonSerializer.Serialize<SignalJson>(new SignalJson() 
            { 
            Name = signal.Name,
            Text = signal.Text,
            Unit = signal.Unit,
            Value = signal.Value,
            });
        }



        public static string Truncate(this string s, int length, string ellipsis = "")
        {
            if (s.Length > length)
                return s.Substring(0, length) + ellipsis;
            return s;
        }

        public static bool[] SubBits(this byte[] bytes, int start, int len)
        {
            if (start + len > bytes.Length * 8)
                return new bool[0];

            bool[] bitArrayAll = new bool[bytes.Length * 8];
            for (int i = 0; i < bytes.Length * 8; i++)
            {
                int index = /*bytes.Length - 1 - */i / 8;
                int bit = 7 - i % 8;
                bitArrayAll[i] = ((bytes[index] >> (bit)) & 1) == 1;
            }

            bool[] bitArray = bitArrayAll.Skip((int)start).Take((int)len).ToArray();
            return bitArray;
        }

        public static byte[] SubBytes(this byte[] bytes, int start, int len, bool bigEndian = false, int byteLen = -1)
        {
            bool[] bits = SubBits(bytes, start, len);
            if (byteLen == -1)
                byteLen = (len - 1) / 8 + 1;

            int stuffBitsCount = byteLen * 8 - bits.Length;
            if (stuffBitsCount > 0)
            {
                bool[] preBits = new bool[stuffBitsCount];
                if (bigEndian)
                    bits = bits.Concat(preBits).ToArray();
                else
                    bits = preBits.Concat(bits).ToArray();
            }

            byte[] newBytes = new byte[byteLen];

            int index = 0;
            for (int bit = 0; bit < bits.Length; bit++)
            {
                newBytes[index] |= bits[bit] ? (byte)(1 << (7 - bit % 8)) : (byte)0;
                if (bit % 8 == 7)
                    index++;
            }

            if (bigEndian)
                return newBytes.Reverse().ToArray();
            else
                return newBytes;
        }

        public static System.Drawing.Color ChangeBrightness(this System.Drawing.Color color, double percent)
        {

            try
            {
                float correctionFactor = (float)percent / 100;
     
                float red = (float)color.R;
                float green = (float)color.G;
                float blue = (float)color.B;

                if (correctionFactor < 0)
                {
                    correctionFactor = 1 + correctionFactor;
                    red *= correctionFactor;
                    green *= correctionFactor;
                    blue *= correctionFactor;

                    red = red < 0 ? 0 : red;
                    green = green < 0 ? 0 : green;
                    green = green < 0 ? 0 : green;
                }
                else
                {
                    red = (255 - red) * correctionFactor + red;
                    green = (255 - green) * correctionFactor + green;
                    blue = (255 - blue) * correctionFactor + blue;

                    red = red > 255 ? 255 : red;
                    green = green > 255 ? 255 : green;
                    green = green > 255 ? 255 : green;
                }
                
                
                return System.Drawing.Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
            }
            catch
            {
                QB.Logger.Error("Change colorbrightness failed");
                return System.Drawing.Color.Black;
            } 

        }
        public static System.Drawing.Color ChangeTransparency(this System.Drawing.Color color, double percent)
        {
            try
            {
                if (percent < 0 || percent > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 100.");
                }

                int alpha = (int)(percent * 255 / 100);

                return System.Drawing.Color.FromArgb(alpha, color.R, color.G, color.B);
            }
            catch (Exception ex)
            {
                // Log the error using your preferred logging framework
                QB.Logger.Error("Change transparency failed: " + ex.Message);
                return System.Drawing.Color.Black;
            }
        }


        public static System.Drawing.Color GetColor(object color) => color.ToColor();

        public static System.Drawing.Color ToColor(this object color)
        {
            try
            {
                if (color is System.Drawing.Color)
                    return (System.Drawing.Color)color;

                if (color is string set)
                {
                    if (set.StartsWith("#"))
                        return System.Drawing.ColorTranslator.FromHtml(set);
                    else
                        return System.Drawing.Color.FromName(set);
                }

                // Log if the input is not a recognized type
                QB.Logger.Error($"ToColor: Unsupported color type - {color?.GetType().Name ?? "null"}");
                return System.Drawing.Color.Black;
            }
            catch (Exception ex)
            {
                // Log exceptions explicitly
                QB.Logger.Error($"ToColor: Error translating color. Exception: {ex.Message}");
                return System.Drawing.Color.Black;
            }
        }

        //Edit Varibales
        public static void ShowEditDialog(ref this double v, string text = "")
        {
            {
                double localValue = v;

                // Pass the local variable to NumBlock
                NumBlock edit = new NumBlock(
                    getter: () => localValue,
                    setter: (value) => localValue = value,
                    text: text
                );

                edit.ShowDialog();

                // Update the ref parameter after dialog closes
                v = localValue;

            }
        }

        public static void ShowEditTextDialog(ref string s, string text = "")
        {
            string localValue = s;

            Keyboard keyboard = new Keyboard(
               getter: () => localValue,
               setter: (val) => localValue = val,
               text: text
           );

            keyboard.Qertz();
            keyboard.ShowNumblock();
            keyboard.ShowDialog();

            // Update the ref parameter after dialog closes
            s = localValue;
        }
        public static void ShowEditDialog(this string v, ref string value, string unit1 = "", string unit2 = "", string unit3 = "", string unit4 = "", string text = "")
        {
            string localValue = value;

            // Pass the local variable to NumBlock
            NumBlock edit = new NumBlock(
                getter: () => localValue,
                setter: (val) => localValue = val,
                text: text,
                unit1: unit1,
                unit2: unit2,
                unit3: unit3,
                unit4: unit4
            );

            edit.ShowDialog();

            // Update the reference parameter after dialog closes
            value = localValue;
        }
        public static void ShowEditDialog(ref this int v, string text = "",string unit = "", string unit2 = "", string unit3 = "", string unit4 = "", int min = int.MinValue, int max = int.MaxValue)
        {
            {
                int localValue = v;

                // Pass the local variable to NumBlock
                NumBlock edit = new NumBlock(
                    getter: () => localValue,
                    setter: (value) => localValue = value,
                    cText: text,
                    cUnit:unit,
                    cMax:max,
                    cMin:min
                
                );

                edit.ShowDialog();

                // Update the ref parameter after dialog closes
                v = localValue;

            }
        }
        //

        //Edit Classes Signal, Module
        public static void ShowEditDialog(this Signal signal, double min = double.NaN, double max = double.NaN)
        {
         NumBlock edit = new NumBlock(signal, min, max);
         edit.ShowDialog(); 
        }
        public static void ShowEditDialog(this Module signal,double min = double.NaN, double max = double.NaN)
        {
            NumBlock edit = new NumBlock(signal,min,max);
            edit.ShowDialog();
        }
        //

        public static Image ConvertBase64ToImage(string base64String)
        {
            // Decode the Base64 string to a byte array
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // Create a MemoryStream from the byte array
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert the byte array to an Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

    }

    public static class ControlExtensions
    {
        public static void EnsureBeginInvoke(this System.Windows.Forms.Control control, System.Action action)
        {
            if (control.InvokeRequired) control.BeginInvoke(action);
            else action();
        }

        public static void EnsureInvoke(this System.Windows.Forms.Control control, System.Action action)
        {
            if (control.InvokeRequired) control.Invoke(action);
            else action();
        }
    }
}


