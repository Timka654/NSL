using System;

namespace NSL.Database.EntityFramework.Logger
{
    public class EntityLogChangesOptions
    {
        public static EntityLogChangesOptions Empty { get; } = new EntityLogChangesOptions();

        public string[] ExcludedProperties { get; set; } = Array.Empty<string>();
        public string[] ExcludedCreateProperties { get; set; } = Array.Empty<string>();
        public string[] ExcludedModifyProperties { get; set; } = Array.Empty<string>();
        public string[] ExcludedDeleteProperties { get; set; } = Array.Empty<string>();
    }
}
