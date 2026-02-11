using AgileSync.IdentityService.Models;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Models;

public class OrganizationTests
{
    [Fact]
    public void Slug_AutoLowercases()
    {
        var org = new Organization { Name = "Test", Slug = "MY-ORG" };
        org.Slug.Should().Be("my-org");
    }

    [Fact]
    public void Slug_TrimsWhitespace()
    {
        var org = new Organization { Name = "Test", Slug = "  my-org  " };
        org.Slug.Should().Be("my-org");
    }

    [Fact]
    public void Slug_LowercasesAndTrims()
    {
        var org = new Organization { Name = "Test", Slug = "  MY-ORG  " };
        org.Slug.Should().Be("my-org");
    }

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var org = new Organization { Name = "Test", Slug = "test" };
        org.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Description_DefaultsToEmpty()
    {
        var org = new Organization { Name = "Test", Slug = "test" };
        org.Description.Should().BeEmpty();
    }

    [Fact]
    public void InheritsBaseEntity_IdGenerated()
    {
        var org = new Organization { Name = "Test", Slug = "test" };
        org.Id.Should().NotBeNullOrWhiteSpace();
    }
}
