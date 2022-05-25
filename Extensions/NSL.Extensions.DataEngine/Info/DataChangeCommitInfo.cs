using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.DataEngine.Info
{
    public class DataChangeCommitInfo
    {
        public string Avtor { get; set; }

        public DateTime LatestModified { get; set; }

        public virtual List<DataChangeInfo> ChangeList { get; set; }
    }
}
