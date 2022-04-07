using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.BinarySerializer
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class BinaryNetworkTypeAttribute : Attribute
    {
    }
}
