using System.Text;

namespace MyLab.FileStorage
{
    public class FsOptions
    {
        public string Directory { get; set; } = "/var/fs/data";

        public string? TransferTokenSecret { get; set; }
        public string? FileTokenSecret { get; set; }

        public int UploadTokenTtlSec { get; set; } = 60 * 60;
        public int DownloadTokenTtlSec { get; set; } = 60 * 60;
        public int DocTokenTtlSec { get; set; } = 60 * 60;

        public long UploadChunkLimitKBytes { get; set; } = 512;
        public long DownloadChunkLimitKBytes { get; set; } = 512;

        public long? StoredFileSizeLimitMBytes { get; set; }
        public void Validate()
        {
            if (Directory == null)
                throw new Exception("Directory is not specified");

            if (TransferTokenSecret == null)
                throw new Exception($"{nameof(TransferTokenSecret)} is not specified");
            if (Encoding.UTF8.GetByteCount(TransferTokenSecret) < 16)
                throw new Exception($"{nameof(TransferTokenSecret)} is too short. It should be longer then 16 bytes");

            if (FileTokenSecret == null)
                throw new Exception($"{nameof(FileTokenSecret)} is not specified");
            if (Encoding.UTF8.GetByteCount(FileTokenSecret) < 16)
                throw new Exception($"{nameof(FileTokenSecret)} is too short. It should be longer then 16 bytes");

            if (UploadTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(UploadTokenTtlSec)} value It should be greater then 0");
            if (DownloadTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(DownloadTokenTtlSec)} value It should be greater then 0");
            if (DocTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(DocTokenTtlSec)} value It should be greater then 0");

        }
    }
}
