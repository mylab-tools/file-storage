using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Services;
using MyLab.FileStorage.Tools;
using MyLab.WebErrors;

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
        public async Task<IActionResult> CreateNewUploading([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]NewFileRequestDto? newFileRequest)
        {
            var newFileId = await _uploadService.CreateNewFileAsync(newFileRequest);

            var uploadToken = _uploadService.CreateUploadToken(newFileId);

            return Ok(uploadToken);
        }

        [HttpPost("next-chunk")]
        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [ErrorToResponse(typeof(FileTooLargeException), HttpStatusCode.RequestEntityTooLarge, "File content is too large")]
        [ErrorToResponse(typeof(DataTooLargeException), HttpStatusCode.RequestEntityTooLarge, "Chunk is too large")]
        public async Task<IActionResult> UploadNextChunk([FromHeader(Name = "X-UploadToken")] string uploadToken)
        {
            if (!Request.ContentLength.HasValue)
                return StatusCode((int)HttpStatusCode.LengthRequired, "Length Required");
            if (Request.ContentLength == 0)
                return BadRequest("Request does not contains a data chunk");
            
            var token = TransferToken.VerifyAndDeserialize(uploadToken, _options.TransferTokenSecret!);

            await _uploadService.AppendFileData(token.FileId, Request.BodyReader, (int)Request.ContentLength.Value);
            
            return Ok();

        }

        [HttpPost("completion")]
        [ErrorToResponse(typeof(SecurityTokenValidationException), HttpStatusCode.Unauthorized, "Invalid token")]
        [ErrorToResponse(typeof(BadChecksumException), HttpStatusCode.Conflict, "Bad checksum")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        [ErrorToResponse(typeof(DirectoryNotFoundException), HttpStatusCode.NotFound, "File not found")]
        public async Task<IActionResult> CompleteUploading([FromHeader(Name = "X-UploadToken")] string uploadToken, [FromBody]UploadCompletionDto uploadCompletionDto)
        {
            var token = TransferToken.VerifyAndDeserialize(uploadToken, _options.TransferTokenSecret!);

            var newFile = await _uploadService.CompleteFileCreation(token.FileId, uploadCompletionDto);

            return Ok(newFile);
        }
    }
}
