using Microsoft.EntityFrameworkCore;
using NSL.BlazorTemplate.Shared.Server.Data;

namespace NSL.BlazorTemplate
{
    public partial class Program
    {
        static void AddDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();
        }
    }
}
