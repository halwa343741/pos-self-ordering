# SPECIFICATION: POS ADMIN & KITCHEN MANAGEMENT PWA (BACKOFFICE)
**Platform:** Blazor Web App (.NET 10) — Integrated Single Solution  
**Architecture:** FE-FIRST + CONTRACT-FIRST + MOCK-FIRST (SHARED IN-MEMORY REACTIVE STORE)  
**Scope:** Admin Portal, Kitchen Display System (KDS), Cashier, Menu & Modifier Management, Table Monitor, Voucher Management, Sales Analytics  
**Target Environment:** Tablet Kasir/Dapur, Desktop PC Manager, Mobile Staff Browser  
**Document Path:** `architecture/spec_admin.md`  
**Status:** 100% Aligned with Customer POS Specification  

---

## 1. PROJECT OVERVIEW & MAPPING TO CUSTOMER POS

### 1.1 Konsep Arsitektur Terintegrasi
Modul Admin ini terintegrasi langsung di dalam satu project Blazor Web App yang sama dengan modul pelanggan (`PosSelfOrdering.Client`).
Semua aksi pelanggan di smartphone/meja memiliki padanan (*counterpart*) langsung di sisi Admin/Dapur:

```text
┌───────────────────────────────────────────────────┬───────────────────────────────────────────────────┐
│ Modul Pelanggan (Customer POS)                    │ Modul Admin / Kasir / Dapur (Backoffice)          │
├───────────────────────────────────────────────────┼───────────────────────────────────────────────────┤
│ 1. Scan QR Meja / Pilih Dine-In / Takeaway        │ 🪑 Table Monitor & QR Sticker Generator (/admin/tables)│
│ 2. Pilih Menu, Ukuran, Level Es/Gula, & Toppings  │ 🍔 Menu & Modifier Group Management (/admin/menu)  │
│ 3. Menu Habis (Sold Out) disabled di katalog      │ ⚡ Instant 1-Click Sold-Out Stock Switch          │
│ 4. Masukkan Kode Promo Diskon (Voucher)           │ 🏷️ Kelola Kode Promo & Diskon (/admin/menu)       │
│ 5. Checkout dengan Catatan Khusus untuk Dapur     │ 🍳 Kitchen Display System (KDS) Live View         │
│ 6. Opsi "Bayar di Kasir"                          │ 💵 Kasir Terima Pembayaran & Konfirmasi Lunas      │
│ 7. Live Tracking (Pending -> Cooking -> Ready)    │ 🔄 Dapur Update Status (Mulai Masak -> Siap)      │
│ 8. Suara Notifikasi Pesanan Siap (Audio Chime)    │ 🔔 Dapur Tekan "Tandai Siap"                      │
│ 9. Struk Pembayaran Digital                       │ 🖨️ Cetak Ulang Struk Thermal Kasir 58mm/80mm       │
│ 10. Pembayaran QRIS Dinamis                       │ 📊 Laporan Rekonsiliasi QRIS vs Tunai (/admin/reports)│
└───────────────────────────────────────────────────┴───────────────────────────────────────────────────┘
```

---

## 2. SHARED IN-MEMORY REACTIVE STORE & REAL-TIME EVENT BUS (MOCK ARCHITECTURE)

Agar perubahan data yang diinput atau diubah oleh admin (seperti update harga, tambah menu baru, atau mematikan stok menu habis) **langsung muncul secara *real-time* di sisi pelanggan**, aplikasi menggunakan arsitektur **Shared In-Memory Reactive Store** yang dilengkapi **Cross-Tab Broadcast Event Bus**.

### 2.1 Konsep Arsitektur Reaktif Tanpa Backend

