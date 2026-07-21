using System;

namespace NSL.ShaderVM
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderBufferAttribute : Attribute
    {
        public int Binding { get; set; } = 0;

        public int Set { get; set; } = 0;

        public bool ReadOnly { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderUniformAttribute : Attribute
    {
        public int Binding { get; set; } = 0;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderSharedAttribute : Attribute
    {
        public int Size { get; set; }
    }
}
