using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    /// <summary>
    /// Order By block
    /// </summary>
    public class EntityFilterOrderBlockModel
    {
        public List<EntityFilterPropertyOrderModel> Properties { get; set; } = new List<EntityFilterPropertyOrderModel>();

        public EntityFilterOrderBlockModel AddProperty(string propertyPath, bool asc = true)
        {
            Properties.Add(new EntityFilterPropertyOrderModel() { PropertyPath = propertyPath, ASC = asc });

            return this;
        }
    }
}
