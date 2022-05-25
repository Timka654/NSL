using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.DataEngine.Interface
{
    public interface ICacheDataEntry
    {
        DateTime LatestModified { get; set; }
    }
}
