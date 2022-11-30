using System.Text;
using MyLab.FileStorage.Tools;

namespace UnitTests
{
    public class Md5ExBehavior
    {
        [Fact]
        public void ShouldCalculateMd5()
        {
            //Arrange
            const string data = "1234567890";

            var data1 = Encoding.UTF8.GetBytes(data.Substring(0, 5));
            var data2 = Encoding.UTF8.GetBytes(data.Substring(5, 5));


            var hasher = new Md5Ex();

            //Act
            hasher.AppendData(data1);
            hasher.AppendData(data2);
            var hash = hasher.FinalHash();
            
            var strHash = BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLower();
            //Assert
            Assert.NotNull(strHash);
            Assert.Equal("e807f1fcf82d132f9bb018ca6738a19f", strHash);
        }

        [Fact]
        public void ShouldRestoreState()
        {
            //Arrange
            const string data = "1234567890";

            var data1 = Encoding.UTF8.GetBytes(data.Substring(0, 5));
            var data2 = Encoding.UTF8.GetBytes(data.Substring(5, 5));


            var initialHasher = new Md5Ex();

            //Act
            initialHasher.AppendData(data1);

            var initialCtx = initialHasher.Context;
            
            var binCtx = initialCtx.SerializeAsync();
            
            var restoredCtx = Md5Ex.Md5Context.DeserializeAsync(binCtx);
            var restoredHasher = new Md5Ex(restoredCtx);

            restoredHasher.AppendData(data2);
            var hash = restoredHasher.FinalHash();

            var strHash = BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLower();
            //Assert
            Assert.NotNull(strHash);
            Assert.Equal("e807f1fcf82d132f9bb018ca6738a19f", strHash);
        }
    }
}
