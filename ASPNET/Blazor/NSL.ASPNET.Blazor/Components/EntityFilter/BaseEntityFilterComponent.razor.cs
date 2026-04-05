using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Localization.Shared;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Generators.EntityPathGenerator.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public partial class BaseEntityFilterComponent : ComponentBase, IEntityFilterBuilderFragmentComponent
    {
        [Parameter] public IEntityFilterBuilderComponentRegistry ComponentRegistry { get; set; }

        public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> TypeLibrary { get; set; }

        private EntityFilterBuilderAcceptDelegate acceptAction;

        private EntityFilterBuilderDataModel data = new EntityFilterBuilderDataModel();

        public bool IsFirst(EntityFilterBuilderBlockDataModel block)
            => data.Tree.FirstOrDefault() == block;

        public void Clear()
        {
            data = new EntityFilterBuilderDataModel();
        }

        public virtual async Task ShowAsync(Type type, IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> typeLibrary, EntityFilterBuilderAcceptDelegate acceptAction)
        {
            data.SetType(type, typeLibrary);
            TypeLibrary = typeLibrary;
            this.acceptAction = acceptAction;

        }

        public virtual async Task HideAsync()
        {

        }

        protected virtual async Task TryAccept(IFormComponent form)
        {
            if (await acceptAction(data, form))
                await HideAsync();
        }

        public void Update()
        {
            StateHasChanged();
        }

        public void Remove(EntityFilterBuilderBlockDataModel block)
        {
            data.Tree.Remove(block);
        }
    }

    public delegate Task<bool> EntityFilterBuilderAcceptDelegate(EntityFilterBuilderDataModel data, IFormComponent form);
    public partial class EntityFilterBuilderBlockComponent : ComponentBase, IEntityFilterBuilderFragmentComponent
    {
        static readonly IReadOnlyDictionary<Type, FilterOperator[]> TypeOperators = new Dictionary<Type, FilterOperator[]>
        {
            [typeof(List<>)] = [FilterOperator.Any],
            [typeof(Enum)] = [FilterOperator.Equal],
            [typeof(bool)] = [FilterOperator.Equal],
            [typeof(string)] = [FilterOperator.Equal, FilterOperator.StartsWith, FilterOperator.Contains, FilterOperator.EndsWith],
            [typeof(sbyte)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(byte)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(ushort)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(short)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(uint)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(int)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(ulong)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(long)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(DateTime)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(TimeSpan)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(DateOnly)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
            [typeof(TimeOnly)] = [FilterOperator.Equal, FilterOperator.GreaterThan, FilterOperator.LessThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual],
        };

        static IReadOnlyCollection<Type> CaseSensitiveTypes = [typeof(string)];


        FilterOperator?[]? CurrentOperators = null;

        EntityFilterBuilderValueComponent? CurrentEditor = null;
        IEntityFilterBuilderComponentData? ComponentData = null;

        [Parameter] public IEntityFilterBuilderFragmentComponent Parent { get; set; }

        [Parameter] public EntityFilterBuilderBlockDataModel Block { get; set; }

        [CascadingParameter] BaseEntityFilterComponent Component { get; set; }

        public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FilterInfo>> TypeLibrary => Parent.TypeLibrary;

        public void Update()
            => StateHasChanged();

        public bool IsFirst(EntityFilterBuilderBlockDataModel block)
            => Block.Tree.FirstOrDefault() == block;

        public void Remove(EntityFilterBuilderBlockDataModel block)
        {
            Block.Tree.Remove(block);
            Update();
        }

        private void SelectField(KeyValuePair<string, FilterInfo> field)
        {
            if (field.Key != Block.Field?.Key)
                Block.Tree.Clear();

            SetField(field);
        }

        private void SetField(KeyValuePair<string, FilterInfo> field)
        {
            Block.Field = field;

            if (field.Value.Meta.TryGetValue("Localization", out var _localization))
                Block.FieldNameKey = (string)_localization;
            else
                Block.FieldNameKey = default;

            if (!TypeOperators.TryGetValue(field.Value.PropertyType, out var operators))
            {
                if (field.Value.PropertyType.IsEnum && !TypeOperators.TryGetValue(typeof(Enum), out operators))
                {
                    operators = null;
                }
            }

            CurrentOperators = operators?.Select(x => (FilterOperator?)x)/*.Prepend(null)*/.ToArray();
            CurrentEditor = Component.ComponentRegistry.GetValueComponent(Block);

            if (CurrentEditor != null && CurrentOperators == null)
                CurrentOperators = [FilterOperator.Equal];

            if (CurrentOperators != null && CurrentEditor != null)
            {
                if (CurrentOperators.Length < 3)
                {
                    CurrentOperators = [CurrentOperators.Last()];
                }

                Block.Operator = CurrentOperators.First();

                ComponentData = Activator.CreateInstance(CurrentEditor.ComponentData) as IEntityFilterBuilderComponentData;
                ComponentData?.Initialize(Block, Block.Field.Value.Key, Block.Field.Value.Value, this);
            }
            else
                ComponentData = null;

            Block.CanNull = field.Value.CanBeNull;

            if (Block.Operator != null && (CurrentOperators == null || !CurrentOperators.Contains(Block.Operator.Value)))
                Block.Operator = null;
        }
    }
}
