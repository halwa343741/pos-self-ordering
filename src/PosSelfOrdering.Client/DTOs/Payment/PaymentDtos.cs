namespace PosSelfOrdering.Client.DTOs.Payment;

public sealed record PaymentStatusDto(
    string OrderNumber,
    bool IsPaid,
    string PaymentStatus,
    string? TransactionId,
    DateTime? PaidAtUtc
);
