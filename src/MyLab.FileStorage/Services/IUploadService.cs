using System.IO.Pipelines;
using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Services
{
    public interface IUploadService
    {
        string CreateUploadToken(Guid fileId);

        Task<Guid> CreateNewFileAsync(NewFileRequestDto? newFileRequest);
        
        Task AppendFileData(Guid fileId, PipeReader pipeReader, int length);

        Task<NewFileDto> CompleteFileCreation(Guid fileId, UploadCompletionDto completion);
    }
}
