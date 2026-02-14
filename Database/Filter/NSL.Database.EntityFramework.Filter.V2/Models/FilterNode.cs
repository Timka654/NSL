using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public enum FilterLogic
    {
        And,
        Or
    }

    public class FilterNode
    {
        /// <summary>
        /// Logical operator to apply to children nodes or filters.
        /// </summary>
        public FilterLogic Logic { get; set; } = FilterLogic.And;

        /// <summary>
        /// Child filter nodes for creating nested logical groups.
        /// </summary>
        public List<FilterNode> Nodes { get; set; }

        /// <summary>
        /// The actual filter conditions in this group.
        /// </summary>
        public List<EntityFilterBlockModel> Filters { get; set; }
    }
}