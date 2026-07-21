using System;
using NSL.ShaderVM;
using NSL.ShaderVM.Utils;

namespace NSL.ShaderVM.Vulkan
{

    [ShaderIgnore]
    public class VulkanExecutionContext : DefaultExecutionContext
    {
        public VulkanExecutionContext(uint globalSize) : base(globalSize) { }

        public VulkanExecutionContext(float[] globalMemory, float[] sharedMemory = null) : base(globalMemory, sharedMemory) { }

        [ShaderField("gl_GlobalInvocationID.x")] public override uint GlobalInvocationIdX { get => base.GlobalInvocationIdX; set => base.GlobalInvocationIdX = value; }

        [ShaderField("gl_GlobalInvocationID.y")] public override uint GlobalInvocationIdY { get => base.GlobalInvocationIdY; set => base.GlobalInvocationIdY = value; }

        [ShaderField("gl_GlobalInvocationID.z")] public override uint GlobalInvocationIdZ { get => base.GlobalInvocationIdZ; set => base.GlobalInvocationIdZ = value; }

        [ShaderField("gl_LocalInvocationID.x")] public override uint LocalInvocationIdX { get => base.LocalInvocationIdX; set => base.LocalInvocationIdX = value; }

        [ShaderField("gl_LocalInvocationID.y")] public override uint LocalInvocationIdY { get => base.LocalInvocationIdY; set => base.LocalInvocationIdY = value; }

        [ShaderField("gl_LocalInvocationID.z")] public override uint LocalInvocationIdZ { get => base.LocalInvocationIdZ; set => base.LocalInvocationIdZ = value; }

        [ShaderField("gl_LocalSize.x")] public override uint LocalSizeX { get => base.LocalSizeX; set => base.LocalSizeX = value; }

        [ShaderField("gl_LocalSize.y")] public override uint LocalSizeY { get => base.LocalSizeY; set => base.LocalSizeY = value; }

        [ShaderField("gl_LocalSize.z")] public override uint LocalSizeZ { get => base.LocalSizeZ; set => base.LocalSizeZ = value; }

        [ShaderField("gl_WorkGroupID.x")] public override uint WorkGroupIdX { get => base.WorkGroupIdX; set => base.WorkGroupIdX = value; }

        [ShaderField("gl_WorkGroupID.y")] public override uint WorkGroupIdY { get => base.WorkGroupIdY; set => base.WorkGroupIdY = value; }

        [ShaderField("gl_WorkGroupID.z")] public override uint WorkGroupIdZ { get => base.WorkGroupIdZ; set => base.WorkGroupIdZ = value; }


        [ShaderFunction("sin(x)")] public float Sin(float x) => MathF.Sin(x);
        [ShaderFunction("cos(x)")] public float Cos(float x) => MathF.Cos(x);
        [ShaderFunction("tan(x)")] public float Tan(float x) => MathF.Tan(x);
        [ShaderFunction("exp(x)")] public float Exp(float x) => MathF.Exp(x);
        [ShaderFunction("log(x)")] public float Log(float x) => MathF.Log(x);
        [ShaderFunction("sqrt(x)")] public float Sqrt(float x) => MathF.Sqrt(x);
        [ShaderFunction("pow(x,y)")] public float Pow(float x, float y) => MathF.Pow(x, y);
        [ShaderFunction("abs(x)")] public float Abs(float x) => MathF.Abs(x);
        [ShaderFunction("fma(a,b,c)")] public float Fma(float a, float b, float c) => MathUtils.SoftwareFusedMultiplyAdd(a, b, c);
        [ShaderFunction("clamp(v,lo,hi)")] public float Clamp(float v, float lo, float hi) => MathUtils.Clamp(v, lo, hi);
        [ShaderFunction("mix(a,b,t)")] public float Mix(float a, float b, float t) => a * (1 - t) + b * t;
        [ShaderFunction("step(edge,x)")] public float Step(float edge, float x) => x >= edge ? 1.0f : 0.0f;
        [ShaderFunction("normalize(v)")] public float3 Normalize(float3 v) { float l = v.Length(); return l > 0 ? v / l : v; }
        [ShaderFunction("length(v)")] public float Length(float3 v) => v.Length();
        [ShaderFunction("dot(a,b)")] public float Dot(float4 a, float4 b) => float4.Dot(a, b);
        [ShaderFunction("cross(a,b)")] public float3 Cross(float3 a, float3 b) => float3.Cross(a, b);


        [ShaderFunction("barrier()")] public override void Barrier() => base.Barrier();
        [ShaderFunction("memoryBarrierBuffer()")] public override void MemoryBarrier() => base.MemoryBarrier();
        [ShaderFunction("memoryBarrierShared()")] public override void MemoryBarrierShared() => base.MemoryBarrierShared();
    }
}