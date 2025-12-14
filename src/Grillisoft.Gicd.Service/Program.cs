using System.IO.Abstractions;
using Grillisoft.Gicd;
using Grillisoft.Gicd.Abstractions;
using Grillisoft.Gicd.Service;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("GICD_");

builder.Services.AddSingleton<IRepository, Repository>();
builder.Services.AddSingleton<IStack, Stack>();
builder.Services.Configure<GithubOptions>(
    builder.Configuration.GetSection(GithubOptions.SectionName));

builder.Services.AddSingleton<IDecryption, SopsDecrypt>();
builder.Services.AddSingleton<DeployManager>();
builder.Services.AddHostedService<DeployJob>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

builder.Services.AddSerilog(config =>
{
    config.Enrich.FromLogContext()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
        //.Enrich.WithProperty("ApplicationVersion", typeof(Program).Assembly.)
        .Enrich.WithProperty("EnvironmentName", builder.Environment.EnvironmentName)
        .WriteTo.Console()
        .WriteTo.OpenTelemetry(options =>
        {
            options.ResourceAttributes = new Dictionary<string, object>()
            {
                { "service.name", "gicd" }
            };
        });
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
