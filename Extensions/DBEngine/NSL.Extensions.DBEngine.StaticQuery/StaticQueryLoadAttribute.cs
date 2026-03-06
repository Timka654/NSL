using System;

namespace NSL.Extensions.DBEngine.StaticQuery
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StaticQueryLoadAttribute : Attribute
    {
        public int Order { get; private set; }

        public string Name { get; private set; }

        public StaticQueryLoadAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }
    }
}
