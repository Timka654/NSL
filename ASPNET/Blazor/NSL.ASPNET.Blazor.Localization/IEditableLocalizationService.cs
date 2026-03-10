using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Blazor.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.ASPNET.Localization.Shared
{
    public delegate void EditableLocalizationChangeEditModeDelegate(bool mode);
    public delegate void EditableLocalizationSearchCallbackDelegate(string key);
    public delegate void EditableLocalizationBeginEditDelegate(string key, RenderFragment defaultFragment, Dictionary<string, object>? args);

    public interface IEditableLocalizationService : ILocalizationService
    {
        bool EditMode { get; set; }

        event EditableLocalizationChangeEditModeDelegate OnChangeEditMode;
        event EditableLocalizationSearchCallbackDelegate OnEditorSearchCallback;
        event EditableLocalizationBeginEditDelegate OnBeginEdit;
        event EditableLocalizationSearchCallbackDelegate OnBeginCreate;

        IEnumerable<IEditableLocalizationSource> GetEditableSources();

        void LocalizationSearchCallback(string key);

        Task<bool> UpdateLocalizationItem(IEditableLocalizationSource source, BaseCreateLocalizationItemRequestModel requestModel);

        bool IsSystemKey(string key);

        Task ShowEditorFor(string key, RenderFragment defaultFragment, Dictionary<string, object> args = null);
    }

    public abstract class BaseEditableLocalizationService : BaseLocalizationService, IEditableLocalizationService
    {
        protected BaseEditableLocalizationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private bool editMode;

        public bool EditMode
        {
            get => editMode;
            set
            {
                if (editMode == value) return;
                editMode = value;
                OnChangeEditMode(value);
            }
        }

        public IEnumerable<IEditableLocalizationSource> GetEditableSources() => GetSources()
            .OfType<IEditableLocalizationSource>()
            .ToArray()!;

        public event EditableLocalizationSearchCallbackDelegate OnEditorSearchCallback = (key) => { };

        public void LocalizationSearchCallback(string key)
            => OnEditorSearchCallback(key);

        public event EditableLocalizationChangeEditModeDelegate OnChangeEditMode = (mode) => { };

        public event EditableLocalizationBeginEditDelegate OnBeginEdit = (key, defaultFragment, args) => { };

        public event EditableLocalizationSearchCallbackDelegate OnBeginCreate = (key) => { };

        public async Task ShowEditorFor(string key, RenderFragment defaultFragment, Dictionary<string, object>? args = null)
        {
            EditMode = false;

            OnBeginEdit(key, defaultFragment, args);
        }


        public async Task<bool> UpdateLocalizationItem(IEditableLocalizationSource source, BaseCreateLocalizationItemRequestModel e)
        {
            if (!await source.UpdateLocalizationItemAsync(e))
                return false;

            if (CurrentLanguage == e.Language)
            {
                await UpdateLibrary();
            }

            return true;
        }

        public bool IsSystemKey(string key)
            => "__support_languages" == key;
    }
}
