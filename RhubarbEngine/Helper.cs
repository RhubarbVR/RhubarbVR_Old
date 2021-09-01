using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine
{
    public static class Helper
    {
        public static string GetFormattedName(this Type type)
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                                    .Select(x => x.Name)
                                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                     + $"<{genericArguments}>";
            }
            return type.Name;
        }

        public static string ToHexString(this ulong ouid)
        {
            string temp = BitConverter.ToString(BitConverter.GetBytes(ouid).Reverse().ToArray()).Replace("-", "");

            while (temp.Substring(0, 1) == "0")
            {
                temp = temp.Substring(1);
            }

            return temp;
        }
    }
}
