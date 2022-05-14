using System;
using System.Linq;

namespace NSL.Utils
{
    public static class PacketEnumExtensions
    {
        /// <summary>
        /// Enum.IsDefined defined default but not all target frameworks have this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefined<T>(ushort value)
            where T : struct, Enum
        {
            var v = Enum.ToObject(typeof(T), value);

            return Enum.GetValues(typeof(T)).Cast<T>().Contains((T)v);
        }
    }
}
