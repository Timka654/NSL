namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public interface IPaginationFilteringQueryModel
    {
        /// <summary>
        /// The number of records to skip (for pagination).
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// The number of records to take (for pagination).
        /// </summary>
        public int? Take { get; set; }
    }
}