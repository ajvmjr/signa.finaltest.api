using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Signa.TemplateCore.Api.Security
{
    public class TokenConfigurations
    {
        public int Minutes { get; set; }
    }

    public class SigningConfigurations
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningConfigurations(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                using (var provider = new RSACryptoServiceProvider(2048))
                {
                    Key = new RsaSecurityKey(provider.ExportParameters(true));
                }
                SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.RsaSha256Signature);
            }
            else
            {
                Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
                SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256Signature);
            }
        }
    }

    public class RefreshTokenData
    {
        public string RefreshToken { get; set; }
        public int UserId { get; set; }
        public int UserGroupId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
