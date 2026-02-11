namespace qbook.Extensions
{
    public static class StringExtensions
    {
        public static string Left(this string s, int length, string ellipsis = "")
        {
            if (s.Length > length)
                return s.Substring(0, length) + ellipsis;
            return s;
        }

    }
}
