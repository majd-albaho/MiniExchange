# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TradingService/MatchingEngineService/MatchingEngineService.Api/MatchingEngineService.Api.csproj", "TradingService/MatchingEngineService/MatchingEngineService.Api/"]
COPY ["TradingService/MatchingEngineService/MatchingEngineService.Domain/MatchingEngineService.Domain.csproj", "TradingService/MatchingEngineService/MatchingEngineService.Domain/"]
COPY ["TradingService/MatchingEngineService/MatchingEngineService.Application/MatchingEngineService.Application.csproj", "TradingService/MatchingEngineService/MatchingEngineService.Application/"]
COPY ["TradingService/MatchingEngineService/MatchingEngineService.Infrastructure/MatchingEngineService.Infrastructure.csproj", "TradingService/MatchingEngineService/MatchingEngineService.Infrastructure/"]
COPY ["References/", "References/"]
RUN dotnet restore "./TradingService/MatchingEngineService/MatchingEngineService.Api/MatchingEngineService.Api.csproj"
COPY TradingService/MatchingEngineService/MatchingEngineService.Api/ TradingService/MatchingEngineService/MatchingEngineService.Api/
COPY TradingService/MatchingEngineService/MatchingEngineService.Domain/ TradingService/MatchingEngineService/MatchingEngineService.Domain/
COPY TradingService/MatchingEngineService/MatchingEngineService.Application/ TradingService/MatchingEngineService/MatchingEngineService.Application/
COPY TradingService/MatchingEngineService/MatchingEngineService.Infrastructure/ TradingService/MatchingEngineService/MatchingEngineService.Infrastructure/
WORKDIR "/src/TradingService/MatchingEngineService/MatchingEngineService.Api"
RUN dotnet build "./MatchingEngineService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/TradingService/MatchingEngineService/MatchingEngineService.Api"
RUN dotnet publish "./MatchingEngineService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchingEngineService.Api.dll"]