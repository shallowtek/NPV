using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using NPV.Shared.DTOs;

namespace NPV.API.EndpointHandlers;

public static class AuthHandler
{
    public static async Task<IResult> RegisterAsync(
        [FromBody] UserRegistrationDto registration,
        UserManager<IdentityUser> userManager)
    {
        var user = new IdentityUser { UserName = registration.Username, Email = registration.Email };
        var result = await userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        // Optionally add a default role
        await userManager.AddToRoleAsync(user, "User");

        return Results.Ok("Registration successful");
    }

    public static async Task<IResult> LoginAsync(
        [FromBody] UserLoginDto login,
        UserManager<IdentityUser> userManager,
        IConfiguration config,
        SignInManager<IdentityUser> signInManager)
    {
        var user = await userManager.FindByNameAsync(login.Username);
        if (user == null)
            return Results.Unauthorized();

        var passwordValid = await userManager.CheckPasswordAsync(user, login.Password);
        if (!passwordValid)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "super-long-demo-key-change-me"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "https://localhost:5001",
            audience: config["Jwt:Audience"] ?? "demo-users",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new LoginResultDto(jwt));
    }
}