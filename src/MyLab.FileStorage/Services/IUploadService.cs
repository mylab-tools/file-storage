using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Services
{
    public interface IUploadService
    {
        string CreateUploadToken();

        Task AppendFileData(Guid fileId, byte[] chunk);

        Task<NewFileDto> CompleteFileCreation(Guid fileId, UploadCompletionDto completion);
    }
}
