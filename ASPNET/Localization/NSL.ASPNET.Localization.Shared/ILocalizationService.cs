using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NSL.ASPNET.Localization.Shared
{
    public interface ILocalizationService
    {
        public string CurrentLanguage { get; }

        event Action OnUpdateLibrary;

        string GetLocalizationValue(string key, Dictionary<string, object> args = null, bool isRequired = false, string codeFragment = null, string defaultValue = null);
        IEnumerable<ILocalizationSource> GetSources();
        string[] GetSupportingLanguages();
        string LocalizationFormatting(string input);
        CachedLocalizationEntityModel TryGetRequestCache(string key);
    }

    public abstract class BaseLocalizationService : ILocalizationService
    {
        protected BaseLocalizationService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public string CurrentLanguage { get; private set; }

        public event Action OnUpdateLibrary = () => { };

        protected virtual Task UpdateLibrary()
        {
            OnUpdateLibrary();

            return Task.CompletedTask;
        }

        public virtual async Task ChangeLanguage(string value)
        {
            CurrentLanguage = value.Trim().ToLower();

            foreach (var item in GetSources())
            {
                await item.ChangeLanguageAsync(value);
            }

            await UpdateLibrary();
        }

        protected IServiceProvider ServiceProvider { get; }

        //protected ILocalizationSource[] localizationSources { get; set; }

        public abstract IEnumerable<ILocalizationSource> GetSources();

        public string GetLocalizationValue(string key
            , Dictionary<string, object>? args = null
            , bool isRequired = false
            , string? codeFragment = null
            , string? defaultValue = null)
        {
            if (key == null) return defaultValue ?? string.Empty;

            key = key.ToLower();

            string? v = default;

            foreach (var item in GetSources())
            {
                v = item.GetValue(key);
                if (v != default) break;
            }

            if (FragmentCacheEnabled && codeFragment != null)
            {
                CachedRequests[key] = new CachedLocalizationEntityModel() { Content = codeFragment, Args = args, DefaultValue = defaultValue };
            }

            if (v == default)
            {
                if (isRequired)
                    throw new Exception($"Localization key not found");

                return defaultValue ?? key;
            }

            if (args != null)
                foreach (var item in args)
                {
                    v = v.Replace($"{{{item.Key}}}", item.Value?.ToString());
                }

            return v;
        }


        #region Formatting

        static Regex formatRegexExpression = new Regex(";@([a-zA-Z0-9_]+)@;");

        public string LocalizationFormatting(string input)
        {
            var result = input;

            foreach (Match item in formatRegexExpression.Matches(input)
                .OrderByDescending(x => x.Index))
            {
                result = result.Replace(item.Value, GetLocalizationValue(item.Value.Substring(2, item.Value.Length - 4)));
            }

            return result;
        }

        #endregion


        protected abstract bool FragmentCacheEnabled { get; }

        protected ConcurrentDictionary<string, CachedLocalizationEntityModel> CachedRequests = new ConcurrentDictionary<string, CachedLocalizationEntityModel>();

        public CachedLocalizationEntityModel? TryGetRequestCache(string key)
        {
            CachedRequests.TryGetValue(key, out var v);

            return v;
        }

        public string[] GetSupportingLanguages()
        {
            var v = GetLocalizationValue("__support_languages");

            return v.Split(';');
        }

        private static readonly string[] emptyLanguageKeys = ["__support_languages"];

        public bool IsEmptyLanguageKey(string key)
            => emptyLanguageKeys.Contains(key);

        /// <summary>
        /// Init localization sources and other things. This method will be called before any localization source is used, so you can do some preparations here.
        /// </summary>
        /// <returns></returns>
        public abstract Task InitializeAsync();
    }
}
