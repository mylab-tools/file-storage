namespace MyLab.FileStorage.Tools
{
    public class FileIdToNameConverter
    {
        private readonly string _basePath;

        public const string ContentFilename = "content.bin";
        public const string MetadataFilename = "metadata.json";
        public const string HashCtxFilename = "hash-ctx.bin";
        public const string ConfirmedFilename = "confirmed.dt";

        public char Separator { get; set; } = Path.DirectorySeparatorChar;

        public FileIdToNameConverter(string basePath)
        {
            _basePath = basePath;
        }

        public string ToDirectory(Guid fileId)
        {
            return _basePath.TrimEnd(Separator) + Separator + GuidToPath(fileId);
        }

        public Guid GetIdFromDirectory(string directory)
        {
            var rel = Path.GetRelativePath(_basePath, directory);
            var guidChars = rel.Replace("\\", "").Replace("/", "");
            return Guid.Parse(guidChars);
        }

        public string ToContentFile(Guid fileId) => ToFile(fileId, ContentFilename);
        public string ToMetadataFile(Guid fileId) => ToFile(fileId, MetadataFilename);
        public string ToHashCtxFile(Guid fileId) => ToFile(fileId, HashCtxFilename);
        public string ToConfirmFile(Guid fileId) => ToFile(fileId, ConfirmedFilename);

        string ToFile(Guid fileId, string filename)
        {
            return ToDirectory(fileId) + Separator + filename;
        }

        string GuidToPath(Guid id)
        {
            var guidStr = id.ToString("N");
            return String.Join(Separator,
                guidStr.Substring(0, 4),
                guidStr.Substring(4, 4),
                guidStr.Substring(8, 24)
            );
        }
    }
}
