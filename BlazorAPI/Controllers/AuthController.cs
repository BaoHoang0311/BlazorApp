using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorAPI.Controllers
{
    public class LoginDTO
    {
        public string? Name { get; set; }
        public string? Password { get; set; }
    }
    public class TokenResponse
    {
        public string AccessToken { get;set;}
        public string RefreshToken { get; set; }
    }
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public AuthController()
        {

        }
        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="Name">Nhập name </param>
        /// <param name="Password">Nhập pass</param>
        /// <returns></returns>
        [HttpPost("/login")]
        public async Task<IActionResult> Login(string Name ="bao", string Password= "bao")
        {
            var model = new LoginDTO
            {
                Name = Name ,
                Password = Password,
            };

            if (model.Name != "bao") return NotFound("password sai");

            #region CreateToken

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("this_is_my_secret_keyyyyyyyyyyyyyyyyyyyyyyyyyyyy");
            var tokenDes = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id","123"),
                    new Claim(ClaimTypes.Name,model.Name),
                    new Claim("hihi","Bao go hi hi"),
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            var roles =new string[] {"Admin", "User" };
            foreach (var role in roles)
            {
                tokenDes.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            var token = tokenHandler.CreateToken(tokenDes);
            #endregion
            var accessToken = tokenHandler.WriteToken(token);
            var tokenResponse = new TokenResponse()
            {
                AccessToken = accessToken,
                RefreshToken = ""
            };
            return Ok(tokenResponse);
        }
    }
}
