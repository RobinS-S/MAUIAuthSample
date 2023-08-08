using AuthenticationSamples.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // This attribute can be set on the entire controller or per action and requires authentication, to require role use [Authorize(Roles = Roles.AdminRoleName)]
    [HttpGet("all")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => GenerateRandomForecast(DateTime.Now.AddDays(index)))
            .ToArray();
    }

    // The AllowAnonymous attribute can be set on controller actions that inherit [Authorize], like an exception. Because it isn't set on the controller, we leave it commented out.
    // [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    public WeatherForecast GetToday()
    {
        return GenerateRandomForecast(DateTime.Now);
    }

    private WeatherForecast GenerateRandomForecast(DateTime date)
    {
        var temp = Random.Shared.Next(-20, 55);

        var summary = temp switch
        {
            < -10 => Summaries[0],
            < 0 => Summaries[1],
            < 10 => Summaries[2],
            < 20 => Summaries[3],
            < 30 => Summaries[4],
            < 35 => Summaries[5],
            < 40 => Summaries[6],
            < 45 => Summaries[7],
            < 50 => Summaries[8],
            _ => Summaries[9]
        };

        return new WeatherForecast
        {
            Date = DateOnly.FromDateTime(date),
            TemperatureC = temp,
            Summary = summary
        };
    }
}