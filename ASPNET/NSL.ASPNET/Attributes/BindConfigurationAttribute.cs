using System;

namespace NSL.ASPNET.Attributes
{
    public class BindConfigurationAttribute(string path, params string[] models) : Attribute
    {
        public string Path { get; } = path;
        public string[] Models { get; } = models;
    }
}
