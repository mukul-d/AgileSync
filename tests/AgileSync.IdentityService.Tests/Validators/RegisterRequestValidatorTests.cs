using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@example.com", "Test User", "password123"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyEmail_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("", "Test User", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task InvalidEmail_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("not-an-email", "Test User", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task ShortPassword_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@example.com", "Test User", "abc"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task EmptyDisplayName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@example.com", "", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public async Task ShortDisplayName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@example.com", "X", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    public async Task PasswordTooShort_Fails(string password)
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@example.com", "Test User", password));
        result.IsValid.Should().BeFalse();
    }
}
