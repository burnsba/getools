using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Formatters
{
    public static class Strings
    {
        public static string ToQuotedString(string s, int? maxLen = null)
        {
            if (maxLen.HasValue && maxLen >= 0)
            {
                if (s.Length > maxLen)
                {
                    s = s.Substring(0, maxLen.Value);
                }
            }

            return $"\"{s}\"";
        }
    }
}
