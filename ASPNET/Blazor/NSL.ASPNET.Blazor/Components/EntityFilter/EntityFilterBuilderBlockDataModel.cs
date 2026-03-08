using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Entity.PathGenerator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public class EntityFilterBuilderBlockDataModel : EntityFilterBuilderDataModel
    {
        public FilterLogic Logic { get; set; }

        public bool Invert { get; set; }

        public bool CanNull { get; set; }

        public bool CaseSensitive { get; set; }

        public bool NullValue { get; set; }

        public string? Value { get; set; }

        public NSL.Database.EntityFramework.Filter.V2.Enums.FilterOperator? Operator { get; set; }

        public KeyValuePair<string, NSL.Entity.PathGenerator.Shared.FilterInfo>? Field { get; set; }

        [JsonIgnore]
        public string FieldNameKey { get; set; }

        protected override void Add(Type entityType, FilterLogic logic, IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> typeLibrary)
        {
            entityType = Field.Value.Value.ElementType ?? Field.Value.Value.PropertyType;

            base.Add(entityType, logic, typeLibrary);
        }

        public void ApplyToNode(FilterNode parentNode, string pathPrefix)
        {
            // 1. Определяем текущее поле
            string currentField = this.Field?.Key;

            // 2. Формируем полный путь. Если есть префикс, склеиваем через точку
            string fullPath = string.IsNullOrEmpty(pathPrefix)
                ? currentField
                : string.IsNullOrEmpty(currentField)
                    ? pathPrefix
                    : $"{pathPrefix}.{currentField}";

            // 3. Если выбран оператор — значит это конкретное правило фильтрации
            if (this.Operator.HasValue)
            {
                var backendOp = (FilterOperator)this.Operator.Value;

                // Если это оператор Any, мы останавливаем сборку пути и создаем NestedFilter
                if (backendOp == FilterOperator.Any)
                {
                    var filterBlock = new EntityFilterBlockModel
                    {
                        Property = fullPath, // Путь до коллекции (например, "User.Orders")
                        Type = backendOp,
                        Not = this.Invert
                    };

                    // Вложенные элементы Any собираются с ЧИСТОГО ЛИСТА (путь сбрасывается)
                    if (this.Tree != null && this.Tree.Any())
                    {
                        filterBlock.NestedFilter = base.ToFilterNode(""); // Передаем пустой префикс
                        if (filterBlock.NestedFilter != null)
                        {
                            filterBlock.NestedFilter.Logic = this.Logic;
                        }
                    }

                    parentNode.Filters.Add(filterBlock);
                }
                else
                {
                    // Стандартное конечное правило (Equal, Contains и т.д.)
                    parentNode.Filters.Add(new EntityFilterBlockModel
                    {
                        Property = fullPath, // Полный путь (например, "User.Role.Name")
                        Type = backendOp,
                        Value = this.NullValue ? null : this.Value,
                        Not = this.Invert
                    });
                }
            }
            else
            {
                // 4. Оператора нет. Это либо чистая логическая группа (AND/OR), либо папка навигации.
                if (string.IsNullOrEmpty(currentField))
                {
                    // Поля нет -> это логическая группа (например, добавили блок "OR")
                    var logicalGroup = base.ToFilterNode(pathPrefix);
                    if (logicalGroup != null)
                    {
                        logicalGroup.Logic = this.Logic;
                        parentNode.Nodes.Add(logicalGroup);
                    }
                }
                else
                {
                    // Поле есть, но оператора нет -> это папка навигации (например, раскрыли "User")
                    // Мы "сплющиваем" дерево: просто прокидываем накопленный fullPath детям
                    if (this.Tree != null && this.Tree.Any())
                    {
                        foreach (var child in this.Tree)
                        {
                            child.ApplyToNode(parentNode, fullPath);
                        }
                    }
                }
            }
        }
    }
}
