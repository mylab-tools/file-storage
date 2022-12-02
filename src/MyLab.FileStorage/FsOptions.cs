using System.Text;

namespace MyLab.FileStorage
{
    public class FsOptions
    {
        public string Directory { get; set; } = "/var/fs/data";

        public string? TokenSecret { get; set; }

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

            if(TokenSecret == null)
                throw new Exception("TokenSecret is not specified");
            if (Encoding.UTF8.GetByteCount(TokenSecret) < 16)
                throw new Exception("TokenSecret is too short. It should be longer then 16 bytes");

            if (UploadTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(UploadTokenTtlSec)} value It should be greater then 0");
            if (DownloadTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(DownloadTokenTtlSec)} value It should be greater then 0");
            if (DocTokenTtlSec <= 0)
                throw new Exception($"Wrong {nameof(DocTokenTtlSec)} value It should be greater then 0");

        }
    }
}
