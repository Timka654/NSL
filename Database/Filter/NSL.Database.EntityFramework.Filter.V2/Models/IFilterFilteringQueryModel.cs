namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public interface IFilterFilteringQueryModel
    {
        /// <summary>
        /// The root node for the filter criteria.
        /// </summary>
        FilterNode Filter { get; set; }
    }
}