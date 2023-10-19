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
        /// Defines a time to live in hours
        /// </summary>
        [JsonProperty("ttlh")]
        public int? TtlHours { get; set; }

        /// <summary>
        /// Defines file purpose. Assigned by target service.
        /// </summary>
        [JsonProperty("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Creation date time
        /// </summary>
        [JsonProperty("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// File MD5 check sum
        /// </summary>
        [JsonProperty("md5")]
        [JsonConverter(typeof(HexJsonConverter))]
        public byte[]? Md5 { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        [JsonProperty("filename")] 
        public string? Filename { get; set; }

        /// <summary>
        /// File length
        /// </summary>
        [JsonProperty("length")] 
        public long Length{ get; set; }

        /// <summary>
        /// Key-value labels
        /// </summary>
        [JsonProperty("labels")] 
        public Dictionary<string, string>? Labels { get; set; }
    }
}
