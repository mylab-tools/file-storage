using MyLab.FileStorage.Cleaner;
using MyLab.HttpMetrics;
using MyLab.Log;
using MyLab.TaskApp;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTaskLogic<CleanerTaskLogic>()
    .AddSingleton<ICleanerStrategy, FileCleanerStrategy>()
    .Configure<CleanerOptions>(builder.Configuration.GetSection("Cleaner"));

builder.Services.AddLogging(lb => lb.AddMyLabConsole())
    .AddUrlBasedHttpMetrics();

var app = builder.Build();

app.UseUrlBasedHttpMetrics();

app.UseTaskApi();

app.MapMetrics();

app.Run();

public partial class Program { }