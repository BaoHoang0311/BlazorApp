using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
    public class TokenGGResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
    public class UserInfo
    {
        //        {
        //    "sub": "103980055730381311248",
        //    "picture": "https://lh3.googleusercontent.com/a-/ALV-UjXhqNylN4McuY4x259A1nu06kAe_n092ACE_CYHwYFdxuQW42C0=s96-c",
        //    "email": "tan.thach36@gmail.com",
        //    "email_verified": true
        //}
        [JsonProperty("sub")]
        public string Sub { get; set; }
        [JsonProperty("picture")]
        public string Picture { get;set;}
        [JsonProperty("email")]
        public string Email { get;set;}
        [JsonProperty("email_verified")]
        public string Email_Verified { get;set;}
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
            var tokenResponse = new TokenGGResponse()
            {
                AccessToken = accessToken,
                RefreshToken = ""
            };
            return Ok(tokenResponse);
        }
        /// <summary>
        ///     Login Google
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost("/login-google")]
        public async Task<IActionResult> LoginGoogle()
        {
            var scopesssss = new List<string>()
            {
                "https://www.googleapis.com/auth/drive.metadata.readonly",
                "https://www.googleapis.com/auth/calendar.readonly",
                "https://www.googleapis.com/auth/userinfo.email"
            };
            string scopes = Uri.EscapeDataString(string.Join(" ", scopesssss));
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id=" +
                $"&redirect_uri=http://localhost:5259/call-back" +
                $"&response_type=code" +
                $"&scope={scopes}" +
                $"&access_type=offline" +
                $"&include_granted_scopes=true";

            return Ok(Redirect(authUrl));
        }
        [HttpGet("/call-back")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            // Exchange code lấy token
            var tokenResponse = await ExchangeCodeForToken(code);

            // Lưu token vào database/session
            // ...

            return Ok(new { access_token = tokenResponse.AccessToken,refresh_token = tokenResponse.RefreshToken });
        }
        [HttpPost("/info/{accesstoken}")]
        public async Task<IActionResult> GetInfo(string accesstoken)
        {
            using var client = new HttpClient();
            string aaaa = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accesstoken}";
            var response = await client.GetAsync(aaaa);
            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserInfo>(json);
            return Ok(user);
        }
        private async Task<TokenGGResponse> ExchangeCodeForToken(string code)
        {
            var clientId = "";
            var clientSecret = "";
            var redirectUri = "http://localhost:5259/call-back";
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"redirect_uri", redirectUri},
                {"grant_type", "authorization_code"}
            });
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenGGResponse>(json);
            return token;
        }
    }
}