```text
               ┌────────────────────────────────────────────────────────┐
               │    SharedPosDataStore (Singleton In-Memory State)      │
               │  • ConcurrentDictionary<string, MenuItemDto>          │
               │  • ConcurrentDictionary<string, AdminOrderDto>        │
               │  • ConcurrentDictionary<string, TableStatusDto>       │
               └───────────────────────────┬────────────────────────────┘
                                           │
                        ┌──────────────────┴──────────────────┐
                        ▼                                     ▼
      ┌────────────────────────────────────┐┌────────────────────────────────────┐
      │       ADMIN / KITCHEN PORTAL       ││       CUSTOMER SELF-ORDERING       │
      │         (URL: /admin/*)            ││          (URL: /menu, /)           │
      ├────────────────────────────────────┤├────────────────────────────────────┤
      │ 1. Admin toggle menu "HABIS"       ││ 1. Menu langsung berubah "HABIS"   │
      │ 2. Admin ubah harga Rp 28.000      ││ 2. Harga menu langsung terupdate   │
      │ 3. Dapur tekan "Pesanan Siap"      ││ 3. HP pelanggan bunyi CHIME bell   │
      │ 4. Dapur terima tiket pesanan baru ││ 4. Pelanggan klik checkout         │
      └────────────────────────────────────┘└────────────────────────────────────┘
                        ▲                                     ▲
                        └──────── BroadcastChannel API ───────┘
                              (Sinkronisasi Antar Tab Browser)
```

---

### 2.2 Desain Teknis `SharedPosDataStore.cs`

```csharp
namespace PosSelfOrdering.Client.Mocks;

public sealed class SharedPosDataStore
{
    // Thread-safe In-Memory Collections
    private readonly ConcurrentDictionary<string, MenuItemDto> _menuItems = new();
    private readonly ConcurrentDictionary<string, CategoryDto> _categories = new();
    private readonly ConcurrentDictionary<string, AdminOrderDto> _orders = new();
    private readonly ConcurrentDictionary<string, TableStatusDto> _tables = new();

    // Reactive C# Event Dispatchers
    public event Action? OnMenuCatalogUpdated;
    public event Action<AdminOrderDto>? OnOrderCreated;
    public event Action<string, string>? OnOrderStatusUpdated; // (OrderNumber, NewStatus)
    public event Action? OnTableStatusUpdated;

    public SharedPosDataStore()
    {
        SeedInitialData();
    }

    // --- MENU MUTATIONS ---
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

    // --- ORDER MUTATIONS ---
    public void AddOrder(AdminOrderDto order)
    {
        _orders[order.OrderNumber] = order;
        OnOrderCreated?.Invoke(order);
    }

    public void UpdateOrderStatus(string orderNumber, string newStatus)
    {
        if (_orders.TryGetValue(orderNumber, out var existing))
        {
            var updated = existing with { 
                Status = newStatus, 
                IsPaid = newStatus is "Cooking" or "Ready" or "Completed" ? true : existing.IsPaid 
            };
            _orders[orderNumber] = updated;
            OnOrderStatusUpdated?.Invoke(orderNumber, newStatus);
        }
    }

    private void NotifyMenuChanged() => OnMenuCatalogUpdated?.Invoke();
}
```

---

### 2.3 Sinkronisasi Antar Tab Browser (Cross-Tab Interop via `BroadcastChannel`)

Jika pelanggan dan admin membuka tab browser berbeda pada satu komputer (atau simulasi demo):
1. **JS Interop `BroadcastChannel('pos_shared_bus')`:**
   * Saat Admin men-toggle status menu di tab `/admin/menu`, sebuah message `{"type": "MENU_UPDATED"}` disiarkan via browser channel.
   * Tab pelanggan di `/menu` menangkap event tersebut dan otomatis memanggil `StateHasChanged()` untuk me-render ulang katalog seketika tanpa perlu refresh halaman manual!

---

## 3. URL ROUTING & PERMISSION MATRIX

Semua rute admin berada di bawah path `/admin/` dengan layout khusus `AdminLayout.razor`:

```text
┌───────────────────────────────┬───────────────────────────────────────────┬──────────────────┐
│ Route                         │ Purpose                                   │ Role Minimum     │
├───────────────────────────────┼───────────────────────────────────────────┼──────────────────┤
│ /admin/login                  │ Halaman otentikasi PIN staff/manajer      │ Public (Admin)   │
│ /admin/orders                 │ Kitchen Display System (KDS) & Order Flow │ Cashier, Kitchen │
│ /admin/menu                   │ Kelola Menu, Modifiers, Harga & Stok      │ Manager          │
│ /admin/tables                 │ Monitor Status Meja & QR Code Generator   │ Cashier, Manager │
│ /admin/reports                │ Laporan Penjualan, Omzet & Metode Bayar   │ Manager          │
│ /admin/settings               │ Profil Kedai J.A, Pajak PB1 & Printer     │ Manager          │
└───────────────────────────────┴───────────────────────────────────────────┴──────────────────┘
```

---

