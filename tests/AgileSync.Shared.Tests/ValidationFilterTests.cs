using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AgileSync.Shared.Tests;

/// <summary>DTO used by ValidationFilter tests. Must be public for NSubstitute proxy generation.</summary>
public record TestDto(string Name);

public class ValidationFilterTests
{
    [Fact]
    public async Task InvokeAsync_NoValidatorRegistered_CallsNext()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };

        var context = CreateFilterContext(httpContext, new TestDto("test"));
        var nextCalled = false;
        var next = new EndpointFilterDelegate(_ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        });

        var filter = new ValidationFilter<TestDto>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task InvokeAsync_NoMatchingArg_CallsNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestDto>>();
        var services = new ServiceCollection()
            .AddSingleton(validator)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        // No TestDto argument in the context
        var context = CreateFilterContext(httpContext);
        var nextCalled = false;
        var next = new EndpointFilterDelegate(_ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        });

        var filter = new ValidationFilter<TestDto>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ValidDto_CallsNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestDto>>();
        validator.ValidateAsync(Arg.Any<TestDto>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var services = new ServiceCollection()
            .AddSingleton(validator)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var dto = new TestDto("valid");
        var context = CreateFilterContext(httpContext, dto);

        var nextCalled = false;
        var next = new EndpointFilterDelegate(_ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        });

        var filter = new ValidationFilter<TestDto>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task InvokeAsync_InvalidDto_Returns400_WithErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Name", "Name too short.")
        };

        var validator = Substitute.For<IValidator<TestDto>>();
        validator.ValidateAsync(Arg.Any<TestDto>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var services = new ServiceCollection()
            .AddSingleton(validator)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var dto = new TestDto("");
        var context = CreateFilterContext(httpContext, dto);

        var next = new EndpointFilterDelegate(_ =>
            ValueTask.FromResult<object?>("should not reach"));

        var filter = new ValidationFilter<TestDto>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        result.Should().NotBeNull();
        var badRequest = result as BadRequest<BaseResponse>;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(400);
        badRequest.Value!.Success.Should().BeFalse();
        badRequest.Value.Message.Should().Be("Validation failed");
        badRequest.Value.Errors.Should().HaveCount(2);
        badRequest.Value.Errors.Should().Contain("Name is required.");
        badRequest.Value.Errors.Should().Contain("Name too short.");
    }

    /// <summary>
    /// Creates a minimal EndpointFilterInvocationContext for testing.
    /// </summary>
    private static EndpointFilterInvocationContext CreateFilterContext(
        HttpContext httpContext, params object[] arguments)
    {
        return new DefaultEndpointFilterInvocationContext(httpContext, arguments);
    }
}
