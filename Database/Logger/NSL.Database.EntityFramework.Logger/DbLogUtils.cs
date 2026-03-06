using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NSL.Database.EntityFramework.Logger.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Logger
{
    public static class DbLogUtils
    {
        static JsonSerializerOptions defaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static async Task LogChangesAsync<TChangeEntity, TContext>(this TContext context
            , Func<EntityEntry, TChangeEntity, IEnumerable<ChangePropertyModel>, Task> onProcessingChange = null
            , bool saveToSet = true
            , EntityLogChangesOptions? options = null
            , JsonSerializerOptions? jsonOptions = null)
            where TContext : DbContext
            where TChangeEntity : class, IChangeLogModel, new()
        {
            if (!context.ChangeTracker.HasChanges())
                return;

            var changes = from e in context.ChangeTracker.Entries()
                          where e.State != EntityState.Unchanged
                          select e;

            changes = changes.ToArray();

            foreach (var change in changes)
            {
                TChangeEntity changeLog = new TChangeEntity();

                //if (!EntityTypes.Contains(change.Entity.GetType()))
                //    EntityTypes.Add(change.Entity.GetType());
                changeLog.EntityName = change.Entity.GetType().Name;
                changeLog.CreateTime = DateTime.UtcNow;

                List<ChangePropertyModel> changeProperties = new List<ChangePropertyModel>();

                var currentValues = change.CurrentValues;
                var originalValues = change.OriginalValues;

                var keys = change.Metadata.GetKeys();

                List<string> entityKeys = new List<string>();

                foreach (var item in keys)
                {
                    foreach (var p in item.Properties)
                    {
                        entityKeys.Add(currentValues[p].ToString());
                    }
                }

                changeLog.Key = string.Join("._", entityKeys);

                if (change.State == EntityState.Added)
                {
                    changeLog.ActionType = EntityActionTypeEnum.Create;

                    var values = currentValues.Properties.Where(x => currentValues[x] != null);

                    if (options != null)
                        values = values.Where(x => !options.ExcludedProperties.Contains(x.Name) && !options.ExcludedCreateProperties.Contains(x.Name));

                    changeProperties.AddRange(values
                        .Select(x => new ChangePropertyModel()
                        {
                            PropertyName = x.Name,
                            NewValue = currentValues[x].ToString()
                        }));

                    // Log Added
                }
                else if (change.State == EntityState.Modified)
                {
                    changeLog.ActionType = EntityActionTypeEnum.Modify;
                    // Log Modified

                    foreach (var property in originalValues.Properties)
                    {
                        var prop_ = change.Property(property.Name);
                        if (prop_.IsModified)
                        {
                            changeProperties.Add(new ChangePropertyModel() { PropertyName = property.Name, NewValue = prop_.CurrentValue, OldValue = prop_.OriginalValue });
                            //log propertyName: original-- > current
                        }
                    }

                }
                else if (change.State == EntityState.Deleted)
                {
                    changeLog.ActionType = EntityActionTypeEnum.Remove;
                    // log deleted

                    changeProperties.AddRange(originalValues.Properties.Where(x => originalValues[x] != null).Select(x => new ChangePropertyModel() { PropertyName = x.Name, NewValue = originalValues[x].ToString() }));
                }

                changeLog.Data = JsonSerializer.Serialize(changeProperties, jsonOptions ?? defaultJsonOptions);

                if (onProcessingChange != null)
                    await onProcessingChange(change, changeLog, changeProperties);

                if (saveToSet)
                    context.Set<TChangeEntity>().Add(changeLog);

                changeProperties.Clear();
            }
        }
    }
}
