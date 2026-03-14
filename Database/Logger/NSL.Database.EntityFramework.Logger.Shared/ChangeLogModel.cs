using System;

namespace NSL.Database.EntityFramework.Logger.Shared
{
    public class ChangeLogModel : IChangeLogModel
    {
        public Guid Id { get; set; }

        public string EntityName { get; set; }

        public EntityActionTypeEnum ActionType { get; set; }

        public string? Data { get; set; }

        public DateTime CreateTime { get; set; }

        public string Key { get; set; }
    }
}
