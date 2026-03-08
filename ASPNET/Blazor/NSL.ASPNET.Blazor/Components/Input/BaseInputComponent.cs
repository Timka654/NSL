using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using NSL.ASPNET.Localization.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

    public abstract class BaseInputComponent<TValue> : ComponentBase, IDisposable
    {
        protected FieldIdentifier? fieldIdentifier;

        protected ElementReference ElementRef;
        private Dictionary<string, object> attributes;

        protected string fieldCssClasses => string.Join(" "
            , @class
            , BuildClass()
            , fieldIdentifier != null ? (EditContext?.FieldCssClass(fieldIdentifier.Value) ?? string.Empty) : string.Empty).Trim();


        [Inject] protected ILocalizationService localizationService { get; set; }
        [Inject] protected IJSRuntime jsRuntime { get; set; }


        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes
        {
            get { OnGetAttributes(attributes); return attributes; }
            set => attributes = value;
        }

        [CascadingParameter] protected EditContext EditContext { get; set; } = default!;

        [CascadingParameter] private IFormComponent? Form { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Parameter] public virtual TValue Value { get; set; } = default!;

        /// <summary>
        /// This event fired on every user keystroke that changes the NumberInput value.
        /// </summary>
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

        [Parameter] public string? placeholder { get; set; }

        [Parameter] public string? placeholderLK { get; set; }

        [Parameter] public string @class { get; set; }

        [Parameter] public bool disabled { get; set; }

        [Parameter] public bool @readonly { get; set; }

        [Parameter] public bool IgnoreFormState { get; set; }

        protected bool _readonly => (!IgnoreFormState && Form != null && !Form.EditState) || @readonly;

        protected string? placeholderContent { get; set; }

        protected virtual bool DisablingOnRO { get; }

        protected virtual void OnGetAttributes(Dictionary<string, object> attribs) { }


        protected override async Task OnInitializedAsync()
        {
            localizationService.OnUpdateLibrary += LocalizationService_OnUpdateLibrary;

            if (placeholderLK != default)
                placeholderContent = localizationService.GetLocalizationValue(placeholderLK, codeFragment: placeholder);
            else
                placeholderContent = placeholder;

            Attributes ??= new Dictionary<string, object>();

            if (Form != default && !IgnoreFormState)
            {
                Form.OnEditStateChanged += Form_OnEditStateChanged;
                Form_OnEditStateChanged(Form.EditState);
            }

            if (Form == null || IgnoreFormState)
                Form_OnEditStateChanged(!@readonly);

            if (ValueExpression != null)
                fieldIdentifier = FieldIdentifier.Create(ValueExpression);


            await base.OnInitializedAsync();
        }

        private void LocalizationService_OnUpdateLibrary()
        {

            if (placeholderLK != default)
                placeholderContent = localizationService.GetLocalizationValue(placeholderLK, codeFragment: placeholder);
            else
                placeholderContent = placeholder;

            StateHasChanged();
        }

        private void Form_OnEditStateChanged(bool obj)
        {
            var a = DisablingOnRO ? "disabled" : "readonly";

            if (obj)
                Attributes.Remove(a);
            else
                Attributes.Add(a, a);

            StateHasChanged();
        }

        protected abstract TValue CastValue(ChangeEventArgs e);

        protected Task OnChange(ChangeEventArgs e)
            => SetValue(CastValue(e));

        protected virtual async Task<bool> SetValue(TValue newValue, bool preventChangeEvent = false)
        {
            var oldValue = Value;

            Value = newValue;

            var neq = !Equals(oldValue, Value);

            if (neq)
            {
                if (!preventChangeEvent)
                    await ValueChanged.InvokeAsync(Value);

                if (fieldIdentifier.HasValue)
                    EditContext?.NotifyFieldChanged(fieldIdentifier.Value);
            }

            EditContext?.NotifyValidationStateChanged();

            return neq;
        }

        public void Dispose()
        {
            if (Form != default)
            {
                Form.OnEditStateChanged -= Form_OnEditStateChanged;
            }

            localizationService.OnUpdateLibrary -= LocalizationService_OnUpdateLibrary;
        }

        protected virtual string BuildClass()
        {
            return null;
        }
    }
}
