using System.Text;

namespace MyLab.FileStorage
{
        /// <summary>
        /// Contains File Storage options
        /// </summary>
        public class FsOptions
        {
            /// <summary>
            /// The base directory for files
            /// </summary>
            /// <remarks>
            /// '/var/fs/data' by default
            /// </remarks>
            public string Directory { get; set; } = "/var/fs/data";
            /// <summary>
            /// Secret for upload and download tokens
            /// </summary>
            /// <remarks>
            /// Must be at least 16 characters long
            /// </remarks>
            public string? TransferTokenSecret { get; set; }
            /// <summary>
            /// Secret for file token
            /// </summary>
            /// <remarks>
            /// Must be at least 16 characters long
            /// </remarks>
            public string? FileTokenSecret { get; set; }
            /// <summary>
            /// Time to upload token live in seconds
            /// </summary>
            /// <remarks>
            /// 1 hour by default
            /// </remarks>
            public int UploadTokenTtlSec { get; set; } = 3600;
            /// <summary>
            /// Time to download token live in seconds
            /// </summary>
            /// <remarks>
            /// 1 hour by default
            /// </remarks>
            public int DownloadTokenTtlSec { get; set; } = 3600;
            /// <summary>
            /// Time to file token live in seconds
            /// </summary>
            /// <remarks>
            /// 1 hour by default
            /// </remarks>
            public int FileTokenTtlSec { get; set; } = 3600;
            /// <summary>
            /// Maximum chunk length when uploading in KiB
            /// </summary>
            /// <remarks>
            /// 0.5 Mib by default
            /// </remarks>
            public long UploadChunkLimitKiB { get; set; } = 512;
            /// <summary>
            /// Maximum chunk length when downloading in KiB
            /// </summary>
            /// <remarks>
            /// 0.5 Mib by default
            /// </remarks>
            public long DownloadChunkLimitKiB { get; set; } = 512;
            /// <summary>
            /// Maximum stored file length in MiB
            /// </summary>
            /// <remarks>
            /// Unlimited by default
            /// </remarks>
            public long? StoredFileSizeLimitMiB { get; set; }

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
                if (FileTokenTtlSec <= 0)
                    throw new Exception($"Wrong {nameof(FileTokenTtlSec)} value It should be greater then 0");

            }
        }
}
