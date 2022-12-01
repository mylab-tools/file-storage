using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient;
using MyLab.FileStorage;
using MyLab.FileStorage.Client.Models;
using MyLab.FileStorage.Services;
using MyLab.FileStorage.Tools;

namespace FuncTests
{
    public partial class UploadingBehavior
    {
        [Fact]
        public async Task ShouldUploadFileDataWithChunks()
        {
            //Arrange
            var resultFileContent = new StringBuilder();

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock
                .Setup(s => s.AppendContentAsync(It.IsAny<Guid>(), It.IsAny<byte[]>()))
                .Callback<Guid, byte[]>((s, data) =>
                {
                    var str = Encoding.UTF8.GetString(data);
                    resultFileContent.Append(str);
                });

            const string fileData = "1234567890";

            var filedDataChunk1 = Encoding.UTF8.GetBytes(fileData.Substring(0, 5));
            var filedDataChunk2 = Encoding.UTF8.GetBytes(fileData.Substring(5, 5));

            var api = _api.StartWithProxy(srv => srv
                .AddSingleton(storageOpMock.Object)
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

            var uploadToken = await api.CreateNewFileAsync();

            //Act
            await api.UploadNextChunkAsync(uploadToken, filedDataChunk1);
            await api.UploadNextChunkAsync(uploadToken, filedDataChunk2);

            //Assert
            Assert.Equal(fileData, resultFileContent.ToString());
        }

        [Theory]
        [InlineData("e807f1fcf82d132f9bb018ca6738a19f", true)]
        [InlineData("00000000000000000000000000000000", false)]
        public async Task ShouldControlFileChecksum(string controlMd5Str, bool successExpected)
        {
            //Arrange
            var fileData = Encoding.UTF8.GetBytes("1234567890");
            var controlMd5 = HexToBytes(controlMd5Str);

            var md5 = new Md5Ex();
            md5.AppendData(fileData);
            var fileMd5Ctx = new Md5Ex.Md5Context(md5.Context);

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock
                .Setup(s => s.ReadHashCtxAsync(It.IsAny<Guid>()))
                .ReturnsAsync(fileMd5Ctx);

            var api = _api.StartWithProxy(srv => srv
                .AddSingleton(storageOpMock.Object)
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

            var uploadToken = await api.CreateNewFileAsync();

            //Meaning await api.UploadNextChunkAsync(uploadToken, fileData);

            var uploadCompletion = new UploadCompletionDto
            {
                Md5 = controlMd5,
                Filename = "foo"
            };

            ResponseCodeException? responseCodeException = null;

            //Act
            try
            {
                await api.CompleteFileUploadingAsync(uploadToken, uploadCompletion);
            }
            catch (ResponseCodeException e)
            {
                responseCodeException = e;
            }

            //Assert
            if (successExpected)
            {
                Assert.Null(responseCodeException);
            }
            else
            {
                Assert.NotNull(responseCodeException);
                Assert.Equal(HttpStatusCode.Conflict, responseCodeException!.StatusCode);
            }
        }
        
        [Fact]
        public async Task ShouldMakeFileArtifacts()
        {
            //Arrange
            const string fileData = "1234567890";
            const string fileDataHash = "e807f1fcf82d132f9bb018ca6738a19f";

            var fileDataHashBin = HexToBytes(fileDataHash);
            var fileDataBin = Encoding.UTF8.GetBytes(fileData);
            
            Md5Ex.Md5Context? fileMd5Ctx = null;

            var storageOpMock = new Mock<IStorageOperator>();

            storageOpMock
                .Setup(s => s.WriteHashCtxAsync(It.IsAny<Guid>(), It.IsAny<Md5Ex.Md5Context>()))
                .Callback<Guid, Md5Ex.Md5Context>((guid, ctx) =>
                {
                    fileMd5Ctx = ctx;
                });

            storageOpMock
                .Setup(s => s.ReadHashCtxAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => fileMd5Ctx);
            
            var api = _api.StartWithProxy(srv => srv
                .AddSingleton(storageOpMock.Object)
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

            var uploadToken = await api.CreateNewFileAsync();

            await api.UploadNextChunkAsync(uploadToken, fileDataBin);

            var uploadCompletion = new UploadCompletionDto
            {
                Md5 = fileDataHashBin,
                Filename = "foo"
            };

            //Act
            var newFile = await api.CompleteFileUploadingAsync(uploadToken, uploadCompletion);

            //Assert
            Assert.NotNull(newFile);
            Assert.NotNull(newFile.File);
            storageOpMock.Verify(s => s.AppendContentAsync(newFile.File!.Id, fileDataBin));
            storageOpMock.Verify(s => s.DeleteHashCtxAsync(newFile.File!.Id));
            storageOpMock.Verify(s => s.WriteMetadataAsync(newFile.File!.Id, It.Is <MyLab.FileStorage.Models.StoredFileMetadataDto>(dto => 
                dto.Filename == "foo" && 
                dto.Labels == null && 
                dto.Id == newFile.File!.Id &&  
                dto.Md5 != null &&
                dto.Md5.SequenceEqual(fileDataHashBin)
                )));
        }
    }
}