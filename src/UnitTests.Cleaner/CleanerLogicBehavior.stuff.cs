using Moq;
using MyLab.FileStorage.Cleaner;
using MyLab.FileStorage.Tools;

namespace UnitTests.Cleaner;

public partial class CleanerLogicBehavior
{
    private const string BaseDir = "/var/fs/data";
    private readonly string _confirmedFreshDir;
    private readonly string _confirmedRottenDir;
    private readonly string _lostFreshDir;
    private readonly string _lostRottenDir;
    private readonly Mock<ICleanerStrategy> _strategyMock;

    public CleanerLogicBehavior()
    {
        var fidConverter = new FileIdToNameConverter(BaseDir)
        {
            Separator = '/'
        };

        var confirmedFreshFid = Guid.NewGuid();
        var confirmedRottenFid = Guid.NewGuid();
        var lostFreshFid = Guid.NewGuid();
        var lostRottenFid = Guid.NewGuid();

        _confirmedFreshDir = fidConverter.ToDirectory(confirmedFreshFid);
        _confirmedRottenDir = fidConverter.ToDirectory(confirmedRottenFid);
        _lostFreshDir = fidConverter.ToDirectory(lostFreshFid);
        _lostRottenDir = fidConverter.ToDirectory(lostRottenFid);

        var files = new FsFile[]
        {
            new(fidConverter.ToDirectory(confirmedFreshFid))
            {
                CreateDt = DateTime.Now.AddHours(0),
                Confirmed = true
            },
            new(fidConverter.ToDirectory(confirmedRottenFid))
            {
                CreateDt = DateTime.Now.AddHours(-2),
                Confirmed = true
            },
            new(fidConverter.ToDirectory(lostFreshFid))
            {
                CreateDt = DateTime.Now.AddHours(0),
                Confirmed = false
            },
            new(fidConverter.ToDirectory(lostRottenFid))
            {
                CreateDt = DateTime.Now.AddHours(-2),
                Confirmed = false
            }
        };

        _strategyMock = new Mock<ICleanerStrategy>();
        _strategyMock.Setup(s => s.GetFileDirectories(It.IsAny<CancellationToken>()))
            .Returns(files);
    }
}