using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class AdminLoginRequestValidatorTests
{
    private readonly AdminLoginRequestValidator _validator = new();

    [Fact]
    public async Task EmptyUsername_Fails()
    {
        var result = await _validator.ValidateAsync(new AdminLoginRequest("", "pass123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public async Task EmptyPassword_Fails()
    {
        var result = await _validator.ValidateAsync(new AdminLoginRequest("admin", ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new AdminLoginRequest("admin", "password"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameTooLong_Fails()
    {
        var result = await _validator.ValidateAsync(new AdminLoginRequest(new string('a', 101), "pass"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }
}
