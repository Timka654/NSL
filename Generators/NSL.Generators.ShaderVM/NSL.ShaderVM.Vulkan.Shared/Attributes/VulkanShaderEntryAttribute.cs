using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.ShaderVM.Vulkan.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class VulkanShaderEntryAttribute : BaseShaderEntryAttribute
    {
        public const string ShortName = "VulkanShaderEntry";
    }
}