## 4. ADMIN LAYOUT & DESIGN SYSTEM

### 4.1 Layout Architecture (`AdminLayout.razor`)
```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ TOPBAR: [Kedai J.A Logo]  Store Status: OPEN  Active Orders: (4)  Staff: Manager (PIN) │
├──────────────┬─────────────────────────────────────────────────────────────────────────┤
│ SIDEBAR      │ MAIN ADMIN CONTENT AREA                                                 │
│ • 📋 Orders  │                                                                         │
│ • 🍔 Menu    │  (Dynamic Component View: KDS Kanban / Menu Editor / Sales Charts)      │
│ • 🪑 Tables  │                                                                         │
│ • 📊 Reports │                                                                         │
│ • ⚙️ Settings│                                                                         │
│ • 🚪 Keluar  │                                                                         │
└──────────────┴─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. DETAILED PAGE SPECIFICATIONS

### 5.1 Admin Authentication (`/admin/login`)
* **Purpose:** Gerbang masuk staff/kasir/manajer menggunakan Virtual PIN Pad atau input keyboard.
* **Default PINs (Mock Mode):**
  * `1234` $\rightarrow$ Role: **Kasir / Staff Dapur** (Akses: Orders & Tables).
  * `8888` $\rightarrow$ Role: **Store Manager** (Akses Penuh: Orders, Menu CRUD, Reports, Settings).
* **State:** `EnteredPin`, `ErrorMessage`, `IsAuthenticating`.

---

### 5.2 Kitchen Display System (KDS) & Order Flow (`/admin/orders`)
* **Purpose:** Pusat kendali operasional dapur dan kasir untuk memproses pesanan masuk secara langsung.
* **Fitur Utama:**
  1. **Notifikasi Suara Pesanan Baru (New Order Alert):** Layar dapur memutar suara notifikasi saat pelanggan selesai checkout.
  2. **Kanban Board 4 Kolom:**
     * **Kolom 1: Menunggu Pembayaran / Kasir (`PendingPayment`)**
       * Menampilkan pesanan "Bayar di Kasir" atau QRIS yang belum lunas.
       * Tombol aksi: **"Konfirmasi Lunas (Cash/EDC)"** atau **"Batalkan / Void"**.
     * **Kolom 2: Sedang Dimasak / Dipersiapkan (`Cooking`)**
       * Menampilkan item makanan/minuman, pilihan modifier (ukuran, less ice, topping), dan catatan khusus pelanggan.
       * Live timer durasi memasak (menghitung menit berjalan).
       * Tombol aksi: **"Tandai Siap (Ready)"**.
     * **Kolom 3: Siap Diambil / Diantar (`Ready`)**
       * Menampilkan nomor meja / nomor antrean pesanan yang sudah siap.
       * Men-trigger bunyi notifikasi di smartphone pelanggan.
       * Tombol aksi: **"Selesaikan Pesanan (Complete)"**.
     * **Kolom 4: Riwayat Selesai (`Completed`)**
       * Menampilkan 10 pesanan terakhir beserta tombol **"Cetak Ulang Struk"**.
  3. **Fitur Batalkan / Void Pesanan:** Modal input alasan pembatalan (misal: bahan baku habis / pelanggan batal).

---

### 5.3 Menu, Modifier, & Voucher Management (`/admin/menu`)
* **Purpose:** Mengelola seluruh katalog hidangan, harga, grup modifier, promo voucher, dan ketersediaan stok.
* **Fitur Utama:**
  1. **Instant Sold-Out Switch:** Toggle switch 1-klik untuk mengubah status menu antara `Tersedia` dan `Habis`. Perubahan langsung dipancarkan ke `SharedPosDataStore` sehingga menu di HP pelanggan seketika menjadi *disabled/Habis*.
  2. **Modal Tambah / Edit Menu:**
     * Field: Nama Menu, Kategori, Harga Dasar, URL Foto, Deskripsi, Badge (Best Seller / Pedas).
     * Kelola Modifier Groups (Contoh: Pilihan Ukuran, Level Es, Topping Tambahan dengan harga ekstra).
  3. **Kelola Promo Voucher Diskon:**
     * Menambah kode voucher baru (misal: `DISKON10` potongan Rp 10.000 atau diskon 15%).
  4. **Hapus Menu:** Menghapus menu dengan modal konfirmasi keselamatan (*Confirmation Dialog*).
  5. **Tambah Menu Baru (Upsert):** Admin dapat menambahkan menu baru melalui modal. Data menu disimpan ke `SharedPosDataStore.UpsertMenuItem`, yang secara otomatis memicu `OnMenuCatalogUpdated` → BroadcastChannel → UI pelanggan memperbarui katalog tanpa refresh.

---

### 5.4 Table Monitor & QR Generator (`/admin/tables`)
* **Purpose:** Memantau keterisian meja restoran secara visual dan mencetak stiker QR code meja.
* **Fitur Utama:**
  * **Grid Status Meja (Meja 1 s/d Meja 20):**
    * 🟢 **Kosong (Available):** Meja siap ditempati pelanggan baru.
    * 🔴 **Terisi (Occupied):** Meja sedang memiliki pesanan aktif (menampilkan ID pesanan & durasi duduk).
    * 🟡 **Menunggu Kasir (Billing):** Meja telah memesan dan menunggu pembayaran kasir.
  * **QR Code Generator & Print Sticker Modal:** Menampilkan QR code yang siap dicetak (*print-ready*) dengan URL pemesanan langsung: `https://domain.com/table/T-05`.
  * **Tombol Kosongkan Meja (Clear Table):** Reset status meja menjadi Available setelah pelanggan meninggalkan restoran.

