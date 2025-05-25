using FluentValidation;
using MediatR;
using Weather.Application.Services;

namespace Weather.Application.Commands;

public record GetHistoricalWeatherCommand(GetHistoricalWeatherRequestDto Dto) : IRequest<GetHistoricalWeatherResponseDto>;

public record GetHistoricalWeatherRequestDto(string City, DateOnly DateFrom, DateOnly DateTo);

public record GetHistoricalWeatherResponseDto(MeasurementDto[] Measurements);

public record MeasurementDto(DateOnly Date, float Temperature);

public class GetHistoricalWeatherRequestDtoValidator : AbstractValidator<GetHistoricalWeatherRequestDto>
{
    public GetHistoricalWeatherRequestDtoValidator()
    {
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.DateTo).Must(date => date <= DateOnly.FromDateTime(DateTime.Today));
        RuleFor(x => x.DateFrom).Must(date => date <= DateOnly.FromDateTime(DateTime.Today));
        RuleFor(x => x).Must(x => x.DateFrom <= x.DateTo);
        RuleFor(x => x).Must(x => x.DateTo.DayNumber - x.DateFrom.DayNumber < 60);
    }
}

public class GetHistoricalWeatherCommandHandler : IRequestHandler<GetHistoricalWeatherCommand, GetHistoricalWeatherResponseDto>
{
    private readonly IWeatherApplicationService _weatherApplicationService;

    public GetHistoricalWeatherCommandHandler(IWeatherApplicationService weatherApplicationService) => 
        _weatherApplicationService = weatherApplicationService;

    public async Task<GetHistoricalWeatherResponseDto> Handle(GetHistoricalWeatherCommand request, CancellationToken cancellationToken)
    {
        var measurements = await _weatherApplicationService.GetMeasurementsAsync(
            request.Dto.City, request.Dto.DateFrom, request.Dto.DateTo, cancellationToken);
        var measurementDtos = measurements.Select(x => new MeasurementDto(x.Date, x.Temperature)).ToArray();
        return new GetHistoricalWeatherResponseDto(measurementDtos);
    }
}