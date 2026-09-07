using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.DTOs.Order;
using PosSelfOrdering.Client.DTOs.Payment;
using PosSelfOrdering.Client.DTOs.Session;

namespace PosSelfOrdering.Client.Repositories.Contracts;

public interface ISessionRepository
{
    Task<ApiResponse<TableSessionDto>> InitSessionAsync(InitSessionRequest request, CancellationToken ct = default);
}

public interface IMenuRepository
{
    Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<MenuItemDto>>> GetMenuItemsAsync(string? categoryId, string? search, CancellationToken ct = default);
    Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(string id, CancellationToken ct = default);
}

public interface IOrderRepository
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(string orderNumber, CancellationToken ct = default);
}

public interface IPaymentRepository
{
    Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(string orderNumber, CancellationToken ct = default);
}
