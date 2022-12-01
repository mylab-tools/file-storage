using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#if SERVER_APP
namespace MyLab.FileStorage.Models
#else
namespace MyLab.FileStorage.Client.Models
#endif
{
    /// <summary>
    /// Stored file information
    /// </summary>
    public class StoredFileMetadataDto
    {
        /// <summary>
        /// File identifier
        /// </summary>
        [JsonConverter(typeof(GuidJsonConverter))]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Creation date time
        /// </summary>
        [JsonProperty("created")]
        private DateTime Created { get; set; }

        /// <summary>
        /// File MD5 check sum
        /// </summary>
        [JsonProperty("md5")]
        [JsonConverter(typeof(HexJsonConverter))]
        public byte[]? Md5 { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        [JsonProperty("filename")] public string? Filename { get; set; }

        /// <summary>
        /// Key-value labels
        /// </summary>
        [JsonProperty("labels")] public Dictionary<string, string>? Labels { get; set; }
    }
}
