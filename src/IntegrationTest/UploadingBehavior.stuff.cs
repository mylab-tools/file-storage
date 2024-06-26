using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.Log.XUnit;
using Xunit.Abstractions;

namespace IntegrationTest;

public partial class UploadingBehavior :
    IClassFixture<TestApi<Program, IFsUploadApiV1>>,
    IClassFixture<TestApi<Program, IFsFilesApiV1>>,
    IDisposable
{
    private readonly IFsUploadApiV1 _uploadApi;
    private readonly byte[] _fileData;
    private readonly byte[] _fileDataHash;
    private readonly IFsFilesApiV1 _fileApi;
    private const string TransferTokenSecret = "1234567890123456";
    private const string FileTokenSecret = "6543210987654321";

    private Guid _curTestFid;

    public UploadingBehavior(
        TestApi<Program, IFsUploadApiV1> uploadApi,
        TestApi<Program, IFsFilesApiV1> fileApi,
        ITestOutputHelper output)
    {

        uploadApi.Output = output;
        fileApi.Output = output;

        _fileData = new byte[300];
        new Random().NextBytes(_fileData);

        var md5 = MD5.Create();
        _fileDataHash = md5.ComputeHash(_fileData);

        TestStuff.TouchDataDir();

        _uploadApi = uploadApi.StartWithProxy(srv => srv.AddLogging(lb => lb
                .ClearProviders()
                .AddFilter(l => true)
                .AddXUnit(output)
            )
            .Configure<FsOptions>(opt =>
            {
                opt.TransferTokenSecret = TransferTokenSecret;
                opt.FileTokenSecret = FileTokenSecret;
                opt.Directory = TestStuff.DataDir;
            }));

        _fileApi = fileApi.StartWithProxy(srv => srv.AddLogging(lb => lb
                .ClearProviders()
                .AddFilter(l => true)
                .AddXUnit(output)
            )
            .Configure<FsOptions>(opt =>
            {
                opt.TransferTokenSecret = TransferTokenSecret;
                opt.FileTokenSecret = FileTokenSecret;
                opt.Directory = TestStuff.DataDir;
            }));
    }

    public void Dispose()
    {
        if(_curTestFid != default)
            TestStuff.DeleteFileDataDir(_curTestFid);
    }
}