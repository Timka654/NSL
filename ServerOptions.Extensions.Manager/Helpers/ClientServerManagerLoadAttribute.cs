using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.Manager.Manager
{
    public class ClientServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public ClientServerManagerLoadAttribute(int offset) : base(offset)
        {
        }
    }
}
