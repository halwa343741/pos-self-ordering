using PosSelfOrdering.Client.DTOs.Menu;
using PosSelfOrdering.Client.Models;
using PosSelfOrdering.Client.State;

namespace PosSelfOrdering.Client.Services;

public interface ICartService
{
    CartState State { get; }
    void AddItem(MenuItemDto item, IReadOnlyList<ModifierOptionDto> options, int quantity, string notes);
    void UpdateQuantity(string cartItemId, int newQty);
    void RemoveItem(string cartItemId);
    void ClearCart();
}

public sealed class CartService : ICartService
{
    private readonly CartState _cartState;

    public CartService(CartState cartState)
    {
        _cartState = cartState;
    }

    public CartState State => _cartState;

    public void AddItem(MenuItemDto item, IReadOnlyList<ModifierOptionDto> options, int quantity, string notes)
    {
        _cartState.AddItem(item, options, quantity, notes);
    }

    public void UpdateQuantity(string cartItemId, int newQty)
    {
        _cartState.UpdateQuantity(cartItemId, newQty);
    }

    public void RemoveItem(string cartItemId)
    {
        _cartState.RemoveItem(cartItemId);
    }

    public void ClearCart()
    {
        _cartState.Clear();
    }
}
