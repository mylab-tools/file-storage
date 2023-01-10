using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

#if SERVER_APP
using MyLab.FileStorage.Models;

namespace MyLab.FileStorage.Tools
#else
using MyLab.FileStorage.Client.Models;
using System.Linq;

namespace MyLab.FileStorage.Client.Tools
#endif
{
    /// <summary>
    /// Represent document token
    /// </summary>
    public class FileToken
    {
        private const string FileMetadataClaim = "fmeta";

        /// <summary>
        /// Stored file metadata
        /// </summary>
        public StoredFileMetadataDto FileMetadata { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FileToken"/>
        /// </summary>
        public FileToken(StoredFileMetadataDto fileMetadata)
        {
            FileMetadata = fileMetadata;
        }

        /// <summary>
        /// Verify sign and deserialize an object
        /// </summary>
        /// <exception cref="SecurityTokenValidationException">Invalid token</exception>
        public static FileToken VerifyAndDeserialize(string tokenStr, string secret)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(tokenStr, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = securityKey
            }, out SecurityToken validatedToken);

            var jwt = validatedToken as JwtSecurityToken;

            if (jwt == null)
                throw new SecurityTokenValidationException("Wrong token format. A JWT token is required");

            var fileMetadataField = jwt.Claims.FirstOrDefault(c => c.Type == FileMetadataClaim);

            if (fileMetadataField == null)
                throw new SecurityTokenValidationException($"The token does not contains a {FileMetadataClaim} claim");

            StoredFileMetadataDto? fileMetadataDto;

            try
            {
                fileMetadataDto = JsonConvert.DeserializeObject<StoredFileMetadataDto>(fileMetadataField.Value);
            }
            catch (JsonException e)
            {
                throw new SecurityTokenValidationException($"The claim {FileMetadataClaim} has wrong format", e);
            }

            if(fileMetadataDto == null)
                throw new SecurityTokenValidationException($"The claim {FileMetadataClaim} has wrong format");

            return new FileToken(fileMetadataDto);

        }

#if SERVER_APP

        /// <summary>
        /// Serializes token to string with secret and TTL
        /// </summary>
        public string Serialize(string secret, TimeSpan lifetime)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    { FileMetadataClaim, JsonConvert.SerializeObject(FileMetadata) }
                },
                Expires = DateTime.UtcNow.Add(lifetime),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

#endif
    }
}
