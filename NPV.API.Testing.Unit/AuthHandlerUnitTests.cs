using Moq;
using Microsoft.AspNetCore.Identity;
using NPV.API.EndpointHandlers;
using Microsoft.Extensions.Configuration;
using NPV.Shared.DTOs;

namespace NVP.API.Testing.Unit;

public class AuthHandlerTests
{
    [Fact]
    public async Task RegisterAsync_Success_ReturnsOk()
    {
        var mockUserManager = IdentityMockFactory.MockUserManager<IdentityUser>();
        mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new UserRegistrationDto("demo", "demo@example.com", "Password123!");

        var result = await AuthHandler.RegisterAsync(dto, mockUserManager.Object);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>(result);
    }

    [Fact]
    public async Task RegisterAsync_Failure_ReturnsBadRequest()
    {
        var mockUserManager = IdentityMockFactory.MockUserManager<IdentityUser>();
        mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Bad password" }));

        var dto = new UserRegistrationDto("demo", "demo@example.com", "bad");

        var result = await AuthHandler.RegisterAsync(dto, mockUserManager.Object);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<IEnumerable<IdentityError>>>(result);
    }

    [Fact]
    public async Task LoginAsync_Success_ReturnsJwt()
    {
        var mockUserManager = IdentityMockFactory.MockUserManager<IdentityUser>();
        var mockSignInManager = IdentityMockFactory.MockSignInManager<IdentityUser>(mockUserManager.Object);

        var user = new IdentityUser("demo") { Id = "userid" };
        mockUserManager.Setup(m => m.FindByNameAsync("demo")).ReturnsAsync(user);
        mockUserManager.Setup(m => m.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var configDict = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "mysuperlongsecurejwtkeyof32characters!!",
            ["Jwt:Issuer"] = "https://localhost:5001",
            ["Jwt:Audience"] = "demo-users"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

        var dto = new UserLoginDto("demo", "Password123!");

        var result = await AuthHandler.LoginAsync(dto, mockUserManager.Object, config, mockSignInManager.Object);

        // Now you can strongly check type and property
        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<LoginResultDto>>(result);
        Assert.False(string.IsNullOrWhiteSpace(okResult.Value.Token));
    }


    [Fact]
    public async Task LoginAsync_BadPassword_ReturnsUnauthorized()
    {
        var mockUserManager = IdentityMockFactory.MockUserManager<IdentityUser>();
        var mockSignInManager = IdentityMockFactory.MockSignInManager<IdentityUser>(mockUserManager.Object);

        var user = new IdentityUser("demo") { Id = "userid" };

        mockUserManager.Setup(m => m.FindByNameAsync("demo")).ReturnsAsync(user);
        mockUserManager.Setup(m => m.CheckPasswordAsync(user, "wrongpassword")).ReturnsAsync(false);

        var configDict = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "super-long-demo-key-change-me",
            ["Jwt:Issuer"] = "https://localhost:5001",
            ["Jwt:Audience"] = "demo-users"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

        var dto = new UserLoginDto("demo", "wrongpassword");

        var result = await AuthHandler.LoginAsync(dto, mockUserManager.Object, config, mockSignInManager.Object);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>(result);
    }
}

// Helper class to mock UserManager/SignInManager for Identity
public static class IdentityMockFactory
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
        return new Mock<SignInManager<TUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
    }
}
