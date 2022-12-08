using MyLab.FileStorage.Cleaner;
using MyLab.TaskApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTaskLogic<CleanerTaskLogic>()
    .AddSingleton<ICleanerStrategy, FileCleanerStrategy>()
    .Configure<CleanerOptions>(builder.Configuration.GetSection("Cleaner"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseTaskApi();

app.Run();

public partial class Program { }