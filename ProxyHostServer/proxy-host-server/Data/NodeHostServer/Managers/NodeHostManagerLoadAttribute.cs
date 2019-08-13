using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Manager;

namespace phs.Data.NodeHostServer.Managers
{
    public class NodeHostManagerLoadAttribute : ManagerLoadAttribute
    {
        public NodeHostManagerLoadAttribute(int offset) : base(offset)
        {
        }
    }
}
