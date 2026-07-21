using System;
using System.Runtime.InteropServices;
using NSL.ShaderVM;

namespace NSL.ShaderVM.Vulkan
{
    /// <summary>GPU-native 2-component float vector. Maps to GLSL <c>vec2</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "vec2")]
    public struct float2
    {
        public float x, y;

        public float2(float x, float y) { this.x = x; this.y = y; }

        public float2(float s) { x = y = s; }


        public static float2 operator +(float2 a, float2 b) => new float2(a.x + b.x, a.y + b.y);

        public static float2 operator -(float2 a, float2 b) => new float2(a.x - b.x, a.y - b.y);

        public static float2 operator *(float2 a, float2 b) => new float2(a.x * b.x, a.y * b.y);

        public static float2 operator *(float s, float2 a) => new float2(a.x * s, a.y * s);

        public static float2 operator *(float2 a, float s) => new float2(a.x * s, a.y * s);

        public static float2 operator /(float2 a, float2 b) => new float2(a.x / b.x, a.y / b.y);

        public static float2 operator /(float2 a, float s) => new float2(a.x / s, a.y / s);

        public static float2 operator -(float2 a) => new float2(-a.x, -a.y);


        public static float Dot(float2 a, float2 b) => a.x * b.x + a.y * b.y;

        public float LengthSquared() => x * x + y * y;

        public float Length() => MathF.Sqrt(LengthSquared());


        public override string ToString() => $"({x}, {y})";
    }

    /// <summary>GPU-native 3-component float vector. Maps to GLSL <c>vec3</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "vec3")]
    public struct float3
    {
        public float x, y, z;

        public float3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public float3(float s) { x = y = z = s; }


        public static float3 operator +(float3 a, float3 b) => new float3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static float3 operator -(float3 a, float3 b) => new float3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static float3 operator *(float3 a, float3 b) => new float3(a.x * b.x, a.y * b.y, a.z * b.z);

        public static float3 operator *(float s, float3 a) => new float3(a.x * s, a.y * s, a.z * s);

        public static float3 operator *(float3 a, float s) => new float3(a.x * s, a.y * s, a.z * s);

        public static float3 operator /(float3 a, float3 b) => new float3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static float3 operator /(float3 a, float s) => new float3(a.x / s, a.y / s, a.z / s);

        public static float3 operator -(float3 a) => new float3(-a.x, -a.y, -a.z);


        public static float Dot(float3 a, float3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

        public static float3 Cross(float3 a, float3 b) => new float3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x);

        public float LengthSquared() => x * x + y * y + z * z;

        public float Length() => MathF.Sqrt(LengthSquared());


        public override string ToString() => $"({x}, {y}, {z})";
    }

    /// <summary>GPU-native 4-component float vector. Maps to GLSL <c>vec4</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "vec4")]
    public struct float4
    {
        public float x, y, z, w;

        public float4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }

        public float4(float s) { x = y = z = w = s; }

        public static float4 operator +(float4 a, float4 b) => new float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        public static float4 operator -(float4 a, float4 b) => new float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        public static float4 operator *(float4 a, float4 b) => new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        public static float4 operator *(float s, float4 a) => new float4(a.x * s, a.y * s, a.z * s, a.w * s);

        public static float4 operator *(float4 a, float s) => new float4(a.x * s, a.y * s, a.z * s, a.w * s);

        public static float4 operator /(float4 a, float4 b) => new float4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);

        public static float4 operator /(float4 a, float s) => new float4(a.x / s, a.y / s, a.z / s, a.w / s);

        public static float4 operator -(float4 a) => new float4(-a.x, -a.y, -a.z, -a.w);


        public static float Dot(float4 a, float4 b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        public float LengthSquared() => x * x + y * y + z * z + w * w;

        public float Length() => MathF.Sqrt(LengthSquared());


        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }
}