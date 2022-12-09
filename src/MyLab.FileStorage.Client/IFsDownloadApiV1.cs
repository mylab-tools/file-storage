using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.FileStorage.Client;

/// <summary>
/// Represents DownloadByFileId API contract
/// </summary>
[Api("v1/files", Key = "fs-download")]
public interface IFsDownloadApiV1
{
    /// <summary>
    /// Creates new download-token
    /// </summary>
    /// <returns>download token</returns>
    [Post("{file_id}/download-token")]
    Task<string> CreateDownloadTokenAsync([Path("file_id")] Guid fileId);

    /// <summary>
    /// Downloads full file by download token
    /// </summary>
    [Get("by-token/content")]
    Task<byte[]> DownloadByToken([Query("token")] string downloadToken);

    /// <summary>
    /// Downloads file range by download token
    /// </summary>
    [Get("by-token/content")]
    [ExpectedCode(HttpStatusCode.PartialContent)]
    Task<byte[]> DownloadByToken([Query("token")] string downloadToken, [Header("Range")] RangeHeaderValue range);

    /// <summary>
    /// Downloads full file by file id
    /// </summary>
    [Get("{file_id}/content")]
    Task<byte[]> DownloadByFileId([Path("file_id")] Guid fileId);

    /// <summary>
    /// Downloads file range by file id
    /// </summary>
    [Get("{file_id}/content")]
    [ExpectedCode(HttpStatusCode.PartialContent)]
    Task<byte[]> DownloadByFileId([Path("file_id")] Guid fileId, [Header("Range")] RangeHeaderValue range);
}