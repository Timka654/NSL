using System;
using System.Runtime.InteropServices;
using NSL.ShaderVM;

namespace NSL.ShaderVM.Vulkan
{

    /// <summary>2x2 float matrix. Maps to GLSL <c>mat2</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "mat2")]
    public struct float2x2
    {
        public float2 col0, col1;


        public float2x2(float2 c0, float2 c1)
        {
            col0 = c0;
            col1 = c1;
        }

        public float2x2(float m00, float m01, float m10, float m11)
        {
            col0 = new float2(m00, m10);
            col1 = new float2(m01, m11);
        }
    }

    /// <summary>3x3 float matrix. Maps to GLSL <c>mat3</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "mat3")]
    public struct float3x3
    {
        public float3 col0, col1, col2;


        public float3x3(float3 c0, float3 c1, float3 c2)
        {
            col0 = c0;
            col1 = c1;
            col2 = c2;
        }
    }

    /// <summary>4x4 float matrix. Maps to GLSL <c>mat4</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [ShaderType(Name = "mat4")]
    public struct float4x4
    {
        public float4 col0, col1, col2, col3;


        public float4x4(float4 c0, float4 c1, float4 c2, float4 c3)
        {
            col0 = c0; col1 = c1; col2 = c2; col3 = c3;
        }


        public static float4x4 Identity => new float4x4(
            new float4(1, 0, 0, 0),
            new float4(0, 1, 0, 0),
            new float4(0, 0, 1, 0),
            new float4(0, 0, 0, 1));
    }
}