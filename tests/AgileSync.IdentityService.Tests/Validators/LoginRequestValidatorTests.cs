using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@example.com", "password"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyEmail_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("", "password"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task InvalidEmail_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("not-email", "password"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task EmptyPassword_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@example.com", ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