---

### 5.5 Laporan Penjualan & Analytics (`/admin/reports`)
* **Purpose:** Ringkasan performa finansial harian restoran bagi manajer/pemilik toko.
* **Komponen & Metrik:**
  1. **Stat Cards:** Total Omzet Hari Ini, Total Pesanan Sukses, Rata-rata Nilai Transaksi.
  2. **Breakdown Metode Pembayaran:** QRIS (%) | GoPay / ShopeePay (%) | Tunai di Kasir (%).
  3. **Top 5 Menu Terlaris (Best Sellers)**.
  4. **Tabel Riwayat Transaksi Harian** lengkap dengan filter tanggal dan status.

---

### 5.6 Store & Printer Settings (`/admin/settings`)
* **Purpose:** Pengaturan nama toko, persentase pajak PB1, service charge, dan simulasi printer thermal.
* **Field:** Nama Toko, Alamat, Tarif PB1 (10%), Service Charge (5%), Lebar Kertas (58mm/80mm), Auto-Print Struk.

---

## 6. UNIFIED ADMIN API CONTRACT & DTOS

```csharp
namespace PosSelfOrdering.Client.DTOs.Admin;

// Staff Authentication
public sealed record AdminLoginRequest(string PinCode);

public sealed record AdminUserDto(
    string UserId,
    string FullName,
    string Role, // "Cashier" | "Kitchen" | "Manager"
    string Token,
    DateTime ExpiresAtUtc
);

// Admin Order Management DTOs
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
    string Status, // "PendingPayment" | "Cooking" | "Ready" | "Completed" | "Cancelled"
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

// Menu & Modifier Management DTOs
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
    IReadOnlyList<ModifierGroupDto> ModifierGroups
);

// Voucher Promo DTOs
public sealed record VoucherDto(
    string Code,
    string Description,
    decimal DiscountAmount,
    bool IsPercentage,
    decimal MinOrderAmount,
    bool IsActive
);

// Table Management DTOs
public sealed record TableStatusDto(
    string TableNumber,
    string Status, // "Available" | "Occupied" | "Billing"
    string? ActiveOrderNumber,
    int? ActiveItemCount,
    DateTime? OccupiedSinceUtc
);

// Reports DTOs
public sealed record SalesSummaryDto(
    decimal TotalRevenueToday,
    int TotalOrdersCount,
    decimal AverageOrderValue,
    IReadOnlyDictionary<string, decimal> RevenueByPaymentMethod,
    IReadOnlyList<TopSellingItemDto> TopSellingItems,
    IReadOnlyList<AdminOrderDto> RecentTransactions
);

public sealed record TopSellingItemDto(
    string Name,
    int QuantitySold,
    decimal TotalRevenue
);
```

---

## 7. REPOSITORY & SERVICE ABSTRACTION (ADMIN)

### 7.1 `IAdminRepository.cs`
```csharp
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
}
```

### 7.2 `IAdminService.cs`
```csharp
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
}
```

---

