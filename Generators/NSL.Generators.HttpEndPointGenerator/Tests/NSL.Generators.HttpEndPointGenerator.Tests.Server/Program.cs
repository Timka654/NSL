namespace NSL.Generators.HttpEndPointGenerator.Tests.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });

            var app = builder.Build();

            app.UseRouting();

            app.MapControllers();

            app.Run();
        }
    }
}
