using EventGroups.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventGroups.App.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly EventGroupDbContext _context;
        private readonly IConfiguration _config;

        public ApiKeyService(EventGroupDbContext dbContext, IConfiguration config)
        {
            _context = dbContext;
            _config = config;
        }

        public async Task<string> GenerateApiKey(string username)
        {
            var user = await AuthenticateUser(username);

            if (user != null)
                return GenerateJSONWebToken(user);

            return String.Empty;
        }

        private async Task<IdentityUser> AuthenticateUser(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username); ;
        }

        private string GenerateJSONWebToken(IdentityUser userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, userInfo.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                //expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
