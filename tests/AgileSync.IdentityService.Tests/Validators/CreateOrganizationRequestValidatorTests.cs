using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Validators;
using FluentAssertions;

namespace AgileSync.IdentityService.Tests.Validators;

public class CreateOrganizationRequestValidatorTests
{
    private readonly CreateOrganizationRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("My Org", "my-org", "A description"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("", "my-org", "desc"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task EmptySlug_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("My Org", "", "desc"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }

    [Theory]
    [InlineData("MY-ORG")]
    [InlineData("my org")]
    [InlineData("my_org")]
    [InlineData("-my-org")]
    [InlineData("my-org-")]
    public async Task InvalidSlugFormat_Fails(string slug)
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("My Org", slug, "desc"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }

    [Theory]
    [InlineData("my-org")]
    [InlineData("org123")]
    [InlineData("my-cool-org")]
    public async Task ValidSlugFormats_Pass(string slug)
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("My Org", slug, "desc"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DescriptionTooLong_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateOrganizationRequest("My Org", "my-org", new string('a', 501)));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
