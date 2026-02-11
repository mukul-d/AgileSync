using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class AddTenantAdminRequestValidatorTests
{
    private readonly AddTenantAdminRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("admin@org.com", "Admin User", "password123"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task InvalidEmail_Fails()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("not-email", "Admin User", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task EmptyEmail_Fails()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("", "Admin User", "password123"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ShortPassword_Fails()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("admin@org.com", "Admin User", "abc"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task EmptyDisplayName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("admin@org.com", "", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public async Task ShortDisplayName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new AddTenantAdminRequest("admin@org.com", "A", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }
}
