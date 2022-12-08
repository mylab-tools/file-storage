using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.FileStorage.Cleaner;
using MyLab.FileStorage.Tools;
using MyLab.TaskApp;
using Xunit.Abstractions;

namespace FuncTests.Cleaner
{
    public class CleanerBehavior : IClassFixture<TestApi<Program, ICleanerApi>>
    {
        private const string BaseDir = "/var/fs/data";

        private readonly TestApi<Program, ICleanerApi> _api;
        
        private readonly string _confirmedFreshDir;
        private readonly string _confirmedRottenDir;
        private readonly string _lostFreshDir;
        private readonly string _lostRottenDir;
        private readonly Mock<ICleanerStrategy> _strategyMock;

        public CleanerBehavior(TestApi<Program, ICleanerApi> api, ITestOutputHelper output)
        {
            _api = api;
            _api.Output = output;

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

            var files = new FsFile[]
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

            _strategyMock = new Mock<ICleanerStrategy>();
            _strategyMock.Setup(s => s.GetFileDirectories(It.IsAny<CancellationToken>()))
                .Returns(files);
        }

        [Fact]
        public async Task ShouldCleanup()
        {
            //Arrange
            var api = _api.StartWithProxy(srv => 
                srv.AddSingleton(_strategyMock.Object)
                    .Configure<CleanerOptions>(opt =>
                    {
                        opt.Directory = BaseDir;
                        opt.LostFileTtlHours = 1;
                    }));

            //Act
            await api.ProcessAsync();
            await Task.Delay(1000);

            //Assert
            _strategyMock.Verify(s => s.GetFileDirectories(It.IsAny<CancellationToken>()), Times.Once);
            _strategyMock.Verify(s => s.DeleteDirectory(_lostRottenDir), Times.Once);
            _strategyMock.VerifyNoOtherCalls();
        }
    }

    [Api]
    public interface ICleanerApi
    {
        [Post("processing")]
        Task ProcessAsync();
    }
}