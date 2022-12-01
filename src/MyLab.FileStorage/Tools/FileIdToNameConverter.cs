namespace MyLab.FileStorage.Tools
{
    public class FileIdToNameConverter
    {
        private readonly string _basePath;

        public char PathSeparator { get; set; } = Path.PathSeparator;

        public FileIdToNameConverter(string basePath)
        {
            _basePath = basePath;
        }

        public string ToDirectory(Guid fileId)
        {
            return _basePath.TrimEnd(PathSeparator) + PathSeparator + GuidToPath(fileId);
        }

        public string ToContentFile(Guid fileId) => ToFile(fileId, "content.bin");
        public string ToMetadataFile(Guid fileId) => ToFile(fileId, "metadata.json");
        public string ToHashCtxFile(Guid fileId) => ToFile(fileId, "hash-ctx.bin");

        string ToFile(Guid fileId, string filename)
        {
            return ToDirectory(fileId) + PathSeparator + filename;
        }

        string GuidToPath(Guid id)
        {
            var guidStr = id.ToString("N");
            return String.Join(PathSeparator,
                guidStr.Substring(0, 4),
                guidStr.Substring(4, 4),
                guidStr.Substring(8, 4),
                guidStr.Substring(12, 4),
                guidStr.Substring(16, 4),
                guidStr.Substring(20, 4),
                guidStr.Substring(24, 4),
                guidStr.Substring(28, 4)
            );
        }
    }
}
