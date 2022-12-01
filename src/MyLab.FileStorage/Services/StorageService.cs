using Microsoft.Extensions.Options;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using MyLab.Log;

namespace MyLab.FileStorage.Services;

class StorageService : IStorageService
{
    private readonly IStorageStrategy _strategy;

    public StorageService(IStorageStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task AppendFileAsync(Guid fileId, byte[] data)
    {
        await _strategy.TouchBaseDirectoryAsync(fileId);
        
        await _strategy.AppendContentAsync(fileId, data);

        var hashCtx = await _strategy.ReadHashCtxAsync(fileId);

        var hash = hashCtx != null
            ? new Md5Ex(hashCtx)
            : new Md5Ex();

        hash.AppendData(data);

        await _strategy.WriteHashCtxAsync(fileId, hash.Context);
    }

    public async Task SaveMetadataAsync(Guid fileId, StoredFileMetadataDto metadata)
    {
        await _strategy.TouchBaseDirectoryAsync(fileId);
        
        await _strategy.WriteMetadataAsync(fileId, metadata);

    }

    public async Task<bool> CheckAndDeleteMd5Async(Guid fileId, byte[] controlMd5)
    {
        var hashCtx = await _strategy.ReadHashCtxAsync(fileId);

        if (hashCtx == null)
            throw new InvalidOperationException("Hash context not found")
                .AndFactIs("file-id", fileId.ToString("N"));

        var storedFileMd5 = new Md5Ex(hashCtx);
        var hash = storedFileMd5.FinalHash();

        bool success = hash.SequenceEqual(controlMd5);

        if (success)
        {
            await _strategy.DeleteHashCtxAsync(fileId);
        }

        return success;
    }
}