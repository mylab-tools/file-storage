using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace IntegrationTest
{
    public class DownloadingBehavior :
        IClassFixture<TestApi<Program, IFsDownloadApiV1>>,
        IDisposable
    {
        private readonly byte[] _fileData;
        private readonly byte[] _fileDataHash;
        private readonly IFsDownloadApiV1 _downloadApi;
        private readonly Guid _fid;
        private const string TransferTokenSecret = "1234567890123456";
        private const string FileTokenSecret = "6543210987654321";

        public DownloadingBehavior(
            TestApi<Program, IFsDownloadApiV1> downloadApi,
            ITestOutputHelper output)
        {
            downloadApi.Output = output;

            _fileData = new byte[300];
            new Random().NextBytes(_fileData);

            var md5 = MD5.Create();
            _fileDataHash = md5.ComputeHash(_fileData);

            if (Directory.Exists("test-data"))
                Directory.Delete("test-data", true);
            Directory.CreateDirectory("test-data");

            _fid = Guid.NewGuid();

            var fidNameConverter = new FileIdToNameConverter("test-data");
            var dirName = fidNameConverter.ToDirectory(_fid);
            
            Directory.CreateDirectory(dirName);

            var contentFn = fidNameConverter.ToContentFile(_fid);
            File.WriteAllBytes(contentFn, _fileData);

            var fileMetadata = new StoredFileMetadataDto
            {
                Id = _fid,
                Length = _fileData.Length,
                Filename = "foo.ext",
                Created = DateTime.Now,
                Md5 = _fileDataHash,
                Purpose = "test"
            };

            var metaJson = JsonConvert.SerializeObject(fileMetadata);
            var metaFn = fidNameConverter.ToMetadataFile(_fid);
            File.WriteAllText(metaFn, metaJson);

            var confirmFn = fidNameConverter.ToConfirmFile(_fid);
            File.WriteAllText(confirmFn, DateTime.Now.ToString("O"));

            _downloadApi = downloadApi.StartWithProxy(srv => srv.AddLogging(lb => lb
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
        public async Task ShouldDownload()
        {
            //Arrange
            const int maxChunkSize = 100;
            byte[] resultBuff = new byte[_fileData.Length];

            //Act
            for (int i = 0; i* maxChunkSize < resultBuff.Length; i++)
            {
                int chunkSize = Math.Min(maxChunkSize, _fileData.Length - maxChunkSize);

                var fileChunk = await _downloadApi.DownloadByFileId(_fid, new RangeHeaderValue(i* maxChunkSize, i * maxChunkSize+chunkSize));

                fileChunk.CopyTo(resultBuff, i * maxChunkSize);
            }

            //Assert
            Assert.Equal(_fileData, resultBuff);
        }

        public void Dispose()
        {
            if (Directory.Exists("test-data"))
                Directory.Delete("test-data", true);
        }
    }
}
