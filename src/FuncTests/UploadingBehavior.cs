using System.Net;
using System.Text;
using Moq;
using MyLab.ApiClient;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.FileStorage.Tools;
using UploadCompletionDto = MyLab.FileStorage.Client.Models.UploadCompletionDto;

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

            var api = StartApp(storageOpMock.Object);

            var uploadToken = await api.CreateNewFileAsync();

            //Act
            await api.UploadNextChunkAsync(uploadToken, filedDataChunk1);
            await api.UploadNextChunkAsync(uploadToken, filedDataChunk2);

            //Assert
            Assert.Equal(fileData, resultFileContent.ToString());
        }

        [Fact]
        public async Task ShouldCreateREsultWithMetadata()
        {
            //Arrange
            var fileData = Encoding.UTF8.GetBytes("1234567890");

            var md5 = new Md5Ex();
            md5.AppendData(fileData);
            var fileDataHash = md5.FinalHash();

            Md5Ex.Md5Context? storedMd5Context = null;

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock.Setup(s => s.WriteHashCtxAsync(It.IsAny<Guid>(), It.IsAny<Md5Ex.Md5Context>()))
                .Callback<Guid, Md5Ex.Md5Context>((guid, context) => storedMd5Context = context);
            storageOpMock.Setup(s => s.ReadHashCtxAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => storedMd5Context);
            storageOpMock.Setup(s => s.GetContentLength(It.IsAny<Guid>()))
                .Returns(() => fileData.Length);

            var api = StartApp(storageOpMock.Object);

            var uploadToken = await api.CreateNewFileAsync();

            await api.UploadNextChunkAsync(uploadToken, fileData);

            var uploadCompletion = new UploadCompletionDto
            {
                Filename = "foo.ext",
                Labels = new Dictionary<string, string>
                {
                    { "foo", "bar"},
                    { "baz", "qoz"}
                },
                Md5 = fileDataHash
            };

            FileToken? fileToken = null;

            //Act
            var uploadResult = await api.CompleteFileUploadingAsync(uploadToken, uploadCompletion);
            
            if(uploadResult.Token != null)
                fileToken = FileToken.VerifyAndDeserialize(uploadResult.Token, FileTokenSecret);

            //Assert
            Assert.NotNull(fileToken);
            Assert.NotNull(fileToken!.FileMetadata);
            Assert.Equal(uploadCompletion.Filename, fileToken.FileMetadata!.Filename);
            Assert.Equal(uploadCompletion.Md5, fileToken.FileMetadata!.Md5);
            Assert.Equal(uploadCompletion.Labels, fileToken.FileMetadata!.Labels);
            Assert.True(DateTime.Now - fileToken.FileMetadata!.Created < TimeSpan.FromSeconds(10));
            Assert.Equal(fileData.Length, fileToken.FileMetadata!.Length);

            Assert.NotNull(uploadResult.File);
            Assert.Equal(uploadCompletion.Filename, uploadResult.File!.Filename);
            Assert.Equal(uploadCompletion.Md5, uploadResult.File!.Md5);
            Assert.Equal(uploadCompletion.Labels, uploadResult.File!.Labels);
            Assert.True(DateTime.Now - uploadResult.File!.Created < TimeSpan.FromSeconds(10));
            Assert.Equal(fileData.Length, uploadResult.File!.Length);
        }

        [Fact]
        public async Task ShouldStoreInitialParameters()
        {
            //Arrange
            var fileData = Encoding.UTF8.GetBytes("1234567890");

            var md5 = new Md5Ex();
            md5.AppendData(fileData);
            var fileDataHash = md5.FinalHash();

            Md5Ex.Md5Context? storedMd5Context = null;
            StoredFileMetadataDto? storedFileMetadata = null;

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock.Setup(s => s.WriteHashCtxAsync(It.IsAny<Guid>(), It.IsAny<Md5Ex.Md5Context>()))
                .Callback<Guid, Md5Ex.Md5Context>((guid, context) => storedMd5Context = context);
            storageOpMock.Setup(s => s.ReadHashCtxAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => storedMd5Context);
            storageOpMock.Setup(s => s.GetContentLength(It.IsAny<Guid>()))
                .Returns(() => fileData.Length);
            storageOpMock.Setup(s => s.WriteMetadataAsync(It.IsAny<Guid>(), It.IsAny<StoredFileMetadataDto>()))
                .Callback<Guid, StoredFileMetadataDto>((id, metadata) => storedFileMetadata = metadata);
            storageOpMock.Setup(s => s.ReadMetadataAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => storedFileMetadata);

            var api = StartApp(storageOpMock.Object);

            var uploadCompletion = new UploadCompletionDto
            {
                Filename = "foo.ext",
                Labels = new Dictionary<string, string>
                {
                    { "foo", "bar"},
                    { "baz", "qoz"}
                },
                Md5 = fileDataHash
            };

            var newFileRequest = new MyLab.FileStorage.Client.Models.NewFileRequestDto
            {
                Purpose = "foo-purpose",
                Labels = new Dictionary<string, string>
                {
                    { "baz", "quz" }
                }
            };

            FileToken? fileToken = null;

            //Act
            var uploadToken = await api.CreateNewFileAsync(newFileRequest);

            await api.UploadNextChunkAsync(uploadToken, fileData);

            var uploadResult = await api.CompleteFileUploadingAsync(uploadToken, uploadCompletion);

            if (uploadResult.Token != null)
                fileToken = FileToken.VerifyAndDeserialize(uploadResult.Token, FileTokenSecret);

            //Assert
            Assert.NotNull(uploadResult);
            Assert.NotNull(uploadResult.File);
            Assert.NotNull(uploadResult.File!.Labels);
            Assert.NotNull(fileToken);

            Assert.Equal("foo-purpose", uploadResult.File!.Purpose);
            Assert.Equal("foo-purpose", fileToken!.FileMetadata!.Purpose);

            Assert.True(uploadResult.File.Labels!.TryGetValue("baz", out var newDtoLbl));
            Assert.Equal("quz", newDtoLbl);
            Assert.True(fileToken.FileMetadata.Labels!.TryGetValue("baz", out var newTokenLbl));
            Assert.Equal("quz", newTokenLbl);
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

            var api = StartApp(storageOpMock.Object);

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

            var api = StartApp(storageOpMock.Object);

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