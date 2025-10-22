using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class BlazorSchoolAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly BlazorSchoolUserService _blazorSchoolUserService;
    public User? CurrentUser { get; set; } = new();
    public NavigationManager _navigation;
    public BlazorSchoolAuthenticationStateProvider(BlazorSchoolUserService blazorSchoolUserService, NavigationManager navigation)
    {
        _blazorSchoolUserService = blazorSchoolUserService;
        _navigation = navigation;
    }
    public async Task LoginAsync(string role ,string returnURL)
    {
        var principal = new ClaimsPrincipal();
        var user = await _blazorSchoolUserService.SendAuthenticateRequestAsync("", "", role);

        if (user is not null)
        {
            principal = user.ToClaimsPrincipal();
            CurrentUser = user;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            // Redirect to return URL
            if (!string.IsNullOrWhiteSpace(returnURL))
            {
                _navigation.NavigateTo(returnURL, forceLoad: true);
            }
            else
            {
                _navigation.NavigateTo("/", forceLoad: true);
            }
        }
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();
        var token = await _blazorSchoolUserService.GetTokenFromBrowserAsync();//AccessToken

        if (!string.IsNullOrEmpty(token))
        {
            var claimsPrincipal =  _blazorSchoolUserService.CreateClaimsPrincipalFromToken(token);

            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                principal = claimsPrincipal;
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            }
            var user = User.UserFromClaimPricipal(claimsPrincipal);
            CurrentUser = user;
        }
        return new(principal);
    }
    public async Task Logout()
    {
        CurrentUser = null;
        await _blazorSchoolUserService.ClearBrowserUserData();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
    }
}