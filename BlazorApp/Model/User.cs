using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.Security.Claims;

public class User
{
    public string Username { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public int Age { get;set;}
    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var claim = new List<Claim>()
        {
            new Claim("unique_name", Username),
            new Claim("age",Age.ToString()),
        };
        foreach (var r in Roles)
        {
            claim.Add(new Claim("role",r));
        }
        var identity = new ClaimsIdentity(claim, "Blazor School");
        return new ClaimsPrincipal(identity);
    }
    public static User UserFromClaimPricipal(ClaimsPrincipal principal)
    {
        var User = new User()
        {
            Username = principal.Claims.FirstOrDefault(x =>
                      x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            )?.Value ?? "",

            Roles = principal.Claims
            .Where(x =>
                x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            )
            .Select(c => c.Value)
            .ToList(),
            Age = Convert.ToInt32(principal.Claims.FirstOrDefault(x => x.Type == "age").Value),
        };

        return User;
    }
}
public class AdultRequirement : IAuthorizationRequirement
{
    public int MinimumAgeToConsiderAnAdult { get; set; } = 18;
}
public class AdultRequirementHandler : AuthorizationHandler<AdultRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdultRequirement requirement)
    {
        var user = User.UserFromClaimPricipal(context.User);

        if (user.Age >= requirement.MinimumAgeToConsiderAnAdult)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class EsrbRequirement : IAuthorizationRequirement
{
}
public class EsrbRequirementHandler : AuthorizationHandler<EsrbRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EsrbRequirement requirement)
    {
        var user = User.UserFromClaimPricipal(context.User);
        int minimumAge = Convert.ToInt32(context.Resource);

        if (user.Age >= minimumAge)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}