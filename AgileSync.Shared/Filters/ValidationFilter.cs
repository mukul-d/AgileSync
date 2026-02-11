using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AgileSync.Shared.Models;

namespace AgileSync.Shared.Filters;

/// <summary>
/// Generic endpoint filter that automatically validates request DTOs using FluentValidation.
/// Returns a <see cref="BaseResponse"/> with validation errors on failure.
/// </summary>
/// <typeparam name="T">The request DTO type to validate.</typeparam>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
            return await next(context);

        var arg = context.Arguments.OfType<T>().FirstOrDefault();
        if (arg is null)
            return await next(context);

        var result = await validator.ValidateAsync(arg, default);
        if (result.IsValid)
            return await next(context);

        var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
        return Results.BadRequest(BaseResponse.Fail("Validation failed", errors));
    }
}
