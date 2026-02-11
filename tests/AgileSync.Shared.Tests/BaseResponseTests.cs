using AgileSync.Shared.Models;
using FluentAssertions;

namespace AgileSync.Shared.Tests;

public class BaseResponseTests
{
    // ── Generic BaseResponse<T> ──

    [Fact]
    public void Ok_SetsSuccessTrue_AndData()
    {
        var response = BaseResponse<string>.Ok("hello");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("hello");
        response.Message.Should().BeNull();
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ok_WithMessage_SetsMessage()
    {
        var response = BaseResponse<int>.Ok(42, "created");

        response.Success.Should().BeTrue();
        response.Data.Should().Be(42);
        response.Message.Should().Be("created");
    }

    [Fact]
    public void Fail_SetsSuccessFalse_AndMessage()
    {
        var response = BaseResponse<string>.Fail("something went wrong");

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("something went wrong");
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithErrors_SetsErrors()
    {
        List<string> errors = ["Error 1", "Error 2"];
        var response = BaseResponse<string>.Fail("validation", errors);

        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(2);
        response.Errors.Should().Contain("Error 1");
        response.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void Fail_WithNullErrors_DefaultsToEmpty()
    {
        var response = BaseResponse<string>.Fail("fail", null);

        response.Errors.Should().BeEmpty();
    }

    // ── Non-generic BaseResponse ──

    [Fact]
    public void NonGeneric_Ok_SetsSuccessTrue()
    {
        var response = BaseResponse.Ok();

        response.Success.Should().BeTrue();
        response.Message.Should().BeNull();
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NonGeneric_Ok_WithMessage()
    {
        var response = BaseResponse.Ok("done");

        response.Success.Should().BeTrue();
        response.Message.Should().Be("done");
    }

    [Fact]
    public void NonGeneric_Fail_SetsSuccessFalse()
    {
        var response = BaseResponse.Fail("bad request");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("bad request");
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NonGeneric_Fail_WithErrors()
    {
        List<string> errors = ["err1", "err2"];
        var response = BaseResponse.Fail("validation", errors);

        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(2);
    }
}
