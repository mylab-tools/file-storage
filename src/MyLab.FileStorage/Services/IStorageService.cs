using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Services
{
    public interface IStorageService
    {
        Task AppendFileAsync(Guid fileId, byte[] data);
        Task SaveMetadataAsync(Guid fileId, StoredFileMetadataDto metadata);
        Task<bool> CheckAndDeleteMd5Async(Guid fileId, byte[] controlMd5);
    }
}
