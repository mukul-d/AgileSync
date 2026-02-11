using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Validators;
using FluentAssertions;

namespace AgileSync.ProjectService.Tests.Validators;

public class CreateSprintRequestValidatorTests
{
    private readonly CreateSprintRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("proj1", "Sprint 1", "Goal", DateTime.UtcNow, DateTime.UtcNow.AddDays(14)));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyName_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("proj1", "", "Goal", null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task EmptyProjectId_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("", "Sprint 1", "Goal", null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }

    [Fact]
    public async Task EndDateBeforeStartDate_Fails()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1);
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("proj1", "Sprint 1", "Goal", start, end));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public async Task NullDates_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("proj1", "Sprint 1", "Goal", null, null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task GoalTooLong_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateSprintRequest("proj1", "Sprint 1", new string('a', 501), null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Goal");
    }
}
