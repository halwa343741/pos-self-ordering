using PosSelfOrdering.Client.DTOs.Menu;

namespace PosSelfOrdering.Client.Models;

public sealed class CartItem
{
    public string CartItemId { get; set; } = Guid.NewGuid().ToString("N");
    public MenuItemDto MenuItem { get; set; }
    public List<ModifierOptionDto> SelectedOptions { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;

    public CartItem(string cartItemId, MenuItemDto menuItem, IEnumerable<ModifierOptionDto> selectedOptions, int quantity, string notes)
    {
        CartItemId = cartItemId;
        MenuItem = menuItem;
        SelectedOptions = selectedOptions.ToList();
        Quantity = Math.Max(1, quantity);
        Notes = notes ?? string.Empty;
    }

    public decimal UnitPrice => MenuItem.BasePrice + SelectedOptions.Sum(o => o.ExtraPrice);

    public decimal TotalPrice => UnitPrice * Quantity;

    public string ModifiersSummary => SelectedOptions.Any()
        ? string.Join(", ", SelectedOptions.Select(o => o.Name))
        : string.Empty;

    public bool IsSameConfiguration(string menuItemId, IReadOnlyList<ModifierOptionDto> options, string notes)
    {
        if (MenuItem.Id != menuItemId) return false;
        if ((Notes ?? string.Empty).Trim() != (notes ?? string.Empty).Trim()) return false;

        if (SelectedOptions.Count != options.Count) return false;

        var currentIds = SelectedOptions.Select(o => o.Id).OrderBy(id => id);
        var targetIds = options.Select(o => o.Id).OrderBy(id => id);

        return currentIds.SequenceEqual(targetIds);
    }
}
