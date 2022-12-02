using System.Net.Http.Headers;
using System.Text;
using MyLab.FileStorage.Tools;
using Xunit.Abstractions;

namespace UnitTests
{
    public class RangeStreamReaderBehavior
    {
        private readonly ITestOutputHelper _output;
        private const string FileData = "foobarbaz";
        
        public RangeStreamReaderBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(GetRangeCases))]
        public async Task ShouldReadRanges(TestRangeRead[] ranges)
        {
            //Arrange
            var rangeHeader = new RangeHeaderValue();

            foreach (var range in ranges)
                rangeHeader.Ranges.Add(range.RangeItem);

            var reader = new RangeStreamReader(rangeHeader);

            var dataBin = Encoding.UTF8.GetBytes(FileData);
            var stream = new MemoryStream(dataBin);

            //Act
            var reads = await reader.ReadAsync(stream);

            var readPairs = reads
                .Select(rd =>
                {
                    var range = ranges.FirstOrDefault(rng => rd.OriginRange.From == rng.RangeItem.From && rd.OriginRange.To == rng.RangeItem.To);

                    return new { Result = Encoding.UTF8.GetString(rd.Data), Range = range };
                })
                .Where(r => r.Range != null)
                .Select(r => new { r.Result, r.Range, Expected = r.Range!.ExpectedResult })
                .ToArray();

            foreach (var readPair in readPairs)
            {
                _output.WriteLine($"Range: from={readPair.Range?.RangeItem.From.ToString() ?? "null"}, to={readPair.Range?.RangeItem.To.ToString() ?? "null"}; Expected: {readPair.Expected}; Actual: {readPair.Result}.");
            }

            //Assert
            Assert.NotNull(reads);
            Assert.Equal(ranges.Length, reads.Length);
            Assert.True(readPairs.All(r => r.Result == r.Expected));
        }

        public static IEnumerable<object[]> GetRangeCases()
        {
            return new[]
            {
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, null, "barbaz"),
                        new(3, 5, "bar"),
                        new(null, 6, "barbaz")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, 5, "bar"),
                        new(null, 6, "barbaz")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, null, "barbaz"),
                        new(null, 6, "barbaz")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, null, "barbaz"),
                        new(3, 5, "bar"),
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, null, "barbaz")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, 5, "bar")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(null, 6, "barbaz")
                    }
                },
                new object[]
                {
                    new TestRangeRead[]
                    {
                        new(3, 20, "barbaz")
                    }
                }
            };
        }

        public class TestRangeRead
        {
            public RangeItemHeaderValue RangeItem { get; }
            public string ExpectedResult { get; }

            public TestRangeRead(long? from, long? to, string expectedResult)
            {
                RangeItem = new RangeItemHeaderValue(from, to);
                ExpectedResult = expectedResult;
            }
        }
    }
}
