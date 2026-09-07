namespace PosSelfOrdering.Client.DTOs.Order;

public sealed record OrderItemRequestDto(
    string MenuItemId,
    int Quantity,
    decimal UnitPrice,
    string? Notes,
    IReadOnlyList<string> SelectedOptionIds
);

public sealed record CreateOrderRequestDto(
    string SessionId,
    string TableNumber,
    string OrderType,
    string? CustomerName,
    string? CustomerNotes,
    string PaymentMethod,
    IReadOnlyList<OrderItemRequestDto> Items
);

public sealed record PaymentDetailsDto(
    string PaymentId,
    string QrString,
    DateTime ExpiresAtUtc
);

public sealed record OrderDto(
    string OrderNumber,
    string SessionId,
    string TableNumber,
    string OrderType,
    string? CustomerName,
    string Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal GrandTotal,
    string PaymentMethod,
    PaymentDetailsDto? PaymentDetails,
    DateTime CreatedAtUtc
);

public sealed record OrderStatusItemDto(
    string Name,
    int Quantity,
    string SelectedModifiersSummary,
    decimal Subtotal
);

public sealed record OrderStatusDto(
    string OrderNumber,
    string Status,
    string QueueNumber,
    int EstimatedMinutesRemaining,
    DateTime? PaidAtUtc,
    IReadOnlyList<OrderStatusItemDto> Items,
    decimal GrandTotal
);
