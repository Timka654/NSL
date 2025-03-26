using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;

namespace NSL.Database.EntityFramework.Filter
{
    [Obsolete("Use EntityFilterBuilder.")]
    public class NavigationFilterBuilder() : EntityFilterTypedBuilder<EntityFilterQueryModel>(new EntityFilterQueryModel())
    {

    }
}
