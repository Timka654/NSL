using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components.Input
{
    public abstract class BaseSelectorComponent<TEntity, TKey, TSearchRequest> : ComponentBase
        where TEntity : new()
        where TKey : struct
    {


        [Parameter] public TEntity Value { get; set; }

        //[Parameter] public EventCallback<TEntity> ValueChanged { get; set; }
        [Parameter] public Action<TEntity>? ValueChanged { get; set; }

        [Parameter] public TKey ValueId { get; set; }

        //[Parameter] public EventCallback<TKey> ValueIdChanged { get; set; }
        [Parameter] public Action<TKey>? ValueIdChanged { get; set; }

        [Parameter] public Nullable<TKey> NullableValueId { get; set; }

        //[Parameter] public EventCallback<TKey?> NullableValueIdChanged { get; set; }
        [Parameter] public Action<TKey?>? NullableValueIdChanged { get; set; }

        [Parameter] public bool CanCreate { get; set; }
        [Parameter] public bool CreateConfirming { get; set; }

        [Parameter] public bool HaveNull { get; set; }

        [Parameter] public Action<TSearchRequest>? SearchRequestBuilder { get; set; }

        [Parameter] public Action<TEntity> OnCreated { get; set; } = _ => { };
        [Parameter] public bool AlwaysEdit { get; set; }
        [Parameter] public bool IgnoreFormState { get; set; }
        [Parameter] public bool EditView { get; set; }

        [CascadingParameter] public IFormComponent? EditorForm { get; set; }


        protected override Task OnInitializedAsync()
        {
            //if (EditorForm?.Purpose == TeachingEditorFormPurposeEnum.Create)
            //{
            //    //AlwaysEdit = true;
            //    EditView = true;
            //}

            return base.OnInitializedAsync();
        }

        protected abstract object? GetKey(TEntity entity);

        protected TRequest FillRequest<TRequest>(TRequest request)
            where TRequest : TSearchRequest
        {
            if (SearchRequestBuilder != null)
                SearchRequestBuilder(request);

            return request;

        }

        protected async Task SetValue(TEntity? selected)
        {
            if (Equals(selected, defaultItem))
                selected = default;

            var nullableKey = (TKey?)GetKey(selected);

            var key = nullableKey ?? default;

            bool haveChanges = false;

            haveChanges = NullableValueIdChanged != null && !Equals(nullableKey, NullableValueId);

            haveChanges = haveChanges || (ValueIdChanged != null && !Equals(key, ValueId));

            haveChanges = haveChanges || (ValueChanged != null && !Equals(selected, Value));

            if (!haveChanges)
                return;

            Value = selected;
            ValueId = key;
            NullableValueId = nullableKey;

            if (ValueChanged != null)
                ValueChanged(selected);
            if (ValueIdChanged != null)
                ValueIdChanged(key);
            if (NullableValueIdChanged != null)
                NullableValueIdChanged(nullableKey);
        }

        TEntity? defaultItem;

        protected IEnumerable<TEntity> PrependItem(IEnumerable<TEntity> collection, TEntity item)
        {
            collection = collection.Except([defaultItem]).Prepend(item);

            if (HaveNull)
                collection = collection.Prepend(defaultItem = new TEntity());

            return collection;
        }

        protected IEnumerable<TEntity> BuildData(IEnumerable<TEntity> collection)
        {
            if (HaveNull)
                collection = collection.Prepend(defaultItem = new TEntity());

            return collection;
        }
    }
}
