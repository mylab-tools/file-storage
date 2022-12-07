using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class PathBehavior
    {
        [Fact]
        public void ShouldGetRelativePath()
        {
            //Arrange
            

            //Act
            var relative = Path.GetRelativePath("/var/fs/data", "/var/fs/data/subdirectory");

            //Assert
            Assert.Equal("subdirectory", relative);
        }
    }
}
