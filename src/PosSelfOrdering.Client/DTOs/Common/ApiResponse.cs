namespace PosSelfOrdering.Client.DTOs.Common;

public sealed record ApiResponse<T>(
    bool Success,
    int StatusCode,
    string Message,
    T? Data,
    IReadOnlyList<string>? Errors = null
)
{
    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new(true, 200, message, data, Array.Empty<string>());

    public static ApiResponse<T> Created(T data, string message = "Created") =>
        new(true, 201, message, data, Array.Empty<string>());

    public static ApiResponse<T> Fail(string message, int statusCode = 400, IReadOnlyList<string>? errors = null) =>
        new(false, statusCode, message, default, errors ?? new[] { message });
}
