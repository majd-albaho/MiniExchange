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
COPY ["TradingPairService/TradingPairService.Api/TradingPairService.Api.csproj", "TradingPairService/TradingPairService.Api/"]
COPY ["TradingPairService/TradingPairService.Domain/TradingPairService.Domain.csproj", "TradingPairService/TradingPairService.Domain/"]
COPY ["TradingPairService/TradingPairService.Application/TradingPairService.Application.csproj", "TradingPairService/TradingPairService.Application/"]
COPY ["TradingPairService/TradingPairService.Infrastructure/TradingPairService.Infrastructure.csproj", "TradingPairService/TradingPairService.Infrastructure/"]
COPY ["TradingPairService/TradingPairService.SqlMigration/TradingPairService.SqlMigration.csproj", "TradingPairService/TradingPairService.SqlMigration/"]
COPY ["References/", "References/"]
RUN dotnet restore "./TradingPairService/TradingPairService.Api/TradingPairService.Api.csproj"
COPY TradingPairService/ TradingPairService/
WORKDIR "/src/TradingPairService/TradingPairService.Api"
RUN dotnet build "./TradingPairService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/TradingPairService/TradingPairService.Api"
RUN dotnet publish "./TradingPairService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingPairService.Api.dll"]