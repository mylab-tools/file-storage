namespace MyLab.FileStorage.Services
{
    public interface IDownloadService
    {
        string CreateDownloadToken(Guid fileId);

        Task<ReadFile> ReadFileAsync(Guid fileId);
    }

    public class ReadFile
    {
        public string? Filename { get; set; }
        
        public DateTime? Created { get; set; }
        
        public Stream ReadStream { get; }

        public string? Md5{ get; set; }

        public ReadFile(Stream readStream) 
            => ReadStream = readStream;

    }
}
