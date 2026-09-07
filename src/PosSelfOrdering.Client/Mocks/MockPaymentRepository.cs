using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Payment;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Mocks;

public sealed class MockPaymentRepository : IPaymentRepository
{
    public async Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);

        // Simulate successful payment
        var result = new PaymentStatusDto(
            OrderNumber: orderNumber,
            IsPaid: true,
            PaymentStatus: "Success",
            TransactionId: $"TRX-MOCK-{Guid.NewGuid():N}"[..18].ToUpper(),
            PaidAtUtc: DateTime.UtcNow
        );

        return ApiResponse<PaymentStatusDto>.Ok(result, "Pembayaran berhasil diverifikasi.");
    }
}
