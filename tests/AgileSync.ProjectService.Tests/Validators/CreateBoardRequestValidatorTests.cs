using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Validators;
using FluentAssertions;

namespace AgileSync.ProjectService.Tests.Validators;

public class CreateBoardRequestValidatorTests
{
    private readonly CreateBoardRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateBoardRequest("proj1", "Main Board"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyProjectId_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateBoardRequest("", "Main Board"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }

    [Fact]
    public async Task EmptyName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateBoardRequest("proj1", ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task ShortName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateBoardRequest("proj1", "A"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task NameTooLong_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateBoardRequest("proj1", new string('a', 101)));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
