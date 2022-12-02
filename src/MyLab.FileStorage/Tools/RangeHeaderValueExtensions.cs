
using System.Net.Http.Headers;

namespace MyLab.FileStorage.Tools;

public static class RangeHeaderValueExtensions
{
    public static long GetTotalLength(this RangeHeaderValue value, long fileLen)
    {
        if (value.Ranges.Count == 0)
            return fileLen;

        return value.Ranges
            .Select(r =>
            {
                if (r.From.HasValue && r.To.HasValue)
                {
                    return r.To.Value - r.From.Value;
                }
                else if (!r.From.HasValue && !r.To.HasValue)
                {
                    return fileLen;
                }
                else if (r.From.HasValue /* && !r.To.HasValue*/)
                {
                    return fileLen - r.From.Value;
                }
                //else if (!r.From.HasValue && r.To.HasValue)*/

                return r.To!.Value;
            })
            .Where(r => r > 0)
            .Sum();
    }
}