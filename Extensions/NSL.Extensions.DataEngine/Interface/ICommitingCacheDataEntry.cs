using NSL.Extensions.DataEngine.Info;
using System.Collections.Generic;

namespace NSL.Extensions.DataEngine.Interface
{
    public interface ICommitingCacheDataEntry<TCommit> : ICacheDataEntry
        where TCommit : DataChangeCommitInfo
    {
        List<TCommit> ModifyCommitList { get; set; }
    }
}
