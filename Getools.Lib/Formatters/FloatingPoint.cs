using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Formatters
{
    public static class FloatingPoint
    {
        public static string ToFloatString(Single val)
        {
            if (Single.IsNaN(val))
            {
                return "NaN";
            }

            var s = val.ToString().ToLower();

            if (s.IndexOf('e') >= 0)
            {
                return s;
            }

            if (s.IndexOf('.') < 0)
            {
                return s + ".0";
            }

            return s;
        }

        public static string ToFloatString(int val)
        {
            return ToFloatString((Single)val);
        }

        public static string ToFloatString(short val)
        {
            return ToFloatString((Single)val);
        }

        public static string ToFloatString(double val)
        {
            return ToFloatString((Single)val);
        }

        public static string ToFloatCLiteral(Single val)
        {
            if (Single.IsNaN(val))
            {
                return "NAN";
            }

            var s = val.ToString().ToLower();

            if (s.IndexOf('e') >= 0)
            {
                return s;
            }

            if (s.IndexOf('.') < 0)
            {
                return s + ".0f";
            }

            return s + "f";
        }

        public static string ToFloatCLiteral(int val)
        {
            return ToFloatCLiteral((Single)val);
        }

        public static string ToFloatCLiteral(short val)
        {
            return ToFloatCLiteral((Single)val);
        }

        public static string ToFloatCLiteral(double val)
        {
            return ToFloatCLiteral((Single)val);
        }
    }
}
