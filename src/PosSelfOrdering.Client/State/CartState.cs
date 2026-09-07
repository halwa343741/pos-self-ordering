using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Models;

namespace PosSelfOrdering.Client.State;

public sealed class CartState
{
    private readonly List<CartItem> _items = new();
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public event Action? OnChange;

    public void AddItem(MenuItemDto item, IReadOnlyList<ModifierOptionDto> selectedModifiers, int quantity, string notes)
    {
        var existing = _items.FirstOrDefault(x => x.IsSameConfiguration(item.Id, selectedModifiers, notes));
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItem(Guid.NewGuid().ToString("N"), item, selectedModifiers, quantity, notes));
        }
        NotifyStateChanged();
    }

    public void UpdateQuantity(string cartItemId, int newQty)
    {
        var item = _items.FirstOrDefault(x => x.CartItemId == cartItemId);
        if (item is null) return;

        if (newQty <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Quantity = newQty;
        }
        NotifyStateChanged();
    }

    public void RemoveItem(string cartItemId)
    {
        var item = _items.FirstOrDefault(x => x.CartItemId == cartItemId);
        if (item is not null)
        {
            _items.Remove(item);
            NotifyStateChanged();
        }
    }

    public void Clear()
    {
        _items.Clear();
        NotifyStateChanged();
    }

    public decimal Subtotal => _items.Sum(i => i.TotalPrice);
    public decimal Tax => Subtotal * 0.10m; // 10% PB1 Restaurant Tax
    public decimal ServiceCharge => Subtotal * 0.05m; // 5% Service Charge
    public decimal GrandTotal => Subtotal + Tax + ServiceCharge;
    public int TotalItemCount => _items.Sum(i => i.Quantity);
    public bool IsEmpty => !_items.Any();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
