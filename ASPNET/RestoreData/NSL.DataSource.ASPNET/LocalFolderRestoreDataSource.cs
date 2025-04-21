using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.DataSource.ASPNET
{
    public class LocalFolderRestoreDataSource(IConfiguration configuration, IOptions<LocalFolderRestoreDataConfigurationModel> options) : IRestoreDataSource
    {
        string RelativePath => options.Value?.RelativePath ?? "restore_data";

        string contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);

        public async Task RemoveDataAsync(string name, CancellationToken cancellationToken)
        {
            var epath = Path.Combine(contentRoot, RelativePath, name);

            if (!File.Exists(epath))
                return;

            File.Delete(epath);
        }

        public async Task SetDataAsync(string name, object data, CancellationToken cancellationToken)
        {
            var epath = Path.Combine(contentRoot, RelativePath);

            if (!Directory.Exists(epath))
                Directory.CreateDirectory(epath);

            epath = Path.Combine(epath, name);

            File.WriteAllText(epath, JsonSerializer.Serialize(data, JsonSerializerOptions.Web));
        }

        public async Task<TData?> TryGetDataAsync<TData>(string name, CancellationToken cancellationToken)
        {
            var epath = Path.Combine(contentRoot, RelativePath, name);

            if (!File.Exists(epath))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TData>(File.ReadAllText(epath), JsonSerializerOptions.Web);
        }
    }
}
