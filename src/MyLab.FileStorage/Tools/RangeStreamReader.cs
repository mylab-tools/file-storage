using System.Dynamic;
using System.Net.Http.Headers;

namespace MyLab.FileStorage.Tools
{
    public class RangeStreamReader
    {
        private readonly RangeHeaderValue _rangeHeader;

        public RangeStreamReader(RangeHeaderValue rangeHeader)
        {
            _rangeHeader = rangeHeader;
        }

        public async Task<ReadRange[]> ReadAsync(Stream stream)
        {
            var reads = new List<ReadRange>();

            long fileLen = stream.Length;

            foreach (var range in _rangeHeader.Ranges)
            {
                if (range.From == null && range.To == null)
                    continue;

                if (range.From != null && range.To == null)
                {
                    stream.Seek(range.From.Value, SeekOrigin.Begin);

                    var rangeLen = stream.Length - range.From.Value;
                    var buff = new byte[rangeLen];

                    var read = await stream.ReadAsync(buff, 0, (int)rangeLen);

                    var rangeHeader = new ContentRangeHeaderValue(range.From.Value, range.From.Value + read, fileLen);
                    reads.Add(new ReadRange(range, rangeHeader, AlignBuff(buff, read)));
                }

                if (range.From == null && range.To != null)
                {
                    stream.Seek(-1*range.To.Value, SeekOrigin.End);

                    var startPos = stream.Position;
                    var rangeLen = range.To.Value;
                    var buff = new byte[rangeLen];

                    var read = await stream.ReadAsync(buff, 0, (int)rangeLen);

                    var rangeHeader = new ContentRangeHeaderValue(startPos, startPos+read, fileLen);
                    reads.Add(new ReadRange(range, rangeHeader, AlignBuff(buff, read)));
                }

                if (range.From != null && range.To != null && range.To.Value - range.From.Value > 0)
                {
                    stream.Seek(range.From.Value, SeekOrigin.Begin);

                    var startPos = stream.Position;
                    var rangeLen = range.To.Value - range.From.Value+1;
                    var buff = new byte[rangeLen];

                    var read = await stream.ReadAsync(buff, 0, (int)rangeLen);

                    var rangeHeader = new ContentRangeHeaderValue(startPos, startPos + read, fileLen);
                    reads.Add(new ReadRange(range, rangeHeader, AlignBuff(buff, read)));
                }
            }

            return reads.ToArray();
        }

        byte[] AlignBuff(byte[] origin, long actualLen)
        {
            if(origin.Length == actualLen)
                return origin;

            byte[] newBuff = new byte[actualLen];
            
            Array.Copy(origin, 0, newBuff, 0, actualLen);

            return newBuff;
        }

        public class ReadRange
        {
            public RangeItemHeaderValue OriginRange { get; }
            public ContentRangeHeaderValue ResultRange { get; }
            public byte[] Data { get; }

            public ReadRange(RangeItemHeaderValue originRange, ContentRangeHeaderValue resultRange, byte[] data)
            {
                OriginRange = originRange;
                ResultRange = resultRange;
                Data = data;
            }
        }
    }
}
