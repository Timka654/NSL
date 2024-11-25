using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Options;

namespace NSL.ASPNET.ModelBinders
{
    /// <summary>
    /// Origin source https://brokul.dev/sending-files-and-additional-data-using-httpclient-in-net-core
    /// </summary>
    public class FormDataJsonBinder : IModelBinder
    {
        private readonly ILogger<FormDataJsonBinder> _logger;
        private readonly Microsoft.AspNetCore.Mvc.JsonOptions jsonOptions;

        public FormDataJsonBinder(ILogger<FormDataJsonBinder> logger, IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions)
        {
            _logger = logger;
            this.jsonOptions = new Microsoft.AspNetCore.Mvc.JsonOptions();
            jsonOptions.Configure(this.jsonOptions);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            try
            {
                var result = JsonSerializer.Deserialize(value, bindingContext.ModelType, jsonOptions.JsonSerializerOptions);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
