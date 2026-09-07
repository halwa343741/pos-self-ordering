using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.State;

namespace PosSelfOrdering.Client.Services;

public sealed class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly AdminState _state;

    public AdminUserDto? CurrentUser => _state.CurrentUser;
    public bool IsAuthenticated => _state.IsAuthenticated;
    public bool IsManager => _state.IsManager;

    public AdminService(IAdminRepository repo, AdminState state)
    {
        _repo = repo;
        _state = state;
    }

    public async Task<bool> LoginWithPinAsync(string pin)
    {
        _state.ClearError();
        var res = await _repo.AuthenticatePinAsync(pin);
        if (res.Success && res.Data != null)
        {
            _state.SetUser(res.Data);
            return true;
        }

        _state.SetError(res.Message ?? "PIN otentikasi gagal.");
        return false;
    }

    public void Logout()
    {
        _state.ClearUser();
    }

    public async Task<IReadOnlyList<AdminOrderDto>> GetActiveOrdersAsync(CancellationToken ct = default)
    {
        var res = await _repo.GetActiveOrdersAsync(ct);
        if (res.Success && res.Data != null)
        {
            var activeCount = res.Data.Count(o => o.Status is "PendingPayment" or "Cooking" or "Ready");
            _state.SetActiveOrdersCount(activeCount);
            return res.Data;
        }
        return Array.Empty<AdminOrderDto>();
    }

    public async Task<bool> AdvanceOrderStatusAsync(string orderNumber, string currentStatus)
    {
        var nextStatus = currentStatus switch
        {
            "PendingPayment" => "Cooking",
            "Cooking" => "Ready",
            "Ready" => "Completed",
            _ => currentStatus
        };

        if (nextStatus == currentStatus) return false;

        var res = await _repo.UpdateOrderStatusAsync(orderNumber, nextStatus);
        return res.Success;
    }

    public async Task<bool> CancelOrderAsync(string orderNumber, string reason)
    {
        var res = await _repo.UpdateOrderStatusAsync(orderNumber, "Cancelled", reason);
        return res.Success;
    }

    public async Task<bool> SaveMenuItemAsync(UpsertMenuItemRequest request)
    {
        var res = await _repo.SaveMenuItemAsync(request);
        return res.Success;
    }

    public async Task<bool> ToggleItemStockAsync(string menuItemId)
    {
        var res = await _repo.ToggleMenuItemAvailabilityAsync(menuItemId);
        return res.Success;
    }

    public async Task<bool> DeleteMenuItemAsync(string menuItemId)
    {
        var res = await _repo.DeleteMenuItemAsync(menuItemId);
        return res.Success;
    }

    public async Task<IReadOnlyList<TableStatusDto>> GetTablesAsync(CancellationToken ct = default)
    {
        var res = await _repo.GetTableStatusesAsync(ct);
        return res.Success && res.Data != null ? res.Data : Array.Empty<TableStatusDto>();
    }

    public async Task<bool> ResetTableAsync(string tableNumber)
    {
        var res = await _repo.ClearTableAsync(tableNumber);
        return res.Success;
    }

    public async Task<SalesSummaryDto?> GetDailySummaryAsync(CancellationToken ct = default)
    {
        var res = await _repo.GetDailySalesReportAsync(DateTime.UtcNow, ct);
        return res.Success ? res.Data : null;
    }

    public async Task<StoreSettingsDto?> GetStoreSettingsAsync(CancellationToken ct = default)
    {
        var res = await _repo.GetStoreSettingsAsync(ct);
        return res.Success ? res.Data : null;
    }

    public async Task<bool> SaveStoreSettingsAsync(StoreSettingsDto settings)
    {
        var res = await _repo.UpdateStoreSettingsAsync(settings);
        return res.Success;
    }

    public async Task<IReadOnlyList<VoucherDto>> GetVouchersAsync(CancellationToken ct = default)
    {
        var res = await _repo.GetVouchersAsync(ct);
        return res.Success && res.Data != null ? res.Data : Array.Empty<VoucherDto>();
    }

    public async Task<bool> SaveVoucherAsync(VoucherDto voucher)
    {
        var res = await _repo.SaveVoucherAsync(voucher);
        return res.Success;
    }

    public async Task<bool> DeleteVoucherAsync(string code)
    {
        var res = await _repo.DeleteVoucherAsync(code);
        return res.Success;
    }
}
