using System;

namespace NSL.Generators.EntityPathGenerator.Shared.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PathNodeAttribute : Attribute
    {
        public string[] Models { get; }
        public PathNodeAttribute(params string[] models) => Models = models;
    }
}