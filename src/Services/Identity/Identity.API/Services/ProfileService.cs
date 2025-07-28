using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Identity.API.Services;

public class ProfileService : IProfileService
{
    public ProfileService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

        var subjectId = subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? string.Empty;

        var user = await _userManager.FindByIdAsync(subjectId);
        if (user == null)
            throw new ArgumentException("Invalid subject identifier");
        
        var roles = await _userManager.GetRolesAsync(user);

        var userClaims = GetClaimsFromUser(user);
        var roleClaims = await GetClaimFromRole(roles.ToList());
        context.IssuedClaims = userClaims.Concat(roleClaims).ToList();
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

        var subjectId = subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? string.Empty;
        var user = await _userManager.FindByIdAsync(subjectId);

        context.IsActive = false;

        if (user != null)
        {
            if (_userManager.SupportsUserSecurityStamp)
            {
                var securityStamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                if (securityStamp != null)
                {
                    var dbSecurityStamp = await _userManager.GetSecurityStampAsync(user);
                    if (dbSecurityStamp != securityStamp)
                        return;
                }
            }

            context.IsActive =
                !user.LockoutEnabled ||
                !user.LockoutEnd.HasValue ||
                user.LockoutEnd <= DateTime.UtcNow;
        }
    }

    private async Task<IEnumerable<Claim>> GetClaimFromRole(List<string> roles)
    {
        var claims = new List<Claim>();
        
        foreach (var roleName in roles)
        {
            claims.Add(new Claim(JwtClaimTypes.Role, roleName));

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                // Only include "permission" claims, if desired
                var permissionClaims = roleClaims
                    .Where(c => c.Type == "permissions");

                claims.AddRange(permissionClaims);
            }
        }

        return claims;
    }
    
    private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
    {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id),
                new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            if (!string.IsNullOrWhiteSpace(user.Name))
                claims.Add(new Claim("name", user.Name));

            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim("last_name", user.LastName));

            if (!string.IsNullOrWhiteSpace(user.CardNumber))
                claims.Add(new Claim("card_number", user.CardNumber));

            if (!string.IsNullOrWhiteSpace(user.CardHolderName))
                claims.Add(new Claim("card_holder", user.CardHolderName));

            if (!string.IsNullOrWhiteSpace(user.SecurityNumber))
                claims.Add(new Claim("card_security_number", user.SecurityNumber));

            if (!string.IsNullOrWhiteSpace(user.Expiration))
                claims.Add(new Claim("card_expiration", user.Expiration));

            if (!string.IsNullOrWhiteSpace(user.City))
                claims.Add(new Claim("address_city", user.City));

            if (!string.IsNullOrWhiteSpace(user.Country))
                claims.Add(new Claim("address_country", user.Country));

            if (!string.IsNullOrWhiteSpace(user.State))
                claims.Add(new Claim("address_state", user.State));

            if (!string.IsNullOrWhiteSpace(user.Street))
                claims.Add(new Claim("address_street", user.Street));

            if (!string.IsNullOrWhiteSpace(user.ZipCode))
                claims.Add(new Claim("address_zip_code", user.ZipCode));

            if (_userManager.SupportsUserEmail)
            {
                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }

            if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                    new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }

            return claims;
    }
}