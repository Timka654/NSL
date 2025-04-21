using NSL.ASPNET.Mvc.Route;

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


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            
            app.Run();
        }
    }
}
