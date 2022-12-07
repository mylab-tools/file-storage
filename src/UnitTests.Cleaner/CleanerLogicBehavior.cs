using Moq;
using MyLab.FileStorage.Cleaner;
using MyLab.FileStorage.Tools;

namespace UnitTests.Cleaner
{
    public class CleanerLogicBehavior
    {
        private const string BaseDir = "/var/fs/data";

        private readonly FsFile[] _files;
        private readonly string _confirmedFreshDir;
        private readonly string _confirmedRottenDir;
        private readonly string _lostFreshDir;
        private readonly string _lostRottenDir;

        public CleanerLogicBehavior()
        {
            var fidConverter = new FileIdToNameConverter(BaseDir)
            {
                PathSeparator = '/'
            };

            var confirmedFreshFid = Guid.NewGuid();
            var confirmedRottenFid = Guid.NewGuid();
            var lostFreshFid = Guid.NewGuid();
            var lostRottenFid = Guid.NewGuid();

            _confirmedFreshDir = fidConverter.ToDirectory(confirmedFreshFid);
            _confirmedRottenDir = fidConverter.ToDirectory(confirmedRottenFid);
            _lostFreshDir = fidConverter.ToDirectory(lostFreshFid);
            _lostRottenDir = fidConverter.ToDirectory(lostRottenFid);

            _files = new FsFile[]
            {
                new (fidConverter.ToDirectory(confirmedFreshFid))
                {
                    CreateDt = DateTime.Now.AddHours(0),
                    Confirmed = true
                },
                new (fidConverter.ToDirectory(confirmedRottenFid))
                {
                    CreateDt = DateTime.Now.AddHours(-2),
                    Confirmed = true
                },
                new (fidConverter.ToDirectory(lostFreshFid))
                {
                    CreateDt = DateTime.Now.AddHours(0),
                    Confirmed = false
                },
                new (fidConverter.ToDirectory(lostRottenFid))
                {
                    CreateDt = DateTime.Now.AddHours(-2),
                    Confirmed = false
                }
            };
        }

        [Fact]
        public async Task ShouldDeleteRottenLostFiles()
        {
            //Arrange
            var strategy = new Mock<ICleanerStrategy>();

            strategy.Setup(s => s.GetFileDirectories(It.IsAny<CancellationToken>()))
                .Returns(_files);

            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(strategy.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);
            
            //Assert
            strategy.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            strategy.Verify(s => s.DeleteDirectory(_lostRottenDir));
            strategy.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldDeleteOnlyRottenLost()
        {
            //Arrange
            var strategy = new Mock<ICleanerStrategy>();

            strategy.Setup(s => s.GetFileDirectories(It.IsAny<CancellationToken>()))
                .Returns(_files);

            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(strategy.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            strategy.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            strategy.Verify(s => s.DeleteDirectory(_lostRottenDir), Times.Once);
            strategy.Verify(s => s.DeleteDirectory(_confirmedRottenDir), Times.Never);
            strategy.Verify(s => s.DeleteDirectory(_lostFreshDir), Times.Never);
            strategy.Verify(s => s.DeleteDirectory(_confirmedFreshDir), Times.Never);
            strategy.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldDeleteEitherStored()
        {
            //Arrange
            var strategy = new Mock<ICleanerStrategy>();

            strategy.Setup(s => s.GetFileDirectories(It.IsAny<CancellationToken>()))
                .Returns(_files);

            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1,
                StoredFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(strategy.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            strategy.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            strategy.Verify(s => s.DeleteDirectory(_lostRottenDir), Times.Once);
            strategy.Verify(s => s.DeleteDirectory(_confirmedRottenDir), Times.Once);
            strategy.Verify(s => s.DeleteDirectory(_lostFreshDir), Times.Never);
            strategy.Verify(s => s.DeleteDirectory(_confirmedFreshDir), Times.Never);
            strategy.VerifyNoOtherCalls();
        }
    }
}