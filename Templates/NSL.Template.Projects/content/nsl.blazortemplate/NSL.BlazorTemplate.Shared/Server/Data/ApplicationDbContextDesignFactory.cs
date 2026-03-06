//-:cnd:noEmit
#if DEBUG

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NSL.BlazorTemplate.Shared.Server.Data
{
    public class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var dbc = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .Options;

            return new ApplicationDbContext(dbc);
        }
    }
}

#endif
//+:cnd:noEmit