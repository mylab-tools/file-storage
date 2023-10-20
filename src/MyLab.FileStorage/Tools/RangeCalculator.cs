using System.Net.Http.Headers;

namespace MyLab.FileStorage.Tools;

class RangeCalculator
{
    private RangeHeaderValue _rangeHeader;

    public RangeCalculator(RangeHeaderValue rangeHeader) => _rangeHeader = rangeHeader;

    public long CalculateResultLength(long fileLen)
    {
        return _rangeHeader.Ranges.Sum(r => CalcRangeResLen(r, fileLen));
    }

    long CalcRangeResLen(RangeItemHeaderValue range, long fileLen)
    {
        long resLen = fileLen;

        if(range.From.HasValue)
            resLen = resLen - range.From.Value;

        if(range.To.HasValue)
            resLen = resLen - (fileLen - range.To.Value);

        return resLen;
    }
}