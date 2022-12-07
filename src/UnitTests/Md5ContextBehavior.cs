using MyLab.FileStorage.Tools;

namespace UnitTests;

public class Md5ContextBehavior
{
    [Fact]
    public void ShouldSerialize()
    {
        //Arrange
        var buffData = Enumerable
            .Repeat(0, 64)
            .Select((val, pos) => (byte)pos)
            .ToArray(); 
        var countData = new uint[]{ 8, 9 };
        var stateData = new uint[] { 0, 1, 2, 3 }; 

        var originCtx = new Md5Ex.Md5Context();
        buffData.CopyTo(originCtx.Buffer, 0);
        countData.CopyTo(originCtx.Count, 0);
        stateData.CopyTo(originCtx.State, 0);
        

        //Act
        var data = originCtx.Serialize();
        var restoredCtx = Md5Ex.Md5Context.Deserialize(data);

        //Assert
        Assert.NotNull(restoredCtx);
        Assert.Equal(originCtx.State, restoredCtx.State);
        Assert.Equal(originCtx.Count, restoredCtx.Count);
        Assert.Equal(originCtx.Buffer, restoredCtx.Buffer);

    }
}