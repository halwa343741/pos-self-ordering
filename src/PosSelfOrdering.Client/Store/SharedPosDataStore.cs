using System.Collections.Concurrent;
using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Mocks.MockData;

namespace PosSelfOrdering.Client.Store;

/// <summary>
/// Thread-safe singleton in-memory reactive store shared between Admin and Customer modules.
/// Any mutation method raises the corresponding event so Blazor components can react immediately.
/// Cross-tab synchronisation is handled by BroadcastChannelService.
/// </summary>
public sealed class SharedPosDataStore
{
    // ─── Internal Storage ───────────────────────────────────────────────────
    private readonly ConcurrentDictionary<string, CategoryDto>   _categories = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, MenuItemDto>   _menuItems  = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, AdminOrderDto>  _orders     = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TableStatusDto> _tables    = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, VoucherDto>    _vouchers   = new(StringComparer.OrdinalIgnoreCase);
    private StoreSettingsDto _settings = new(
        StoreName: "Kedai J.A",
        Address: "Jl. Senopati No. 45, Kebayoran Baru, Jakarta Selatan",
        TaxRatePercent: 10m,
        ServiceChargePercent: 5m,
        ThermalPaperWidthMm: 58,
        AutoPrintReceipt: true
    );

    // ─── Public Read Views ──────────────────────────────────────────────────
    public IReadOnlyDictionary<string, CategoryDto>    Categories => _categories;
    public IReadOnlyDictionary<string, MenuItemDto>    MenuItems  => _menuItems;
    public IReadOnlyDictionary<string, AdminOrderDto>  Orders     => _orders;
    public IReadOnlyDictionary<string, TableStatusDto> Tables     => _tables;
    public IReadOnlyDictionary<string, VoucherDto>     Vouchers   => _vouchers;
    public StoreSettingsDto Settings => _settings;

    // ─── Events ─────────────────────────────────────────────────────────────
    /// <summary>Fired whenever the menu catalogue changes (add/edit/delete/toggle).</summary>
    public event Action? OnMenuCatalogUpdated;

    /// <summary>Fired when a new customer order is added to the store.</summary>
    public event Action<AdminOrderDto>? OnOrderCreated;

    /// <summary>Fired when an existing order's status changes.</summary>
    public event Action<string, string>? OnOrderStatusUpdated;   // (orderNumber, newStatus)

    /// <summary>Fired when a table status changes.</summary>
    public event Action? OnTableStatusUpdated;

    /// <summary>Fired when vouchers are updated.</summary>
    public event Action? OnVouchersUpdated;

    /// <summary>Fired when store settings are updated.</summary>
    public event Action? OnSettingsUpdated;

    public SharedPosDataStore()
    {
        SeedInitialData();
    }

