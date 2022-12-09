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
            Separator = '/'
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
        Assert.Equal("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6", dir);
    }

    [Fact]
    public void ShouldProvideDataFilename()
    {
        //Arrange


        //Act
        var fn = _converter.ToContentFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6/content.bin", fn);
    }

    [Fact]
    public void ShouldProvideMetaFilename()
    {
        //Arrange


        //Act
        var fn = _converter.ToMetadataFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6/metadata.json", fn);
    }

    [Fact]
    public void ShouldProvideHashCtxFilename()
    {
        //Arrange


        //Act
        var fn = _converter.ToHashCtxFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6/hash-ctx.bin", fn);
    }

    [Fact]
    public void ShouldProvideConfirmFilename()
    {
        //Arrange


        //Act
        var fn = _converter.ToConfirmFile(_fileId);

        //Assert
        Assert.Equal("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6/confirmed.dt", fn);
    }

    [Fact]
    public void ShouldProvideIdFromDirectoryPath()
    {
        //Arrange
        

        //Act
        var fid = _converter.GetIdFromDirectory("/var/fs/data/afd6/7969/19894dc18348726ace4bd4f6");

        //Assert
        Assert.Equal(_fileId, fid);
    }
}