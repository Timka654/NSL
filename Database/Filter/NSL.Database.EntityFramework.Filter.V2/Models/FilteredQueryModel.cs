using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    /// <summary>
    /// Represents a query model with only filtering enabled.
    /// </summary>
    public class FilterQueryModel : BaseFilteringQueryModel, IFilterFilteringQueryModel
    {
        public FilterNode? Filter { get; set; }
    }

    /// <summary>
    /// Represents a query model with filtering and pagination.
    /// </summary>
    public class PagedFilterQueryModel : BaseFilteringQueryModel, IFilterFilteringQueryModel, IPaginationFilteringQueryModel
    {
        public FilterNode? Filter { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    /// <summary>
    /// Represents a query model with filtering and sorting.
    /// </summary>
    public class SortedFilterQueryModel : BaseFilteringQueryModel, IFilterFilteringQueryModel, ISortedFilteringQueryModel
    {
        public FilterNode? Filter { get; set; }
        public List<SortModel>? Sortings { get; set; }
    }

    /// <summary>
    /// Represents a comprehensive query model that includes filtering, sorting, and pagination parameters.
    /// </summary>
    public class FilteredQueryModel : BaseFilteringQueryModel, IFilterFilteringQueryModel, ISortedFilteringQueryModel, IPaginationFilteringQueryModel
    {
        /// <summary>
        /// The root node for the filter criteria.
        /// </summary>
        public FilterNode? Filter { get; set; }

        /// <summary>
        /// A list of properties to sort by, in order.
        /// </summary>
        public List<SortModel>? Sortings { get; set; }

        /// <summary>
        /// The number of records to skip (for pagination).
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// The number of records to take (for pagination).
        /// </summary>
        public int? Take { get; set; }
    }

    /// <summary>
    /// Represents a full query model with all features enabled.
    /// </summary>
    public class FullFilteredQueryModel : BaseFilteringQueryModel, IFilterFilteringQueryModel, ISortedFilteringQueryModel, IPaginationFilteringQueryModel, ISelectFilteringQueryModel, IIncludeFilteringQueryModel
    {
        public FilterNode? Filter { get; set; }
        public List<SortModel>? Sortings { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public List<string>? Properties { get; set; }
        public List<string>? Includes { get; set; }
    }
}