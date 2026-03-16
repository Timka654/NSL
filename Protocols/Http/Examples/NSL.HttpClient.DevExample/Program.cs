using NSL.HttpClient;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HttpResponseMessage r = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) {  Content = new StringContent("{\"\":[{\"message\":\"{profile_not_found}\",\"args\":[]}]}", Encoding.UTF8, "application/json") };

        var t = await r.ReadErrorsAsync();

    }
}