using System;

namespace NSL.Entity.PathGenerator.Shared.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PathProxyAttribute : Attribute
    {
        public string To { get; }
        public string[] From { get; }

        public PathProxyAttribute(string to, params string[] from)
        { To = to; From = from; }

        public PathProxyAttribute(string to)
        { To = to; From = Array.Empty<string>(); }
    }
}