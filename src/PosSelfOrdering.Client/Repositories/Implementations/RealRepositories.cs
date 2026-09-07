using PosSelfOrdering.Client.Api;
using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.DTOs.Order;
using PosSelfOrdering.Client.DTOs.Payment;
using PosSelfOrdering.Client.DTOs.Session;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Repositories.Implementations;

public sealed class SessionRepository : ISessionRepository
{
    private readonly IApiClient _api;
    public SessionRepository(IApiClient api) => _api = api;

    public async Task<ApiResponse<TableSessionDto>> InitSessionAsync(InitSessionRequest request, CancellationToken ct = default) =>
        await _api.PostAsync<InitSessionRequest, TableSessionDto>("/api/v1/sessions/init", request, ct)
        ?? ApiResponse<TableSessionDto>.Fail("Gagal menginisialisasi sesi.");
}

public sealed class MenuRepository : IMenuRepository
{
    private readonly IApiClient _api;
    public MenuRepository(IApiClient api) => _api = api;

    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default) =>
        await _api.GetAsync<IReadOnlyList<CategoryDto>>("/api/v1/menu/categories", ct)
        ?? ApiResponse<IReadOnlyList<CategoryDto>>.Fail("Gagal mengambil kategori menu.");

    public async Task<ApiResponse<IReadOnlyList<MenuItemDto>>> GetMenuItemsAsync(string? categoryId, string? search, CancellationToken ct = default)
    {
        var endpoint = $"/api/v1/menu/items?categoryId={Uri.EscapeDataString(categoryId ?? "")}&search={Uri.EscapeDataString(search ?? "")}";
        return await _api.GetAsync<IReadOnlyList<MenuItemDto>>(endpoint, ct)
               ?? ApiResponse<IReadOnlyList<MenuItemDto>>.Fail("Gagal mengambil daftar menu.");
    }

    public async Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(string id, CancellationToken ct = default) =>
        await _api.GetAsync<MenuItemDto>($"/api/v1/menu/items/{id}", ct)
        ?? ApiResponse<MenuItemDto>.Fail("Menu item tidak ditemukan.", 404);
}

public sealed class OrderRepository : IOrderRepository
{
    private readonly IApiClient _api;
    public OrderRepository(IApiClient api) => _api = api;

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken ct = default) =>
        await _api.PostAsync<CreateOrderRequestDto, OrderDto>("/api/v1/orders", request, ct)
        ?? ApiResponse<OrderDto>.Fail("Gagal memproses pesanan.");

    public async Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(string orderNumber, CancellationToken ct = default) =>
        await _api.GetAsync<OrderStatusDto>($"/api/v1/orders/{orderNumber}", ct)
        ?? ApiResponse<OrderStatusDto>.Fail("Gagal mengambil status pesanan.");
}

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly IApiClient _api;
    public PaymentRepository(IApiClient api) => _api = api;

    public async Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(string orderNumber, CancellationToken ct = default) =>
        await _api.GetAsync<PaymentStatusDto>($"/api/v1/payments/{orderNumber}/status", ct)
        ?? ApiResponse<PaymentStatusDto>.Fail("Gagal mengambil status pembayaran.");
}
