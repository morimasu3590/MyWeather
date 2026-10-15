using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;
using MyWeather.WebApi;
using Scalar.AspNetCore;

// Local secrets (e.g. OpenWeather__ApiKey) come from the gitignored .env in the working directory
// (MyWeather.WebApi for `dotnet run` and F5). It is loaded as environment variables before the builder
// reads configuration, and NoClobber keeps any variable that is already set, so real environment
// variables and command-line args win over .env. A missing file is ignored.
DotNetEnv.Env.NoClobber().Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
InfraConfiguration.ConfigureServices(builder.Services, builder.Configuration, builder.Environment.ContentRootPath);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Apply pending EF Core migrations. Other environments run `dotnet ef database update` instead.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MyWeatherDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("MyWeather API");
    });
}

app.UseHttpsRedirection();

app.MapCountriesEndpointBuilder();
app.MapWeatherForecastEndpointBuilder();
app.MapFavoriteCitiesEndpointBuilder();

app.Run();