    // ─── Seed Data ──────────────────────────────────────────────────────────
    private void SeedInitialData()
    {
        // 1. Seed Categories
        foreach (var cat in MockCategories.All)
        {
            _categories[cat.Id] = cat;
        }

        // 2. Seed Menu Items
        foreach (var item in MockMenuItems.All)
        {
            _menuItems[item.Id] = item;
        }

        // 3. Seed Tables (Meja 1 s/d 20)
        for (int i = 1; i <= 20; i++)
        {
            var tableNum = $"T-{i:D2}";
            _tables[tableNum] = new TableStatusDto(
                TableNumber: tableNum,
                Status: "Available",
                ActiveOrderNumber: null,
                ActiveItemCount: null,
                OccupiedSinceUtc: null
            );
        }

        // 4. Seed Vouchers
        _vouchers["DISKON10"] = new VoucherDto("DISKON10", "Diskon Rp 10.000 (Min. Transaksi Rp 50.000)", 10000m, false, 50000m, true);
        _vouchers["HEMAT20"] = new VoucherDto("HEMAT20", "Diskon 20% Maksimal Rp 30.000", 20m, true, 75000m, true);
        _vouchers["WELCOME5"] = new VoucherDto("WELCOME5", "Voucher Pelanggan Baru Rp 5.000", 5000m, false, 25000m, true);

        // 5. Seed Sample Orders for operational demo
        var now = DateTime.UtcNow;

        var sample1 = new AdminOrderDto(
            OrderNumber: "ORD-20260815-101",
            SessionId: "sess-01",
            TableNumber: "T-01",
            OrderType: "DineIn",
            CustomerName: "Budi Santoso",
            CustomerNotes: "Tolong kopinya dibuat agak panas ya",
            Status: "PendingPayment",
            Subtotal: 56000m,
            TaxAmount: 5600m,
            ServiceCharge: 2800m,
            GrandTotal: 64400m,
            PaymentMethod: "Cashier",
            IsPaid: false,
            CreatedAtUtc: now.AddMinutes(-4),
            Items: new List<AdminOrderItemDto>
            {
                new("c01", "Kopi Susu Gula Aren", 2, 22000m, 44000m, "Regular, Normal Ice, Less Sugar", "Jangan terlalu manis"),
                new("s01", "Cireng Crispy Bumbu Rujak", 1, 12000m, 12000m, "Standard", "Sambal dipisah")
            }
        );

        var sample2 = new AdminOrderDto(
            OrderNumber: "ORD-20260815-102",
            SessionId: "sess-02",
            TableNumber: "T-03",
            OrderType: "DineIn",
            CustomerName: "Siti Rahma",
            CustomerNotes: null,
            Status: "Cooking",
            Subtotal: 82000m,
            TaxAmount: 8200m,
            ServiceCharge: 4100m,
            GrandTotal: 94300m,
            PaymentMethod: "QRIS",
            IsPaid: true,
            CreatedAtUtc: now.AddMinutes(-12),
            Items: new List<AdminOrderItemDto>
            {
                new("m01", "Nasi Goreng Spesial J.A", 1, 35000m, 35000m, "Level Pedas Sedang, Telur Ceplok Matang", null),
                new("m02", "Mie Goreng Dok-Dok", 1, 27000m, 27000m, "Pedas", null),
                new("nc02", "Matcha Latte", 1, 20000m, 20000m, "Large, Less Ice", null)
            }
        );

        var sample3 = new AdminOrderDto(
            OrderNumber: "ORD-20260815-103",
            SessionId: "sess-03",
            TableNumber: "T-05",
            OrderType: "Takeaway",
            CustomerName: "Dimas",
            CustomerNotes: "Bungkus terpisah",
            Status: "Ready",
            Subtotal: 38000m,
            TaxAmount: 3800m,
            ServiceCharge: 1900m,
            GrandTotal: 43700m,
            PaymentMethod: "ShopeePay",
            IsPaid: true,
            CreatedAtUtc: now.AddMinutes(-20),
            Items: new List<AdminOrderItemDto>
            {
                new("d01", "Croissant Butter Almond", 1, 20000m, 20000m, "Hangatkan", null),
                new("c03", "Caramel Macchiato", 1, 18000m, 18000m, "Hot", null)
            }
        );

        var sample4 = new AdminOrderDto(
            OrderNumber: "ORD-20260815-098",
            SessionId: "sess-04",
            TableNumber: "T-02",
            OrderType: "DineIn",
            CustomerName: "Anisa",
            CustomerNotes: null,
            Status: "Completed",
            Subtotal: 45000m,
            TaxAmount: 4500m,
            ServiceCharge: 2250m,
            GrandTotal: 51750m,
            PaymentMethod: "QRIS",
            IsPaid: true,
            CreatedAtUtc: now.AddMinutes(-55),
            Items: new List<AdminOrderItemDto>
            {
                new("c02", "Americano Double Shot", 1, 20000m, 20000m, "Ice", null),
                new("s02", "Kentang Goreng Keju", 1, 25000m, 25000m, "Extra Cheese", null)
            }
        );

        _orders[sample1.OrderNumber] = sample1;
        _orders[sample2.OrderNumber] = sample2;
        _orders[sample3.OrderNumber] = sample3;
        _orders[sample4.OrderNumber] = sample4;

        // Mark corresponding tables as occupied
        _tables["T-01"] = _tables["T-01"] with
        {
            Status = "Billing",
            ActiveOrderNumber = sample1.OrderNumber,
            ActiveItemCount = 3,
            OccupiedSinceUtc = now.AddMinutes(-4)
        };

        _tables["T-03"] = _tables["T-03"] with
        {
            Status = "Occupied",
            ActiveOrderNumber = sample2.OrderNumber,
            ActiveItemCount = 3,
            OccupiedSinceUtc = now.AddMinutes(-12)
        };
    }

