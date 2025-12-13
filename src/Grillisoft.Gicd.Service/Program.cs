using Grillisoft.Gicd;
using Grillisoft.Gicd.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository, Repository>();
builder.Services.AddSingleton<IStack, Stack>();
builder.Services.Configure<GithubOptions>(
    builder.Configuration.GetSection(GithubOptions.SectionName));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
