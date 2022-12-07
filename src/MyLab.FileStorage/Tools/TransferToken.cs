using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyLab.FileStorage.Tools
{
    class TransferToken
    {
        private const string FileIdClaim = "fid";

        public Guid FileId { get; }

        public TransferToken(Guid fileId)
        {
            FileId = fileId;
        }

        public static TransferToken New()
        {
            return new TransferToken(Guid.NewGuid());
        }

        public static TransferToken VerifyAndDeserialize(string tokenStr, string secret)
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

            var fileIdField = jwt.Claims.FirstOrDefault(c => c.Type == FileIdClaim);

            if (fileIdField == null)
                throw new SecurityTokenValidationException($"The token does not contains a {FileIdClaim} claim");

            if (fileIdField.Value == null)
                throw new SecurityTokenValidationException($"The token contains an empty {FileIdClaim} claim");

            if(!Guid.TryParse(fileIdField.Value, out var fileId))
                throw new SecurityTokenValidationException($"The token contains {FileIdClaim} claim with wrong format");

            return new TransferToken(fileId);

        }

        public string Serialize(string secret, TimeSpan lifetime)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    { FileIdClaim, FileId.ToString("N") } 
                },
                Expires = DateTime.UtcNow.Add(lifetime),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
