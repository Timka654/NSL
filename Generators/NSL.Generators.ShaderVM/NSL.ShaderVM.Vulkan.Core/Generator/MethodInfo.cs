using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NSL.ShaderVM.Vulkan
{
    public partial class VulkanSourceGenerator
    {
        private class MethodInfo
        {
            public string Name = "";

            public string ReturnType = "void";
            
            public BlockSyntax BodySyntax;

            public ParameterListSyntax ParameterListSyntax;
        }
    }
}