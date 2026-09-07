namespace PosSelfOrdering.Client.DTOs.Menu;

public sealed record CategoryDto(
    string Id,
    string Name,
    int DisplayOrder,
    string IconUrl,
    int ItemCount
);

public sealed record ModifierOptionDto(
    string Id,
    string Name,
    decimal ExtraPrice,
    bool IsDefault
);

public sealed record ModifierGroupDto(
    string Id,
    string Name,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    IReadOnlyList<ModifierOptionDto> Options
);

public sealed record MenuItemDto(
    string Id,
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
