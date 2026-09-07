using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Payment;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Services;

public interface IPaymentService
{
    Task<ApiResponse<PaymentStatusDto>> CheckPaymentStatusAsync(string orderNumber, CancellationToken ct = default);
}

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;

    public PaymentService(IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public async Task<ApiResponse<PaymentStatusDto>> CheckPaymentStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _paymentRepo.GetPaymentStatusAsync(orderNumber, ct);
    }
}
