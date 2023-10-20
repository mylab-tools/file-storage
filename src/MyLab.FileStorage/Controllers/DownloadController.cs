using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
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
        private readonly IStorageOperator _storageOperator;
        private readonly IDownloadService _downloadService;
        private readonly FsOptions _options;

        public DownloadController(
            IDownloadService downloadService,
            IStorageOperator storageOperator,
            IOptions<FsOptions> options)
        {
            _storageOperator = storageOperator;
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
        [ErrorToResponse(typeof(MultipleRangeNotSupportedException), HttpStatusCode.RequestedRangeNotSatisfiable, "Multiple range is not supported")]
        public async Task<IActionResult> DownloadFile([FromRoute(Name = "file_id")] string fileId)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");
            
            RangeHeaderValue? rangeHeader;

            try
            {
                rangeHeader = GetRange();
            }
            catch(FormatException e)
            {
                return StatusCode((int) HttpStatusCode.RequestedRangeNotSatisfiable, e.Message);
            }

            if(!CheckSizeLimit(guidId, rangeHeader))
            {
                return StatusCode((int) HttpStatusCode.RequestedRangeNotSatisfiable, "Requested data is too large"); 
            }
            
            var readFile = await _downloadService.ReadFileAsync(guidId);

            var tag = readFile.Md5 != null
                ? new EntityTagHeaderValue("\"" + readFile.Md5 + "\"")
                : EntityTagHeaderValue.Any;

            return File
            (
                readFile.ReadStream, 
                MediaTypeNames.Application.Octet,
                readFile.Filename,
                readFile.Created,
                tag,
                enableRangeProcessing: true
            );
        }

        [HttpGet("by-token/content")]
        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        [ErrorToResponse(typeof(MultipleRangeNotSupportedException), HttpStatusCode.RequestedRangeNotSatisfiable, "Multiple range is not supported")]
        public async Task<IActionResult> DownloadFileByToken([FromQuery(Name = "token")] string downloadToken)
        {
            var token = TransferToken.VerifyAndDeserialize(downloadToken, _options.TransferTokenSecret!);

            RangeHeaderValue? rangeHeader;

            try
            {
                rangeHeader = GetRange();
            }
            catch(FormatException e)
            {
                return StatusCode((int) HttpStatusCode.RequestedRangeNotSatisfiable, e.Message);
            }

            if(!CheckSizeLimit(token.FileId, rangeHeader))
            {
                return StatusCode((int) HttpStatusCode.RequestedRangeNotSatisfiable, "Requested data is too large"); 
            }

            var readFile = await _downloadService.ReadFileAsync(token.FileId);

            var tag = readFile.Md5 != null
                ? new EntityTagHeaderValue("\"" + readFile.Md5 + "\"")
                : EntityTagHeaderValue.Any;

            return File
            (
                readFile.ReadStream, 
                MediaTypeNames.Application.Octet,
                readFile.Filename,
                readFile.Created,
                tag,
                enableRangeProcessing: true
            );
        }

        bool CheckSizeLimit(Guid fid, RangeHeaderValue? rangeHeader)
        {
            long fileLen = _storageOperator.GetContentLength(fid);

            long resultLen;

            if(rangeHeader == null)
                resultLen = fileLen;
            else
            {
                var calc = new RangeCalculator(rangeHeader);
                resultLen = calc.CalculateResultLength(fileLen);
            }            

            return resultLen <= _options.DownloadChunkLimitKiB * 1024;
        }

        RangeHeaderValue? GetRange()
        {
            if(Request.Headers.Range.Count == 0)
            {
                return null;
            }
            
            var rangeHeader = Request.Headers.Range.First();
            if(!RangeHeaderValue.TryParse(rangeHeader, out var rangeValue))
            {
                throw new FormatException("Range header has wrong format");
            }

            return rangeValue;
        }
    }
}
