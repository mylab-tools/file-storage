using MyLab.FileStorage.Cleaner;
using MyLab.TaskApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

builder.Services.AddTaskLogic<CleanerTaskLogic>()
    .Configure<CleanerOptions>(builder.Configuration.GetSection("Cleaner"));

// Configure the HTTP request pipeline.

app.UseTaskApi();

app.Run();