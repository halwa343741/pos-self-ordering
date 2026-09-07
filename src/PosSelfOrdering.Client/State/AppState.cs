namespace PosSelfOrdering.Client.State;

public sealed class AppState
{
    public bool IsOffline { get; private set; }
    public string? ActiveToastMessage { get; private set; }
    public string ActiveToastType { get; private set; } = "info";

    public event Action? OnChange;

    public void SetOffline(bool offline)
    {
        if (IsOffline != offline)
        {
            IsOffline = offline;
            NotifyStateChanged();
        }
    }

    public void ShowToast(string message, string type = "info")
    {
        ActiveToastMessage = message;
        ActiveToastType = type;
        NotifyStateChanged();
    }

    public void ClearToast()
    {
        ActiveToastMessage = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
