using System;

namespace NSL.ShaderVM
{
    /// <summary>Classification of a [ShaderFunction] method body.</summary>
    public enum ShaderFunctionKind
    {
        /// <summary>GLSL built-in: the Name string is a GLSL expression template (e.g. "exp(x)"). No C# body translation.</summary>
        Core,

        /// <summary>C# body → standalone GLSL function, callable from any shader in the compilation.</summary>
        Managed,

        /// <summary>C# body → expanded inline at every call site (not emitted as a standalone function).</summary>
        Inline
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderFunctionAttribute : Attribute
    {
        public string Name { get; }

        public string Include { get; set; }

        public string MinVersion { get; set; } = ShaderTargetVersion.Default;

        /// <summary>What kind of shader function this is: Core (GLSL built-in), Managed (standalone), or Inline.</summary>
        public ShaderFunctionKind Kind { get; set; } = ShaderFunctionKind.Core;

        // Backward-compat only — prefer Kind
        [Obsolete("Use Kind = ShaderFunctionKind.Managed instead.")]
        public bool CSharpCode
        {
            get => Kind == ShaderFunctionKind.Managed;
            set { if (value) Kind = ShaderFunctionKind.Managed; }
        }

        [Obsolete("Use Kind = ShaderFunctionKind.Inline instead.")]
        public bool Inline
        {
            get => Kind == ShaderFunctionKind.Inline;
            set { if (value) Kind = ShaderFunctionKind.Inline; }
        }

        public ShaderFunctionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderFieldAttribute : Attribute
    {
        public string Name { get; }

        public string MinVersion { get; set; } = ShaderTargetVersion.Default;

        public ShaderFieldAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
