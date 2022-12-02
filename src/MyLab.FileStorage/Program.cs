using MyLab.FileStorage.Services;
using MyLab.WebErrors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers(opt => opt.AddExceptionProcessing())
    .AddNewtonsoftJson();
builder.Services.AddSingleton<IUploadService, UploadService>();
builder.Services.AddSingleton<IDownloadService, DownloadService>();
builder.Services.AddSingleton<IStorageOperator, FileStorageOperator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }