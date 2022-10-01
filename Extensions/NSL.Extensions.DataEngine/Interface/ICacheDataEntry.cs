using System;

namespace NSL.Extensions.DataEngine.Interface
{
    public interface ICacheDataEntry
    {
        DateTime LatestModified { get; set; }
    }
}
