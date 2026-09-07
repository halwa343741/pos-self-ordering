using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Order;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.State;

namespace PosSelfOrdering.Client.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> SubmitOrderAsync(string paymentMethod, string? customerNotes = null, CancellationToken ct = default);
    Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(string orderNumber, CancellationToken ct = default);
}

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly CartState _cartState;
    private readonly SessionState _sessionState;

    public OrderService(IOrderRepository orderRepo, CartState cartState, SessionState sessionState)
    {
        _orderRepo = orderRepo;
        _cartState = cartState;
        _sessionState = sessionState;
    }

    public async Task<ApiResponse<OrderDto>> SubmitOrderAsync(string paymentMethod, string? customerNotes = null, CancellationToken ct = default)
    {
        if (_cartState.IsEmpty)
        {
            return ApiResponse<OrderDto>.Fail("Keranjang kosong. Silakan pilih menu terlebih dahulu.");
        }

        var session = _sessionState.CurrentSession;
        var sessionId = session?.SessionId ?? "sess_guest";
        var tableNumber = session?.TableNumber ?? "T-01";
        var orderType = session?.OrderType ?? "DineIn";

        var orderItems = _cartState.Items.Select(item => new OrderItemRequestDto(
            MenuItemId: item.MenuItem.Id,
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice,
            Notes: item.Notes,
            SelectedOptionIds: item.SelectedOptions.Select(o => o.Id).ToList()
        )).ToList();

        var request = new CreateOrderRequestDto(
            SessionId: sessionId,
            TableNumber: tableNumber,
            OrderType: orderType,
            CustomerName: null,
            CustomerNotes: customerNotes,
            PaymentMethod: paymentMethod,
            Items: orderItems
        );

        var result = await _orderRepo.CreateOrderAsync(request, ct);
        if (result.Success)
        {
            _cartState.Clear();
        }

        return result;
    }

    public async Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _orderRepo.GetOrderStatusAsync(orderNumber, ct);
    }
}
