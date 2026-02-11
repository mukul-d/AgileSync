using AgileSync.IdentityService.Models;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Models;

public class UserTests
{
    [Fact]
    public void Email_AutoLowercases()
    {
        var user = new User
        {
            Email = "USER@EXAMPLE.COM",
            DisplayName = "Test",
            PasswordHash = "hash"
        };
        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Email_TrimsWhitespace()
    {
        var user = new User
        {
            Email = "  user@example.com  ",
            DisplayName = "Test",
            PasswordHash = "hash"
        };
        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Role_DefaultsToMember()
    {
        var user = new User
        {
            Email = "user@example.com",
            DisplayName = "Test",
            PasswordHash = "hash"
        };
        user.Role.Should().Be(UserRole.Member);
    }

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var user = new User
        {
            Email = "user@example.com",
            DisplayName = "Test",
            PasswordHash = "hash"
        };
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Memberships_DefaultsToEmpty()
    {
        var user = new User
        {
            Email = "user@example.com",
            DisplayName = "Test",
            PasswordHash = "hash"
        };
        user.Memberships.Should().BeEmpty();
    }

    [Fact]
    public void ThemePreferences_DefaultsToDarkForBothPlatforms()
    {
        var prefs = new ThemePreferences();
        prefs.Web.Should().Be("dark");
        prefs.Pwa.Should().Be("dark");
    }

    [Theory]
    [InlineData("dark", "dark")]
    [InlineData("light", "light")]
    public void ThemePreferences_AcceptsValidValues(string value, string expected)
    {
        var prefs = new ThemePreferences { Web = value, Pwa = value };
        prefs.Web.Should().Be(expected);
        prefs.Pwa.Should().Be(expected);
    }

    [Theory]
    [InlineData("blue")]
    [InlineData("red")]
    [InlineData("")]
    [InlineData("invalid")]
    public void ThemePreferences_RejectsInvalidValues_FallsBackToDark(string value)
    {
        var prefs = new ThemePreferences { Web = value, Pwa = value };
        prefs.Web.Should().Be("dark");
        prefs.Pwa.Should().Be("dark");
    }
}
