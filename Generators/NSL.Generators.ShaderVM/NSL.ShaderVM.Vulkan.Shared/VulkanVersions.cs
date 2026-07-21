using System;
using System.Collections.Generic;

namespace NSL.ShaderVM.Vulkan
{

    /// <summary>
    /// Vulkan-specific version constants and ordering.
    /// Use these in <see cref="ShaderEntryAttribute.TargetVersion"/> and <see cref="ShaderFunctionAttribute.MinVersion"/>.
    /// </summary>
    public static class VulkanVersions
    {
        public const string Vulkan10 = "vulkan1.0";
        public const string Vulkan11 = "vulkan1.1";
        public const string Vulkan12 = "vulkan1.2";
        public const string Vulkan13 = "vulkan1.3";

        private static readonly Dictionary<string, int> Order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { Vulkan10, 0 },
            { Vulkan11, 1 },
            { Vulkan12, 2 },
            { Vulkan13, 3 },
        };

        public static bool IsKnownVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return true;
            return Order.ContainsKey(version);
        }

        public static bool Satisfies(string target, string minRequired)
        {
            if (string.IsNullOrEmpty(minRequired)) return true;
            if (string.IsNullOrEmpty(target)) return true;
            if (Order.TryGetValue(target, out int t) && Order.TryGetValue(minRequired, out int m))
                return t >= m;
            return string.Equals(target, minRequired, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToGlslVersion(string target)
        {
            switch (target.ToLowerInvariant())
            {
                case Vulkan10: return "450";
                case Vulkan11: return "450";
                case Vulkan12: return "450";
                case Vulkan13: return "460";
                default: return "450";
            }
        }

        public static string[] GetExtensions(string target)
        {
            // Only emit extensions that are actually needed.
            // GL_EXT_buffer_reference* are for buffer device address — not used by SSBO shaders.
            // 16bit/8bit storage extensions are for half/byte types — emit only when needed.
            switch (target.ToLowerInvariant())
            {
                case Vulkan10: return Array.Empty<string>();
                case Vulkan11: return new[] { "GL_EXT_shader_16bit_storage" };
                case Vulkan12: return new[] { "GL_EXT_shader_16bit_storage", "GL_EXT_shader_8bit_storage" };
                case Vulkan13: return new[] { "GL_EXT_shader_16bit_storage", "GL_EXT_shader_8bit_storage" };
                default: return new[] { "GL_EXT_buffer_reference", "GL_EXT_buffer_reference_uvec2" };
            }
        }
    }
}