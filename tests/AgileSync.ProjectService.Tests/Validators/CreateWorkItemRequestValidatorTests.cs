using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Validators;
using FluentAssertions;

namespace AgileSync.ProjectService.Tests.Validators;

public class CreateWorkItemRequestValidatorTests
{
    private readonly CreateWorkItemRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Fix bug", "Description", "Bug", "High", null, null, 5));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("Feature")]
    [InlineData("")]
    public async Task InvalidType_Fails(string type)
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Fix bug", "desc", type, "High", null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Theory]
    [InlineData("Epic")]
    [InlineData("Story")]
    [InlineData("Task")]
    [InlineData("Bug")]
    [InlineData("bug")]
    [InlineData("EPIC")]
    public async Task ValidType_Passes(string type)
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Title", "desc", type, "High", null, null, null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Urgent")]
    [InlineData("")]
    public async Task InvalidPriority_Fails(string priority)
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Title", "desc", "Bug", priority, null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Fact]
    public async Task StoryPointsTooHigh_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Title", "desc", "Bug", "High", null, null, 101));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StoryPoints");
    }

    [Fact]
    public async Task StoryPointsNegative_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Title", "desc", "Bug", "High", null, null, -1));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StoryPoints");
    }

    [Fact]
    public async Task NullStoryPoints_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "Title", "desc", "Bug", "High", null, null, null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyTitle_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("proj1", "board1", "", "desc", "Bug", "High", null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task EmptyProjectId_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateWorkItemRequest("", "board1", "Title", "desc", "Bug", "High", null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }
}
