using System;
using System.Runtime.InteropServices;
using NSL.ShaderVM;

namespace NSL.ShaderVM.Vulkan
{

    /// <summary>GPU-native 2-component int vector. Maps to GLSL <c>ivec2</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "ivec2")]
    public struct int2
    {
        public int x, y;


        public int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int2(int s)
        {
            x = y = s;
        }


        public static int2 operator +(int2 a, int2 b) => new int2(a.x + b.x, a.y + b.y);

        public static int2 operator -(int2 a, int2 b) => new int2(a.x - b.x, a.y - b.y);

        public static int2 operator *(int2 a, int2 b) => new int2(a.x * b.x, a.y * b.y);

        public static int2 operator -(int2 a) => new int2(-a.x, -a.y);


        public override string ToString() => $"({x}, {y})";
    }

    /// <summary>GPU-native 3-component int vector. Maps to GLSL <c>ivec3</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "ivec3")]
    public struct int3
    {
        public int x, y, z;


        public int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int3(int s)
        {
            x = y = z = s;
        }


        public static int3 operator +(int3 a, int3 b) => new int3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static int3 operator -(int3 a, int3 b) => new int3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static int3 operator *(int3 a, int3 b) => new int3(a.x * b.x, a.y * b.y, a.z * b.z);

        public static int3 operator -(int3 a) => new int3(-a.x, -a.y, -a.z);


        public override string ToString() => $"({x}, {y}, {z})";
    }

    /// <summary>GPU-native 4-component int vector. Maps to GLSL <c>ivec4</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "ivec4")]
    public struct int4
    {
        public int x, y, z, w;


        public int4(int x, int y, int z, int w)
        {
            this.x = x; this.y = y; this.z = z; this.w = w;
        }

        public int4(int s)
        {
            x = y = z = w = s;
        }


        public static int4 operator +(int4 a, int4 b) => new int4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        public static int4 operator -(int4 a, int4 b) => new int4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        public static int4 operator *(int4 a, int4 b) => new int4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        public static int4 operator -(int4 a) => new int4(-a.x, -a.y, -a.z, -a.w);


        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }
}