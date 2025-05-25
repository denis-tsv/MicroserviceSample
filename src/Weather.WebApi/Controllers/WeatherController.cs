using MediatR;
using Microsoft.AspNetCore.Mvc;
using Weather.Application.Commands;

namespace Weather.WebApi.Controllers;

[ApiController]
[Route("weather")]
public class WeatherController : ControllerBase
{
    private readonly ISender _sender;

    public WeatherController(ISender sender) => _sender = sender;

    [HttpGet("current")]
    public async Task<GetCurrentWeatherResponseDto> GetCurrent(
        [FromQuery] GetCurrentWeatherRequestDto dto,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetCurrentWeatherCommand(dto), cancellationToken);
    }
    
    [HttpGet("historical")]
    public async Task<GetHistoricalWeatherResponseDto> GetHistory(
        [FromQuery] GetHistoricalWeatherRequestDto dto,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetHistoricalWeatherCommand(dto), cancellationToken);
    }
    
    [HttpGet("forecast")]
    public async Task<GetForecastWeatherResponseDto> GetForecast(
        [FromQuery] GetForecastWeatherRequestDto dto,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetForecastWeatherCommand(dto), cancellationToken);
    }
}