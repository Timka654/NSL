using System;

namespace NSL.ShaderVM.Vulkan.Attributes
{
    /// <summary>
    /// Marks a shader field as a push constant. Generates:
    /// layout(push_constant) uniform PC { Type Name; } pc;
    /// Access in shader code: pc.FieldName
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ShaderPushConstantAttribute : Attribute
    {
    }
}
