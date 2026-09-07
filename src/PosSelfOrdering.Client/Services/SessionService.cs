using PosSelfOrdering.Client.DTOs.Session;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.State;

namespace PosSelfOrdering.Client.Services;

public interface ISessionService
{
    TableSessionDto? CurrentSession { get; }
    bool HasActiveSession { get; }
    Task<bool> InitializeSessionAsync(string tableNumber, string orderType, string? customerName = null, CancellationToken ct = default);
    void ClearSession();
}

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepo;
    private readonly SessionState _sessionState;

    public SessionService(ISessionRepository sessionRepo, SessionState sessionState)
    {
        _sessionRepo = sessionRepo;
        _sessionState = sessionState;
    }

    public TableSessionDto? CurrentSession => _sessionState.CurrentSession;
    public bool HasActiveSession => _sessionState.HasActiveSession;

    public async Task<bool> InitializeSessionAsync(string tableNumber, string orderType, string? customerName = null, CancellationToken ct = default)
    {
        var request = new InitSessionRequest(tableNumber, orderType, customerName);
        var response = await _sessionRepo.InitSessionAsync(request, ct);

        if (response.Success && response.Data is not null)
        {
            _sessionState.SetSession(response.Data);
            return true;
        }

        return false;
    }

    public void ClearSession()
    {
        _sessionState.ClearSession();
    }
}
