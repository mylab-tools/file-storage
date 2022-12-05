namespace MyLab.FileStorage.Cleaner;

public interface ICleanerStrategy
{
    IEnumerable<FsFile> GetFileDirectories(CancellationToken cancellationToken);

    void DeleteDirectory(string directory);
}