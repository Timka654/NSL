using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class EntityFilterOrderBlockModel
    {
        public List<FilterPropertyOrderViewModel> Properties { get; set; } = new List<FilterPropertyOrderViewModel>();
    }
}
