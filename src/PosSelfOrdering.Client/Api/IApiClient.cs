using PosSelfOrdering.Client.DTOs.Common;

namespace PosSelfOrdering.Client.Api;

public interface IApiClient
{
    Task<ApiResponse<TResponse>?> GetAsync<TResponse>(string endpoint, CancellationToken ct = default);
    Task<ApiResponse<TResponse>?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default);
    Task<ApiResponse<TResponse>?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default);
    Task<ApiResponse<TResponse>?> DeleteAsync<TResponse>(string endpoint, CancellationToken ct = default);
}
