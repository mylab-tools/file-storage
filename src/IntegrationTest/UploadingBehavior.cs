using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Client.Models;
using MyLab.FileStorage.Tools;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace IntegrationTest
{
    public class UploadingBehavior : 
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

            if (Directory.Exists("test-data"))
            {
                Directory.Delete("test-data", true);
            }

            Directory.CreateDirectory("test-data");

            _uploadApi = uploadApi.StartWithProxy(srv => srv.AddLogging(lb => lb
                    .ClearProviders()
                    .AddFilter(l => true)
                    .AddXUnit(output)
                )
                .Configure<FsOptions>(opt =>
                {
                    opt.TransferTokenSecret = TransferTokenSecret;
                    opt.FileTokenSecret = FileTokenSecret;
                    opt.Directory = "test-data";
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
                    opt.Directory = "test-data";
                }));
        }

        [Fact]
        public async Task ShouldUploadFile()
        {
            //Arrange
            var newFileReq = new NewFileRequestDto
            {
                Purpose = "test"
            };

            int maxChunkSize = 100;

            var uploadCompletion = new UploadCompletionDto
            {
                Filename = "foo.ext",
                Md5 = _fileDataHash
            };

            var fnConverter = new FileIdToNameConverter("test-data");

            //Act
            var uploadToken = await _uploadApi.CreateNewFileAsync(newFileReq);

            for (int i = 0; i* maxChunkSize < _fileData.Length; i++)
            {
                int chunkSize = Math.Min(maxChunkSize, _fileData.Length- maxChunkSize);

                byte[] buff = new byte[chunkSize];

                Array.Copy(_fileData, i*chunkSize, buff, 0, chunkSize);

                await _uploadApi.UploadNextChunkAsync(uploadToken, buff);
            }

            var newFile = await _uploadApi.CompleteFileUploadingAsync(uploadToken, uploadCompletion);

            if (newFile.File != null)
            {
                await _fileApi.ConfirmFileAsync(newFile.File.Id);
            }

            var uDataFn = fnConverter.ToContentFile(newFile.File!.Id);
            byte[]? resData = null;

            if (File.Exists(uDataFn))
            {
                resData = await File.ReadAllBytesAsync(uDataFn);
            }


            var uMetaFn = fnConverter.ToMetadataFile(newFile.File!.Id);
            StoredFileMetadataDto? metadataDto = null;

            if (File.Exists(uMetaFn))
            {
                var strMeta = await File.ReadAllTextAsync(uMetaFn);
                metadataDto = JsonConvert.DeserializeObject<StoredFileMetadataDto>(strMeta);
            }

            var uConfirmFn = fnConverter.ToConfirmFile(newFile.File!.Id);
            DateTime confirmationDt = default;

            if (File.Exists(uConfirmFn))
            {
                var strDt = await File.ReadAllTextAsync(uConfirmFn);
                DateTime.TryParse(strDt, out confirmationDt);
            }

            //Assert
            Assert.NotNull(newFile);
            Assert.Equal(_fileData.Length, newFile.File.Length);
            Assert.Equal("foo.ext", newFile.File.Filename);
            Assert.Equal("test", newFile.File.Purpose);
            Assert.Null(newFile.File.Labels);
            Assert.Equal(_fileDataHash, newFile.File.Md5);

            Assert.NotNull(resData);
            Assert.Equal(_fileData, resData);

            Assert.NotNull(metadataDto);
            Assert.Equal(newFile.File.Id, metadataDto!.Id);
            Assert.Equal(_fileData.Length, metadataDto.Length);
            Assert.Equal("foo.ext", metadataDto.Filename);
            Assert.Equal("test", metadataDto.Purpose);
            Assert.Null(metadataDto.Labels);
            Assert.Equal(_fileDataHash, metadataDto.Md5);

            Assert.NotEqual(default, confirmationDt);
        }

        public void Dispose()
        {
            if (Directory.Exists("test-data"))
            {
                Directory.Delete("test-data", true);
            }
        }
    }
}