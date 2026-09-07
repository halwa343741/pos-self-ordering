using PosSelfOrdering.Client.DTOs.Session;

namespace PosSelfOrdering.Client.State;

public sealed class SessionState
{
    public TableSessionDto? CurrentSession { get; private set; }
    public bool HasActiveSession => CurrentSession is not null;

    public event Action? OnChange;

    public void SetSession(TableSessionDto session)
    {
        CurrentSession = session;
        NotifyStateChanged();
    }

    public void UpdateTable(string tableNumber, string orderType)
    {
        if (CurrentSession is not null)
        {
            CurrentSession = CurrentSession with
            {
                TableNumber = orderType == "Takeaway" ? "TAKEAWAY" : tableNumber.Trim().ToUpper(),
                OrderType = orderType
            };
            NotifyStateChanged();
        }
    }

    public void ClearSession()
    {
        CurrentSession = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