    // ─── Menu Mutations ──────────────────────────────────────────────────────
    public void UpsertMenuItem(MenuItemDto item)
    {
        _menuItems[item.Id] = item;
        NotifyMenuChanged();
    }

    public void ToggleItemAvailability(string itemId)
    {
        if (_menuItems.TryGetValue(itemId, out var existing))
        {
            _menuItems[itemId] = existing with { IsAvailable = !existing.IsAvailable };
            NotifyMenuChanged();
        }
    }

    public void DeleteMenuItem(string itemId)
    {
        _menuItems.TryRemove(itemId, out _);
        NotifyMenuChanged();
    }

    private void NotifyMenuChanged() => OnMenuCatalogUpdated?.Invoke();

    // ─── Order Mutations ─────────────────────────────────────────────────────
    public void AddOrder(AdminOrderDto order)
    {
        _orders[order.OrderNumber] = order;
        OnOrderCreated?.Invoke(order);

        // Mark table as Occupied or Billing based on payment
        if (!string.IsNullOrWhiteSpace(order.TableNumber) && _tables.TryGetValue(order.TableNumber, out var table))
        {
            var status = order.IsPaid ? "Occupied" : "Billing";
            _tables[order.TableNumber] = table with
            {
                Status = status,
                ActiveOrderNumber = order.OrderNumber,
                ActiveItemCount = order.Items?.Count ?? 1,
                OccupiedSinceUtc = order.CreatedAtUtc
            };
            OnTableStatusUpdated?.Invoke();
        }
    }

    public void UpdateOrderStatus(string orderNumber, string newStatus, string? reason = null)
    {
        if (_orders.TryGetValue(orderNumber, out var existing))
        {
            var isPaid = newStatus is "Cooking" or "Ready" or "Completed" ? true : existing.IsPaid;
            var updated = existing with { Status = newStatus, IsPaid = isPaid };
            _orders[orderNumber] = updated;
            OnOrderStatusUpdated?.Invoke(orderNumber, newStatus);

            // Free table when order is completed or cancelled
            if (newStatus is "Completed" or "Cancelled" &&
                !string.IsNullOrWhiteSpace(existing.TableNumber) &&
                _tables.TryGetValue(existing.TableNumber, out var table))
            {
                if (table.ActiveOrderNumber == orderNumber)
                {
                    _tables[existing.TableNumber] = table with
                    {
                        Status = "Available",
                        ActiveOrderNumber = null,
                        ActiveItemCount = null,
                        OccupiedSinceUtc = null
                    };
                    OnTableStatusUpdated?.Invoke();
                }
            }
            else if (newStatus is "Cooking" &&
                     !string.IsNullOrWhiteSpace(existing.TableNumber) &&
                     _tables.TryGetValue(existing.TableNumber, out var occTable))
            {
                _tables[existing.TableNumber] = occTable with { Status = "Occupied" };
                OnTableStatusUpdated?.Invoke();
            }
        }
    }

    // ─── Table Mutations ─────────────────────────────────────────────────────
    public void UpsertTable(TableStatusDto table)
    {
        _tables[table.TableNumber] = table;
        OnTableStatusUpdated?.Invoke();
    }

    public void ClearTable(string tableNumber)
    {
        if (_tables.TryGetValue(tableNumber, out var table))
        {
            _tables[tableNumber] = table with
            {
                Status = "Available",
                ActiveOrderNumber = null,
                ActiveItemCount = null,
                OccupiedSinceUtc = null
            };
            OnTableStatusUpdated?.Invoke();
        }
    }

    // ─── Voucher Mutations ───────────────────────────────────────────────────
    public void UpsertVoucher(VoucherDto voucher)
    {
        _vouchers[voucher.Code] = voucher;
        OnVouchersUpdated?.Invoke();
    }

    public void DeleteVoucher(string code)
    {
        _vouchers.TryRemove(code, out _);
        OnVouchersUpdated?.Invoke();
    }

    // ─── Settings Mutations ──────────────────────────────────────────────────
    public void UpdateSettings(StoreSettingsDto settings)
    {
        _settings = settings;
        OnSettingsUpdated?.Invoke();
    }
}
