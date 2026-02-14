using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public interface ISortedFilteringQueryModel
    {
        /// <summary>
        /// A list of properties to sort by, in order.
        /// </summary>
        public List<SortModel> Sortings { get; set; }
    }
}