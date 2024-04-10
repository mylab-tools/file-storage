using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.Log.XUnit;
using Xunit.Abstractions;

namespace IntegrationTest
{
    public class FilesBehavior : IClassFixture<TestApiFixture<Program, IFsFilesApiV1>>, IDisposable
    {
        private const string TransferTokenSecret = "1234567890123456";
        private const string FileTokenSecret = "6543210987654321";

        private readonly Guid _curTestFid = Guid.NewGuid();
        private readonly TestApiFixture<Program, IFsFilesApiV1> _fxt;
        private readonly ITestOutputHelper _output;

        public FilesBehavior(TestApiFixture<Program, IFsFilesApiV1> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;
            _fxt = fxt;
            _output = output;
        }

        [Fact]
        public async Task ShouldOverrideTtlWhenConfirm()
        {
            //Arrange
            var filesApi = _fxt.StartWithProxy(srv => srv.AddLogging(lb => lb
                    .ClearProviders()
                    .AddFilter(_ => true)
                    .AddXUnit(_output)
                )
                .Configure<FsOptions>(opt =>
                {
                    opt.TransferTokenSecret = TransferTokenSecret;
                    opt.FileTokenSecret = FileTokenSecret;
                    opt.Directory = TestStuff.DataDir;
                }));

            var storageOperator = filesApi.ServiceProvider.GetRequiredService<IStorageOperator>();

            await storageOperator.TouchBaseDirectoryAsync(_curTestFid);
            await storageOperator.WriteMetadataAsync(_curTestFid, new StoredFileMetadataDto
            {
                TtlHours = 10
            });

            //Act
            await filesApi.ApiClient.ConfirmFileAsync(_curTestFid, 20);

            var actualMeta = await storageOperator.ReadMetadataAsync(_curTestFid);

            //Assert
            Assert.NotNull(actualMeta);
            Assert.Equal(20, actualMeta.TtlHours);
        }

        public void Dispose()
        {
            if (_curTestFid != default)
                TestStuff.DeleteFileDataDir(_curTestFid);
        }
    }
}
