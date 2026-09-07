using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Order;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.Store;

namespace PosSelfOrdering.Client.Mocks;

public sealed class MockOrderRepository : IOrderRepository
{
    private readonly SharedPosDataStore _store;
    private static int _orderCounter = 200;

    public MockOrderRepository(SharedPosDataStore store)
    {
        _store = store;
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);

        if (request.Items == null || !request.Items.Any())
        {
            return ApiResponse<OrderDto>.Fail("Keranjang pesanan tidak boleh kosong.", 400);
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Interlocked.Increment(ref _orderCounter)}";
        var subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        var taxRate = _store.Settings.TaxRatePercent / 100m;
        var serviceRate = _store.Settings.ServiceChargePercent / 100m;
        var tax = Math.Round(subtotal * taxRate, 0);
        var service = Math.Round(subtotal * serviceRate, 0);
        var grandTotal = subtotal + tax + service;

        // Generate dynamic QRIS payload
        var qrisString = $"00020101021226580016ID.CO.QRIS.WWW0118936009180000010042520454995802ID5909KEDAI J.A6007JAKARTA61051219062070703A016304{Guid.NewGuid():N}"[..60];

        var initialStatus = "PendingPayment";

        var adminItems = request.Items.Select(i =>
        {
            _store.MenuItems.TryGetValue(i.MenuItemId, out var menuItem);
            var itemName = menuItem?.Name ?? "Item Menu";

            var optionNames = new List<string>();
            if (i.SelectedOptionIds != null && menuItem?.ModifierGroups != null)
            {
                foreach (var grp in menuItem.ModifierGroups)
                {
                    var selected = grp.Options.Where(o => i.SelectedOptionIds.Contains(o.Id)).Select(o => o.Name);
                    if (selected.Any())
                    {
                        optionNames.Add($"{grp.Name}: {string.Join('/', selected)}");
                    }
                }
            }
            var itemSummary = optionNames.Any() ? string.Join(", ", optionNames) : "Standar";

            return new AdminOrderItemDto(
                MenuItemId: i.MenuItemId,
                Name: itemName,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice,
                Subtotal: i.UnitPrice * i.Quantity,
                SelectedModifiersSummary: itemSummary,
                Notes: i.Notes
            );
        }).ToList();

        var adminOrder = new AdminOrderDto(
            OrderNumber: orderNumber,
            SessionId: request.SessionId,
            TableNumber: request.TableNumber,
            OrderType: request.OrderType,
            CustomerName: request.CustomerName,
            CustomerNotes: request.CustomerNotes,
            Status: initialStatus,
            Subtotal: subtotal,
            TaxAmount: tax,
            ServiceCharge: service,
            GrandTotal: grandTotal,
            PaymentMethod: request.PaymentMethod,
            IsPaid: false,
            CreatedAtUtc: DateTime.UtcNow,
            Items: adminItems
        );

        _store.AddOrder(adminOrder);

        var orderDto = new OrderDto(
            OrderNumber: orderNumber,
            SessionId: request.SessionId,
            TableNumber: request.TableNumber,
            OrderType: request.OrderType,
            CustomerName: request.CustomerName,
            Status: initialStatus,
            Subtotal: subtotal,
            TaxAmount: tax,
            ServiceCharge: service,
            GrandTotal: grandTotal,
            PaymentMethod: request.PaymentMethod,
            PaymentDetails: new PaymentDetailsDto(
                PaymentId: $"pay_{Guid.NewGuid():N}"[..16],
                QrString: qrisString,
                ExpiresAtUtc: DateTime.UtcNow.AddMinutes(15)
            ),
            CreatedAtUtc: DateTime.UtcNow
        );

        return ApiResponse<OrderDto>.Created(orderDto, "Pesanan berhasil dibuat.");
    }

    public async Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        if (!_store.Orders.TryGetValue(orderNumber, out var order))
        {
            return ApiResponse<OrderStatusDto>.Fail("Pesanan tidak ditemukan.", 404);
        }

        var queueNumber = $"A-{(Math.Abs(order.OrderNumber.GetHashCode()) % 90) + 10}";
        var elapsed = (DateTime.UtcNow - order.CreatedAtUtc).TotalMinutes;
        var minutesRemaining = Math.Max(0, 15 - (int)elapsed);

        var statusDto = new OrderStatusDto(
            OrderNumber: order.OrderNumber,
            Status: order.Status,
            QueueNumber: queueNumber,
            EstimatedMinutesRemaining: minutesRemaining,
            PaidAtUtc: order.IsPaid ? order.CreatedAtUtc.AddSeconds(5) : null,
            Items: order.Items.Select(i => new OrderStatusItemDto(
                Name: i.Name,
                Quantity: i.Quantity,
                SelectedModifiersSummary: i.SelectedModifiersSummary,
                Subtotal: i.Subtotal
            )).ToList(),
            GrandTotal: order.GrandTotal
        );

        return ApiResponse<OrderStatusDto>.Ok(statusDto);
    }
}
