using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class PacketEnumExtensions
    {
        public static bool IsDefined<T>(ushort value)
            where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().Select(x=>Convert.ToUInt16(x));

            return values.Contains(value);
        }
    }
}
