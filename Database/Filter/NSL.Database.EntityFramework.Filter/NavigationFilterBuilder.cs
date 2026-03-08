using System;
using NSL.Database.EntityFramework.Filter.Models;

namespace NSL.Database.EntityFramework.Filter
{
    [Obsolete("Use EntityFilterBuilder.")]
    public class NavigationFilterBuilder() : EntityFilterTypedBuilder<EntityFilterQueryModel>(new EntityFilterQueryModel())
    {

    }
}
