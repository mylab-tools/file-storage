using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MyLab.FileStorage.Tools;

namespace MyLab.FileStorage.Models;

public class PartialContentResult : IActionResult
{
    private readonly RangeStreamReader.ReadRange[] _ranges;

    public PartialContentResult(RangeStreamReader.ReadRange[] ranges)
    {
        _ranges = ranges;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.PartialContent;

        var content = GetContent();

        context.HttpContext.Response.ContentType = content.Headers.ContentType!.ToString();

        return content.CopyToAsync(context.HttpContext.Response.Body);
    }

    private HttpContent GetContent()
    {

        if (_ranges.Length == 1)
        {
            var singleRange = _ranges.Single();
            
            return new ByteArrayContent(singleRange.Data)
            {
                Headers =
                {
                    ContentLength = singleRange.Data.Length,
                    ContentRange = singleRange.ResultRange,
                    ContentType = new MediaTypeHeaderValue("application/octet-stream")
                }
            };
        }
        
        var content = new MultipartContent("byteranges");
        
        foreach (var range in _ranges)
        {
            var partialContent = new ByteArrayContent(range.Data)
            {
                Headers =
                {
                    ContentLength = range.Data.Length,
                    ContentRange = range.ResultRange
                }
            };

            content.Add(partialContent);
        }
        
        return content;
    }
}