using MyLab.FileStorage;
using MyLab.FileStorage.Services;
using MyLab.WebErrors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var srv = builder.Services;

srv
    .AddControllers(opt => opt.AddExceptionProcessing())
    .AddNewtonsoftJson();

srv.AddSingleton<IUploadService, UploadService>()
    .AddSingleton<IDownloadService, DownloadService>()
    .AddSingleton<IStorageOperator, FileStorageOperator>()
    .Configure<FsOptions>(builder.Configuration.GetSection("FS"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }