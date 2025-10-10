//using Application.Common.Models.Authen;
//using Application.Common.Models.User;

//namespace Application.Common.Interfaces;
//public interface IJwtUserService
//{
//    LogonUserInfo GetCurrentUser();
//    JwtTokenResultModel GenerateToken(LogonUserInfo model);
//}

//using Application.Common.Interfaces;
//using Application.Common.Models.Authen;
//using Application.Common.Models.User;
//using Application.Extensions;
//using Domain.Constants;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace Infrastructure.Service;
//public class JwtUserService : IJwtUserService
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly JwtOption _jwtOption;
//    public JwtUserService(IHttpContextAccessor httpContextAccessor, IOptions<JwtOption> jwtOption)
//    {
//        _httpContextAccessor = httpContextAccessor;
//        _jwtOption = jwtOption.Value;
//    }

//    public JwtTokenResultModel GenerateToken(LogonUserInfo model)
//    {
//        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.Key));
//        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
//        var claims = new List<Claim>()
//            {
//            new Claim(ClaimTypes.NameIdentifier,model.Username),
//            new Claim(ClaimTypes.Name,model.Username),
//            new Claim(ClaimTypes.Role,model.RoleName),
//            new Claim("RoleID",model.RoleID.ToString(),ClaimValueTypes.Integer32),
//            new Claim("UserID",model.UserID.ToString(),ClaimValueTypes.Integer32),
//        };
//        var token = new JwtSecurityToken(_jwtOption.Issuer, _jwtOption.Audience, claims,
//                 expires: DateTime.UtcNow.AddMinutes(_jwtOption.ExpiredTime),
//                 signingCredentials: credentials);
//        string jtoken = new JwtSecurityTokenHandler().WriteToken(token);
//        return new JwtTokenResultModel() { JwtToken = jtoken, Valid = true };
//    }

//    public LogonUserInfo GetCurrentUser()
//    {
//        try
//        {
//            if (_httpContextAccessor.HttpContext != null)
//            {
//                var user = _httpContextAccessor.HttpContext.User;
//                if (user != null && user.Claims.Count() > 0)
//                {
//                    var result = new LogonUserInfo();
//                    result.Username = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Tester";
//                    result.DisplayName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Tester";
//                    result.RoleName = user.FindFirst(ClaimTypes.Role)?.Value ?? "Tester";
//                    result.RoleID = user.Claims.FirstOrDefault(k => k.Type.ToString().Equals("RoleID")).Value.ToInt32();
//                    result.UserID = user.Claims.FirstOrDefault(k => k.Type.ToString().Equals("UserID")).Value.ToInt32();
//                    return result;
//                }
//                return new LogonUserInfo();
//            }
//            return new LogonUserInfo();
//        }
//        catch (Exception)
//        {
//            return new LogonUserInfo()
//            {
//                DisplayName = "Tester",
//                RoleID = 1,
//                RoleName = "Developer",
//                Email = "trankhoaa7@gmail.com",
//                UserID = 1,
//                Username = "tester"
//            };
//        }
//    }
//}
