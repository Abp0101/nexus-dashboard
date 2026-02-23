using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Consumes WeatherService and exposes formatted weather data for UI binding.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class WeatherViewModel : ObservableObject, IDisposable
{
    private readonly WeatherService _weather;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private string _temperature = "Loading...";

    [ObservableProperty]
    private string _description = "⏳ Loading...";

    [ObservableProperty]
    private string _windSpeed = "—";

    [ObservableProperty]
    private string _humidity = "—";

    public WeatherViewModel(WeatherService weatherService)
    {
        _weather = weatherService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _weather.WeatherUpdated += OnWeatherUpdated;
    }

    private void OnWeatherUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(() =>
        {
            Temperature = $"{_weather.Temperature:F1} °C";
            Description = $"{_weather.Emoji} {_weather.Description}";
            WindSpeed = $"{_weather.WindSpeed:F0} km/h";
            Humidity = $"{_weather.Humidity} %";
        });
    }

    public void Dispose()
    {
        _weather.WeatherUpdated -= OnWeatherUpdated;
    }
}
#pragma warning restore MVVMTK0045
