using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace UnitTests
{
    public class RangeHeaderBehavior
    {
        private readonly ITestOutputHelper _output;

        public RangeHeaderBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldSerializeSimpleValue()
        {
            //Arrange
            RangeHeaderValue r = new RangeHeaderValue();

            r.Ranges.Add(new RangeItemHeaderValue(100, 200));

            //Act
            var rangeValueStr = r.ToString();
            _output.WriteLine(rangeValueStr);

            //Assert
            Assert.Equal("bytes=100-200", rangeValueStr);
        }

        [Fact]
        public void ShouldSerializeMultipleValues()
        {
            //Arrange
            RangeHeaderValue r = new RangeHeaderValue();

            r.Ranges.Add(new RangeItemHeaderValue(100, 200));
            r.Ranges.Add(new RangeItemHeaderValue(300, 400));
            
            //Act
            var rangeValueStr = r.ToString();
            _output.WriteLine(rangeValueStr);
            
            //Assert
            Assert.Equal("bytes=100-200, 300-400", rangeValueStr);
        }

        [Fact]
        public void ShouldParseSimpleValue()
        {
            //Arrange
            RangeHeaderValue r = RangeHeaderValue.Parse("bytes=100-200");

            //Act
            var rangeValueStr = r.ToString();
            _output.WriteLine(rangeValueStr);

            //Assert
            Assert.Equal("bytes=100-200", rangeValueStr);
        }

        [Fact]
        public void ShouldParseMultipleValues()
        {
            //Arrange
            RangeHeaderValue r = RangeHeaderValue.Parse("bytes=100-200, 300-400");

            //Act
            var rangeValueStr = r.ToString();
            _output.WriteLine(rangeValueStr);

            //Assert
            Assert.Equal("bytes=100-200, 300-400", rangeValueStr);
        }
    }
}
