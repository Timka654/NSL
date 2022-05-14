using System;

namespace NSL.ServerOptions.Extensions.Manager
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ManagerLoadAttribute : Attribute
    {
        public int Offset { get; private set; }

        public string ManagerName { get; set; }

        public ManagerLoadAttribute(int offset)
        {
            Offset = offset;
        }
    }
}
