using Blazored.LocalStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
public class LoginDTO
{
    public string? Name { get;set;}
    public string? Password { get;set; }
    public string Role { get; set; }
}
public class Token
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
public class BlazorSchoolUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public BlazorSchoolUserService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }
    public async Task<User?> SendAuthenticateRequestAsync(string username, string password, string role)
    {
        var data = new LoginDTO()
        {
            Name = "bao",
            Password= "bao",
            Role = role
        };
        var response = await _httpClient.PostAsJsonAsync<LoginDTO>($"http://localhost:5259/login",data);

        if (response.IsSuccessStatusCode)
        {
            string token = await response.Content.ReadAsStringAsync();
            var TokenResponse = JsonConvert.DeserializeObject<Token>(token);
            var claimPrincipal = MapClaimsPrincipalFromToken(TokenResponse.AccessToken);// get claim from token
            var user = User.UserFromClaimPricipal(claimPrincipal);
            await PersistUserToBrowser(TokenResponse.AccessToken);

            return user;
        }

        return null;
    }
    public ClaimsPrincipal MapClaimsPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var identity = new ClaimsIdentity();

        if (tokenHandler.CanReadToken(token))
        {
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims.Select(c =>
            {
                if (c.Type == "role") // 👈 map thủ công
                    return new Claim(ClaimTypes.Role, c.Value);
                if (c.Type == "unique_name")
                    return new Claim(ClaimTypes.Name, c.Value);
                return c;
            }).ToList();
            identity = new(claims, "Blazor School");
        }

        return new(identity);
    }
    private async Task PersistUserToBrowser(string token) => await _localStorage.SetItemAsStringAsync("Token", token  );
    public async Task ClearBrowserUserData() => await _localStorage.RemoveItemAsync("Token");
    // 🟢 Lấy token khi load lại app
    public async Task<string?> GetTokenFromBrowserAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("Token");
            return token;
        }
        catch
        {
            return null;
        }
    }
}