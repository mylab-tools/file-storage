using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.Options;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using MyLab.Log;

namespace MyLab.FileStorage.Services;

class UploadService : IUploadService
{
    private readonly IStorageOperator _operator;
    private readonly FsOptions _options;

    public UploadService(IStorageOperator @operator, IOptions<FsOptions> options)
    {
        _operator = @operator;
        _options = options.Value;
    }

    public string CreateUploadToken()
    {
        var uploadToken = TransferToken.New();

        return uploadToken.Serialize(_options.TokenSecret!, TimeSpan.FromSeconds(_options.UploadTokenTtlSec));
    }

    public async Task AppendFileData(Guid fileId, PipeReader pipeReader, int length)
    {
        if (length / 1024 > _options.UploadChunkLimitKBytes)
            throw new DataTooLargeException();

        if (_options.StoredFileSizeLimitMBytes.HasValue)
        {
            var existentFileLen = _operator.GetContentLength(fileId);

            if ((existentFileLen + length) / (1024*1024) > _options.StoredFileSizeLimitMBytes.Value)
                throw new FileTooLargeException()
                    .AndFactIs("file-id", fileId);
        }

        var chunk = await ReadDataFromPipe(pipeReader, length);

        await _operator.TouchBaseDirectoryAsync(fileId);

        await _operator.AppendContentAsync(fileId, chunk);

        var hashCtx = await _operator.ReadHashCtxAsync(fileId);

        var hash = hashCtx != null
            ? new Md5Ex(hashCtx)
            : new Md5Ex();

        hash.AppendData(chunk);

        await _operator.WriteHashCtxAsync(fileId, hash.Context);
    }

    public async Task<NewFileDto> CompleteFileCreation(Guid fileId, UploadCompletionDto completion)
    {
        if (completion.Md5 == null)
            throw new BadChecksumException();

        bool fileHashOk = await CheckAndDeleteMd5Async(fileId, completion.Md5);

        if (!fileHashOk)
            throw new BadChecksumException();

        var metadata = new StoredFileMetadataDto
        {
            Md5 = completion.Md5,
            Filename = completion.Filename,
            Id = fileId,
            Labels = completion.Labels
        };
        
        await _operator.WriteMetadataAsync(fileId, metadata);

        var docToken = new DocumentToken(metadata);

        return new NewFileDto
        {
            File = metadata,
            Token = docToken.Serialize(_options.TokenSecret!, TimeSpan.FromSeconds(_options.DocTokenTtlSec))
        };
    }

    async Task<bool> CheckAndDeleteMd5Async(Guid fileId, byte[] controlMd5)
    {
        var hashCtx = await _operator.ReadHashCtxAsync(fileId);

        if (hashCtx == null)
            throw new InvalidOperationException("Hash context not found")
                .AndFactIs("file-id", fileId.ToString("N"));

        var storedFileMd5 = new Md5Ex(hashCtx);
        var hash = storedFileMd5.FinalHash();

        bool success = hash.SequenceEqual(controlMd5);

        if (success)
        {
            await _operator.DeleteHashCtxAsync(fileId);
        }

        return success;
    }

    private async Task<byte[]> ReadDataFromPipe(PipeReader reader, int contentLength)
    {
        var readTimeout = TimeSpan.FromSeconds(30);
        var cts = new CancellationTokenSource(readTimeout);
        var readResult = await reader.ReadAtLeastAsync(contentLength, cts.Token);

        if (readResult.Buffer.Length != contentLength)
        {
            throw new InvalidOperationException($"Cant read input stream in {readTimeout}");
        }

        return readResult.Buffer.ToArray();
    }
}