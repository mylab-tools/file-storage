using MyLab.ApiClient.Test;
using MyLab.FileStorage.Client;
using Xunit.Abstractions;

namespace FuncTests;

public partial class UploadingBehavior : IClassFixture<TestApi<Program, IFsUploadApiV1>>
{
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
}