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

    public string CreateUploadToken(Guid fileId)
    {
        var uploadToken = new TransferToken(fileId);

        return uploadToken.Serialize(_options.TransferTokenSecret!, TimeSpan.FromSeconds(_options.UploadTokenTtlSec));
    }

    public async Task<Guid> CreateNewFileAsync(NewFileRequestDto? newFileRequest)
    {
        Guid fileId = Guid.NewGuid();
        var metadata = new StoredFileMetadataDto
        {
            Id = fileId,
            Created = DateTime.Now,
            Labels = newFileRequest?.Labels,
            Purpose = newFileRequest?.Purpose
        };

        await _operator.TouchBaseDirectoryAsync(fileId);
        await _operator.WriteMetadataAsync(fileId, metadata);

        return fileId;
    }

    public async Task AppendFileData(Guid fileId, PipeReader pipeReader, int length)
    {
        if (length / 1024 > _options.UploadChunkLimitKiB)
            throw new DataTooLargeException();

        if (_options.StoredFileSizeLimitMiB.HasValue)
        {
            var existentFileLen = _operator.GetContentLength(fileId);

            if ((existentFileLen + length) / (1024*1024) > _options.StoredFileSizeLimitMiB.Value)
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

        var length = _operator.GetContentLength(fileId);

        var initialMetadata = await _operator.ReadMetadataAsync(fileId);
        
        var metadata = new StoredFileMetadataDto
        {
            Id = fileId,
            Md5 = completion.Md5,
            Filename = completion.Filename,
            Labels = JoinLabels(initialMetadata?.Labels, completion.Labels),
            Length = length,
            Created = initialMetadata != null
                ? initialMetadata.Created
                : DateTime.Now,
            Purpose = initialMetadata?.Purpose
        };
        
        await _operator.WriteMetadataAsync(fileId, metadata);

        var docToken = new FileToken(metadata);

        return new NewFileDto
        {
            File = metadata,
            Token = docToken.Serialize(_options.FileTokenSecret!, TimeSpan.FromSeconds(_options.FileTokenTtlSec))
        };
    }

    private Dictionary<string, string>? JoinLabels(Dictionary<string, string>? initialMetadataLabels, Dictionary<string, string>? completionLabels)
    {
        if(initialMetadataLabels == null && completionLabels == null) 
            return null;

        var newDict = completionLabels != null 
            ? new Dictionary<string, string>(completionLabels)
            : new Dictionary<string, string>();

        if (initialMetadataLabels != null)
        {
            foreach (var label in initialMetadataLabels)
            {
                if (newDict.ContainsKey(label.Key))
                    newDict[label.Key] = label.Value;
            }
        }

        return newDict;
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