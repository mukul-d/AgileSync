using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class UpdateThemeRequestValidatorTests
{
    private readonly UpdateThemeRequestValidator _validator = new();

    [Theory]
    [InlineData("web", "dark")]
    [InlineData("web", "light")]
    [InlineData("pwa", "dark")]
    [InlineData("pwa", "light")]
    [InlineData("WEB", "DARK")]
    [InlineData("PWA", "LIGHT")]
    public async Task ValidCombinations_Pass(string platform, string theme)
    {
        var result = await _validator.ValidateAsync(new UpdateThemeRequest(platform, theme));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("mobile", "dark")]
    [InlineData("desktop", "light")]
    [InlineData("", "dark")]
    public async Task InvalidPlatform_Fails(string platform, string theme)
    {
        var result = await _validator.ValidateAsync(new UpdateThemeRequest(platform, theme));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Platform");
    }

    [Theory]
    [InlineData("web", "blue")]
    [InlineData("pwa", "red")]
    [InlineData("web", "")]
    public async Task InvalidTheme_Fails(string platform, string theme)
    {
        var result = await _validator.ValidateAsync(new UpdateThemeRequest(platform, theme));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Theme");
    }
}