## 8. IMPLEMENTATION PHASES (ADMIN MODULE)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ Phase A1: Shared In-Memory Data Store & Event Hub                      │
│ • Implement SharedPosDataStore.cs (Singleton thread-safe memory DB)    │
│ • Implement Cross-tab BroadcastChannel JS Interop                      │
│ • AdminLayout.razor (Sidebar, Topbar, Store status)                    │
│ • /admin/login (Virtual PIN Pad: Kasir 1234, Manager 8888)             │
│ • AdminState.cs & IAdminService session management                     │
├────────────────────────────────────────────────────────────────────────┤
│ Phase A2: Kitchen Display System (KDS) & Order Flow                    │
│ • /admin/orders (Kanban Board: PendingPayment -> Cooking -> Ready)     │
│ • OrderKanbanCard.razor with timer & action triggers                   │
│ • Audio alert on new incoming order from customer checkout            │
├────────────────────────────────────────────────────────────────────────┤
│ Phase A3: Menu, Modifier & Stock Management                            │
│ • /admin/menu (Menu list, instant Sold-Out toggle, price editing)      │
│ • MenuEditorModal.razor (Create/Edit menu & modifier groups)           │
│ • Real-time event propagation to /menu customer catalog                │
├────────────────────────────────────────────────────────────────────────┤
│ Phase A4: Table Monitor & Sales Analytics                              │
│ • /admin/tables (Table occupancy grid & QR code print modal)           │
│ • /admin/reports (StatCards, payment breakdown, top sellers)           │
│ • /admin/settings (Store profile & printer simulation)                 │
├────────────────────────────────────────────────────────────────────────┤
│ Phase A5: Verification & End-to-End Testing                            │
│ • Build verification (dotnet build)                                    │
│ • E2E Flow: Pelanggan Pesan -> Dapur Masak -> Kasir Lunas -> Laporan  │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 9. CHECKPOINT & PROGRESS TRACKING (`checkpoint2.md`)

> **Aturan Implementasi:** Setiap kali mengerjakan spesifikasi ini, **wajib mencatat progress secara real-time** ke file `architecture/checkpoint2.md`. Tujuannya agar jika token limit tercapai, pekerjaan dapat dilanjutkan dari titik terakhir tanpa harus menganalisis ulang dari awal.

### 9.0 ⚠️ WAJIB DILAKUKAN SEBELUM MULAI IMPLEMENTASI APAPUN

> **STOP. Baca dulu `checkpoint2.md` sebelum menulis satu baris kode pun.**

Langkah wajib sebelum memulai:
1. **Buka dan baca `architecture/checkpoint2.md`** — lihat section `✅ Completed`, `🔄 In Progress`, dan `⏳ Pending`.
2. **Verifikasi file yang tercatat sebagai Completed** — pastikan file tersebut benar-benar ada di filesystem dan isinya sudah benar (bukan hanya tertulis di checkpoint, tapi belum dibuat).
3. **Identifikasi titik lanjut** — lanjutkan dari item pertama di `🔄 In Progress`, bukan dari awal.
4. **Jika checkpoint2.md belum ada** — buat file tersebut terlebih dahulu dengan format di bawah, lalu mulai dari Phase A1.
5. **Jangan ulangi pekerjaan yang sudah selesai** — cek ulang filesystem sebelum membuat file baru.

### Format `checkpoint2.md`

```markdown
# CHECKPOINT 2 — Admin Module Implementation Progress

**Last Updated:** <timestamp>
**Phase:** <current phase>
**Status:** <IN PROGRESS | DONE | BLOCKED>

---

## ✅ Completed
- [ Fase / File / Task yang sudah selesai ]

## 🔄 In Progress
- [ Task yang sedang dikerjakan saat ini ]
- File: <path file>
- Langkah berikutnya: <deskripsi langkah selanjutnya>

## ⏳ Pending
- [ Task yang belum dikerjakan ]

## ⚠️ Blockers / Notes
- [ Catatan kendala atau keputusan desain penting ]
```

### Kapan Harus Update `checkpoint2.md`
1. **Sebelum mulai** setiap phase baru → catat phase yang akan dikerjakan.
2. **Setelah selesai** setiap file baru dibuat atau dimodifikasi → tandai ✅ Completed.
3. **Sebelum berhenti** (karena token limit atau instruksi user) → catat posisi terakhir di bagian 🔄 In Progress dengan detail langkah berikutnya.

