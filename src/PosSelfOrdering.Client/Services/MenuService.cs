using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Services;

public interface IMenuService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemDto>> GetMenuItemsAsync(string? categoryId = null, string? search = null, CancellationToken ct = default);
    Task<MenuItemDto?> GetMenuItemByIdAsync(string id, CancellationToken ct = default);
}

public sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepo;

    public MenuService(IMenuRepository menuRepo)
    {
        _menuRepo = menuRepo;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var response = await _menuRepo.GetCategoriesAsync(ct);
        return response.Success && response.Data is not null ? response.Data : Array.Empty<CategoryDto>();
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetMenuItemsAsync(string? categoryId = null, string? search = null, CancellationToken ct = default)
    {
        var response = await _menuRepo.GetMenuItemsAsync(categoryId, search, ct);
        return response.Success && response.Data is not null ? response.Data : Array.Empty<MenuItemDto>();
    }

    public async Task<MenuItemDto?> GetMenuItemByIdAsync(string id, CancellationToken ct = default)
    {
        var response = await _menuRepo.GetMenuItemByIdAsync(id, ct);
        return response.Success ? response.Data : null;
    }
}
