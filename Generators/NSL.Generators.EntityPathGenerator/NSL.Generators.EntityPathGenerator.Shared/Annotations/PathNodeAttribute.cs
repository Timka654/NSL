using System;

namespace NSL.Entity.PathGenerator.Shared.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PathNodeAttribute : Attribute
    {
        public string[] Models { get; }
        public PathNodeAttribute(params string[] models) => Models = models;
    }
}