
// Код будет скомпилирован ТОЛЬКО если целевой фреймворк НИЖЕ .NET 7
#if !NET7_0_OR_GREATER

using System.Numerics;
using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UInt128 : IComparable<UInt128>, IEquatable<UInt128>
    {
        // Важно: порядок полей _lower, затем _upper обеспечивает Little-Endian формат, 
        // идентичный встроенному типу в .NET 7+
        private readonly ulong _lower;
        private readonly ulong _upper;

        public static readonly UInt128 Zero = new UInt128(0, 0);
        public static readonly UInt128 One = new UInt128(0, 1);
        public static readonly UInt128 MaxValue = new UInt128(ulong.MaxValue, ulong.MaxValue);
        public static readonly UInt128 MinValue = Zero;

        public UInt128(ulong upper, ulong lower)
        {
            _upper = upper;
            _lower = lower;
        }

        // --- Математические операторы ---

        public static UInt128 operator +(UInt128 a, UInt128 b)
        {
            ulong lower = a._lower + b._lower;
            ulong carry = lower < a._lower ? 1ul : 0ul;
            ulong upper = a._upper + b._upper + carry;
            return new UInt128(upper, lower);
        }

        public static UInt128 operator -(UInt128 a, UInt128 b)
        {
            ulong lower = a._lower - b._lower;
            ulong borrow = a._lower < b._lower ? 1ul : 0ul;
            ulong upper = a._upper - b._upper - borrow;
            return new UInt128(upper, lower);
        }

        public static UInt128 operator *(UInt128 a, UInt128 b)
        {
            ulong lower = a._lower * b._lower;
            ulong upper = MultiplyHigh(a._lower, b._lower)
                        + (a._upper * b._lower)
                        + (a._lower * b._upper);
            return new UInt128(upper, lower);
        }

        public static UInt128 operator /(UInt128 a, UInt128 b)
        {
            DivRem(a, b, out UInt128 q, out _);
            return q;
        }

        public static UInt128 operator %(UInt128 a, UInt128 b)
        {
            DivRem(a, b, out _, out UInt128 r);
            return r;
        }

        // --- Битовые операторы и сдвиги ---

        public static UInt128 operator &(UInt128 a, UInt128 b) => new UInt128(a._upper & b._upper, a._lower & b._lower);
        public static UInt128 operator |(UInt128 a, UInt128 b) => new UInt128(a._upper | b._upper, a._lower | b._lower);
        public static UInt128 operator ^(UInt128 a, UInt128 b) => new UInt128(a._upper ^ b._upper, a._lower ^ b._lower);
        public static UInt128 operator ~(UInt128 a) => new UInt128(~a._upper, ~a._lower);

        public static UInt128 operator <<(UInt128 value, int shiftAmount)
        {
            shiftAmount &= 0x7F;
            if (shiftAmount == 0) return value;
            if (shiftAmount < 64)
                return new UInt128((value._upper << shiftAmount) | (value._lower >> (64 - shiftAmount)), value._lower << shiftAmount);
            return new UInt128(value._lower << (shiftAmount - 64), 0);
        }

        public static UInt128 operator >>(UInt128 value, int shiftAmount)
        {
            shiftAmount &= 0x7F;
            if (shiftAmount == 0) return value;
            if (shiftAmount < 64)
                return new UInt128(value._upper >> shiftAmount, (value._lower >> shiftAmount) | (value._upper << (64 - shiftAmount)));
            return new UInt128(0, value._upper >> (shiftAmount - 64));
        }

        // --- Сравнения ---

        public static bool operator ==(UInt128 a, UInt128 b) => a._lower == b._lower && a._upper == b._upper;
        public static bool operator !=(UInt128 a, UInt128 b) => !(a == b);
        public static bool operator <(UInt128 a, UInt128 b) => a._upper == b._upper ? a._lower < b._lower : a._upper < b._upper;
        public static bool operator >(UInt128 a, UInt128 b) => a._upper == b._upper ? a._lower > b._lower : a._upper > b._upper;
        public static bool operator <=(UInt128 a, UInt128 b) => !(a > b);
        public static bool operator >=(UInt128 a, UInt128 b) => !(a < b);

        // --- Конвертации ---

        public static implicit operator UInt128(ulong value) => new UInt128(0, value);
        public static explicit operator ulong(UInt128 value) => value._lower;

        // --- Вспомогательные методы ---

        public override bool Equals(object obj) => obj is UInt128 other && this == other;
        public bool Equals(UInt128 other) => this == other;
        public override int GetHashCode() => _lower.GetHashCode() ^ _upper.GetHashCode();
        public int CompareTo(UInt128 other) => this < other ? -1 : (this > other ? 1 : 0);

        // Форматирование делегируем в BigInteger, чтобы не писать 300 строк алгоритма ToString
        public override string ToString()
        {
            BigInteger b = _upper;
            b <<= 64;
            b |= _lower;
            return b.ToString();
        }

        // --- Внутренние высокопроизводительные алгоритмы без аллокаций ---

        private static ulong MultiplyHigh(ulong a, ulong b)
        {
            uint a0 = (uint)a, a1 = (uint)(a >> 32);
            uint b0 = (uint)b, b1 = (uint)(b >> 32);
            ulong p00 = (ulong)a0 * b0, p01 = (ulong)a0 * b1, p10 = (ulong)a1 * b0, p11 = (ulong)a1 * b1;
            ulong mid = p01 + (p00 >> 32);
            ulong mid1 = (uint)mid + p10;
            return p11 + (mid >> 32) + (mid1 >> 32);
        }

        private static void DivRem(UInt128 dividend, UInt128 divisor, out UInt128 quotient, out UInt128 remainder)
        {
            if (divisor == Zero) throw new DivideByZeroException();
            quotient = Zero;
            remainder = Zero;
            // Базовый Shift-and-Subtract алгоритм деления
            for (int i = 127; i >= 0; i--)
            {
                remainder <<= 1;
                if (((dividend >> i)._lower & 1) != 0) remainder |= 1;
                if (remainder >= divisor)
                {
                    remainder -= divisor;
                    quotient |= (One << i);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Int128 : IComparable<Int128>, IEquatable<Int128>
    {
        private readonly ulong _lower;
        private readonly ulong _upper; // Храним как ulong для битовой идентичности и легкой трансляции

        public static readonly Int128 Zero = new Int128(0, 0);
        public static readonly Int128 One = new Int128(0, 1);
        public static readonly Int128 MaxValue = new Int128(long.MaxValue, ulong.MaxValue);
        public static readonly Int128 MinValue = new Int128(long.MinValue, 0);

        public Int128(long upper, ulong lower)
        {
            _upper = (ulong)upper;
            _lower = lower;
        }

        private Int128(ulong upper, ulong lower)
        {
            _upper = upper;
            _lower = lower;
        }

        // Сложение, вычитание и умножение на битовом уровне полностью идентичны беззнаковым типам
        public static Int128 operator +(Int128 a, Int128 b)
        {
            ulong lower = a._lower + b._lower;
            ulong upper = a._upper + b._upper + (lower < a._lower ? 1ul : 0ul);
            return new Int128(upper, lower);
        }

        public static Int128 operator -(Int128 a, Int128 b)
        {
            ulong lower = a._lower - b._lower;
            ulong upper = a._upper - b._upper - (a._lower < b._lower ? 1ul : 0ul);
            return new Int128(upper, lower);
        }

        // Операторы сдвига для знакового числа отличаются (нужно сохранять знак при сдвиге вправо)
        public static Int128 operator <<(Int128 value, int shiftAmount)
        {
            shiftAmount &= 0x7F;
            if (shiftAmount == 0) return value;
            if (shiftAmount < 64)
                return new Int128((value._upper << shiftAmount) | (value._lower >> (64 - shiftAmount)), value._lower << shiftAmount);
            return new Int128(value._lower << (shiftAmount - 64), 0);
        }

        public static Int128 operator >>(Int128 value, int shiftAmount)
        {
            shiftAmount &= 0x7F;
            if (shiftAmount == 0) return value;
            long upper = (long)value._upper;
            if (shiftAmount < 64)
                return new Int128((ulong)(upper >> shiftAmount), (value._lower >> shiftAmount) | (value._upper << (64 - shiftAmount)));
            return new Int128((ulong)(upper >> 63), (ulong)(upper >> (shiftAmount - 64)));
        }

        // Сравнения с учетом знака
        public static bool operator ==(Int128 a, Int128 b) => a._lower == b._lower && a._upper == b._upper;
        public static bool operator !=(Int128 a, Int128 b) => !(a == b);
        public static bool operator <(Int128 a, Int128 b) => (long)a._upper == (long)b._upper ? a._lower < b._lower : (long)a._upper < (long)b._upper;
        public static bool operator >(Int128 a, Int128 b) => (long)a._upper == (long)b._upper ? a._lower > b._lower : (long)a._upper > (long)b._upper;
        public static bool operator <=(Int128 a, Int128 b) => !(a > b);
        public static bool operator >=(Int128 a, Int128 b) => !(a < b);

        // Конвертации
        public static implicit operator Int128(long value) => new Int128(value < 0 ? -1L : 0L, (ulong)value);
        public static explicit operator long(Int128 value) => (long)value._lower;

        public override bool Equals(object obj) => obj is Int128 other && this == other;
        public bool Equals(Int128 other) => this == other;
        public override int GetHashCode() => _lower.GetHashCode() ^ _upper.GetHashCode();
        public int CompareTo(Int128 other) => this < other ? -1 : (this > other ? 1 : 0);

        public override string ToString()
        {
            BigInteger b = new BigInteger(_upper); // Для знаковых требуется чуть иная логика в BigInteger
            b <<= 64;
            b |= _lower;
            return b.ToString();
        }
    }
}

#endif