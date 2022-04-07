using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.ObjectInterface
{
    public interface IIdentityObject
    {
        string NodeIdentity { get; internal set; }
    }
}
