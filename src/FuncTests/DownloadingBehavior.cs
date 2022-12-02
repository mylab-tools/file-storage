using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using Xunit.Abstractions;

namespace FuncTests;

public class DownloadingBehavior : IClassFixture<TestApi<Program, IFsDownloadApiV1>>
{
    private const string FileData = "foobarbaz";
    
    private readonly IFsDownloadApiV1 _api;
    private readonly ITestOutputHelper _output;

    private Stream _fileStream;
    private Guid _fileId;
    private readonly StoredFileMetadataDto _fileMetadata;
    private readonly Mock<IStorageOperator> _storageOpMock;

    public DownloadingBehavior(TestApi<Program, IFsDownloadApiV1> api, ITestOutputHelper output)
    {
        api.Output = output;
        _output = output;

        var fileDataBin = Encoding.UTF8.GetBytes(FileData);
        _fileStream = new MemoryStream(fileDataBin);
        _fileId = Guid.NewGuid();

        _fileMetadata = new StoredFileMetadataDto
        {
            Filename = "foo.ext"
        };

        _storageOpMock = new Mock<IStorageOperator>();
        _storageOpMock.Setup(op => op.OpenContentRead(It.Is<Guid>(id => id == _fileId)))
            .Returns(_fileStream);
        _storageOpMock.Setup(op => op.ReadMetadataAsync(It.Is<Guid>(id => id == _fileId)))
            .ReturnsAsync(_fileMetadata);

        _api = api.StartWithProxy(srv => srv
            .AddSingleton(_storageOpMock.Object)
            .AddLogging(lb => lb
                .ClearProviders()
                .AddFilter(l => true)
                .AddXUnit(_output)
            )
            .Configure<FsOptions>(opt =>
            {
                opt.TokenSecret = "1234567890123456";
            })
        );
    }

    [Fact]
    public async Task ShouldDownloadFullFileByToken()
    {
        //Arrange
        var token = await _api.CreateDownloadTokenAsync(_fileId);

        //Act
        var downloadedFileData = await _api.DownloadByToken(token);

        //Assert
        Assert.NotNull(downloadedFileData);
        Assert.Equal("foobarbaz", Encoding.UTF8.GetString(downloadedFileData));
    }

    [Theory]
    [MemberData(nameof(GetRangeCases))]
    public async Task ShouldDownloadPartialFileByToken(RangeItemHeaderValue[] rangeItems, string expectedResult)
    {
        //Arrange
        _fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();

        foreach (var item in rangeItems)
            rangeHeader.Ranges.Add(item);

        var token = await _api.CreateDownloadTokenAsync(_fileId);

        //Act
        var downloadedFileData = await _api.DownloadByToken(token, rangeHeader);

        //Assert
        Assert.NotNull(downloadedFileData);
        Assert.Equal(expectedResult, Encoding.UTF8.GetString(downloadedFileData));
    }

    [Fact]
    public async Task ShouldNotDownloadMultiplePartialFileByToken()
    {
        //Arrange
        _fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));

        var token = await _api.CreateDownloadTokenAsync(_fileId);

        //Act && Assert
        var e = await Assert.ThrowsAsync<ResponseCodeException>(() => _api.DownloadByToken(token, rangeHeader));
        Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        Assert.Contains("not supported", e.ServerMessage);
    }

    [Fact]
    public async Task ShouldDownloadFullFileById()
    {
        //Arrange

        //Act
        var downloadedFileData = await _api.DownloadByFileId(_fileId);

        //Assert
        Assert.NotNull(downloadedFileData);
        Assert.Equal("foobarbaz", Encoding.UTF8.GetString(downloadedFileData));
    }

    [Theory]
    [MemberData(nameof(GetRangeCases))]
    public async Task ShouldDownloadPartialFileById(RangeItemHeaderValue[] rangeItems, string expectedResult)
    {
        //Arrange
        _fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();

        foreach (var item in rangeItems)
            rangeHeader.Ranges.Add(item);

        //Act
        var downloadedFileData = await _api.DownloadByFileId(_fileId, rangeHeader);

        //Assert
        Assert.NotNull(downloadedFileData);
        Assert.Equal(expectedResult, Encoding.UTF8.GetString(downloadedFileData));
    }

    [Fact]
    public async Task ShouldNotDownloadMultiplePartialFileById()
    {
        //Arrange
        _fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));

        //Act && Assert
        var e = await Assert.ThrowsAsync<ResponseCodeException>(() => _api.DownloadByFileId(_fileId, rangeHeader));
        Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        Assert.Contains("not supported", e.ServerMessage);
    }

    public static IEnumerable<object[]> GetRangeCases()
    {
        return new[]
        {
            new object[]
            {
                new RangeItemHeaderValue[]
                {
                    new(3, 5)
                },
                "bar"
            },
            new object[]
            {
                new RangeItemHeaderValue[]
                {
                    new(null, 5)
                },
                "arbaz"
            },
            new object[]
            {
                new RangeItemHeaderValue[]
                {
                    new(3, null)
                },
                "barbaz"
            },
            new object[]
            {
                new RangeItemHeaderValue[]
                {
                    new(3, 20)
                },
                "barbaz"
            }
        };
    }
}