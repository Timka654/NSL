using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Localization.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Localization
{
    public partial class LocalizationEditorComponent<TElement> : ComponentBase, IDisposable where TElement : BaseStaticLocalizationItemModel, new()
    {
        public class NewKeyFormModel
        {
            public string Key { get; set; }
        }

        [Inject] ILocalizationService LocalizationService { get; set; }

        IEditableLocalizationService EditableLocalizationService { get; set; }


        protected LocalizationEditContext Context = new LocalizationEditContext();


        protected NewKeyFormModel NewKeyForm { get; set; }

        protected override void OnInitialized()
        {
            if (LocalizationService is IEditableLocalizationService editable)
            {
                EditableLocalizationService = editable;

                EditableLocalizationService.OnBeginEdit += LocalizationService_OnEdit;
                EditableLocalizationService.OnBeginCreate += LocalizationService_OnBeginСreate;
            }

            base.OnInitialized();
        }

        protected virtual async void LocalizationService_OnBeginСreate()
        {
            NewKeyForm = new NewKeyFormModel() {  };

            StateHasChanged();
        }

        private string SelectedLang { get => Context.RequestModel.Language; set => SelectLanguage(value); }

        protected virtual void SelectLanguage(string language)
        {
            var value = Context.Values.FirstOrDefault(x => x.Language == language);

            Context.RequestModel = new()
            {
                Key = Context.Key.ToLower().Trim(),
                Language = language,
                Value = ""
            };

            if (value != null)
            {
                Context.RequestModel.Value = value.Value;
            }
        }

        private async void LocalizationService_OnEdit(string key, RenderFragment defaultFragment, Dictionary<string, object>? args)
        {
            key = key.ToLower();

            Context = new LocalizationEditContext()
            {
                Key = key,
                SystemKey = EditableLocalizationService.IsSystemKey(key),
                args = args,
                renderFragment = defaultFragment,
            };

            await ShowEdit();
        }

        protected virtual async Task LoadContext()
        {
            if (!Context.ValuesCache.TryGetValue(SelectedSource, out var values))
            {
                values = (await SelectedSource.GetLocalizationValuesAsync(Context.Key))?.ToList();

                Context.ValuesCache[SelectedSource] = values;
            }

            Context.Values = values;

            //if (Context.Values == null || !Context.Values.Any())
            //{
            //    var vals = LocalizationService.GetLocalizationValue(Context.Key);

            //    if (vals != Context.Key)
            //        Context.Values = [new() { Key = Context.Key, Language = "<Default>", Value = vals }];
            //}


        }

        protected virtual async Task ShowEdit()
        {
            Sources = EditableLocalizationService.GetEditableSources();

            SelectedSource = Sources.First();

            await LoadContext();

            if (EditableLocalizationService.IsSystemKey(Context.Key))
                SelectLanguage(string.Empty);
            else
                SelectLanguage(LocalizationService.CurrentLanguage);
            //Console.WriteLine("sssssssssssssssssssssssssss1"); // 1

            //await modalRef.ShowAsync();

            //StateHasChanged();
        }

        protected virtual async Task NewLocalization()
        {
            var key = NewKeyForm.Key.ToLower();

            Context = new LocalizationEditContext()
            {
                Key = key,
                SystemKey = EditableLocalizationService.IsSystemKey(key),
            };

            await ShowEdit();

            EditableLocalizationService.LocalizationSearchCallback(Context.Key);
        }

        protected virtual async Task UpdateLocalization()
        {
            Context.RequestModel.Key = Context.RequestModel.Key.ToLower();

            Context.RequestModel.Value = Context.RequestModel.Value.Trim();

            if (await EditableLocalizationService.UpdateLocalizationItem(SelectedSource, Context.RequestModel))
            {
                var e = Context.Values.FirstOrDefault(x => x.Language == Context.RequestModel.Language);

                if (e == null)
                {
                    e = new TElement();

                    Context.Values.Add(e);
                }

                e.Key = Context.RequestModel.Key;
                e.Value = Context.RequestModel.Value;
                e.Language = Context.RequestModel.Language;
            }

            StateHasChanged();
        }

        IEnumerable<IEditableLocalizationSource> Sources = Enumerable.Empty<IEditableLocalizationSource>();

        private async Task SelectSource(IEditableLocalizationSource source)
        {
            if (SelectedSource == source) return;

            SelectedSource = source;
            await this.LoadContext();
        }

        public void Dispose()
        {
            if (EditableLocalizationService != null)
            {
                EditableLocalizationService.OnBeginEdit -= LocalizationService_OnEdit;
                EditableLocalizationService.OnBeginCreate -= LocalizationService_OnBeginСreate;
            }
        }

        IEditableLocalizationSource SelectedSource;
    }

    public class LocalizationEditContext
    {
        public BaseCreateLocalizationItemRequestModel RequestModel { get; set; }

        public Dictionary<IEditableLocalizationSource, List<BaseStaticLocalizationItemModel>> ValuesCache = new();

        public List<BaseStaticLocalizationItemModel> Values = new List<BaseStaticLocalizationItemModel>();

        public RenderFragment renderFragment { get; set; }

        public Dictionary<string, object>? args { get; set; }

        public string Key { get; set; }

        public bool SystemKey { get; set; }
    }
}
