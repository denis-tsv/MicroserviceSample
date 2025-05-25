FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Weather.WebApi/Weather.WebApi.csproj", "src/Weather.WebApi/"]
COPY ["src/Weather.Application/Weather.Application.csproj", "src/Weather.Application/"]
COPY ["src/Weather.Infrastructure.Abstractions/Weather.Infrastructure.Abstractions.csproj", "src/Weather.Infrastructure.Abstractions/"]
COPY ["src/Weather.Domain/Weather.Domain.csproj", "src/Weather.Domain/"]
COPY ["src/Weather.Infrastructure.WeatherServiceClient/Weather.Infrastructure.WeatherServiceClient.csproj", "src/Weather.Infrastructure.WeatherServiceClient/"]
COPY ["src/Weather.Infrastructure.DataAccess/Weather.Infrastructure.DataAccess.csproj", "src/Weather.Infrastructure.DataAccess/"]
RUN dotnet restore "src/Weather.WebApi/Weather.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Weather.WebApi"
RUN dotnet build "./Weather.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Weather.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weather.WebApi.dll"]
