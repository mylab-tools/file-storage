using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MyLab.FileStorage.Models;
using MyLab.FileStorage.Tools;
using Xunit.Abstractions;

namespace UnitTests
{
    public class FileTokenBehavior
    {
        private readonly ITestOutputHelper _output;
        private const string Secret = "1234567890123456";

        /// <summary>
        /// Initializes a new instance of <see cref="FileTokenBehavior"/>
        /// </summary>
        public FileTokenBehavior(ITestOutputHelper output)
        {
            _output = output;

            IdentityModelEventSource.ShowPII = true;
        }

        [Fact]
        public void ShouldPassValidToken()
        {
            //Arrange
            StoredFileMetadataDto storedFileMetadataDto = new StoredFileMetadataDto
            {
                Id = Guid.NewGuid(),
                Filename = "foo.ext"
            };

            var fileToken = new FileToken(storedFileMetadataDto);
            var serialized = fileToken.Serialize(Secret, TimeSpan.FromSeconds(50));

            _output.WriteLine(serialized);

            FileToken actualFileToken;

            //Act
            try
            {
                actualFileToken = FileToken.VerifyAndDeserialize(serialized, Secret);
            }
            catch (SecurityTokenValidationException e)
            {
                throw;
            }

            //Assert
            Assert.NotNull(actualFileToken);
            Assert.NotNull(actualFileToken.FileMetadata);
            Assert.Equal(storedFileMetadataDto.Id, actualFileToken.FileMetadata!.Id);
            Assert.Equal(storedFileMetadataDto.Filename, actualFileToken.FileMetadata.Filename);
        }

        [Theory]
        [MemberData(nameof(GetInvalidTokenCases))]
        public void ShouldFailInvalidTokens(string desc, string token, Type expectedExceptionType)
        {
            //Arrange
            SecurityTokenValidationException? exception = null;

            //Act
            try
            {
                FileToken.VerifyAndDeserialize(token, Secret);
            }
            catch (SecurityTokenValidationException e)
            {
                exception = e;
            }

            //Assert
            Assert.NotNull(exception);
            Assert.IsType(expectedExceptionType, exception);
        }

        public static IEnumerable<object[]> GetInvalidTokenCases()
        {
            return new[]
            {
                new object[] { "expired", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmbWV0YSI6IntcImlkXCI6XCJjMTcwOGE1YTA0YmE0YjgzYjE0NzFjNjI2ZGQ2NjgwMVwiLFwicHVycG9zZVwiOm51bGwsXCJjcmVhdGVkXCI6bnVsbCxcIm1kNVwiOm51bGwsXCJmaWxlbmFtZVwiOlwiZm9vLmV4dFwiLFwibGVuZ3RoXCI6MCxcImxhYmVsc1wiOm51bGx9IiwibmJmIjoxNjcxMTEzNzkyLCJleHAiOjE2NzExMTM4NDIsImlhdCI6MTY3MTExMzc5Mn0.TWKSmIVEM0jJ5ZqJDwwS8tcvxQeKUaGEpPgdc025qPY", typeof(SecurityTokenExpiredException)},
                new object[] { "wrong sign", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmbWV0YSI6IntcImlkXCI6XCJjMTcwOGE1YTA0YmE0YjgzYjE0NzFjNjI2ZGQ2NjgwMVwiLFwicHVycG9zZVwiOm51bGwsXCJjcmVhdGVkXCI6bnVsbCxcIm1kNVwiOm51bGwsXCJmaWxlbmFtZVwiOlwiZm9vLmV4dFwiLFwibGVuZ3RoXCI6MCxcImxhYmVsc1wiOm51bGx9IiwibmJmIjoxNjcxMTEzNzkyLCJleHAiOjE4NzExMTM4NDIsImlhdCI6MTY3MTExMzc5Mn0.81ebUXX1CNS9WwdlGW0qOM_fL1cgU1A9qpyBYEz-FFc", typeof(SecurityTokenSignatureKeyNotFoundException)},
                new object[] { "wrong fmeta", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmbWV0YSI6Indyb25nLXZhbHVlIiwibmJmIjoxNjcxMTEzNzkyLCJleHAiOjE4NzExMTM4NDIsImlhdCI6MTY3MTExMzc5Mn0.cTPFjBEBsRa_-9Q-OSuX__HQf6FgEJ0siZP6Fzoi3No", typeof(SecurityTokenValidationException) },
                new object[] { "without fmeta", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2NzExMTM3OTIsImV4cCI6MTg3MTExMzg0MiwiaWF0IjoxNjcxMTEzNzkyfQ.UP7gdfDDxe1y7TUXebcbhpRhMDYEQmcNbg2q_11W_ic", typeof(SecurityTokenValidationException) },
            };
        }
    }
}
