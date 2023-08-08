#nullable enable
using System.Net.Http.Json;
using AuthenticationSamples.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiXamlClient.Auth;

namespace MauiXamlClient.Pages.ForecastToday;

public partial class ForecastTodayViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private WeatherForecast? _forecastToday;

    public ForecastTodayViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [RelayCommand]
    private async Task LoadData()
    {
        var forecast =
            await _httpClient.GetFromJsonAsync<WeatherForecast>($"{ApiConfig.Instance.ApiUrl}/api/weatherforecast");
        ForecastToday = forecast;
    }
}