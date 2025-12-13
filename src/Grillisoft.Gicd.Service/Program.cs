using Grillisoft.Gicd;
using Grillisoft.Gicd.Abstractions;
using Grillisoft.Gicd.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository, Repository>();
builder.Services.AddSingleton<IStack, Stack>();
builder.Services.Configure<GithubOptions>(
    builder.Configuration.GetSection(GithubOptions.SectionName));

builder.Services.AddSingleton<IDecryption, SopsDecrypt>();
builder.Services.AddSingleton<DeployManager>();
builder.Services.AddHostedService<DeployJob>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
