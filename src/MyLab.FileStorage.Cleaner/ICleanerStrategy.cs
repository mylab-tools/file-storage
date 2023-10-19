namespace MyLab.FileStorage.Cleaner;

public interface ICleanerStrategy
{
    Task<IEnumerable<FsFile>> GetFileDirectories(CancellationToken cancellationToken);

    void DeleteDirectory(string directory);
}