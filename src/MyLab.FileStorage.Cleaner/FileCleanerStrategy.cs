using Microsoft.Extensions.Options;
using MyLab.FileStorage.Tools;

namespace MyLab.FileStorage.Cleaner;

class FileCleanerStrategy : ICleanerStrategy
{
    private readonly CleanerOptions _options;

    public FileCleanerStrategy(IOptions<CleanerOptions> opts)
    {
        _options = opts.Value;
    }

    public IEnumerable<FsFile> GetFileDirectories(CancellationToken cancellationToken)
    {
        return Directory
            .EnumerateDirectories(_options.Directory, "*", SearchOption.AllDirectories)
            .Select(d => new FsFile(d)
            {
                CreateDt = Directory.GetCreationTime(d),
                Confirmed = File.Exists(Path.Combine(d, FileIdToNameConverter.ConfirmedFilename))
            });
    }

    public void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, true);
    }
}