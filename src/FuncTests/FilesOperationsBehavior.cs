﻿using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient.Test;
using MyLab.FileStorage;
using MyLab.FileStorage.Client;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using Xunit.Abstractions;

namespace FuncTests
{
    public class FilesOperationsBehavior : IClassFixture<TestApi<Program, IFsFilesApiV1>>
    {
        private readonly TestApi<Program, IFsFilesApiV1> _api;
        private readonly ITestOutputHelper _output;

        public FilesOperationsBehavior(TestApi<Program, IFsFilesApiV1> api, ITestOutputHelper output)
        {
            api.Output = output;
            _api = api;
            _output = output;
        }

        [Fact]
        public async Task ShouldProvideFileMetadata()
        {
            //Arrange
            var fileId = Guid.NewGuid();

            var storageOpMock = new Mock<IStorageOperator>();
            storageOpMock
                .Setup(s => s.ReadMetadataAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Func<Guid, StoredFileMetadataDto>)(id => 
                    new StoredFileMetadataDto
                    {
                        Id = id,
                        Filename = "foo"
                    }
                ));

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

            //Act
            var file = await api.GetFileMetadataAsync(fileId);

            //Assert
            Assert.NotNull(file);
            Assert.Equal(fileId, file.Id);
            Assert.Equal("foo", file.Filename);
        }

        [Fact]
        public async Task ShouldDeleteFile()
        {
            //Arrange
            var fileId = Guid.NewGuid();

            var storageOpMock = new Mock<IStorageOperator>();

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

            //Act
            await api.DeleteFileAsync(fileId);

            //Assert
            storageOpMock.Verify(op => op.DeleteFile(fileId));
        }
    }
}