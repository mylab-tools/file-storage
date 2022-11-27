using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files/{file_id}")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet]
        Task<IActionResult> GetFile([FromRoute(Name = "file_id")] string fileId)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        Task<IActionResult> DeleteFile([FromRoute(Name = "file_id")] string fileId)
        {
            throw new NotImplementedException();
        }
    }
}
