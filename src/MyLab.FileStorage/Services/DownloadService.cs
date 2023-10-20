using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using MyLab.Log;

namespace MyLab.FileStorage.Services;

class DownloadService : IDownloadService
{
    private readonly IStorageOperator _storageOperator;
    private readonly FsOptions _options;

    public DownloadService(IStorageOperator storageOperator, IOptions<FsOptions> options)
    {
        _storageOperator = storageOperator;
        _options = options.Value;
    }
    public string CreateDownloadToken(Guid fileId)
    {
        // if file does not exists or is not confirmed
        if (!_storageOperator.IsConfirmedFileExists(fileId))
            throw new FileNotFoundException()
                .AndFactIs("file-id", fileId);

        return new TransferToken(fileId)
            .Serialize(_options.TransferTokenSecret!, TimeSpan.FromSeconds(_options.DownloadTokenTtlSec));
    }

    public async Task<ReadFile> ReadFileAsync(Guid fileId)
    {
        var metadata = await GetMetadataAsync(fileId);

        var fileStream = _storageOperator.OpenContentRead(fileId);

        var md5Str = metadata.Md5 != null 
            ? BitConverter.ToString(metadata.Md5).Replace("-", "")
            : null;

        return new ReadFile(fileStream)
        {
            Created = metadata.Created,
            Filename = metadata.Filename,
            Md5 = md5Str,
        };
    }

    private async Task<StoredFileMetadataDto> GetMetadataAsync(Guid fileId)
    {
        var metadata = await _storageOperator.ReadMetadataAsync(fileId);

        if (metadata == null)
            throw new InvalidOperationException("Metadata not found")
                .AndFactIs("file-id", fileId);
        return metadata;
    }
}