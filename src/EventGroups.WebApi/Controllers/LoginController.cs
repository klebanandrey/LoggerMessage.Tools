using EventGroups.Contract;
using EventGroups.Storage;
using EventGroups.Storage.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventGroups.WebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class LoginController : Controller
    {
        private IConfiguration _config;
        private readonly EventGroupDbContext _context;
        private PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public LoginController(IConfiguration config, EventGroupDbContext context)
        {
            _config = config;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost("GetApiKey")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(LoginResponseDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<LoginResponseDTO>> GetApiKey([FromBody] UserDTO login)
        {
            ActionResult response = Unauthorized(new LoginResponseDTO());
            var user = await AuthenticateUser(login);

            if (user != null)
                return Ok(new LoginResponseDTO() { Token = GenerateJSONWebToken(user) });

            return response;
        }

        [AllowAnonymous]
        [HttpPost("SingIn")]
        public async Task<IActionResult> SingIn([FromBody] UserDTO login)
        {
            var user = await AuthenticateUser(login);
            if (user == null)
            {
                user = new User()
                {
                    EmailAddress = login.EmailAddress,
                    Username = login.Username
                };
                user.Password = _hasher.HashPassword(user, login.Password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var tokenString = GenerateJSONWebToken(user);           
            return Ok(new { token = tokenString });
        }

        [Authorize]
        [HttpGet("TestConnection")]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult TestConnection()
        {
            var curUserId = HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (_context.Users.Any(u => u.Id == curUserId))
                return Ok();

            return Unauthorized();
        }

        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.EmailAddress),                
                new Claim(JwtRegisteredClaimNames.Jti, userInfo.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                //expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<User> AuthenticateUser(UserDTO login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

            if (user != null && _hasher.VerifyHashedPassword(user, user.Password, login.Password) == PasswordVerificationResult.Success)
                return user;

            return null;
        }
    }
}
