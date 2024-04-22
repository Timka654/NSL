using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSL.HttpClient.Validators;
using NSL.HttpClient.Models;

namespace NSL.HttpClient.Blazor.Validators
{
    public class ServerModelValidator : ComponentBase, IHttpResponseContentValidator
    {
        private ValidationMessageStore _messageStore;

        [CascadingParameter] EditContext CurrentEditContext { get; set; }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(ServerModelValidator)} requires a cascading parameter " +
                    $"of type {nameof(EditContext)}. For example, you can use {nameof(ServerModelValidator)} inside " +
                    $"an {nameof(EditForm)}.");
            }

            _messageStore = new ValidationMessageStore(CurrentEditContext);
            CurrentEditContext.OnValidationRequested += (s, e) => _messageStore.Clear();
            CurrentEditContext.OnFieldChanged += (s, e) => _messageStore.Clear(e.FieldIdentifier);
        }

        public void DisplayErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var err in errors)
            {
                _messageStore.Add(CurrentEditContext.Field(err.Key.Split('.').Last()), err.Value);
            }
            CurrentEditContext.NotifyValidationStateChanged();
        }

        public void DisplayApiErrors(BaseResponse apiResult)
        {
            if (apiResult != null && apiResult.Errors != null && apiResult.Errors.Any())
            {
                DisplayErrors(apiResult.Errors);
            }
        }


        public void DisplayError(string field, string validationMessage)
        {
            var dictionary = new Dictionary<string, List<string>>
            {
                { field, new List<string> { validationMessage } }
            };

            DisplayErrors(dictionary);
        }

        private KeyValuePair<string, string> GetErrorKeyValuePair(string error)
        {
            return new KeyValuePair<string, string>(string.Empty, error);
        }

        private KeyValuePair<string, string> GetErrorKeyValuePair(string error, Type validatedModelType)
        {
            var splitMessage = error.Split('\'', '\'');

            // Find a matching property on T and get it's display name
            // if no display name use the property name
            // if the property wasn't found use the name from the original error message - this shouldn't ever happen unless there is a property mismatch
            var properties = validatedModelType.GetProperties();
            var matchedProperty = properties?.FirstOrDefault(p => p.Name == splitMessage[1]);
            var displayAttribute = matchedProperty?.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(DisplayAttribute)) as DisplayAttribute;
            var displayName = (displayAttribute?.Name ?? matchedProperty?.Name) ?? splitMessage[1];

            var errorMessage = $"The {displayName} field {splitMessage[2]}.";

            return new KeyValuePair<string, string>(splitMessage[1], errorMessage);
        }
    }
}
