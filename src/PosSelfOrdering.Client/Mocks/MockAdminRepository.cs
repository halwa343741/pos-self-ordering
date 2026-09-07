using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.Store;

namespace PosSelfOrdering.Client.Mocks;

public sealed class MockAdminRepository : IAdminRepository
{
    private readonly SharedPosDataStore _store;

    public MockAdminRepository(SharedPosDataStore store)
    {
        _store = store;
    }

    public async Task<ApiResponse<AdminUserDto>> AuthenticatePinAsync(string pin, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);

        return pin switch
        {
            "1234" => ApiResponse<AdminUserDto>.Ok(new AdminUserDto(
                UserId: "usr-cashier-01",
                FullName: "Kasir & Dapur Staff",
                Role: "Cashier",
                Token: $"tok_cashier_{Guid.NewGuid():N}",
                ExpiresAtUtc: DateTime.UtcNow.AddHours(8)
            )),
            "8888" => ApiResponse<AdminUserDto>.Ok(new AdminUserDto(
                UserId: "usr-mgr-01",
                FullName: "Store Manager J.A",
                Role: "Manager",
                Token: $"tok_manager_{Guid.NewGuid():N}",
                ExpiresAtUtc: DateTime.UtcNow.AddHours(8)
            )),
            _ => ApiResponse<AdminUserDto>.Fail("PIN salah! Gunakan 1234 (Kasir/Dapur) atau 8888 (Manager).", 401)
        };
    }

    public async Task<ApiResponse<IReadOnlyList<AdminOrderDto>>> GetActiveOrdersAsync(CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var orders = _store.Orders.Values
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToList();
        return ApiResponse<IReadOnlyList<AdminOrderDto>>.Ok(orders);
    }

    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(string orderNumber, string newStatus, string? reason = null, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        if (_store.Orders.ContainsKey(orderNumber))
        {
            _store.UpdateOrderStatus(orderNumber, newStatus, reason);
            return ApiResponse<bool>.Ok(true, $"Status pesanan {orderNumber} diubah ke {newStatus}.");
        }
        return ApiResponse<bool>.Fail("Pesanan tidak ditemukan.", 404);
    }

    public async Task<ApiResponse<MenuItemDto>> SaveMenuItemAsync(UpsertMenuItemRequest request, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        var id = !string.IsNullOrWhiteSpace(request.Id) ? request.Id : $"menu_{Guid.NewGuid():N}"[..8];

        var existingModifierGroups = _store.MenuItems.TryGetValue(id, out var ex) ? ex.ModifierGroups : new List<ModifierGroupDto>();
        var modifierGroups = request.ModifierGroups ?? existingModifierGroups ?? new List<ModifierGroupDto>();

        var menuItem = new MenuItemDto(
            Id: id,
            CategoryId: request.CategoryId,
            Name: request.Name,
            Description: request.Description,
            BasePrice: request.BasePrice,
            ImageUrl: !string.IsNullOrWhiteSpace(request.ImageUrl) ? request.ImageUrl : "https://images.unsplash.com/photo-1541167760496-1628856ab772?w=500&auto=format&fit=crop&q=60",
            IsAvailable: request.IsAvailable,
            IsBestSeller: request.IsBestSeller,
            IsSpicy: request.IsSpicy,
            ModifierGroups: modifierGroups
        );

        _store.UpsertMenuItem(menuItem);
        return ApiResponse<MenuItemDto>.Ok(menuItem, "Menu berhasil disimpan.");
    }

    public async Task<ApiResponse<bool>> ToggleMenuItemAvailabilityAsync(string menuItemId, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        if (_store.MenuItems.ContainsKey(menuItemId))
        {
            _store.ToggleItemAvailability(menuItemId);
            return ApiResponse<bool>.Ok(true, "Status stok ketersediaan menu berhasil diperbarui.");
        }
        return ApiResponse<bool>.Fail("Menu tidak ditemukan.", 404);
    }

    public async Task<ApiResponse<bool>> DeleteMenuItemAsync(string menuItemId, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        if (_store.MenuItems.ContainsKey(menuItemId))
        {
            _store.DeleteMenuItem(menuItemId);
            return ApiResponse<bool>.Ok(true, "Menu berhasil dihapus.");
        }
        return ApiResponse<bool>.Fail("Menu tidak ditemukan.", 404);
    }

    public async Task<ApiResponse<IReadOnlyList<TableStatusDto>>> GetTableStatusesAsync(CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var tables = _store.Tables.Values.OrderBy(t => t.TableNumber).ToList();
        return ApiResponse<IReadOnlyList<TableStatusDto>>.Ok(tables);
    }

    public async Task<ApiResponse<bool>> ClearTableAsync(string tableNumber, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        if (_store.Tables.ContainsKey(tableNumber))
        {
            _store.ClearTable(tableNumber);
            return ApiResponse<bool>.Ok(true, $"Meja {tableNumber} berhasil di-reset menjadi Kosong.");
        }
        return ApiResponse<bool>.Fail("Meja tidak ditemukan.", 404);
    }

    public async Task<ApiResponse<SalesSummaryDto>> GetDailySalesReportAsync(DateTime date, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        var allOrders = _store.Orders.Values.ToList();
        var paidOrders = allOrders.Where(o => o.IsPaid || o.Status is "Cooking" or "Ready" or "Completed").ToList();

        var totalRevenue = paidOrders.Sum(o => o.GrandTotal);
        var totalOrders = paidOrders.Count;
        var aov = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

        var revByMethod = paidOrders
            .GroupBy(o => string.IsNullOrWhiteSpace(o.PaymentMethod) ? "Cashier" : o.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.GrandTotal));

        // Top selling items
        var itemGroups = paidOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.Name)
            .Select(g => new TopSellingItemDto(
                Name: g.Key,
                QuantitySold: g.Sum(x => x.Quantity),
                TotalRevenue: g.Sum(x => x.Subtotal)
            ))
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToList();

        var summary = new SalesSummaryDto(
            TotalRevenueToday: totalRevenue,
            TotalOrdersCount: totalOrders,
            AverageOrderValue: aov,
            RevenueByPaymentMethod: revByMethod,
            TopSellingItems: itemGroups,
            RecentTransactions: allOrders.OrderByDescending(o => o.CreatedAtUtc).ToList()
        );

        return ApiResponse<SalesSummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<StoreSettingsDto>> GetStoreSettingsAsync(CancellationToken ct = default)
    {
        await Task.Delay(20, ct);
        return ApiResponse<StoreSettingsDto>.Ok(_store.Settings);
    }

    public async Task<ApiResponse<bool>> UpdateStoreSettingsAsync(StoreSettingsDto settings, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        _store.UpdateSettings(settings);
        return ApiResponse<bool>.Ok(true, "Pengaturan toko berhasil disimpan.");
    }

    public async Task<ApiResponse<IReadOnlyList<VoucherDto>>> GetVouchersAsync(CancellationToken ct = default)
    {
        await Task.Delay(30, ct);
        return ApiResponse<IReadOnlyList<VoucherDto>>.Ok(_store.Vouchers.Values.ToList());
    }

    public async Task<ApiResponse<bool>> SaveVoucherAsync(VoucherDto voucher, CancellationToken ct = default)
    {
        await Task.Delay(30, ct);
        _store.UpsertVoucher(voucher);
        return ApiResponse<bool>.Ok(true, $"Voucher {voucher.Code} berhasil disimpan.");
    }

    public async Task<ApiResponse<bool>> DeleteVoucherAsync(string code, CancellationToken ct = default)
    {
        await Task.Delay(30, ct);
        _store.DeleteVoucher(code);
        return ApiResponse<bool>.Ok(true, $"Voucher {code} berhasil dihapus.");
    }
}
