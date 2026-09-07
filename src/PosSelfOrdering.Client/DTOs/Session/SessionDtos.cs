namespace PosSelfOrdering.Client.DTOs.Session;

public sealed record InitSessionRequest(
    string TableNumber,
    string OrderType,
    string? CustomerName = null,
    string? DeviceIdentifier = null
);

public sealed record TableSessionDto(
    string SessionId,
    string Token,
    string TableNumber,
    string OrderType,
    string StoreName,
    DateTime ExpiresAtUtc
);
