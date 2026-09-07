using System.Text.Json;
using Microsoft.JSInterop;
using PosSelfOrdering.Client.DTOs.Admin;
using PosSelfOrdering.Client.Store;

namespace PosSelfOrdering.Client.Services;

public sealed class BroadcastChannelService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly SharedPosDataStore _store;
    private DotNetObjectReference<BroadcastChannelService>? _dotNetRef;
    private bool _isInitialized;

    public event Action? OnMenuUpdated;
    public event Action<string>? OnOrderCreated;
    public event Action<string, string>? OnOrderStatusUpdated;
    public event Action? OnTableUpdated;

    public BroadcastChannelService(IJSRuntime js, SharedPosDataStore store)
    {
        _js = js;
        _store = store;

        // Subscribe to store events to automatically broadcast to other tabs
        _store.OnMenuCatalogUpdated += () => _ = PostMessageAsync("MENU_UPDATED");
        _store.OnOrderCreated += (order) => _ = PostMessageAsync("ORDER_CREATED", order.OrderNumber);
        _store.OnOrderStatusUpdated += (orderNum, status) => _ = PostMessageAsync("ORDER_STATUS_UPDATED", new { OrderNumber = orderNum, Status = status });
        _store.OnTableStatusUpdated += () => _ = PostMessageAsync("TABLE_UPDATED");
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("posBroadcast.init", _dotNetRef);
            _isInitialized = true;
        }
        catch
        {
            // Prerendering fallback or browser without BroadcastChannel
        }
    }

    public async Task PostMessageAsync(string type, object? payload = null)
    {
        try
        {
            await _js.InvokeVoidAsync("posBroadcast.postMessage", type, payload);
        }
        catch
        {
            // Silently ignore if JS is not ready yet
        }
    }

    [JSInvokable]
    public void OnBroadcastMessageReceived(string jsonPayload)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonPayload);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "MENU_UPDATED":
                    OnMenuUpdated?.Invoke();
                    break;
                case "ORDER_CREATED":
                    var orderNum = root.TryGetProperty("payload", out var p1) ? p1.GetString() ?? "" : "";
                    OnOrderCreated?.Invoke(orderNum);
                    break;
                case "ORDER_STATUS_UPDATED":
                    if (root.TryGetProperty("payload", out var p2))
                    {
                        var oNum = p2.GetProperty("OrderNumber").GetString() ?? "";
                        var nStatus = p2.GetProperty("Status").GetString() ?? "";
                        OnOrderStatusUpdated?.Invoke(oNum, nStatus);
                    }
                    break;
                case "TABLE_UPDATED":
                    OnTableUpdated?.Invoke();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Broadcast parsing error: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        await ValueTask.CompletedTask;
    }
}
