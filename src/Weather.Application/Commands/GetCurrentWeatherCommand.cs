using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Weather.Domain.Entities;
using Weather.Infrastructure.Abstractions.DataAccess;
using Weather.Infrastructure.Abstractions.WeatherServiceClient;

namespace Weather.Application.Commands;

public record GetCurrentWeatherCommand(GetCurrentWeatherRequestDto Dto) : IRequest<GetCurrentWeatherResponseDto>;

public record GetCurrentWeatherRequestDto(string City);

public record GetCurrentWeatherResponseDto(float Temperature);

public class GetCurrentWeatherRequestDtoValidator : AbstractValidator<GetCurrentWeatherRequestDto>
{
    public GetCurrentWeatherRequestDtoValidator()
    {
        RuleFor(x => x.City).NotEmpty();
    }
}

public class GetCurrentWeatherCommandHandler : IRequestHandler<GetCurrentWeatherCommand, GetCurrentWeatherResponseDto>
{
    private readonly IWeatherServiceClient _weatherServiceClient;
    private readonly IAppDbContext _dbContext;

    public GetCurrentWeatherCommandHandler(
        IWeatherServiceClient weatherServiceClient,
        IAppDbContext dbContext)
    {
        _weatherServiceClient = weatherServiceClient;
        _dbContext = dbContext;
    }
    
    public async Task<GetCurrentWeatherResponseDto> Handle(GetCurrentWeatherCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var measurement = await _dbContext.Measurements
            .FirstOrDefaultAsync(x => x.City == request.Dto.City && x.Date == today, cancellationToken);
        if (measurement != null)
            return new GetCurrentWeatherResponseDto(measurement.Temperature);
        
        var temperature = await _weatherServiceClient.GetCurrentWeatherAsync(request.Dto.City, cancellationToken);
        _dbContext.Measurements.Add(new Measurement
        {
            City = request.Dto.City,
            Date = today,
            Temperature = temperature
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new GetCurrentWeatherResponseDto(temperature);
    }
}