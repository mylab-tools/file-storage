using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using MyLab.Log;
using Newtonsoft.Json;

namespace MyLab.FileStorage.Services;

class FileStorageOperator : IStorageOperator
{

    private readonly FileIdToNameConverter _fileIdConverter;

    public FileStorageOperator(IOptions<FsOptions> options)
    {
        _fileIdConverter = new FileIdToNameConverter(options.Value.Directory);
    }

    public Task TouchBaseDirectoryAsync(Guid fileId)
    {
        var directory = new DirectoryInfo(_fileIdConverter.ToDirectory(fileId));

        if (!directory.Exists)
            directory.Create();

        return Task.CompletedTask;
    }

    public async Task AppendContentAsync(Guid fileId, byte[] data)
    {
        await using var fs = new FileStream(_fileIdConverter.ToContentFile(fileId), FileMode.Append, FileAccess.Write);

        await fs.WriteAsync(data, 0, data.Length);
    }

    public Stream OpenContentRead(Guid fileId)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);
        
        return new FileStream(filename, FileMode.Open);
    }

    public async Task WriteMetadataAsync(Guid fileId, StoredFileMetadataDto metadata)
    {
        var metadataStr = JsonConvert.SerializeObject(metadata);

        await using var fs = new FileStream(_fileIdConverter.ToMetadataFile(fileId), FileMode.Append, FileAccess.Write);
        await using var wrtr = new StreamWriter(fs, Encoding.UTF8);

        await wrtr.WriteAsync(metadataStr);
    }

    public async Task<StoredFileMetadataDto> ReadMetadataAsync(Guid fileId)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);
        
        await using var strm = new FileStream(filename, FileMode.Open);
        using var rdr = new StreamReader(strm);

        var str = await rdr.ReadToEndAsync();

        var dto = JsonConvert.DeserializeObject<StoredFileMetadataDto>(str);

        if (dto == null)
            throw new FormatException("Metadata file has from format")
                .AndFactIs("file-id", fileId);

        return dto;
    }

    public Task WriteHashCtxAsync(Guid fileId, Md5Ex.Md5Context context)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);
        return File.WriteAllBytesAsync(filename, context.Serialize());
    }

    public async Task<Md5Ex.Md5Context?> ReadHashCtxAsync(Guid fileId)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);
        
        if (!File.Exists(filename)) return null;

        var ctxBin = await File.ReadAllBytesAsync(filename);

        return Md5Ex.Md5Context.Deserialize(ctxBin);
    }

    public Task DeleteHashCtxAsync(Guid fileId)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);

        var fi = new FileInfo(filename);
        
        if(fi.Exists) fi.Delete();

        return Task.CompletedTask;
    }

    public Task DeleteFile(Guid fileId)
    {
        var path = _fileIdConverter.ToDirectory(fileId);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        return Task.CompletedTask;
    }

    public long GetContentLength(Guid fileId)
    {
        var filename = _fileIdConverter.ToMetadataFile(fileId);

        var fi = new FileInfo(filename);
        
        return fi.Length;
    }
}