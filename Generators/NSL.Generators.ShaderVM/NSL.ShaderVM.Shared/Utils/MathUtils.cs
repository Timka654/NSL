using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.ShaderVM.Utils
{
    public class MathUtils
    {
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static float SoftwareFusedMultiplyAdd(float x, float y, float z)
        {
            // Конвертируем все операнды в double
            double dx = x;
            double dy = y;
            double dz = z;

            // Вычисляем с двойной точностью. 
            // Поскольку dx * dy помещается в double без потерь, округления на этапе умножения не происходит.
            double result = (dx * dy) + dz;

            // Делаем единственное округление при приведении обратно к float
            return (float)result;
        }
    }
}
