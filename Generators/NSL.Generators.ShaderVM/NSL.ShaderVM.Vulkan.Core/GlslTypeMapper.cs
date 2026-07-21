using System;
using System.Collections.Generic;
using System.Reflection;

namespace NSL.ShaderVM.Vulkan
{
    /// <summary>
    /// Maps C# types to GLSL types and generates type declarations.
    /// </summary>
    public class GlslTypeMapper
    {
        private static readonly Dictionary<string, string> TypeMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Scalar types (both C# keywords and System.* full names)
            { "float", "float" },
            { "System.Single", "float" },
            { "Single", "float" },
            { "int", "int" },
            { "System.Int32", "int" },
            { "Int32", "int" },
            { "uint", "uint" },
            { "System.UInt32", "uint" },
            { "UInt32", "uint" },
            { "bool", "bool" },
            { "System.Boolean", "bool" },
            { "Boolean", "bool" },
            { "double", "double" },
            { "System.Double", "double" },
            { "Double", "double" },
            { "void", "void" },
            { "System.Void", "void" },
            { "long", "int64_t" },
            { "System.Int64", "int64_t" },
            { "Int64", "int64_t" },
            { "ulong", "uint64_t" },
            { "System.UInt64", "uint64_t" },
            { "UInt64", "uint64_t" },
            // Small integer types (GLSL has no 8/16-bit types without extensions; map to 32-bit)
            { "byte", "int" },
            { "System.Byte", "int" },
            { "Byte", "int" },
            { "sbyte", "int" },
            { "System.SByte", "int" },
            { "SByte", "int" },
            { "short", "int" },
            { "System.Int16", "int" },
            { "Int16", "int" },
            { "ushort", "uint" },
            { "System.UInt16", "uint" },
            { "UInt16", "uint" },
            // // Vector types
            // { "float2", "vec2" },
            // { "float3", "vec3" },
            // { "float4", "vec4" },
            // { "int2", "ivec2" },
            // { "int3", "ivec3" },
            // { "int4", "ivec4" },
            // { "half", "float16_t" },
            // { "half2", "f16vec2" },
            // { "half4", "f16vec4" },
            // // Matrix types
            // { "float2x2", "mat2" },
            // { "float3x3", "mat3" },
            // { "float4x4", "mat4" },
        };

        /// <summary>Map a C# type name to its GLSL equivalent.</summary>
        public static string MapType(string csharpType)
        {
            if (TypeMap.TryGetValue(csharpType, out string glsl))
                return glsl;

            return csharpType; // fallback
        }

        /// <summary>Map a C# Type to its GLSL equivalent.</summary>
        public static string MapType(Type type)
        {
            return MapType(type.Name);
        }

        /// <summary>Check if the type is a known shader-compatible type.</summary>
        public static bool IsShaderType(string csharpType) => TypeMap.ContainsKey(csharpType);
    }
}