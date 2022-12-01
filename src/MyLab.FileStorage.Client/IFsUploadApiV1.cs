﻿using System.Threading.Tasks;
using MyLab.ApiClient;
using MyLab.FileStorage.Client.Models;

namespace MyLab.FileStorage.Client
{
    /// <summary>
    /// Defines Upload API contract
    /// </summary>
    [Api("v1/files/new")]
    public interface IFsUploadApiV1
    {
        /// <summary>
        /// Creates new file uploading
        /// </summary>
        /// <returns>Upload token</returns>
        [Post]
        Task<string> CreateNewFileAsync();

        /// <summary>
        /// Uploads next file chunk
        /// </summary>
        [Post("next-chunk")]
        Task UploadNextChunkAsync([Header("X-UploadToken")] string uploadToken, [BinContent] byte[] chunk);

        /// <summary>
        /// Completes upload and posts metadata
        /// </summary>
        [Post("completion")]
        Task<NewFileDto> CompleteFileUploading([Header("X-UploadToken")] string uploadToken, [JsonContent] UploadCompletionDto uploadCompletion);
    }
}
