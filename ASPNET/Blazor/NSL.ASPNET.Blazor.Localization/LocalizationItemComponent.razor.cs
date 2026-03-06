using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Localization.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Localization
{
    public partial class LocalizationItemComponent : ComponentBase, IDisposable
    {
        [Inject] protected ILocalizationService LocalizationService { get; set; }

        IEditableLocalizationService? EditingLocalizationService { get; set; }

        [Parameter] public RenderFragment ChildContent { get; set; }


        private RenderFragment CurrentFragment { get; set; }

        [Parameter] public string? Key { get => _key; set => _key = value?.ToLower().Trim(); }

        [Parameter] public string? LDefaultValue { get; set; } = null;

        [Parameter] public bool IsRequired { get; set; }

        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> Parameters { get; set; }

        private bool EditMode => EditingLocalizationService?.EditMode == true;

        private string? _key;

        protected override void OnInitialized()
        {
            LocalizationService.OnUpdateLibrary += LocalizationService_OnUpdateLibrary;

            if(LocalizationService is IEditableLocalizationService els)
                EditingLocalizationService = els;

            if (EditingLocalizationService != null)
            {
                EditingLocalizationService.OnChangeEditMode += LocalizationService_OnChangeEditMode;
                EditingLocalizationService.OnEditorSearchCallback += LocalizationService_OnEditorSearchCallback;
            }

            CurrentFragment = new RenderFragment(b =>
            {
                b.AddMarkupContent(0, Key);
            });

            LocalizationService_OnUpdateLibrary();
        }

        private async void LocalizationService_OnEditorSearchCallback(string obj)
        {
            if (!string.Equals(obj, _key, StringComparison.InvariantCultureIgnoreCase))
                return;

            await ShowEdit();
        }

        protected override void OnParametersSet()
        {
            LocalizationService_OnUpdateLibrary();
        }

        private void LocalizationService_OnUpdateLibrary()
        {
            if (_key == default)
                return;

            var result = LocalizationService.GetLocalizationValue(_key, Parameters, IsRequired, defaultValue: LDefaultValue);

            CurrentFragment = new RenderFragment(b =>
            {
                b.AddMarkupContent(0, result);
            });


            StateHasChanged();
        }

        private void LocalizationService_OnChangeEditMode(bool obj)
        {
            //Console.WriteLine($"change edit mode handle value = {obj}");

            StateHasChanged();
        }

        private async Task ShowEdit()
        {
            if (EditingLocalizationService != null)
            await EditingLocalizationService.ShowEditorFor(_key, ChildContent, Parameters);
        }

        public void Dispose()
        {
            LocalizationService.OnUpdateLibrary -= LocalizationService_OnUpdateLibrary;
            if (EditingLocalizationService != null)
            {
                EditingLocalizationService.OnChangeEditMode -= LocalizationService_OnChangeEditMode;
                EditingLocalizationService.OnEditorSearchCallback -= LocalizationService_OnEditorSearchCallback;
            }
        }
    }
}
