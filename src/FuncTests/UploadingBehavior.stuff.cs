using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Services;
using MyLab.Log.XUnit;
using Xunit.Abstractions;

namespace FuncTests;

public partial class UploadingBehavior : IClassFixture<TestApi<Program, IFsUploadApiV1>>
{
    private const string TransferTokenSecret = "1234567890123456";
    private const string FileTokenSecret = "6543210987654321";

    private readonly TestApi<Program, IFsUploadApiV1> _api;
    private readonly ITestOutputHelper _output;

    public UploadingBehavior(TestApi<Program, IFsUploadApiV1> api, ITestOutputHelper output)
    {
        api.Output = output;
        _api = api;
        _output = output;
    }

    private byte[] HexToBytes(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }

    IFsUploadApiV1 StartApp(IStorageOperator storageOperator)
    {
        return _api.StartWithProxy(srv => srv
            .AddSingleton(storageOperator)
            .AddLogging(lb => lb
                .ClearProviders()
                .AddFilter(l => true)
                .AddXUnit(_output)
            )
            .Configure<FsOptions>(opt =>
            {
                opt.TransferTokenSecret = TransferTokenSecret;
                opt.FileTokenSecret = FileTokenSecret;
            })
        );
    }

}