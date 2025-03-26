using System;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class EntityFilterPropertyOrderModel : EntityFilterBasePropertyModel
    { 
        public bool ASC { get; set; }
    }

    [Obsolete("Use \"EntityFilterPropertyOrderModel\"", true)]
    public class FilterPropertyOrderViewModel : EntityFilterPropertyOrderModel
    {
    }
}
