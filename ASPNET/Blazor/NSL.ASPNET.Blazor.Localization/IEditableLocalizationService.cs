using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.ASPNET.Localization.Shared
{
    public delegate void EditableLocalizationChangeEditModeDelegate(bool mode);
    public delegate void EditableLocalizationSearchCallbackDelegate(string key);
    public delegate void EditableLocalizationBeginEditDelegate(string key, RenderFragment defaultFragment, Dictionary<string, object>? args);

    public interface IEditableLocalizationService
    {
        bool EditMode { get; set; }

        event EditableLocalizationChangeEditModeDelegate OnChangeEditMode;
        event EditableLocalizationSearchCallbackDelegate OnEditorSearchCallback;
        event EditableLocalizationBeginEditDelegate OnBeginEdit;

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

        public event EditableLocalizationChangeEditModeDelegate OnChangeEditMode = (mode) => { };

        public event EditableLocalizationSearchCallbackDelegate OnEditorSearchCallback = (key) => { };

        public event EditableLocalizationBeginEditDelegate OnBeginEdit = (key, defaultFragment, args) => { };

        public async Task ShowEditorFor(string key, RenderFragment defaultFragment, Dictionary<string, object>? args = null)
        {
            EditMode = false;

            OnBeginEdit(key, defaultFragment, args);
        }

        public IEditableLocalizationSource[] GetEditableSources()
            => GetSources()
                .Where(x => x is IEditableLocalizationSource els && els.HaveLocalizationPermission())
                .Select(x => x as IEditableLocalizationSource)
                .OrderByDescending(x => x.Order)
                .ToArray()!;
    }
}
