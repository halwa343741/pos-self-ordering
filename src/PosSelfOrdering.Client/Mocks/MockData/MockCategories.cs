using PosSelfOrdering.Client.DTOs.Menu;

namespace PosSelfOrdering.Client.Mocks.MockData;

public static class MockCategories
{
    public static readonly IReadOnlyList<CategoryDto> All = new List<CategoryDto>
    {
        new("cat_coffee", "Coffee & Espresso", 1, "☕", 6),
        new("cat_non_coffee", "Non-Coffee & Tea", 2, "🧋", 5),
        new("cat_main_course", "Makanan Utama", 3, "🍛", 6),
        new("cat_snacks", "Snack & Pastry", 4, "🥐", 5),
        new("cat_desserts", "Dessert & Ice Cream", 5, "🍨", 4)
    };
}
