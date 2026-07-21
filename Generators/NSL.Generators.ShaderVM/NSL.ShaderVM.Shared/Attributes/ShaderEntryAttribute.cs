using System;

namespace NSL.ShaderVM
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public abstract class BaseShaderEntryAttribute : Attribute
    {
        //public Type CompilerType { get; }

        public string ShaderName { get; set; }

        public string TargetVersion { get; set; } = ShaderTargetVersion.Default;

        public uint LocalSizeX { get; set; } = 1;

        public uint LocalSizeY { get; set; } = 1;

        public uint LocalSizeZ { get; set; } = 1;

        /// <summary>Emit uniforms as SSBO (layout(buffer) readonly buffer) instead of push_constant.</summary>
        public bool UniformsViaSSBO { get; set; }

        //public ShaderEntryAttribute(Type compilerType)
        //{
        //    CompilerType = compilerType ?? throw new ArgumentNullException(nameof(compilerType));
        //}
    }
}
