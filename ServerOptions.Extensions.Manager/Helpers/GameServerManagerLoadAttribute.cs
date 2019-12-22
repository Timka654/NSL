using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.Manager.Manager
{
    public class GameServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public GameServerManagerLoadAttribute(int offset) : base(offset)
        {
        }
    }
}
