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
COPY ["TradingService/TradingService.Api/TradingService.Api.csproj", "TradingService/TradingService.Api/"]
COPY ["TradingService/TradingService.Domain/TradingService.Domain.csproj", "TradingService/TradingService.Domain/"]
COPY ["TradingService/TradingService.Application/TradingService.Application.csproj", "TradingService/TradingService.Application/"]
COPY ["TradingService/TradingService.Infrastructure/TradingService.Infrastructure.csproj", "TradingService/TradingService.Infrastructure/"]
COPY ["TradingService/TradingService.SqlMigration/TradingService.SqlMigration.csproj", "TradingService/TradingService.SqlMigration/"]
COPY ["References/", "References/"]
RUN dotnet restore "./TradingService/TradingService.Api/TradingService.Api.csproj"
COPY TradingService/TradingService.Api/ TradingService/TradingService.Api/
COPY TradingService/TradingService.Domain/ TradingService/TradingService.Domain/
COPY TradingService/TradingService.Application/ TradingService/TradingService.Application/
COPY TradingService/TradingService.Infrastructure/ TradingService/TradingService.Infrastructure/
COPY TradingService/TradingService.SqlMigration/ TradingService/TradingService.SqlMigration/
WORKDIR "/src/TradingService/TradingService.Api"
RUN dotnet build "./TradingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/TradingService/TradingService.Api"
RUN dotnet publish "./TradingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingService.Api.dll"]