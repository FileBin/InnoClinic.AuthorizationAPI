using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Filebin.Common.Util;
using Filebin.Common.Models.Auth;
using Filebin.Domain.Auth.Abstraction;
using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer.Services;

public class TokenService(IConfiguration config, UserManager<IdentityUser> userManager)
: ITokenService {
    private readonly SymmetricSecurityKey jwtSecurityKey = config.GetSecurityKey();

    public async Task<ITokenPair> GenerateTokenAsync(IdentityUser user) {
        var id_claim = new Claim(ClaimTypes.NameIdentifier, user.Id);

        var access_claims = new[] {
            new Claim(ClaimTypes.Name, user.UserName ?? Misc.NullMarker),
            id_claim,
        }.ToList();

        var roles = await userManager.GetRolesAsync(user);
        access_claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));

        var access_lifetime = TimeSpan.FromMinutes(Convert.ToInt32(config.GetOrThrow("AccessExpiryInMinutes")));
        var refresh_lifetime = TimeSpan.FromDays(Convert.ToInt32(config.GetOrThrow("RefreshExpiryInDays")));

        return new LoginResultDto {
            AccessToken = GenerateToken(access_claims, access_lifetime),
            RefreshToken = GenerateToken([id_claim], refresh_lifetime),
        };
    }

    string GenerateToken(IEnumerable<Claim> claims, TimeSpan lifetime) {

        var token = new JwtSecurityToken(
            config.GetOrThrow("JwtIssuer"),
            config.GetOrThrow("JwtAudience"),
            claims,
            expires: DateTime.Now.Add(lifetime),
            signingCredentials: new SigningCredentials(jwtSecurityKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}