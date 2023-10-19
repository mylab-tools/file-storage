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
using Xunit;
using Xunit.Abstractions;

namespace FuncTests;

public class DownloadingBehavior : IClassFixture<TestApi<Program, IFsDownloadApiV1>>
{
    private readonly ITestOutputHelper _output;
    private const string FileData = "foobarbaz";
    
    private readonly IFsDownloadApiV1 _api;

    private readonly Stream _fileStream;
    private readonly Guid _fileId;

    public DownloadingBehavior(TestApi<Program, IFsDownloadApiV1> api, ITestOutputHelper output)
    {
        _output = output;
        api.Output = output;

        var fileDataBin = Encoding.UTF8.GetBytes(FileData);
        _fileStream = new MemoryStream(fileDataBin);
        _fileId = Guid.NewGuid();

        var fileMetadata = new StoredFileMetadataDto
        {
            Filename = "foo.ext"
        };

        var storageOpMock = new Mock<IStorageOperator>();
        storageOpMock.Setup(op => op.OpenContentRead(It.Is<Guid>(id => id == _fileId)))
            .Returns(_fileStream);
        storageOpMock.Setup(op => op.ReadMetadataAsync(It.Is<Guid>(id => id == _fileId)))
            .ReturnsAsync(fileMetadata);
        storageOpMock.Setup(op => op.IsConfirmedFileExists(It.Is<Guid>(id => id == _fileId)))
            .Returns(true);

        _api = api.StartWithProxy(srv => srv
            .AddSingleton(storageOpMock.Object)
            .AddLogging(lb => lb
                .ClearProviders()
                .AddFilter(l => true)
                .AddXUnit(output)
            )
            .Configure<FsOptions>(opt =>
            {
                opt.TransferTokenSecret = "1234567890123456";
                opt.FileTokenSecret = "6543210987654321";
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
        //_fileStream.Seek(0, SeekOrigin.Begin);

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
        //_fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));

        var token = await _api.CreateDownloadTokenAsync(_fileId);

        //Act && Assert
        var e = await Assert.ThrowsAsync<ResponseCodeException>(() => _api.DownloadByToken(token, rangeHeader));
        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, e.StatusCode);
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
        //_fileStream.Seek(0, SeekOrigin.Begin);

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
        //_fileStream.Seek(0, SeekOrigin.Begin);

        var rangeHeader = new RangeHeaderValue();
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 1));

        //Act && Assert
        var e = await Assert.ThrowsAsync<ResponseCodeException>(() => _api.DownloadByFileId(_fileId, rangeHeader));
        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, e.StatusCode);
        Assert.Contains("not supported", e.ServerMessage);
    }

    [Fact]
    public async Task ShouldNot416WhenPartiallyOutOfRangeDownload()
    {
        //Arrange
        byte[]? receivedFileChunk = null;
        ResponseCodeException? exception416 = null;

        //Act
        try
        {
            receivedFileChunk = await _api.DownloadByFileId(_fileId, new RangeHeaderValue(_fileStream.Length -1, _fileStream.Length + 10));
        }
        catch(ResponseCodeException e) when(e.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            exception416 = e;
        }

        //Assert
        Assert.Null(exception416);
        Assert.NotNull(receivedFileChunk);
        Assert.Single(receivedFileChunk);
    }

    [Fact]
    public async Task Should416WhenFullOutOfRangeDownload()
    {
        //Arrange
        ResponseCodeException? exception416 = null;

        //Act
        try
        {
            await _api.DownloadByFileId(_fileId, new RangeHeaderValue(_fileStream.Length + 1, _fileStream.Length + 10));
        }
        catch(ResponseCodeException e) when(e.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            exception416 = e;
        }

        //Assert
        Assert.NotNull(exception416);
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

    [Fact]
    public void ShouldValidContract()
    {
        //Arrange
        var apiContractValidator = new ApiContractValidator
        {
            ContractKeyMustBeSpecified = true
        };

        //Act
        var validationResult = apiContractValidator.Validate(typeof(IFsDownloadApiV1));

        if (!validationResult.Success)
        {
            _output.WriteLine("Errors:");

            for (int i = 0; i < validationResult.Count; i++)
            {
                _output.WriteLine($"{i+1}. {validationResult[i].Reason}");
            }
        }

        //Assert
        Assert.True(validationResult.Success);
    }
}