using System.IO.Pipelines;
using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Services
{
    public interface IUploadService
    {
        string CreateUploadToken();
        
        Task AppendFileData(Guid fileId, PipeReader pipeReader, int length);

        Task<NewFileDto> CompleteFileCreation(Guid fileId, UploadCompletionDto completion);
    }
}
