using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

#if SERVER_APP
namespace MyLab.FileStorage.Models
#else
namespace MyLab.FileStorage.Client.Models
#endif
{
    /// <summary>
    /// Contains info for upload completion
    /// </summary>
    public class UploadCompletionDto
    {
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
