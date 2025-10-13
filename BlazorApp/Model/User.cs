using System.Reflection;
using System.Security.Claims;

public class User
{
    public string Username { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var claim = new List<Claim>()
        {
            new Claim("unique_name", Username),
        };
        foreach (var r in Roles)
        {
            claim.Add(new Claim(ClaimTypes.Role,r));
        }
        var identity = new ClaimsIdentity(claim, "Blazor School");
        return new ClaimsPrincipal(identity);
    }
    public static User UserFromClaimPricipal(ClaimsPrincipal principal)
    {
        var User = new User()
        {
            Username = principal.Claims.FirstOrDefault(x =>
                      x.Type == ClaimTypes.Name ||
                      x.Type == "name" ||
                      x.Type == "unique_name" ||
                      x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            )?.Value ?? "",

            Roles = principal.Claims
            .Where(x =>
                x.Type == ClaimTypes.Role ||
                x.Type == "role" ||
                x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            )
            .Select(c => c.Value)
            .ToList(),
        };
        return User;
    }
}