using Microsoft.Extensions.Options;
using MyLab.FileStorage.Client.Models;
using MyLab.FileStorage.Tools;
using Newtonsoft.Json;

namespace MyLab.FileStorage.Cleaner;

class FileCleanerStrategy : ICleanerStrategy
{
    private readonly CleanerOptions _options;

    public FileCleanerStrategy(IOptions<CleanerOptions> opts)
    {
        _options = opts.Value;
    }

    public async Task<IEnumerable<FsFile>> GetFileDirectories(CancellationToken cancellationToken)
    {
        var dirs = Directory.EnumerateDirectories(_options.Directory, "*", SearchOption.AllDirectories);
        var resultFiles = new List<FsFile>();

        foreach(var dir in dirs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var metadataFn = Path.Combine(dir, FileIdToNameConverter.MetadataFilename);
            int? ttlHours = null;

            if(File.Exists(metadataFn))
            {
                var fileMetadataStr = await File.ReadAllTextAsync(metadataFn);
                var metadata = JsonConvert.DeserializeObject<StoredFileMetadataDto>(fileMetadataStr);
                ttlHours = metadata?.TtlHours;
            }

            resultFiles.Add(new FsFile(dir)
            {
                CreateDt = Directory.GetCreationTime(dir),
                Confirmed = File.Exists(Path.Combine(dir, FileIdToNameConverter.ConfirmedFilename)),
                TtlHours = ttlHours
            });
        }

        return resultFiles;
    }

    public void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, true);
    }
}