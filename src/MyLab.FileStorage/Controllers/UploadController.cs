using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Controllers
{
    [Route("v1/files/new")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        Task<IActionResult> CreateNewUploading()
        {
            throw new NotImplementedException();
        }

        [HttpPost("next-chunk")]
        Task<IActionResult> UploadNextChunk([FromHeader(Name = "X-UploadToken")] string uploadToken)
        {
            throw new NotImplementedException();
        }

        [HttpPost("completion")]
        Task<IActionResult> CompleteUploading([FromHeader(Name = "X-UploadToken")] string uploadToken, [FromBody]UploadCompletion uploadCompletion)
        {
            throw new NotImplementedException();
        }
    }
}
