using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NSL.ShaderVM.Vulkan
{
    public partial class VulkanSourceGenerator
    {
        private class ShaderClassInfo
        {
            public ClassDeclarationSyntax ClassDeclaration { get; set; }
            public SemanticModel SemanticModel { get; internal set; }

            //public string ClassName = "";

            //public string Modifiers = "";

            //public string Namespace = "";

            public string ShaderName = "";


            public uint LocalSizeX = 1;

            public uint LocalSizeY = 1;

            public uint LocalSizeZ = 1;


            public string TargetVersion = ShaderTargetVersion.Default;

            public bool UniformsViaSSBO;

            public List<FieldInfo> Buffers = new List<FieldInfo>();

            public List<FieldInfo> Uniforms = new List<FieldInfo>();

            public List<FieldInfo> PushConstants = new List<FieldInfo>();

            public List<FieldInfo> Shared = new List<FieldInfo>();

            public List<ConstFieldInfo> Consts = new List<ConstFieldInfo>();

            /// <summary>[ShaderType] structs used in shader — emitted as GLSL struct declarations.</summary>
            public List<ShaderStructInfo> StructDeclarations = new List<ShaderStructInfo>();

            public List<MethodInfo> Methods = new List<MethodInfo>();
        }

        /// <summary>Info for emitting a GLSL struct declaration from a [ShaderType] C# struct.</summary>
        private class ShaderStructInfo
        {
            public string GlslName = "";
            public List<(string FieldType, string FieldName)> Fields = new List<(string, string)>();
        }
    }
}