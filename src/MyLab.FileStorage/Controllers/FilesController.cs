using System.Net;
using Microsoft.AspNetCore.Mvc;
using MyLab.FileStorage.Services;
using MyLab.WebErrors;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files/{file_id}")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IStorageOperator _storageOperator;

        public FilesController(IStorageOperator storageOperator)
        {
            _storageOperator = storageOperator;
        }

        [HttpGet]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        public async Task<IActionResult> GetFile([FromRoute(Name = "file_id")] string fileId)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");

            bool hasConfirmation = _storageOperator.IsConfirmedFileExists(guidId);
            if (!hasConfirmation)
                throw new FileNotFoundException("Confirmation not found");

            var fileMetadata = await _storageOperator.ReadMetadataAsync(guidId);

            return Ok(fileMetadata);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromRoute(Name = "file_id")] string fileId)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");

            await _storageOperator.DeleteFile(guidId);

            return NoContent();
        }

        [HttpPost("confirmation")]
        [ErrorToResponse(typeof(FileNotFoundException), HttpStatusCode.NotFound, "File not found")]
        [ErrorToResponse(typeof(ConflictException), HttpStatusCode.Conflict, "Conflict")]
        public async Task<IActionResult> ConfirmFile([FromRoute(Name = "file_id")] string fileId, [FromQuery] int? ttlh)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");

            bool hasConfirmation = _storageOperator.IsConfirmedFileExists(guidId);
            if (hasConfirmation)
                throw new ConflictException("Confirmation already exists");

            if (ttlh.HasValue)
            {
                var fMeta = await _storageOperator.ReadMetadataAsync(guidId);
                fMeta.TtlHours = ttlh;
                await _storageOperator.WriteMetadataAsync(guidId, fMeta);
            }

            await _storageOperator.WriteConfirmedFile(guidId, DateTime.Now);

            return NoContent();
        }
    }
}
