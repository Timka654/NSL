using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class FilterBlockOrderViewModel
    {
        public List<FilterPropertyOrderViewModel> Properties { get; set; } = new List<FilterPropertyOrderViewModel>();
    }
}
