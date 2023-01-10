using MyLab.FileStorage;
using MyLab.FileStorage.Services;
using MyLab.HttpMetrics;
using MyLab.Log;
using MyLab.WebErrors;
using Prometheus;

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

srv.AddLogging(lb => lb.AddMyLabConsole())
    .AddUrlBasedHttpMetrics();

var app = builder.Build();

app.UseUrlBasedHttpMetrics();

app.MapControllers();
app.MapMetrics();

app.Run();

public partial class Program { }