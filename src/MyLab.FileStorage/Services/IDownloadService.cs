using System.Net.Http.Headers;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;

namespace MyLab.FileStorage.Services
{
    public interface IDownloadService
    {
        string CreateDownloadToken(Guid fileId);

        Task<(RangeStreamReader.ReadRange[] FileReads, StoredFileMetadataDto? Metadata)> ReadContentAsync(Guid fileId, RangeHeaderValue rangeHeader);

        Task<(byte[] Content, StoredFileMetadataDto? Metadata)> ReadContentAsync(Guid fileId);
    }
}
