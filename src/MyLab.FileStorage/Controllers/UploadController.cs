using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.FileStorage.Tools;
using MyLab.WebErrors;
using Newtonsoft.Json;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files/new")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;
        private readonly FsOptions _options;

        public UploadController(IUploadService uploadService, IOptions<FsOptions> options)
        {
            _uploadService = uploadService;
            _options = options.Value;
            _options.Validate();
        }

        [HttpPost]
        public IActionResult CreateNewUploading()
        {
            return Ok(_uploadService.CreateUploadToken());
        }

        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [HttpPost("next-chunk")]
        public async Task<IActionResult> UploadNextChunk([FromHeader(Name = "X-UploadToken")] string uploadToken)
        {
            if (!Request.ContentLength.HasValue)
                return StatusCode((int)HttpStatusCode.LengthRequired, "Length Required");
            if (Request.ContentLength / 1024 > _options.UploadChunkLimitKBytes)
                return StatusCode((int)HttpStatusCode.RequestEntityTooLarge, "Chunk too large");
            if (Request.ContentLength == 0)
                return BadRequest("Request does not contains a data chunk");

            var token = UploadToken.VerifyAndDeserialize(uploadToken, _options.TokenSecret!);


            byte[] data = await ReadChunkFromRequest(Request.BodyReader, (int)Request.ContentLength.Value);

            await _uploadService.AppendFileData(token.FileId, data);
            
            return Ok();

        }

        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [ErrorToResponse(typeof(BadChecksumException), HttpStatusCode.Conflict, "Bad checksum")]
        [HttpPost("completion")]
        public async Task<IActionResult> CompleteUploading([FromHeader(Name = "X-UploadToken")] string uploadToken, [FromBody]UploadCompletionDto uploadCompletionDto)
        {
            if (uploadCompletionDto.Md5 == null)
                return BadRequest("Checksum is not specified");
            if (uploadCompletionDto.Filename == null)
                return BadRequest("Filename is not specified");

            var token = UploadToken.VerifyAndDeserialize(uploadToken, _options.TokenSecret!);

            var newFile = await _uploadService.CompleteFileCreation(token.FileId, uploadCompletionDto);

            return Ok(newFile);
        }

        private async Task<byte[]> ReadChunkFromRequest(PipeReader reader, int contentLength)
        {
            var readTimeout = TimeSpan.FromSeconds(30);
            var cts = new CancellationTokenSource(readTimeout);
            var readResult = await reader.ReadAtLeastAsync(contentLength, cts.Token);

            if (readResult.Buffer.Length != contentLength)
            {
                throw new InvalidOperationException($"Cant read input stream in {readTimeout}");
            }

            return readResult.Buffer.ToArray();
        }
    }
}
