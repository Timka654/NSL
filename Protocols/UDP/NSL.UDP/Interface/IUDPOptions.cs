using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Interface
{
    public interface IUDPOptions
    {
        int SendFragmentSize { get; set; }
    }
}
