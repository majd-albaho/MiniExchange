1. Create a template folder
mkdir ExchangeMicroserviceTemplate
cd ExchangeMicroserviceTemplate

Inside it, create a sample service using a placeholder name:

mkdir ExchangeService
cd ExchangeService

dotnet new sln -n ExchangeService

dotnet new webapi -n ExchangeService.Api
dotnet new classlib -n ExchangeService.Application
dotnet new classlib -n ExchangeService.Domain
dotnet new classlib -n ExchangeService.Infrastructure
dotnet new xunit -n ExchangeService.Tests

Add projects:

dotnet sln add ExchangeService.Api
dotnet sln add ExchangeService.Application
dotnet sln add ExchangeService.Domain
dotnet sln add ExchangeService.Infrastructure
dotnet sln add ExchangeService.Tests

Add references:

dotnet add ExchangeService.Api reference ExchangeService.Application
dotnet add ExchangeService.Api reference ExchangeService.Infrastructure

dotnet add ExchangeService.Application reference ExchangeService.Domain

dotnet add ExchangeService.Infrastructure reference ExchangeService.Application
dotnet add ExchangeService.Infrastructure reference ExchangeService.Domain

dotnet add ExchangeService.Tests reference ExchangeService.Application
dotnet add ExchangeService.Tests reference ExchangeService.Domain