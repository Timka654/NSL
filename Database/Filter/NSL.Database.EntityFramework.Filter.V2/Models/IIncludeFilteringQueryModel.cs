using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public interface IIncludeFilteringQueryModel
    {
        /// <summary>
        /// A list of properties to include in the query.
        /// </summary>
        public List<string> Includes { get; set; }
    }
}