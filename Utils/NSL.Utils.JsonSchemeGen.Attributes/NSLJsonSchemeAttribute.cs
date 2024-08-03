namespace NSL.Utils.JsonSchemeGen.Attributes
{
    public class NSLJsonSchemeAttribute : Attribute
    {
        public string JsonFile { get; set; } = "appsettings.json";

        public string Path { get; set; } = "/";

        public string Name { get; }

        public NSLJsonSchemeAttribute(string name)
        {
            Name = name;
        }
    }
}
