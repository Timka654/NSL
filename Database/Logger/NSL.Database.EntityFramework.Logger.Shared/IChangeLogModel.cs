using System;

namespace NSL.Database.EntityFramework.Logger.Shared
{
    public interface IChangeLogModel
    {
        Guid Id { get; set; }

        string EntityName { get; set; }

        EntityActionTypeEnum ActionType { get; set; }

        string? Data { get; set; }

        DateTime CreateTime { get; set; }

        string Key { get; set; }
    }
}
