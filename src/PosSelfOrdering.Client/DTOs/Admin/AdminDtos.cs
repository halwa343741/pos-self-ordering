using PosSelfOrdering.Client.DTOs.Menu;

namespace PosSelfOrdering.Client.DTOs.Admin;

// ─── Auth ─────────────────────────────────────────────────────────────────────
public sealed record AdminLoginRequest(string PinCode);

public sealed record AdminUserDto(
    string UserId,
    string FullName,
    string Role,        // "Cashier" | "Kitchen" | "Manager"
    string Token,
    DateTime ExpiresAtUtc
);

// ─── Order Management ─────────────────────────────────────────────────────────
public sealed record AdminOrderItemDto(
    string MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    string SelectedModifiersSummary,
    string? Notes
);

public sealed record AdminOrderDto(
    string OrderNumber,
    string SessionId,
    string TableNumber,
    string OrderType,
    string? CustomerName,
    string? CustomerNotes,
    string Status,          // "PendingPayment" | "Cooking" | "Ready" | "Completed" | "Cancelled"
    decimal Subtotal,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal GrandTotal,
    string PaymentMethod,
    bool IsPaid,
    DateTime CreatedAtUtc,
    IReadOnlyList<AdminOrderItemDto> Items
);

public sealed record UpdateOrderStatusRequest(
    string OrderNumber,
    string NewStatus,
    string? Reason = null
);

// ─── Menu Management ──────────────────────────────────────────────────────────
public sealed record UpsertMenuItemRequest(
    string? Id,
    string CategoryId,
    string Name,
    string Description,
    decimal BasePrice,
    string ImageUrl,
    bool IsAvailable,
    bool IsBestSeller,
    bool IsSpicy,
    IReadOnlyList<ModifierGroupDto>? ModifierGroups = null
);

// ─── Voucher ──────────────────────────────────────────────────────────────────
public sealed record VoucherDto(
    string Code,
    string Description,
    decimal DiscountAmount,
    bool IsPercentage,
    decimal MinOrderAmount,
    bool IsActive
);

// ─── Table Management ─────────────────────────────────────────────────────────
public sealed record TableStatusDto(
    string TableNumber,
    string Status,               // "Available" | "Occupied" | "Billing"
    string? ActiveOrderNumber,
    int? ActiveItemCount,
    DateTime? OccupiedSinceUtc
);

// ─── Reports ──────────────────────────────────────────────────────────────────
public sealed record TopSellingItemDto(
    string Name,
    int QuantitySold,
    decimal TotalRevenue
);

public sealed record SalesSummaryDto(
    decimal TotalRevenueToday,
    int TotalOrdersCount,
    decimal AverageOrderValue,
    IReadOnlyDictionary<string, decimal> RevenueByPaymentMethod,
    IReadOnlyList<TopSellingItemDto> TopSellingItems,
    IReadOnlyList<AdminOrderDto> RecentTransactions
);

// ─── Settings ─────────────────────────────────────────────────────────────────
public sealed record StoreSettingsDto(
    string StoreName,
    string Address,
    decimal TaxRatePercent,
    decimal ServiceChargePercent,
    int ThermalPaperWidthMm,    // 58 or 80
    bool AutoPrintReceipt
);
