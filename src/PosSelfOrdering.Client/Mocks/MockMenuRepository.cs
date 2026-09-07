using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.Store;

namespace PosSelfOrdering.Client.Mocks;

public sealed class MockMenuRepository : IMenuRepository
{
    private readonly SharedPosDataStore _store;

    public MockMenuRepository(SharedPosDataStore store)
    {
        _store = store;
    }

    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var categories = _store.Categories.Values.OrderBy(c => c.DisplayOrder).ToList();
        return ApiResponse<IReadOnlyList<CategoryDto>>.Ok(categories);
    }

    public async Task<ApiResponse<IReadOnlyList<MenuItemDto>>> GetMenuItemsAsync(string? categoryId, string? search, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        var query = _store.MenuItems.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(x => x.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return ApiResponse<IReadOnlyList<MenuItemDto>>.Ok(query.ToList());
    }

    public async Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(string id, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        if (_store.MenuItems.TryGetValue(id, out var item))
        {
            return ApiResponse<MenuItemDto>.Ok(item);
        }

        return ApiResponse<MenuItemDto>.Fail("Menu item tidak ditemukan.", 404);
    }
}
