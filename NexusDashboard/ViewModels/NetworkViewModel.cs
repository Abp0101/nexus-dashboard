using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Exposes formatted network speed and latency for UI binding.
/// Only updates observable properties when values actually change.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class NetworkViewModel : ObservableObject, IDisposable
{
    private readonly NetworkService _net;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private string _downloadSpeedText = "—";

    [ObservableProperty]
    private string _uploadSpeedText = "—";

    [ObservableProperty]
    private string _latencyText = "—";

    [ObservableProperty]
    private bool _hasConnection;

    public NetworkViewModel(NetworkService networkService)
    {
        _net = networkService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _net.NetworkUpdated += OnNetworkUpdated;
    }

    private void OnNetworkUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(() =>
        {
            HasConnection = _net.HasConnection;

            if (!_net.HasConnection)
            {
                DownloadSpeedText = "No connection";
                UploadSpeedText = "No connection";
                LatencyText = "—";
                return;
            }

            var newDown = $"{_net.DownloadSpeedMBps:F2} MB/s";
            var newUp = $"{_net.UploadSpeedMBps:F2} MB/s";
            var newLat = _net.LatencyMs >= 0 ? $"{_net.LatencyMs} ms" : "Timeout";

            // Only set when changed to avoid unnecessary UI invalidation
            if (DownloadSpeedText != newDown) DownloadSpeedText = newDown;
            if (UploadSpeedText != newUp) UploadSpeedText = newUp;
            if (LatencyText != newLat) LatencyText = newLat;
        });
    }

    public void Dispose()
    {
        _net.NetworkUpdated -= OnNetworkUpdated;
    }
}
#pragma warning restore MVVMTK0045
