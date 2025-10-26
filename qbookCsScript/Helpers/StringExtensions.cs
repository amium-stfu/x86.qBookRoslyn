using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QB
{
    public static class StringExtensions
    {
        public static string ReplaceInsideQuotes(this string str, string from, string to)
        {
            var splits = str.Split('\"');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < splits.Length; i++)
            {
                if ((i & 1) == 1)
                {
                    sb.Append("\"" + splits[i].Replace(from, to) + "\"");
                }
                else
                {
                    sb.Append(splits[i]);
                }
            }
            return sb.ToString();
        }

        public static string ReplaceOutsideQuotes(this string str, string from, string to)
        {
            var splits = str.Split('\"');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < splits.Length; i++)
            {
                if ((i & 1) == 1)
                {
                    sb.Append("\"" + splits[i] + "\"");
                }
                else
                {
                    sb.Append(splits[i].Replace(from, to));
                }
            }
            return sb.ToString();
        }

        public static string[] SplitOutsideQuotes(this string str, char splitChar = ',', int maxItems = 0)
        {
            if (str == null)
                return null;
            var splits = Regex.Split(str, splitChar + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (maxItems > 0 && splits.Length > maxItems)
            {
                var splitsNew = splits.Take(maxItems - 1).ToList();
                splitsNew.Add(string.Join(splitChar + "", splits.Skip(maxItems - 1)));
                return splitsNew.ToArray();
            }
            else
            {
                return splits;
            }
        }

        public static string[] SplitOutsideSingleQuotes(this string str, char splitChar = ',', int maxItems = 0)
        {
            if (str == null)
                return null;
            var splits = Regex.Split(str, splitChar + "(?=(?:[^\']*\'[^\']*\')*[^\']*$)");
            if (maxItems > 0 && splits.Length > maxItems)
            {
                var splitsNew = splits.Take(maxItems - 1).ToList();
                splitsNew.Add(string.Join(splitChar + "", splits.Skip(maxItems - 1)));
                return splitsNew.ToArray();
            }
            else
            {
                return splits;
            }
        }

        public static string[] SplitOutsideQuotesAndParenthesis(this string str, char splitChar = ',')
        {
            List<string> splits = new List<string>();
            int parCount = 0;
            bool inQuotes = false;
            int start = 0;
            int len = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == splitChar)
                {
                    if (!inQuotes && (parCount == 0))
                    {
                        if (start + len < str.Length)
                            splits.Add(str.Substring(start, len));
                        else if (start < str.Length)
                            splits.Add(str.Substring(start));
                        start += len + 1;
                        len = 0;
                    }
                    else
                    {
                        len++;
                    }
                }
                else
                {
                    len++;
                    if (str[i] == '(')
                        parCount++;
                    else if (str[i] == ')')
                        parCount--;
                    else if (str[i] == '\"')
                        inQuotes = !inQuotes;
                }
            }
            if (start < str.Length)
                splits.Add(str.Substring(start));
            return splits.ToArray();
        }

        public static string ToMagesTerm(this string str, string path)
        {
            return str;
        }

        public static string FromMagesTerm(this string str)
        {
            return str;
        }

        ////replace only outside quotes (also handling escaped quotes!) --> https://stackoverflow.com/questions/6462578/regex-to-match-all-instances-not-inside-quotes
        //// regex (replace 'text'): \\"|"(?:\\"|[^"])*"|(text)
        //static Regex hierachyRxSimple = new Regex(@"\\([^\d])", RegexOptions.Compiled);
        //static Regex hierachyRx = new Regex(@"\\""|""(?:\\""|[^""])*""|(\\([^\d]))", RegexOptions.Compiled);
        //static Regex indexer1Rx = new Regex(@"\\""|""(?:\\""|[^""])*""|((([_a-zA-Z][_a-zA-Z0-9\\]*)\[([^\]]+)\])(?!\.)\s*)", RegexOptions.Compiled);
        //static Regex indexer2Rx = new Regex(@"\\""|""(?:\\""|[^""])*""|((([_a-zA-Z][_a-zA-Z0-9\\]*)\[([^\]]+)\])((\.[_a-zA-Z][_a-zA-Z0-9\\]*?))\s*)", RegexOptions.Compiled);
        //static Regex identifierOutsideQuotes = new Regex(@"\\""|""(?:\\""|[^""])*""|([\._a-zA-Z\\$][_a-zA-Z0-9\\\.]*)", RegexOptions.Compiled);

        //static string ReplaceHierarchy(Match m)
        //{
        //    if (!m.Groups[1].Success)
        //        return m.ToString();
        //    else
        //        return $"»{m.Groups[2].Value}";
        //}
        //static string ReplaceIndexer1(Match m)
        //{
        //    if (!m.Groups[1].Success)
        //        return m.ToString();
        //    else
        //        //return "$2.at($3).value";
        //        return $"{m.Groups[3].Value}.at({m.Groups[4].Value}).value";
        //}
        //static string ReplaceIndexer2(Match m)
        //{
        //    if (!m.Groups[1].Success)
        //        return m.ToString();
        //    else
        //        return $"{m.Groups[3].Value}.at({m.Groups[4].Value}){m.Groups[5].Value}";

        //}
        //static string ReplaceIdentifierOutsideQuotes(Match m, string _this = null)
        //{
        //    if (m.Groups[1].Success)
        //    {
        //        string _parent = null;
        //        if (_this != null)
        //        {
        //            _this = _this.TrimEnd('\\');
        //            int lastSloshPos = _this.LastIndexOf('\\');
        //            if (lastSloshPos >= 0)
        //                _parent = _this.Substring(0, lastSloshPos);
        //        }

        //        //Console.WriteLine("g1:" + m.Groups[1]);
        //        var s = m.Groups[1].Value;
        //        if (s.StartsWith("$this\\"))
        //            return ($"{_this}\\{m.Groups[1].Value.Substring(6)}").TrimStart('\\');
        //        else if (s.StartsWith("$this."))
        //            return ($"{_this}.{m.Groups[1].Value.Substring(6)}").TrimStart('\\');
        //        else if (s.StartsWith("$parent\\"))
        //            return ($"{_parent}\\{m.Groups[1].Value.Substring(8)}").TrimStart('\\');
        //        else if (s.StartsWith("$parent."))
        //            return ($"{_parent}.{m.Groups[1].Value.Substring(8)}").TrimStart('\\');
        //        else if (s.StartsWith("\\"))
        //            return s.Substring(1);
        //        else if (s.StartsWith("."))
        //            return s;
        //        else
        //        {
        //            if (!Main.Qb.KnownObjectNames.Contains(s))
        //                return ($"{_this}\\{s}").TrimStart('\\');
        //            else
        //                return $"{s}";
        //        }
        //    }
        //    else
        //        return m.ToString();
        //}

        //static MatchEvaluator ReplaceIndexer1MatchEvaluator = new MatchEvaluator(ReplaceIndexer1);
        //static MatchEvaluator ReplaceIndexer2MatchEvaluator = new MatchEvaluator(ReplaceIndexer2);
        //static MatchEvaluator ReplaceHierachyMatchEvaluator = new MatchEvaluator(ReplaceHierarchy);
        ////static MatchEvaluator ReplaceIdentifierOutsideQuotesEvaluator = new MatchEvaluator(ReplaceIdentifierOutsideQuotes);

        //public static string ToMagesTerm(this string str, string path)
        //{
        //    //if (false)
        //    //{ 
        //    //    var splits = str.Split('\"');
        //    //    StringBuilder sb = new StringBuilder();
        //    //    for (int i = 0; i < splits.Length; i++)
        //    //    {
        //    //        if ((i & 1) == 0)
        //    //        {
        //    //            //sb.Append(Regex.Replace(splits[i], @"\\([^\d])", "»$1")); 
        //    //            string term = splits[i];
        //    //            //term = Regex.Replace(term, @"\\([^\d])", "»$1");

        //    //            //1. add "$this." to all identifiers not starting with "\"
        //    //            term = term.ReplaceOutsideQuotes("$this.", ""); 
        //    //            term = identifierOutsideQuotes.Replace(term, new MatchEvaluator(ReplaceIdentifierOutsideQuotesEvaluator));
        //    //            term = term.ReplaceOutsideQuotes("$$this.parent.", "$parent.");
        //    //            //2. replace "\" with "»"
        //    //            term = hierachyRxSimple.Replace(term, "»$1");

        //    //            if (true) //replace "[...]" with ".at(...).value" and "[...].xyz" with ".at(...).xyz"
        //    //            {
        //    //                term = indexer1Rx.Replace(term, new MatchEvaluator(ReplaceIndexer1));
        //    //                term = indexer2Rx.Replace(term, new MatchEvaluator(ReplaceIndexer2));
        //    //            }
        //    //            sb.Append(term);

        //    //        }
        //    //        else
        //    //        {
        //    //            sb.Append("\"" + splits[i] + "\"");
        //    //        }
        //    //        return sb.ToString();
        //    //    }
        //    //}

        //    if (true)
        //    {



        //        if (string.IsNullOrEmpty(str))
        //            return "";
        //        string term = str;
        //        //1. add "$this." to all identifiers not starting with "\"

        //        //SCAN - TODO
        //        //if (path == null)
        //        term = identifierOutsideQuotes.Replace(term, match => ReplaceIdentifierOutsideQuotes(match, path)); //new MatchEvaluator(ReplaceIdentifierOutsideQuotesEvaluator));
        //        //2. replace "\" with "»"
        //        term = hierachyRx.Replace(term, ReplaceHierachyMatchEvaluator); // new MatchEvaluator(ReplaceHierarchy));
        //        if (term.EndsWith("\\"))
        //            term = term.Substring(0, term.Length - 1)+"»";
        //        if (term.Contains("[") && term.Contains("]")) //performance?!
        //        {
        //            //replace "[...]" with ".at(...).value" and "[...].xyz" with ".at(...).xyz"
        //            term = indexer1Rx.Replace(term, ReplaceIndexer1MatchEvaluator); // new MatchEvaluator(ReplaceIndexer1));
        //            term = indexer2Rx.Replace(term, ReplaceIndexer2MatchEvaluator); // new MatchEvaluator(ReplaceIndexer2));
        //        }
        //        //if (_this != null)
        //        //{
        //        //    _this = _this.TrimEnd('.');>	qbook.exe!StringExtensions.ToMagesTerm(string str, string path) Line 225	C#

        //        //    term = term.Replace("$this.", _this + ".");
        //        //}

        //        //SCAN - TODO
        //        string t = term;
        //        t = t.Replace("»»", "»");
        //        t = t.Trim('»');
        //        if (t != term)
        //        {
        //            term = t;
        //        }

        //        return term;
        //    }

        //}

        //public static string FromMagesTerm(this string str)
        //{
        //    var splits = str.Split('\"');
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < splits.Length; i++)
        //    {
        //        if ((i & 1) == 0)
        //        {
        //            //sb.Append(splits[i].Replace(from, to));
        //            sb.Append(Regex.Replace(splits[i], @"»([^\d])", "\\$1"));
        //        }
        //        else
        //        {
        //            sb.Append(splits[i]);
        //        }
        //    }
        //    return sb.ToString();
        //}

        //public static string x_ToMagesTerm(this string str)
        //{
        //    var splits = str.Split('\"');
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < splits.Length; i++)
        //    {
        //        if ((i & 1) == 0)
        //        {
        //            sb.Append(Regex.Replace(splits[i], @"\.([^\d])", "»$1"));
        //        }
        //        else
        //        {
        //            sb.Append("\"" + splits[i] + "\"");
        //        }
        //    }
        //    return sb.ToString();
        //}

        //public static string x_FromMagesTerm(this string str)
        //{
        //    var splits = str.Split('\"');
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < splits.Length; i++)
        //    {
        //        if ((i & 1) == 0)
        //        {
        //            //sb.Append(splits[i].Replace(from, to));
        //            sb.Append(Regex.Replace(splits[i], @"»([^\d])", ".$1"));
        //        }
        //        else
        //        {
        //            sb.Append(splits[i]);
        //        }
        //    }
        //    return sb.ToString();
        //}


        public static double ToDouble(this object o, double defaultValue = double.NaN)
        {
            if (o == null)
                return double.NaN;
            else if (o is double)
                return (double)o;
            else if (o is int)
                return (double)(int)o;
            else if (o is float)
                return (double)(float)o;
            else
            {
                if (double.TryParse(o.ToString(), out double d))
                    return d;
                else
                    return defaultValue;
            }
        }

        public static float ToFloat(this object o, double defaultValue = double.NaN)
        {
            var d = o.ToDouble();
            if (double.IsNaN(d))
                return float.NaN;
            else
                return (float)d;
        }

        public static double ToMm(this object o, double max)
        {
            if (o is string)
            {
                string v = (o as string).Trim();
                if (v.EndsWith("%"))
                {
                    var dp = (v.Substring(0, v.Length - 1)).ToDouble();
                    if (double.IsNaN(dp))
                        return double.NaN;
                    else
                        return (double)dp / 100.0f * max;
                }


            }
            var d = o.ToDouble();
            if (double.IsNaN(d))
                return double.NaN;
            else
                return (double)d;
        }

        public static double ToSeconds(this object o)
        {

            if (o is string)
            {
                string v = (o as string).Trim();
                if (v.EndsWith("s"))
                {
                    var dp = (v.Substring(0, v.Length - 1)).ToDouble();
                    return (double)dp;
                }
                if (v.EndsWith("m"))
                {
                    var dp = (v.Substring(0, v.Length - 1)).ToDouble();
                    return (double)dp * 60;
                }
                if (v.EndsWith("h"))
                {
                    var dp = (v.Substring(0, v.Length - 1)).ToDouble();
                    return (double)dp * 3600;
                }
            }
            var d = o.ToDouble();
            return (double)d;
        }

        public static Int32 ToInt32(this object o, Int32 defaultValue = 0)
        {
            try
            {
                if (o == null)
                    return 0;
                else if (o is double)
                    return (double.IsNaN((double)o) ? (Int32)0 : Convert.ToInt32(o));
                else if (o is int)
                    return (Int32)o;
                else if (o is float)
                    return (float.IsNaN((float)o) ? (Int32)0 : Convert.ToInt32(o));
                else
                {
                    Int32.TryParse(o.ToString(), out Int32 d);
                    return d;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ToBool(this object o)
        {
            if (o == null)
                return false;
            else if (o is double)
                return (double.IsNaN((double)o) ? false : ((double)o > 0.5));
            else if (o is int)
                return (int)o > 0;
            else if (o is float)
                return (float.IsNaN((float)o) ? false : ((float)o > 0.5));
            else
            {
                bool.TryParse(o.ToString(), out bool d);
                return d;
            }
        }

        public static TEnum ToEnum<TEnum>(this string value, TEnum defaultValue)
        {
            //SCAN
            if (value == null)
                return defaultValue;
            //\SCAN
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), value, true);
            }
            catch
            {
                return defaultValue;
            }

        }

        public static string FormatNumber(this Int32 v, string format, int width = 20, bool rightAlign = true)
        {
            var str = v.ToString(format);
            if (rightAlign)
                str = str.PadLeft(width);
            else
                str = str.PadRight(width);
            return str;
        }
        public static string FormatNumber(this double v, string format, int width = 20, bool rightAlign = true)
        {
            var str = v.ToString(format);
            if (rightAlign)
                str = str.PadLeft(width);
            else
                str = str.PadRight(width);
            return str;
        }

        public static bool IsFormula(this string term)
        {
            //formula or constant?
            return (term.IndexOf('+') >= 0 || term.IndexOf('-') >= 0 || term.IndexOf('*') >= 0 || term.IndexOf('/') >= 0 ||
                term.IndexOf('(') >= 0 || term.IndexOf('{') >= 0 || term.IndexOf(';') >= 0);
        }



        //public static List<Main.Helpers.ObjectSettingItem> GetSettings(this string settings)
        //{
        //    return null;
        //}

        public static string PadRightFixed(this string str, int len)
        {
            return str.Substring(0, Math.Min(len, str.Length)).PadRight(len);
        }

        public static int CharCount(this string str, char c)
        {
            int count = 0;
            foreach (char c1 in str)
            {
                if (c1 == c)
                    count++;
            }
            return count;
        }

        public static string ExcelIndexToColumnLetters(int colIndex)
        {
            //100 -> CV
            int div = colIndex;
            string colLetter = String.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colLetter = (char)(65 + mod) + colLetter;
                div = (int)((div - mod) / 26);
            }
            return colLetter;
        }

        public static int ExcelColumnLettersToIndex(string name)
        {
            int number = 0;
            int pow = 1;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }

            return number;
        }
    }
}
