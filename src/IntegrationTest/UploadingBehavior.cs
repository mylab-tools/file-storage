using MyLab.FileStorage.Client.Models;
using MyLab.FileStorage.Tools;
using Newtonsoft.Json;

namespace IntegrationTest
{
    public partial class UploadingBehavior 
    {
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
    }
}