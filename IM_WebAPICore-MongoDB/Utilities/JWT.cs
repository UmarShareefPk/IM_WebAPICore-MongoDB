using IM_DataAccess.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IM_WebAPICore_MongoDB.Utilities
{
    public interface IJWT
    {
        string GenerateToken(User user, DateTime expiration);
    }

    public class JWT : IJWT
    {
        private readonly IConfiguration _config;

        public JWT(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user, DateTime expiration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JwtSecret")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstNmae", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var token = new JwtSecurityToken("IM Core with Mongo DB",
                "all",
              claims,
              expires: expiration,
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
