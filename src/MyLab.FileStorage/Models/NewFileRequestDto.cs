using Newtonsoft.Json;

#if SERVER_APP
namespace MyLab.FileStorage.Models
#else
using System.Collections.Generic;
namespace MyLab.FileStorage.Client.Models
#endif
{
    /// <summary>
    ///  Contains parameters for new file
    /// </summary>
    public class NewFileRequestDto
    {
        /// <summary>
        /// Defines file purpose. May be used by target service.
        /// </summary>
        [JsonProperty("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Defines a time to live in hours
        /// </summary>
        [JsonProperty("ttlh")]
        public int? TtlHours { get; set; }

        /// <summary>
        /// Key-value labels
        /// </summary>
        [JsonProperty("labels")]
        public Dictionary<string, string>? Labels { get; set; }
    }
}

