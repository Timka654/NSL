using System;

namespace NSL.SocketCore.Utils.Manager
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ManagerLoadAttribute : Attribute
    {
        public int Offset { get; }
        public string ManagerName { get; set; }

        public ManagerLoadAttribute(int offset)
        {
            Offset = offset;
        }
    }
}
