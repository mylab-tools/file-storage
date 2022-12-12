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

    public async Task<(RangeStreamReader.ReadRange[] FileReads, StoredFileMetadataDto? Metadata)> ReadContentAsync(Guid fileId, RangeHeaderValue rangeHeader)
    {
        // if file does not exists or is not confirmed
        if (!_storageOperator.IsConfirmedFileExists(fileId))
            throw new FileNotFoundException()
                .AndFactIs("file-id", fileId);

        var fileLen = _storageOperator.GetContentLength(fileId);

        if (rangeHeader.GetTotalLength(fileLen) / 1024 > _options.DownloadChunkLimitKiB)
            throw new DataTooLargeException();

        var metadata = await GetMetadataAsync(fileId);

        await using var fileStream = GetFileStream(fileId);

        var reader = new RangeStreamReader(rangeHeader);

        var reads = await reader.ReadAsync(fileStream!);

        return (reads, metadata);
    }

    public async Task<(byte[] Content, StoredFileMetadataDto? Metadata)> ReadContentAsync(Guid fileId)
    {
        // if file does not exists or is not confirmed
        if (!_storageOperator.IsConfirmedFileExists(fileId))
            throw new FileNotFoundException()
                .AndFactIs("file-id", fileId);

        var fileLen = _storageOperator.GetContentLength(fileId);

        if (fileLen / 1024 > _options.DownloadChunkLimitKiB)
            throw new DataTooLargeException();

        var metadata = await GetMetadataAsync(fileId);

        await using var fileStream = GetFileStream(fileId);

        byte[] buff = new byte[fileStream!.Length];

        var read = await fileStream.ReadAsync(buff, 0, buff.Length);

        return (buff, metadata);
    }

    private Stream GetFileStream(Guid fileId)
    {
        var fileStream = _storageOperator.OpenContentRead(fileId);

        if (fileStream == null)
        {
            throw new InvalidOperationException("Content not found")
                .AndFactIs("file-id", fileId);
        }

        return fileStream;
    }

    private async Task<StoredFileMetadataDto?> GetMetadataAsync(Guid fileId)
    {
        var metadata = await _storageOperator.ReadMetadataAsync(fileId);

        if (metadata == null)
            throw new InvalidOperationException("Metadata not found")
                .AndFactIs("file-id", fileId);
        return metadata;
    }
}