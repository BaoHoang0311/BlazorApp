using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class GoogleAccessTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleAccessTokenAuthenticationHandler> _logger;

    public GoogleAccessTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory)
        : base(options, loggerFactory, encoder)
    {
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<GoogleAccessTokenAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        string authHeader = Request.Headers.Authorization!;
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Authorization Header");

        string accessToken = authHeader["Bearer ".Length..].Trim();

        try
        {
            // 1️⃣ Gọi Google API để xác thực token

            //var httpClient = _httpClientFactory.CreateClient();
            //httpClient.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            //if (!response.IsSuccessStatusCode)
            //    return AuthenticateResult.Fail("Invalid Google Access Token");

            //var payload = await response.Content.ReadAsStringAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "bao"),
                new Claim(ClaimTypes.Email, "tan.thach36@gmail.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Cus"),
            };

            var identity = new ClaimsIdentity(claims, "GoogleAccessToken");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "GoogleAccessToken");

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google access token");
            return AuthenticateResult.Fail("Exception during token validation");
        }
    }
}

