using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpPost("{file_id}/download-token")]
        Task<IActionResult> CreateNewDownloadToken([FromRoute(Name = "file_id")] string fileId)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{file_id}/content")]
        Task<IActionResult> DownloadFile([FromRoute(Name = "file_id")] string fileId, [FromHeader(Name = "Range")]string rangeHeader)
        {
            throw new NotImplementedException();
        }

        [HttpGet("by-token/content")]
        Task<IActionResult> DownloadFileByToken([FromRoute(Name = "file_id")] string fileId, [FromQuery(Name = "token")] string downloadToken)
        {
            throw new NotImplementedException();
        }
    }
}
