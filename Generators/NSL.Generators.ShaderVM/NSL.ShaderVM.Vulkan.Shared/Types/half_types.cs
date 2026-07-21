using System;
using System.Runtime.InteropServices;
using NSL.ShaderVM;

namespace NSL.ShaderVM.Vulkan
{
    /// <summary>Half-precision float (16-bit). Maps to GLSL <c>float16_t</c>. Requires Vulkan 1.3+.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "float16_t", MinVersion = VulkanVersions.Vulkan13)]
    public struct half
    {
        public ushort RawValue;


        public half(float val)
        {
            RawValue = FloatToHalf(val);
        }

        public half(ushort raw)
        {
            RawValue = raw;
        }


        public static implicit operator float(half h) => HalfToFloat(h.RawValue);

        public static implicit operator half(float f) => new half(f);


        public static half operator +(half a, half b) => (float)a + (float)b;

        public static half operator -(half a, half b) => (float)a - (float)b;

        public static half operator *(half a, half b) => (float)a * (float)b;

        public static half operator /(half a, half b) => (float)a / (float)b;


        public override string ToString() => ((float)this).ToString();


        // IEEE 754 half-precision conversion
        private static ushort FloatToHalf(float f)
        {
            uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
            uint sign = (bits >> 16) & 0x8000;
            int exp = (int)((bits >> 23) & 0xFF) - 127 + 15;
            uint mant = (bits >> 13) & 0x3FF;
            if (exp <= 0) return (ushort)(sign | ((uint)((int)mant | 0x400) >> (1 - exp)));
            if (exp >= 31) return (ushort)(sign | 0x7C00);
            return (ushort)(sign | ((uint)exp << 10) | mant);
        }

        private static float HalfToFloat(ushort h)
        {
            uint sign = (uint)(h & 0x8000) << 16;
            int exp = (h >> 10) & 0x1F;
            uint mant = (uint)(h & 0x3FF) << 13;
            if (exp == 0) { if (mant != 0) { exp = 1 - 15 + 127; while ((mant & 0x800000) == 0) { mant <<= 1; exp--; } mant &= 0x7FFFFF; } }
            else if (exp == 31) { exp = 255; }
            else { exp = exp - 15 + 127; }
            uint bits = sign | ((uint)exp << 23) | mant;
            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }
    }

    /// <summary>2-component half-precision float vector. Requires Vulkan 1.3+.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "f16vec2", MinVersion = VulkanVersions.Vulkan13)]
    public struct half2
    {
        public half x, y;


        public half2(half x, half y)
        {
            this.x = x;
            this.y = y;
        }

        public half2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }


        public override string ToString() => $"({x}, {y})";
    }

    /// <summary>4-component half-precision float vector. Requires Vulkan 1.3+.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "f16vec4", MinVersion = VulkanVersions.Vulkan13)]
    public struct half4
    {
        public half x, y, z, w;


        public half4(half x, half y, half z, half w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public half4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }


        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }
}