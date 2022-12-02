using System.Net.Http.Headers;
using MyLab.FileStorage.Tools;

namespace UnitTests
{
    public class RangeHeaderValueExtensionsBehavior
    {
        private const long FileLen = 40;

        [Theory]
        [MemberData(nameof(GetRangeCases))]
        public void ShouldCalcTotalLen(RangeItemHeaderValue[] items, long expectedSum)
        {
            //Arrange
            var range = new RangeHeaderValue();

            foreach (var item in items)
                range.Ranges.Add(item);

            //Act
            var sum = range.GetTotalLength(FileLen);

            //Assert
            Assert.Equal(expectedSum, sum);
        }

        public static IEnumerable<object[]> GetRangeCases()
        {
            return new []
            {
                new object[]
                {
                    new RangeItemHeaderValue[]
                    {
                        new (10, 20)
                    },
                    10L
                },
                new object[]
                {
                    new RangeItemHeaderValue[]
                    {
                        new (null, 20)
                    },
                    20L
                },
                new object[]
                {
                    new RangeItemHeaderValue[]
                    {
                        new (10, null)
                    },
                    30L
                },
                new object[]
                {
                    new RangeItemHeaderValue[]
                    {
                        
                    },
                    40L
                },
                new object[]
                {
                    new RangeItemHeaderValue[]
                    {
                        new (10, 20),
                        new (null, 20),
                        new (10, null)
                    },
                    60L
                },
            };
        }
    }
}
