using System;
using System.Net;
using System.Threading.Tasks;
using MyLab.ApiClient;
using MyLab.FileStorage.Client.Models;

namespace MyLab.FileStorage.Client;

/// <summary>
/// Represents Files API contract
/// </summary>
[Api("v1/files/{file_id}", Key = "fs-files")]
public interface IFsFilesApiV1
{
    /// <summary>
    /// Gets file metadata
    /// </summary>
    [Get]
    Task<StoredFileMetadataDto> GetFileMetadataAsync([Path("file_id")]Guid fileId);

    /// <summary>
    /// Deletes a file
    /// </summary>
    [Delete]
    [ExpectedCode(HttpStatusCode.NoContent)]
    Task DeleteFileAsync([Path("file_id")] Guid fileId);

    /// <summary>
    /// Confirm file after uploading
    /// </summary>
    [Post("confirmation")]
    [ExpectedCode(HttpStatusCode.NoContent)]
    Task ConfirmFileAsync([Path("file_id")] Guid fileId);
}