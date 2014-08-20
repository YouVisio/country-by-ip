using System;
using System.Globalization;

namespace CountryByIp
{
    public static class Extensions
    {
        public static string Args(this string str, params object[] objs)
        {
            return String.Format(str, objs);
        }
        public static string PadLeft(this int i, int limit, char ch)
        {
            return i.ToString(CultureInfo.InvariantCulture).PadLeft(limit, ch);
        }
    }
}