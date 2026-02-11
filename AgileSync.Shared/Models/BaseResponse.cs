namespace AgileSync.Shared.Models;

/// <summary>
/// Standard API response wrapper providing consistent structure across all endpoints.
/// </summary>
/// <typeparam name="T">The type of the response payload.</typeparam>
public record BaseResponse<T>
{
    /// <summary>Whether the request was successful.</summary>
    public bool Success { get; init; }

    /// <summary>The response payload (null on failure).</summary>
    public T? Data { get; init; }

    /// <summary>Human-readable message describing the result.</summary>
    public string? Message { get; init; }

    /// <summary>Validation or processing errors, if any.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Creates a successful response with data.</summary>
    public static BaseResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>Creates a failure response with a message and optional errors.</summary>
    public static BaseResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}

/// <summary>
/// Non-generic response for operations that don't return data.
/// </summary>
public record BaseResponse
{
    /// <summary>Whether the request was successful.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable message describing the result.</summary>
    public string? Message { get; init; }

    /// <summary>Validation or processing errors, if any.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Creates a successful response.</summary>
    public static BaseResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    /// <summary>Creates a failure response.</summary>
    public static BaseResponse Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}
