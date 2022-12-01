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
        private readonly IStorageService _storageService;
        private readonly FsOptions _options;

        public UploadController(IStorageService storageService, IOptions<FsOptions> options)
        {
            _storageService = storageService;
            _options = options.Value;
            _options.Validate();
        }

        [HttpPost]
        public IActionResult CreateNewUploading()
        {
            var uploadToken = UploadToken.New();

            var tokenStr = uploadToken.Serialize(_options.TokenSecret!, TimeSpan.FromSeconds(_options.UploadTokenTtlSec));

            return Ok(tokenStr);
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
            
            await _storageService.AppendFileAsync(token.FileId, data);

            return Ok(token);

        }

        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [HttpPost("completion")]
        public async Task<IActionResult> CompleteUploading([FromHeader(Name = "X-UploadToken")] string uploadToken, [FromBody]UploadCompletionDto uploadCompletionDto)
        {
            if (uploadCompletionDto.Md5 == null)
                return BadRequest("Checksum is not specified");
            if (uploadCompletionDto.Filename == null)
                return BadRequest("Filename is not specified");

            var token = UploadToken.VerifyAndDeserialize(uploadToken, _options.TokenSecret!);
            
            bool fileHashOk = await _storageService.CheckAndDeleteMd5Async(token.FileId, uploadCompletionDto.Md5);

            if (!fileHashOk)
                return Conflict("Invalid checksum");

            var metadata = new StoredFileMetadataDto
            {
                Md5 = uploadCompletionDto.Md5,
                Filename = uploadCompletionDto.Filename,
                Id = token.FileId,
                Labels = uploadCompletionDto.Labels
            };

            await _storageService.SaveMetadataAsync(token.FileId, metadata);

            var docToken = new DocumentToken(metadata);

            var newFile = new NewFileDto
            {
                File = metadata,
                Token = docToken.Serialize(_options.TokenSecret!, TimeSpan.FromSeconds(_options.DocTokenTtlSec))
            };

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
