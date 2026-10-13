using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qbook
{
    internal class Helpers
    {
        public static string TranslateText(string input, string from, string to)
        {
            // Set the language from/to in the url (or pass it into this function)
            string url = String.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
             from, to, Uri.EscapeUriString(input));
            HttpClient httpClient = new HttpClient();
            string result = httpClient.GetStringAsync(url).Result;

            using var jsonData = System.Text.Json.JsonDocument.Parse(result);
            string translation = string.Join(" ", jsonData.RootElement[0].EnumerateArray()
                .Select(item => item[0].GetString()));
            return translation;
        }
    }
}
