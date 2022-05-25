using NSL.Extensions.DataEngine.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.DataEngine.Interface
{
    public interface ICommitingCacheDataEntry<TCommit> : ICacheDataEntry
        where TCommit : DataChangeCommitInfo
    {
        List<TCommit> ModifyCommitList { get; set; }
    }
}
