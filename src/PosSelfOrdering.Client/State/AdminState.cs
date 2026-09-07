using PosSelfOrdering.Client.DTOs.Admin;

namespace PosSelfOrdering.Client.State;

public sealed class AdminState
{
    public AdminUserDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null && CurrentUser.ExpiresAtUtc > DateTime.UtcNow;
    public bool IsManager => CurrentUser?.Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true;
    public int ActiveOrdersCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? OnStateChanged;

    public void SetUser(AdminUserDto user)
    {
        CurrentUser = user;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        CurrentUser = null;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public void SetActiveOrdersCount(int count)
    {
        if (ActiveOrdersCount != count)
        {
            ActiveOrdersCount = count;
            NotifyStateChanged();
        }
    }

    public void SetError(string message)
    {
        ErrorMessage = message;
        NotifyStateChanged();
    }

    public void ClearError()
    {
        if (ErrorMessage != null)
        {
            ErrorMessage = null;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
