using BlazorApp;
using BlazorApp.Model;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<BlazorSchoolUserService>();
builder.Services.AddScoped<BlazorSchoolAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<BlazorSchoolAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthorizationHandler, AdultRequirementHandler>();
builder.Services.AddAuthorizationCore(config =>
{
    config.AddPolicy("AdultAdminOnly", policy =>
    {
        policy.AddRequirements(new AdultRequirement());
        policy.RequireRole("admin");
    });
    config.AddPolicy("EsrbPolicy", policy => policy.AddRequirements(new EsrbRequirement()));

});

builder.Services.AddHttpClient<SecondApiHttpClientWrapper>(httpClient => httpClient.BaseAddress = new("http://localhost:5259"));

await builder.Build().RunAsync();
