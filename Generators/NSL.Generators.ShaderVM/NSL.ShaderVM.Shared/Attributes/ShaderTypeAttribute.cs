using System;

namespace NSL.ShaderVM
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderTypeAttribute : Attribute
    {
        public string MinVersion { get; set; } = ShaderTargetVersion.Default;

        public string Name { get; set; }
    }
}
