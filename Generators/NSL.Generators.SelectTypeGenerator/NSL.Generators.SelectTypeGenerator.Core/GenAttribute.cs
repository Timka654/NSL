namespace NSL.Generators.SelectTypeGenerator.Core
{
    internal class GenAttribute
    {
        public string[] Models { get; set; }

        public bool Dto { get; set; }

        public bool Typed { get; set; }

        public string DtoSuffix { get; set; }
    }
}
