using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
public class LoginDTO
{
    public string? Name { get;set;}
    public string? Password { get;set; }
}
public class Token
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
public class BlazorSchoolUserService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationDataMemoryStorage _authenticationDataMemoryStorage;

    public BlazorSchoolUserService(HttpClient httpClient, AuthenticationDataMemoryStorage authenticationDataMemoryStorage)
    {
        _httpClient = httpClient;
        _authenticationDataMemoryStorage = authenticationDataMemoryStorage;
    }
    public async Task<User?> SendAuthenticateRequestAsync(string username, string password)
    {
        var data = new LoginDTO()
        {
            Name = "bao",
            Password= "bao"
        };
        var response = await _httpClient.PostAsJsonAsync<LoginDTO>($"http://localhost:5259/login",data);

        if (response.IsSuccessStatusCode)
        {
            string token = await response.Content.ReadAsStringAsync();
            var TokenResponse = JsonConvert.DeserializeObject<Token>(token);
            var claimPrincipal = CreateClaimsPrincipalFromToken(TokenResponse.AccessToken);
            var user = User.FromClaimsPrincipal(claimPrincipal);
            PersistUserToBrowser(token);

            return user;
        }

        return null;
    }
    private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var identity = new ClaimsIdentity();

        if (tokenHandler.CanReadToken(token))
        {
            var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
            identity = new(jwtSecurityToken.Claims, "Blazor School");
        }

        return new(identity);
    }
    private void PersistUserToBrowser(string token) => _authenticationDataMemoryStorage.Token = token;
    public User? FetchUserFromBrowser()
    {
        var claimsPrincipal = CreateClaimsPrincipalFromToken(_authenticationDataMemoryStorage.Token);
        var user = User.FromClaimsPrincipal(claimsPrincipal);

        return user;
    }
    public void ClearBrowserUserData() => _authenticationDataMemoryStorage.Token = "";
}