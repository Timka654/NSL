namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public class SortModel
    {
        /// <summary>
        /// Path to property for sorting (e.g., "User.Name").
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Indicates if the sort order is descending.
        /// </summary>
        public bool Descending { get; set; }
    }
}