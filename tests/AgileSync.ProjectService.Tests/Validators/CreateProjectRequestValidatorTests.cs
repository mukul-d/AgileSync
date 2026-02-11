using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Validators;
using FluentAssertions;

namespace AgileSync.ProjectService.Tests.Validators;

public class CreateProjectRequestValidatorTests
{
    private readonly CreateProjectRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("My Project", "A description", "PROJ", "owner123"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("", "desc", "PROJ", "owner"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task EmptyKey_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Project", "desc", "", "owner"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Key");
    }

    [Theory]
    [InlineData("proj")]
    [InlineData("1ABC")]
    [InlineData("A")]
    [InlineData("ABCDEFGHIJK")]
    [InlineData("AB CD")]
    public async Task InvalidKeyFormat_Fails(string key)
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Project", "desc", key, "owner"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Key");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("PROJ")]
    [InlineData("A1")]
    [InlineData("ABCDEFGHIJ")]
    public async Task ValidKeyFormats_Pass(string key)
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Project", "desc", key, "owner"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyOwnerId_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Project", "desc", "PROJ", ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OwnerId");
    }

    [Fact]
    public async Task DescriptionTooLong_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Project", new string('a', 1001), "PROJ", "owner"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
