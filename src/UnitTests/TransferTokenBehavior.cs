using MyLab.FileStorage.Tools;
using Xunit.Abstractions;

namespace UnitTests
{
    public class TransferTokenBehavior
    {
        private readonly ITestOutputHelper _output;
        private const string TestSecret = "1234567890123456";
        
        public TransferTokenBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldRestoreUploadToken()
        {
            //Arrange
            const string fileIdStr = "37a954db963b464c961a8fc37e7a5684";
            const string tokenStr =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmaWQiOiIzN2E5NTRkYjk2M2I0NjRjOTYxYThmYzM3ZTdhNTY4NCIsIm5iZiI6MTY2OTY0ODgyMCwiZXhwIjoxNzk5NjU4ODIxLCJpYXQiOjE2Njk2NDg4MjB9.AoNWlKrpR2_DciWB2d-qUoF8SvFTMxKYu2X56YDJngY";
            var fileId = Guid.Parse(fileIdStr);

            //Act
            var actualToken = TransferToken.VerifyAndDeserialize(tokenStr, TestSecret);

            //Assert
            Assert.NotNull(actualToken);
            Assert.Equal(fileId, actualToken.FileId);

        }

        [Fact]
        public void ShouldRestoreSelfSerializedToken()
        {
            //Arrange
            var originToken = TransferToken.New();
            var tokenStr = originToken.Serialize(TestSecret, TimeSpan.FromSeconds(1));

            _output.WriteLine(tokenStr);

            //Act
            var actualToken = TransferToken.VerifyAndDeserialize(tokenStr, TestSecret);

            //Assert
            Assert.NotNull(actualToken);
            Assert.Equal(originToken.FileId, actualToken.FileId);
        }

        [Fact]
        public void ShouldGenerateNewFileId()
        {
            //Arrange
            var token = TransferToken.New();

            //Act


            //Assert
            Assert.NotEqual(Guid.Empty, token.FileId);
        }
    }
}