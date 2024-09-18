using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class EntityFilterQueryModel
    {
        public int Offset { get; set; } = 0;

        public int Count { get; set; } = int.MaxValue;

        public List<EntityFilterBlockModel>? FilterQuery { get; set; }

        public List<EntityFilterOrderBlockModel>? OrderQuery { get; set; }
    }
}
