using System;

namespace NSL.ShaderVM
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderCallAttribute : Attribute
    {
        public string Name { get; set; } = "Run";
    }
}
