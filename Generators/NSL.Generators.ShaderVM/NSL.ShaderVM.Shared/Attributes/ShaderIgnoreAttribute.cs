using System;

namespace NSL.ShaderVM
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class ShaderIgnoreAttribute : Attribute
    {
    }
}
