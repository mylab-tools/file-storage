using Moq;
using MyLab.FileStorage.Cleaner;

namespace UnitTests.Cleaner
{
    public partial class CleanerLogicBehavior
    {
        [Fact]
        public async Task ShouldDeleteRottenLostFiles()
        {
            //Arrange
            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(_strategyMock.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            _strategyMock.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            _strategyMock.Verify(s => s.DeleteDirectory(_lostRottenDir));
            _strategyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldDeleteOnlyRottenLost()
        {
            //Arrange
            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(_strategyMock.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            _strategyMock.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            _strategyMock.Verify(s => s.DeleteDirectory(_lostRottenDir), Times.Once);
            _strategyMock.Verify(s => s.DeleteDirectory(_confirmedRottenDir), Times.Never);
            _strategyMock.Verify(s => s.DeleteDirectory(_lostFreshDir), Times.Never);
            _strategyMock.Verify(s => s.DeleteDirectory(_confirmedFreshDir), Times.Never);
            _strategyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldDeleteEitherStored()
        {
            //Arrange
            var options = new CleanerOptions
            {
                Directory = BaseDir,
                LostFileTtlHours = 1,
                StoredFileTtlHours = 1
            };

            var logic = new CleanerTaskLogic(_strategyMock.Object, options);

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            _strategyMock.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()));
            _strategyMock.Verify(s => s.DeleteDirectory(_lostRottenDir), Times.Once);
            _strategyMock.Verify(s => s.DeleteDirectory(_confirmedRottenDir), Times.Once);
            _strategyMock.Verify(s => s.DeleteDirectory(_lostFreshDir), Times.Never);
            _strategyMock.Verify(s => s.DeleteDirectory(_confirmedFreshDir), Times.Never);
            _strategyMock.VerifyNoOtherCalls();
        }
    }
}