namespace PosSelfOrdering.Client.Models.Enums;

public enum OrderStatus
{
    PendingPayment,
    Confirmed,
    Cooking,
    Ready,
    Completed,
    Cancelled
}

public enum OrderType
{
    DineIn,
    Takeaway
}

public enum PaymentMethod
{
    QRIS,
    GoPay,
    ShopeePay,
    Cashier
}
