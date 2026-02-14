using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public interface ISelectFilteringQueryModel
    {
        /// <summary>
        /// A list of properties for selection in the query.
        /// </summary>
        public List<string> Properties { get; set; }
    }
}