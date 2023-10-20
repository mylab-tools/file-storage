using System.ComponentModel;
using System.Net.Http.Headers;
using MyLab.FileStorage.Tools;

namespace UnitTests;

public class RangeCalculatorBehavior
{
    [Theory(DisplayName = "Shoult calc single ranges")]
    [InlineData(10, null, 90)]
    [InlineData(null, 10, 10)]
    [InlineData(10, 20, 10)]
    public void ShouldCalcSingleRange(long? from, long? to, long expected)
    {
        //Arrange
        var rangeHeader = new RangeHeaderValue(from, to);
        var calc = new RangeCalculator(rangeHeader); 
        
        //Act
        long resLen = calc.CalculateResultLength(100); 
        
        //Assert
        Assert.Equal(expected, resLen);
    }

    [Theory(DisplayName = "Shoult calc several ranges")]
    [InlineData(10, null, 180)]
    [InlineData(null, 10, 20)]
    [InlineData(10, 20, 20)]
    public void ShouldCalcSeveralRange(long? from, long? to, long expected)
    {
        //Arrange
        var rangeHeader = new RangeHeaderValue();
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(from, to));
        rangeHeader.Ranges.Add(new RangeItemHeaderValue(from, to));

        var calc = new RangeCalculator(rangeHeader); 
        
        //Act
        long resLen = calc.CalculateResultLength(100); 
        
        //Assert
        Assert.Equal(expected, resLen);
    }
}
