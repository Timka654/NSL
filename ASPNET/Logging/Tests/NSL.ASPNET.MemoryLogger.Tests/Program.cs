using NSL.ASPNET.MemoryLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddMemoryLogger();
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

Enumerable.Range(0, 10).Select(x => Task.Run(async () =>
{
    var rnd = new Random();
    while (true)
    {
        app.Logger.LogError($"{rnd.Next()}");
        await Task.Delay(rnd.Next(10,30));
    }
})).ToArray();

app.MapMemoryLoggerViewDefaultRoute(null);

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
