using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Generators.EntityPathGenerator.Shared;
using System;
using System.Collections.Generic;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public class EntityFilterBuilderDataModel
    {
        public Type EntityType { get; private set; }

        public void SetType(Type entityType, IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> library)
        {
            EntityType = entityType;

            if (!library.TryGetValue(entityType, out var _typeMap))
                throw new InvalidOperationException("");

            FieldsData = _typeMap;
        }

        public IReadOnlyDictionary<string, FilterInfo>? FieldsData { get; set; }

        public List<EntityFilterBuilderBlockDataModel> Tree { get; set; } = new();



        protected virtual void Add(Type entityType, FilterLogic logic, IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> typeLibrary)
        {
            //var entityType = Field.Value.Value.ElementType ?? Field.Value.Value.PropertyType;

            if (!typeLibrary.TryGetValue(entityType, out var _typeMap))
                throw new InvalidOperationException("");

            var e = new EntityFilterBuilderBlockDataModel()
            {
                Logic = logic,
                FieldsData = _typeMap
            };

            e.SetType(entityType, typeLibrary);

            Tree.Add(e);
        }

        public virtual void AddOr(IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> typeLibrary)
        {
            Add(EntityType, FilterLogic.Or, typeLibrary);
        }

        public virtual void AddAnd(IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> typeLibrary)
        {
            Add(EntityType, FilterLogic.And, typeLibrary);
        }
        public virtual FilterNode? ToFilterNode(string pathPrefix = "")
        {
            var node = new FilterNode
            {
                Logic = FilterLogic.And, // Логика по умолчанию для группы
                Filters = new List<EntityFilterBlockModel>(),
                Nodes = new List<FilterNode>()
            };

            if (Tree != null)
            {
                foreach (var block in Tree)
                {
                    // Делегируем сборку узла самому блоку, передавая ему родительский node и накопленный путь
                    block.ApplyToNode(node, pathPrefix);
                }
            }

            // Очищаем пустые коллекции
            if (node.Filters.Count == 0) node.Filters = null;
            if (node.Nodes.Count == 0) node.Nodes = null;

            // Если узел пуст, возвращаем null, чтобы не отправлять мусор на бек
            return (node.Filters == null && node.Nodes == null) ? null : node;
        }
    }
}
