namespace NSL.ShaderVM.Vulkan
{
    public partial class VulkanSourceGenerator
    {
        private class FieldInfo { public string Name = "", TypeName = ""; public int Binding, Set, Size; public bool ReadOnly; }
        private class ConstFieldInfo { public string Name = "", TypeName = "", Value = ""; }
    }
}