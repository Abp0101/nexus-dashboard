using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Timers;

namespace NEXUS.Services;

/// <summary>
/// Fetches current weather from the Open-Meteo free API (no API key required).
/// Location: Chartham, Canterbury, UK (51.27°N, 1.08°E).
/// Refreshes every 10 minutes.
/// </summary>
public sealed class WeatherService : IDisposable
{
    private const string ApiUrl =
        "https://api.open-meteo.com/v1/forecast?latitude=51.27&longitude=1.08" +
        "&current_weather=true&hourly=relativehumidity_2m,windspeed_10m,visibility";

    // Static HttpClient — reused across the app lifetime to avoid socket exhaustion
    private static readonly HttpClient s_http = new();
    private readonly System.Timers.Timer _refreshTimer;
    private bool _disposed;

    public float Temperature { get; private set; }
    public float WindSpeed { get; private set; }
    public int WeatherCode { get; private set; }
    public int Humidity { get; private set; }
    public string Description { get; private set; } = "Loading...";
    public string Emoji { get; private set; } = "⏳";

    /// <summary>Fires after each successful (or failed) refresh.</summary>
    public event EventHandler? WeatherUpdated;

    public WeatherService()
    {
        // Initial fetch
        _ = RefreshAsync();

        // Refresh every 10 minutes
        _refreshTimer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
        _refreshTimer.Elapsed += async (_, _) => await RefreshAsync();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var json = await s_http.GetStringAsync(ApiUrl);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // ── current_weather ──
            if (root.TryGetProperty("current_weather", out var cw))
            {
                Temperature = cw.GetProperty("temperature").GetSingle();
                WindSpeed = cw.GetProperty("windspeed").GetSingle();
                WeatherCode = cw.GetProperty("weathercode").GetInt32();
                (Description, Emoji) = MapWeatherCode(WeatherCode);
            }

            // ── humidity from hourly[0] ──
            if (root.TryGetProperty("hourly", out var hourly) &&
                hourly.TryGetProperty("relativehumidity_2m", out var humArr) &&
                humArr.GetArrayLength() > 0)
            {
                Humidity = humArr[0].GetInt32();
            }

            Debug.WriteLine($"[Weather] {Temperature}°C, {Description} {Emoji}, Wind {WindSpeed} km/h, Humidity {Humidity}%");
        }
        catch (Exception ex)
        {
            Description = "Offline";
            Emoji = "❌";
            Debug.WriteLine($"[Weather] Error: {ex.Message}");
        }

        WeatherUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Maps WMO weather codes to human-readable descriptions + emoji.
    /// https://open-meteo.com/en/docs → "WMO Weather interpretation codes"
    /// </summary>
    private static (string description, string emoji) MapWeatherCode(int code) => code switch
    {
        0 => ("Clear sky", "☀️"),
        1 => ("Mainly clear", "🌤️"),
        2 => ("Partly cloudy", "⛅"),
        3 => ("Overcast", "☁️"),
        45 or 48 => ("Foggy", "🌫️"),
        51 or 53 or 55 => ("Drizzle", "🌦️"),
        56 or 57 => ("Freezing drizzle", "🌧️"),
        61 or 63 or 65 => ("Rain", "🌧️"),
        66 or 67 => ("Freezing rain", "🌧️"),
        71 or 73 or 75 => ("Snowfall", "🌨️"),
        77 => ("Snow grains", "🌨️"),
        80 or 81 or 82 => ("Rain showers", "🌦️"),
        85 or 86 => ("Snow showers", "🌨️"),
        95 => ("Thunderstorm", "⛈️"),
        96 or 99 => ("Thunderstorm + hail", "⛈️"),
        _ => ($"Code {code}", "🌡️"),
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        // s_http is static — not disposed here
    }
}
