using PosSelfOrdering.Client.DTOs.Admin;

namespace PosSelfOrdering.Client.Services;

public interface IAdminService
{
    AdminUserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsManager { get; }

    Task<bool> LoginWithPinAsync(string pin);
    void Logout();

    Task<IReadOnlyList<AdminOrderDto>> GetActiveOrdersAsync(CancellationToken ct = default);
    Task<bool> AdvanceOrderStatusAsync(string orderNumber, string currentStatus);
    Task<bool> CancelOrderAsync(string orderNumber, string reason);
    Task<bool> SaveMenuItemAsync(UpsertMenuItemRequest request);
    Task<bool> ToggleItemStockAsync(string menuItemId);
    Task<bool> DeleteMenuItemAsync(string menuItemId);
    Task<IReadOnlyList<TableStatusDto>> GetTablesAsync(CancellationToken ct = default);
    Task<bool> ResetTableAsync(string tableNumber);
    Task<SalesSummaryDto?> GetDailySummaryAsync(CancellationToken ct = default);
    Task<StoreSettingsDto?> GetStoreSettingsAsync(CancellationToken ct = default);
    Task<bool> SaveStoreSettingsAsync(StoreSettingsDto settings);
    Task<IReadOnlyList<VoucherDto>> GetVouchersAsync(CancellationToken ct = default);
    Task<bool> SaveVoucherAsync(VoucherDto voucher);
    Task<bool> DeleteVoucherAsync(string code);
}
