using PosSelfOrdering.Client.Api;
using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Repositories.Implementations;

public sealed class RealAdminRepository : IAdminRepository
{
    private readonly IApiClient _api;

    public RealAdminRepository(IApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResponse<AdminUserDto>> AuthenticatePinAsync(string pin, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<AdminLoginRequest, AdminUserDto>("/api/admin/auth/login", new AdminLoginRequest(pin), ct);
        return res ?? ApiResponse<AdminUserDto>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<IReadOnlyList<AdminOrderDto>>> GetActiveOrdersAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<IReadOnlyList<AdminOrderDto>>("/api/admin/orders/active", ct);
        return res ?? ApiResponse<IReadOnlyList<AdminOrderDto>>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(string orderNumber, string newStatus, string? reason = null, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<UpdateOrderStatusRequest, bool>($"/api/admin/orders/{orderNumber}/status", new UpdateOrderStatusRequest(orderNumber, newStatus, reason), ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<MenuItemDto>> SaveMenuItemAsync(UpsertMenuItemRequest request, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<UpsertMenuItemRequest, MenuItemDto>("/api/admin/menu/upsert", request, ct);
        return res ?? ApiResponse<MenuItemDto>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> ToggleMenuItemAvailabilityAsync(string menuItemId, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<object, bool>($"/api/admin/menu/{menuItemId}/toggle-stock", new { }, ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> DeleteMenuItemAsync(string menuItemId, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<bool>($"/api/admin/menu/{menuItemId}", ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<IReadOnlyList<TableStatusDto>>> GetTableStatusesAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<IReadOnlyList<TableStatusDto>>("/api/admin/tables", ct);
        return res ?? ApiResponse<IReadOnlyList<TableStatusDto>>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> ClearTableAsync(string tableNumber, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object, bool>($"/api/admin/tables/{tableNumber}/clear", new { }, ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<SalesSummaryDto>> GetDailySalesReportAsync(DateTime date, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<SalesSummaryDto>($"/api/admin/reports/daily?date={date:yyyy-MM-dd}", ct);
        return res ?? ApiResponse<SalesSummaryDto>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<StoreSettingsDto>> GetStoreSettingsAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<StoreSettingsDto>("/api/admin/settings", ct);
        return res ?? ApiResponse<StoreSettingsDto>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> UpdateStoreSettingsAsync(StoreSettingsDto settings, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<StoreSettingsDto, bool>("/api/admin/settings", settings, ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<IReadOnlyList<VoucherDto>>> GetVouchersAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<IReadOnlyList<VoucherDto>>("/api/admin/vouchers", ct);
        return res ?? ApiResponse<IReadOnlyList<VoucherDto>>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> SaveVoucherAsync(VoucherDto voucher, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<VoucherDto, bool>("/api/admin/vouchers", voucher, ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }

    public async Task<ApiResponse<bool>> DeleteVoucherAsync(string code, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<bool>($"/api/admin/vouchers/{code}", ct);
        return res ?? ApiResponse<bool>.Fail("No response from server.", 500);
    }
}
