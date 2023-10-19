using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.FileStorage.Tools;
using MyLab.Log;
using MyLab.WebErrors;
using RangeHeaderValue = System.Net.Http.Headers.RangeHeaderValue;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;
        private readonly FsOptions _options;

        public DownloadController(IDownloadService downloadService, IOptions<FsOptions> options)
        {
            _downloadService = downloadService;
            _options = options.Value;
            _options.Validate();
        }

        [HttpPost("{file_id}/download-token")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        public IActionResult CreateNewDownloadToken([FromRoute(Name = "file_id")] string fileId)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");
            
            var downloadToken = _downloadService.CreateDownloadToken(guidId);

            return Ok(downloadToken);
        }

        [HttpGet("{file_id}/content")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        [ErrorToResponse(typeof(DataTooLargeException), HttpStatusCode.RequestEntityTooLarge, "Requested range is too large")]
        [ErrorToResponse(typeof(MultipleRangeNotSupportedException), HttpStatusCode.RequestedRangeNotSatisfiable, "Multiple range is not supported")]
        public async Task<IActionResult> DownloadFile([FromRoute(Name = "file_id")] string fileId, [FromHeader(Name = "Range")]string? rangeHeader)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");

            RangeHeaderValue.TryParse(rangeHeader, out var rangeValue);

            if (rangeValue != null)
            {
                if (rangeValue.Ranges.Count > 1)
                    throw new MultipleRangeNotSupportedException()
                        .AndFactIs("file-id", fileId)
                        .AndFactIs("range-header", rangeHeader);

                var readResult = await _downloadService.ReadContentAsync(guidId, rangeValue);

                if (readResult.FileReads.Length == 0)
                    return StatusCode((int)HttpStatusCode.RequestedRangeNotSatisfiable);

                return new PartialContentResult(readResult.FileReads);
            }
            else
            {
                var readResult = await _downloadService.ReadContentAsync(guidId);

                return new FileContentResult(readResult.Content, "application/octet-stream")
                {
                    FileDownloadName = readResult.Metadata?.Filename,
                    LastModified = DateTimeToOffset(readResult.Metadata?.Created)
                };
            }
        }

        [HttpGet("by-token/content")]
        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        [ErrorToResponse(typeof(DataTooLargeException), HttpStatusCode.RequestEntityTooLarge, "Requested range is too large")]
        [ErrorToResponse(typeof(MultipleRangeNotSupportedException), HttpStatusCode.RequestedRangeNotSatisfiable, "Multiple range is not supported")]
        public async Task<IActionResult> DownloadFileByToken([FromQuery(Name = "token")] string downloadToken, [FromHeader(Name = "Range")] string? rangeHeader)
        {
            var token = TransferToken.VerifyAndDeserialize(downloadToken, _options.TransferTokenSecret!);

            RangeHeaderValue.TryParse(rangeHeader, out var rangeValue);

            if (rangeValue != null)
            {
                var readResult = await _downloadService.ReadContentAsync(token.FileId, rangeValue);

                if (rangeValue.Ranges.Count > 1)
                    throw new MultipleRangeNotSupportedException()
                        .AndFactIs("file-id", token.FileId)
                        .AndFactIs("range-header", rangeHeader);

                return new PartialContentResult(readResult.FileReads);
            }
            else
            {
                var readResult = await _downloadService.ReadContentAsync(token.FileId);

                return new FileContentResult(readResult.Content, "application/octet-stream")
                {
                    FileDownloadName = readResult.Metadata?.Filename,
                    LastModified = DateTimeToOffset(readResult.Metadata?.Created)
                };
            }
        }

        DateTimeOffset? DateTimeToOffset(DateTime? dateTime)
        {
            return dateTime != null
                ? new DateTimeOffset(dateTime.Value)
                : null;
        }
    }
}
