using NSL.ASPNET.Mvc.Route;
using NSL.ASPNET.Test.Services;

namespace NSL.ASPNET.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddMvc(o => { });

            builder.AddNSLModelStateFilter();

            builder.Services.RegisterServices("basicModel");

            try
            {
                builder.Services.RegisterServices("errorKeyModel");

                throw new Exception();
            }
            catch (InvalidOperationException ex)
            {
            }

            builder.Services.RegisterServices("keyModel");
            //builder.Services.AddSingleton<keyService3>();

            try
            {
                builder.Services.RegisterServices("lifeTimeConflictModel");

                throw new Exception();
            }
            catch (InvalidOperationException ex)
            {
            }

            builder.Services.Configure<testOptions>(c => { c.Test = "DevDev1"; });

            builder.Services.RegisterServices("optionsRequiredModel");

            builder.Services.RegisterServices("serviceProviderRequiredModel");

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            var sprs1 = app.Services.GetRequiredService<serviceProviderRequiredService1>();
            var ors1 = app.Services.GetRequiredService<optionsRequiredService1>();

            var ks1 = app.Services.GetRequiredKeyedService<keyService1>("key1");
            var ks2 = app.Services.GetRequiredService<keyService2>();

            var bs1 = app.Services.GetRequiredService<basicService1>();
            var bs2 = app.Services.GetRequiredService<basicService2>();
            //var ks3 = app.Services.GetRequiredService<keyService3>();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
