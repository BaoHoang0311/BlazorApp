using Google.Apis.Auth;
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
        [JsonProperty("id_token")]
        public string IdToken { get; set; }
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
        private readonly IConfiguration _config;
        private string clientId;
        private string clientSecret;
        public AuthController(IConfiguration configuration)
        {
            _config = configuration;
            clientId = _config["Authentication:Google:ClientId"];
            clientSecret = _config["Authentication:Google:ClientSecret"];
        }
        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="Name">Nhập name </param>
        /// <param name="Password">Nhập pass</param>
        /// <param name="Role">Nhập pass</param>
        /// <returns></returns>
        public class LoginDTO
        {
            public string Name { get; set; }
            public string Password { get;set;}
            public string Role { get;set;}
        }
        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody]LoginDTO data)
        {
            var model = new LoginDTO
            {
                Name = data.Name ,
                Password = data.Password,
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
                    new Claim(ClaimTypes.Role,data.Role),
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            //var roles =new string[] {"Admin", "User" };
            //foreach (var role in roles)
            //{
            //    tokenDes.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            //}

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
                "https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/userinfo.profile",
            };
            string scopes = Uri.EscapeDataString(string.Join(" ", scopesssss));
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={clientId}" +
                $"&redirect_uri=http://localhost:5259/call-back" +
                $"&response_type=code" +
                $"&scope={scopes}" +
                $"&access_type=offline" +
                $"&include_granted_scopes=true";
            return Ok(authUrl);
        }
        [HttpGet("/call-back")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            // Exchange code lấy token
            var tokenResponse = await ExchangeCodeForToken(code);

            // Lưu token vào database/session
            // ...

            return Ok(tokenResponse);
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
        [HttpPost("/check-token")]
        public async Task<IActionResult> CheckToken(string IdToken)
        {
            var payload = await CheckGGToken(IdToken);
            return Ok(payload);
        }
        private async Task<GoogleJsonWebSignature.Payload> CheckGGToken(string IdToken)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(IdToken);
            return payload;
        }
    }
}
