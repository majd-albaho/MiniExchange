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
COPY ["WalletService/WalletService.Api/WalletService.Api.csproj", "WalletService/WalletService.Api/"]
COPY ["WalletService/WalletService.Domain/WalletService.Domain.csproj", "WalletService/WalletService.Domain/"]
COPY ["WalletService/WalletService.Application/WalletService.Application.csproj", "WalletService/WalletService.Application/"]
COPY ["WalletService/WalletService.Infrastructure/WalletService.Infrastructure.csproj", "WalletService/WalletService.Infrastructure/"]
COPY ["WalletService/WalletService.SqlMigration/WalletService.SqlMigration.csproj", "WalletService/WalletService.SqlMigration/"]
COPY ["References/", "References/"]
RUN dotnet restore "./WalletService/WalletService.Api/WalletService.Api.csproj"
COPY WalletService/ WalletService/
WORKDIR "/src/WalletService/WalletService.Api"
RUN dotnet build "./WalletService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/WalletService/WalletService.Api"
RUN dotnet publish "./WalletService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletService.Api.dll"]