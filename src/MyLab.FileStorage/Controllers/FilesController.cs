using Microsoft.AspNetCore.Mvc;
using MyLab.FileStorage.Services;

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
        public async Task<IActionResult> GetFile([FromRoute(Name = "file_id")] string fileId)
        {
            if (!Guid.TryParse(fileId, out var guidId))
                return BadRequest("Bad file id");

            var fileMetadata = await _storageOperator.ReadMetadataAsync(guidId);

            if (fileMetadata == null)
                return NotFound("File not found");

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
    }
}
