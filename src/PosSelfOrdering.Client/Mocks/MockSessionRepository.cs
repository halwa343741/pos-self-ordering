using PosSelfOrdering.Client.DTOs.Common;
using PosSelfOrdering.Client.DTOs.Session;
using PosSelfOrdering.Client.Repositories.Contracts;

namespace PosSelfOrdering.Client.Mocks;

public sealed class MockSessionRepository : ISessionRepository
{
    public async Task<ApiResponse<TableSessionDto>> InitSessionAsync(InitSessionRequest request, CancellationToken ct = default)
    {
        await Task.Delay(350, ct);

        if (string.IsNullOrWhiteSpace(request.TableNumber) && request.OrderType == "DineIn")
        {
            return ApiResponse<TableSessionDto>.Fail("Nomor meja wajib diisi untuk pesanan Dine-In.", 400);
        }

        var session = new TableSessionDto(
            SessionId: $"sess_{Guid.NewGuid():N}"[..12],
            Token: $"jwt_mock_{Guid.NewGuid():N}",
            TableNumber: request.OrderType == "Takeaway" ? "TAKEAWAY" : request.TableNumber.Trim().ToUpper(),
            OrderType: request.OrderType,
            StoreName: "Kedai J.A",
            ExpiresAtUtc: DateTime.UtcNow.AddHours(4)
        );

        return ApiResponse<TableSessionDto>.Ok(session, "Sesi berhasil diinisialisasi.");
    }
}
