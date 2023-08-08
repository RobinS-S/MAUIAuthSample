#nullable enable
using System.Net;
using System.Net.Http.Json;
using AuthenticationSamples.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiXamlClient.Auth;
using MauiXamlClient.Auth.Services;

namespace MauiXamlClient.Pages.ForecastOverview;

public partial class ForecastOverviewViewModel : ObservableObject
{
    private readonly AuthenticatedHttpClientService _httpClientService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private IEnumerable<WeatherForecast>? _forecastOverview;

    public ForecastOverviewViewModel(AuthenticatedHttpClientService httpClientService, AuthService authService)
    {
        _httpClientService = httpClientService;
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoadData()
    {
        var forecastsResponse =
            await _httpClientService.Client.GetAsync($"{ApiConfig.Instance.ApiUrl}/api/weatherforecast/all");

        // Handle Unauthorized (401) which may mean token expired or logged out
        if (forecastsResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            var handled = await _authService.HandleUnauthorizedHttpRequest();
            if (handled) // If the token has been refreshed, execute the API call again
            {
                await LoadData();
            }

            return;
        }

        ForecastOverview = await forecastsResponse.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
    }
}