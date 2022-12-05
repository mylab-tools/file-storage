using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;

namespace MyLab.FileStorage.Services;

public interface IStorageOperator
{
    Task TouchBaseDirectoryAsync(Guid fileId);

    Task AppendContentAsync(Guid fileId, byte[] data);

    Stream OpenContentRead(Guid fileId);

    Task WriteMetadataAsync(Guid fileId, StoredFileMetadataDto metadata);

    Task<StoredFileMetadataDto?> ReadMetadataAsync(Guid fileId);

    Task WriteHashCtxAsync(Guid fileId, Md5Ex.Md5Context context);

    Task<Md5Ex.Md5Context?> ReadHashCtxAsync(Guid fileId);

    Task DeleteHashCtxAsync(Guid fileId);

    Task DeleteFile(Guid fileId);

    long GetContentLength(Guid fileId);
}