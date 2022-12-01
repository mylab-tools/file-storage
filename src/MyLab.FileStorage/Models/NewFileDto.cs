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
    public class NewFileDto
    {
        /// <summary>
        /// File properties
        /// </summary>
        [JsonProperty("file")]
        public StoredFileMetadataDto? File { get; set; }

        /// <summary>
        /// File token 
        /// </summary>
        [JsonProperty("token")]
        public string? Token{ get; set; }
    }
}
