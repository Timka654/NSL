
using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework;
using NSL.Database.EntityFramework.ASPNET;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.EF.Tests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DevDbContext>(x => x.UseNpgsql("Host=localhost;Port=5432;Database=devdb_1;Username=postgres;Password=postgres"));

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            await app.AcceptDbMigrations<DevDbContext>();


            await app.Services.InvokeDbTransactionAsync<DevDbContext>(async c =>
            {
                if (await c.Dev1.AnyAsync())
                    return false;

                c.Dev1.AddRange(new Dev1Model()
                {
                    Data = "parent1",
                    Childs = new List<Dev2Model>() {
                        new Dev2Model() { Data = "child1_1" },
                        new Dev2Model() { Data = "child1_2" }
                    }
                }, new Dev1Model()
                {
                    Data = "parent2",
                    Childs = new List<Dev2Model>() {
                        new Dev2Model() { Data = "child2_1" },
                        new Dev2Model() { Data = "child2_2" }
                    }
                });

                return true;
            });

            await app.Services.InvokeDbTransactionAsync<DevDbContext>(async c =>
            {
                var test = await c.Dev1.Include(x => x.Childs)
                .SelectGetTest()
                .ToArrayAsync();

                return false;
            });


            app.Run();
        }
    }
}
