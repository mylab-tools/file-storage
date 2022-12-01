using MyLab.FileStorage.Tools;

namespace UnitTests;

public class FileIdToNameConverterBehavior
{
    private const string BasePath = "/var/fs/data";

    private readonly Guid _fileId;
    private readonly FileIdToNameConverter _converter;

    public FileIdToNameConverterBehavior()
    {
        _converter = new FileIdToNameConverter(BasePath)
        {
            PathSeparator = '/'
        };
        _fileId = Guid.Parse("afd6796919894dc18348726ace4bd4f6");
    }

    [Fact]
    public void ShouldProvideDirectory()
    {
        //Arrange
        

        //Act
        var dir = _converter.ToDirectory(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/1989/4dc1/8348/726a/ce4b/d4f6", dir);
    }

    [Fact]
    public void ShouldProvideDataFilename()
    {
        //Arrange


        //Act
        var dir = _converter.ToContentFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/1989/4dc1/8348/726a/ce4b/d4f6/content.bin", dir);
    }

    [Fact]
    public void ShouldProvideMetaFilename()
    {
        //Arrange


        //Act
        var dir = _converter.ToMetadataFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/1989/4dc1/8348/726a/ce4b/d4f6/metadata.json", dir);
    }

    [Fact]
    public void ShouldProvideHashCtxFilename()
    {
        //Arrange


        //Act
        var dir = _converter.ToHashCtxFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/1989/4dc1/8348/726a/ce4b/d4f6/hash-ctx.bin", dir);
    }
}