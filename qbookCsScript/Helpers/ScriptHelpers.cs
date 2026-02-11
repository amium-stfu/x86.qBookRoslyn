using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QB
{
    class ScriptHelpers
    {
        static Regex usingRegex = new Regex(@"^\s*using\s+(?<term>.*);\s*$");
        public static List<string> GetUsingsFromCode(string code)
        {
            List<string> usings = code.Replace("\r", "").Split('\n').Where(l => usingRegex.IsMatch(l.Trim())).ToList();
            List<string> usingTerms = new List<string>();
            foreach (var u in usings)
            {
                Match m = usingRegex.Match(u);
                if (m.Success)
                    usingTerms.Add(m.Groups["term"].Value.Trim());
            }

            return usingTerms;
        }

        public static string StripUsingsFromCode(string code, out int offset)
        {
            string[] lines = code.Replace("\r", "").Split('\n');
            List<string> linesWithoutUsings = lines.Where(l => !usingRegex.IsMatch(l.Trim())).ToList();
            offset = lines.Count() - linesWithoutUsings.Count() + 1;
            return string.Join("\r\n", linesWithoutUsings);
        }
    }
}
