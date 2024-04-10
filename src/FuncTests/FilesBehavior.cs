using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.Log.XUnit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class FilesBehavior : IClassFixture<TestApiFixture<Program, IFsFilesApiV1>>
    {
        private readonly TestApiFixture<Program, IFsFilesApiV1> _fxt;
        private readonly ITestOutputHelper _output;
        private const string TransferTokenSecret = "1234567890123456";
        private const string FileTokenSecret = "6543210987654321";

        public FilesBehavior(TestApiFixture<Program, IFsFilesApiV1> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;
            _fxt = fxt;
            _output = output;
        }

        [Fact]
        public async Task ShouldApplyTtlWenConfirm()
        {
            //Arrange
            StoredFileMetadataDto initialMeta = new StoredFileMetadataDto
            {
                TtlHours = 10
            };
            Guid fileId = Guid.NewGuid();

            StoredFileMetadataDto? resultMeta = null;
            DateTime confirmDt = default;
            Guid confirmId = default;

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock.Setup(s => s.WriteConfirmedFile(It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .Callback<Guid,DateTime>((guid, dt) =>
                {
                    confirmId = guid;
                    confirmDt = dt;
                });
            storageOpMock.Setup(s => s.WriteMetadataAsync(It.IsAny<Guid>(), It.IsAny<StoredFileMetadataDto>()))
                .Callback<Guid, StoredFileMetadataDto>((id, meta) => resultMeta = meta);
            storageOpMock.Setup(s => s.ReadMetadataAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => initialMeta!);


            var filesApi = _fxt.StartWithProxy(srv => srv
                .AddLogging(l => l
                    .ClearProviders()
                    .AddFilter(_ => true)
                    .AddXUnit(_output)
                )
                .AddSingleton(storageOpMock.Object)
                .Configure<FsOptions>(opt =>
                    {
                        opt.TransferTokenSecret = TransferTokenSecret;
                        opt.FileTokenSecret = FileTokenSecret;
                    })
            );

            //Act
            await filesApi.ApiClient.ConfirmFileAsync(fileId, 20);

            //Assert
            Assert.Equal(fileId, confirmId);
            Assert.NotEqual(default, confirmDt);
            Assert.NotNull(resultMeta);
            Assert.Equal(20, resultMeta!.TtlHours);
        }
    }
}
