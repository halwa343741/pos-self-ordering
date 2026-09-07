using System.Net.Http.Json;
using System.Text.Json;
using PosSelfOrdering.Client.DTOs.Common;

namespace PosSelfOrdering.Client.Api;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<TResponse>?> GetAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(endpoint, ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(_jsonOptions, ct);
            }

            return ApiResponse<TResponse>.Fail($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Network error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<TResponse>?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(endpoint, request, _jsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(_jsonOptions, ct);
            }

            return ApiResponse<TResponse>.Fail($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Network error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<TResponse>?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(endpoint, request, _jsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(_jsonOptions, ct);
            }

            return ApiResponse<TResponse>.Fail($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Network error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<TResponse>?> DeleteAsync<TResponse>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync(endpoint, ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(_jsonOptions, ct);
            }

            return ApiResponse<TResponse>.Fail($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Network error: {ex.Message}", 500);
        }
    }
}
