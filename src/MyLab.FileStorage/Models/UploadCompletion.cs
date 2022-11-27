using Newtonsoft.Json;

namespace MyLab.FileStorage.Models
{
    public class UploadCompletion
    {
        [JsonProperty("md5")]
        public string? Md5 { get; set; }

        [JsonProperty("metadata")]
        public FileMetadata? Metadata { get; set; }
    }

    public class FileMetadata
    {
        [JsonProperty("filename")]
        public string? Filename { get; set; }

        [JsonProperty("labels")]
        public Dictionary<string, string>? Labels { get; set; }
    }
}
