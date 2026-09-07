using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;

namespace PosSelfOrdering.Client.Repositories.Contracts;

public interface IAdminRepository
{
    Task<ApiResponse<AdminUserDto>> AuthenticatePinAsync(string pin, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<AdminOrderDto>>> GetActiveOrdersAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateOrderStatusAsync(string orderNumber, string newStatus, string? reason = null, CancellationToken ct = default);
    Task<ApiResponse<MenuItemDto>> SaveMenuItemAsync(UpsertMenuItemRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> ToggleMenuItemAvailabilityAsync(string menuItemId, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteMenuItemAsync(string menuItemId, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<TableStatusDto>>> GetTableStatusesAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> ClearTableAsync(string tableNumber, CancellationToken ct = default);
    Task<ApiResponse<SalesSummaryDto>> GetDailySalesReportAsync(DateTime date, CancellationToken ct = default);
    Task<ApiResponse<StoreSettingsDto>> GetStoreSettingsAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateStoreSettingsAsync(StoreSettingsDto settings, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<VoucherDto>>> GetVouchersAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> SaveVoucherAsync(VoucherDto voucher, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteVoucherAsync(string code, CancellationToken ct = default);
}
