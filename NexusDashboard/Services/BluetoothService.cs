using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace NEXUS.Services;

/// <summary>
/// Enumerates paired Bluetooth devices using WinRT APIs.
/// Battery level: tries Windows device property first, then GATT Battery Service.
/// Refreshes every 60 seconds.
/// </summary>
public sealed class BluetoothService : IDisposable
{
    // Windows internal property key for Bluetooth device battery level
    private const string BatteryLevelPropertyKey = "{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2";

    private readonly System.Timers.Timer _refreshTimer;
    private bool _disposed;

    /// <summary>Current snapshot of paired Bluetooth devices.</summary>
    public ObservableCollection<BluetoothDeviceInfo> Devices { get; } = new();

    /// <summary>Fires after each refresh cycle.</summary>
    public event EventHandler? DevicesUpdated;

    public BluetoothService()
    {
        _ = RefreshAsync();

        // Refresh every 60 seconds (battery levels don't change fast enough for shorter intervals)
        _refreshTimer = new System.Timers.Timer(60_000);
        _refreshTimer.Elapsed += async (_, _) => await RefreshAsync();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var results = new List<BluetoothDeviceInfo>();

            // ── Classic Bluetooth paired devices ──
            await EnumerateClassicDevicesAsync(results);

            // ── BLE paired devices ──
            await EnumerateBleDevicesAsync(results);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BT] Scan error: {ex.Message}");
        }

        DevicesUpdated?.Invoke(this, EventArgs.Empty);
    }

    private async Task EnumerateClassicDevicesAsync(List<BluetoothDeviceInfo> results)
    {
        var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);

        // Request the battery property alongside the standard device info
        string[] requestedProperties = [BatteryLevelPropertyKey];
        var devices = await DeviceInformation.FindAllAsync(selector, requestedProperties);

        foreach (var devInfo in devices)
        {
            try
            {
                using var btDevice = await BluetoothDevice.FromIdAsync(devInfo.Id);
                if (btDevice is null) continue;

                var isConnected = btDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                var name = btDevice.Name ?? "Unknown";

                // Method 2 (primary): Windows battery property
                int? battery = TryReadBatteryFromProperty(devInfo);

                results.Add(new BluetoothDeviceInfo
                {
                    Name = name,
                    IsConnected = isConnected,
                    BatteryLevel = battery
                });

                Debug.WriteLine($"[BT Classic] {name} | Connected={isConnected} | Battery={battery?.ToString() ?? "N/A"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BT Classic] Error: {devInfo.Name}: {ex.Message}");
            }
        }
    }

    private async Task EnumerateBleDevicesAsync(List<BluetoothDeviceInfo> results)
    {
        var selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);

        string[] requestedProperties = [BatteryLevelPropertyKey];
        var devices = await DeviceInformation.FindAllAsync(selector, requestedProperties);

        foreach (var devInfo in devices)
        {
            try
            {
                using var bleDevice = await BluetoothLEDevice.FromIdAsync(devInfo.Id);
                if (bleDevice is null) continue;

                var isConnected = bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                var name = bleDevice.Name ?? "Unknown";

                // Skip duplicates (some devices appear in both classic + LE)
                if (results.Exists(r => r.Name == name)) continue;

                // Method 2 (primary): Windows battery property
                int? battery = TryReadBatteryFromProperty(devInfo);

                // Method 1 (fallback): GATT Battery Service 0x180F
                if (battery is null && isConnected)
                {
                    battery = await TryReadBatteryFromGattAsync(bleDevice);
                }

                results.Add(new BluetoothDeviceInfo
                {
                    Name = name,
                    IsConnected = isConnected,
                    BatteryLevel = battery
                });

                Debug.WriteLine($"[BT LE] {name} | Connected={isConnected} | Battery={battery?.ToString() ?? "N/A"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BT LE] Error: {devInfo.Name}: {ex.Message}");
            }
        }

        // Replace collection contents
        Devices.Clear();
        foreach (var d in results)
        {
            Devices.Add(d);
        }
    }

    // ─── Battery reading methods ─────────────────────────────────────

    /// <summary>
    /// Method 2 (primary): Reads battery from the Windows device property
    /// "{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2".
    /// Works for AirPods, DualSense, most mice and keyboards without GATT.
    /// </summary>
    private static int? TryReadBatteryFromProperty(DeviceInformation devInfo)
    {
        try
        {
            if (devInfo.Properties.TryGetValue(BatteryLevelPropertyKey, out var value) &&
                value is byte b)
            {
                return b;
            }

            // Some drivers report as int or uint
            if (value is int i && i is >= 0 and <= 100) return i;
            if (value is uint u && u <= 100) return (int)u;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BT] Property battery read error: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Method 1 (fallback): Reads battery from GATT Battery Service (UUID 0x180F).
    /// Only works on connected BLE devices that expose the standard battery characteristic.
    /// </summary>
    private static async Task<int?> TryReadBatteryFromGattAsync(BluetoothLEDevice device)
    {
        try
        {
            var gattResult = await device.GetGattServicesForUuidAsync(
                Windows.Devices.Bluetooth.GenericAttributeProfile.GattServiceUuids.Battery,
                BluetoothCacheMode.Cached);

            if (gattResult.Status != Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus.Success ||
                gattResult.Services.Count == 0)
                return null;

            var service = gattResult.Services[0];
            var charResult = await service.GetCharacteristicsForUuidAsync(
                Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicUuids.BatteryLevel,
                BluetoothCacheMode.Cached);

            if (charResult.Status != Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus.Success ||
                charResult.Characteristics.Count == 0)
                return null;

            var readResult = await charResult.Characteristics[0].ReadValueAsync(BluetoothCacheMode.Cached);

            if (readResult.Status != Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus.Success)
                return null;

            var reader = Windows.Storage.Streams.DataReader.FromBuffer(readResult.Value);
            return reader.ReadByte();
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _refreshTimer.Dispose();
    }
}

/// <summary>Represents a paired Bluetooth device.</summary>
public class BluetoothDeviceInfo
{
    public string Name { get; init; } = "";
    public bool IsConnected { get; init; }
    /// <summary>Battery level 0-100, or null if unavailable.</summary>
    public int? BatteryLevel { get; init; }
}
