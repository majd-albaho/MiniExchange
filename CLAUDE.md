# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Mini Exchange is a learning project that simulates a crypto exchange. Its purpose is to practice microservices architecture, Domain-Driven Design (DDD) layering, event-driven communication, and low-latency service design in .NET, with an Angular SPA as the single UI for every backend service.

## Services and ports

| Service | Purpose | Local port (from `Commands/`) | DB |
|---|---|---|---|
| AuthService | Registration, login, JWT issuance, refresh tokens | 5003 | `MiniExchangeAuth` / `AuthDb` |
| WalletService | User wallets, on-chain addresses, balances, fund locking, ETH send/receive via Nethereum+Alchemy | 5002 | `WalletDb` |
| TradingPairService | Tradeable pair catalog (symbol, precision, min qty/value) | — | `MiniExchangeTradingPair` |
| TradingService | Order intake/lifecycle (create/cancel), persists `Order` | — | `MiniExchangeTrading` |
| TradingService/MatchingEngineService | Order-matching engine — see performance rules below. Currently a scaffold (gRPC `Ping` only, no matching logic yet) | — | — |
| MarketDataService | Price cache + SignalR hub (`/hubs/market-data`) for live ticker/order-book push | — | — |
| AzureFunctions/BlockchainScanner | Timer-triggered (every 30s) Ethereum deposit scanner | — | — |
| FrontEndApp | Angular 21 SPA, the UI for all of the above | 4200 | — |

RabbitMQ runs as its own container (`rabbitmq:4-management`, ports 5672/15672) and is the async backbone (e.g. `AuthService` publishes `wallet.user.registered` → `WalletService.UserRegisteredConsumer` provisions the wallet).

Each service is fully independent: own solution (`.slnx`), own DbContext/migrations project, own Dockerfile, no shared database.

## Solution / project layout (per service)

Every backend service follows the same generated template (see `*/Readme.txt` and `*/.template.config/template.json`, authored as a reusable `dotnet new` template):

```
<Service>/
  <Service>.Api/            ASP.NET Core host: Controllers, gRPC endpoints (Grpc/, Protos/), Program.cs, Dockerfile
  <Service>.Application/     Use-case/orchestration layer: Services, Dto, Interfaces (Repositories + Services), DependencyInjection.cs
  <Service>.Domain/          Entities, Enums — no framework dependencies
  <Service>.Infrastructure/  EF Core DbContext, Repositories, external clients (gRPC clients, blockchain clients), DependencyInjection.cs
  <Service>.SqlMigration/    Separate migrations project + DbContextFactory
  <Service>.Tests/           xUnit
```

Dependency direction: `Api → Application, Infrastructure`; `Application → Domain`; `Infrastructure → Application, Domain`. Domain has no outward dependencies. When adding a new microservice, scaffold it with this same 6-project layout rather than inventing a new structure.

All entities inherit `SharedLibrary.Entities.EntityBase<T>` (`Shared/SharedLibrary/Entities/EntityBase.cs`), which supplies `Id`, `CreatedDate`/`CreatedBy`, `ModifiedDate`/`ModifiedBy`, `DeletedDate`/`DeletedBy`. Deletes are soft: repositories filter on `DeletedDate == default` rather than removing rows (see `OrderRepository`).

## Coding style (applies to every service and the frontend)

- **Strict DDD layering + SOLID.** Domain stays framework-free; Application depends only on Domain + its own interfaces; Infrastructure implements those interfaces. A class should have one reason to change — if a service class is doing repository work, validation, mapping, and external calls all at once, split it into collaborators instead of growing one file.
- **Don't put everything in one file/class.** Prefer several small, single-purpose classes (a validator, a mapper, a repository, a service) over one large file that does it all, even across layers that already separate Api/Application/Domain/Infrastructure.
- **Optimize for a human reading it, not for line count.** A longer, clearly-named, straightforwardly-structured implementation beats a short, clever one that's hard to follow. Prefer explicit code over dense one-liners.
- **Minimal comments.** Don't narrate what the code already says. Add a comment only when something isn't obvious from the code itself (a non-obvious business rule, a workaround, a reason for an unusual choice) — otherwise leave it uncommented.

## Shared library (`Shared/SharedLibrary`)

- `Entities/EntityBase.cs` — audit-field base class described above.
- `EventDriven/RabbitMqMessageBroker.cs` — `IMessageBroker.PublishAsync<T>(queueName, message)`, durable queue, JSON body, one connection/channel per publish.
- `EventDriven/Models/UserRegisteredEvent.cs` — shape of the one cross-service event that exists today.

New cross-service events should go in this project as `SharedLibrary.EventDriven.Models.*` so both publisher and consumer share the contract.

## Communication rules observed

- **Synchronous, same-request** cross-service calls → gRPC (each `*.Api/Protos/*.proto` + `Grpc/` folder; e.g. `TradingPairService.Infrastructure/GrpcClients`).
- **Asynchronous, fire-and-forget** cross-service notifications → RabbitMQ via `IMessageBroker` / a `BackgroundService` consumer (see `WalletService.Api.BackgroundServices.UserRegisteredConsumer`).
- **Real-time push to the browser** → SignalR (`MarketDataService.Infrastructure.Hubs.MarketDataHub`).
- REST controllers remain the public-facing API the Angular app talks to; gRPC is internal-only.

## Cross-cutting conventions (apply to every new service)

- Serilog configured in `Program.cs` via `builder.Host.UseSerilog(...)`, reading the `Serilog` section from `appsettings.json` (Compact JSON console sink, enriched with machine/thread/span). Set `Serilog:Properties:ServiceName` / `Application` to the service's own name — check this value whenever `appsettings.json` is copied from another service.
- A correlation-ID middleware (reads/sets `X-Correlation-ID`, pushes `CorrelationId`/`TraceId`/`SpanId`/`RequestPath`/`RequestMethod` into the Serilog `LogContext`) runs before `UseSerilogRequestLogging`. All API projects now follow this — keep it that way for any new service.
- EF Core migrations are applied automatically on startup (`db.Database.MigrateAsync()` inside a `using var scope` right after `builder.Build()`), not via a separate migration step.
- Controllers/services expose both REST (`AddControllers`) and gRPC (`AddGrpc` + `MapGrpcService<...>`) from the same `Api` project.
- JWT auth (issuer/audience/symmetric key validation) is configured only in `AuthService` today; other services don't yet validate the token themselves.

## Matching engine performance rules

`MatchingEngineService` is the centerpiece for practicing low-latency design. When it's implemented, follow these rules:

- **In-memory order book, one per trading pair.** The book (bids/asks, price-time priority) lives in memory; it is not read from or written to the database on the hot path.
- **No synchronous DB or network I/O in the match loop.** Persistence of trades/order-status changes happens asynchronously (write-behind, e.g. via a queue or background writer) after a match decision is made, never blocking the next match.
- **Single-writer per trading pair.** Each pair's book is only ever mutated by one logical thread/actor at a time (e.g. a dedicated processing loop or partitioned actor per pair) so it does not need locking on every operation; avoid taking locks on the matching hot path in general.
- **Use price-time priority** (better price first, then earlier timestamp) as the matching algorithm unless a specific task says otherwise.
- **Measure before optimizing further.** Add lightweight timing/benchmarks around the match loop before reaching for more advanced tricks (custom data structures, object pooling, etc.) — don't add complexity the profiling doesn't justify.

## Secrets policy

Some secrets are already committed in this repo (JWT signing key in `AuthService/appsettings.json`, SQL `sa` password in `Commands/*.txt`, an Alchemy API URL/key in `NethereumWalletBlockchainClient.cs`), and `UserWalletAddress.PrivateKey` is stored unencrypted in the wallet DB. These are left as-is since this is a localhost-only learning sandbox — don't spend time scrubbing them.

**Going forward: no new secrets get committed.** Anything added from now on (API keys, connection strings with real credentials, signing keys, etc.) goes through configuration/environment variables or user-secrets, not hardcoded into source or checked-in `appsettings.json`.

## Frontend (`FrontEndApp`)

Angular 21, standalone components (no NgModules), Angular Material, signals for local state, functional guards/interceptors, lazy-loaded feature routes. See `FrontEndApp/MINI_EXCHANGE_GUIDE.md` for the full page-by-page reference — the important structural points:

- `src/app/core/` — guards, interceptors, models, and one `*.service.ts` per backend concern (auth, wallet, trade, transaction, user, market, notification, theme).
- `src/app/features/<name>/` — one folder per route/page; each is lazy-loaded in `app.routes.ts`.
- `src/environments/environment*.ts` — `apiBase` maps one base URL per backend service (`auth`, `wallet`, `trade`, `transactions`, `market`, `user`); update these, not the services, when a real backend URL changes.
- **Every service method follows try (real HTTP call) / catch (return realistic dummy data)** so the UI works before a backend exists. When wiring a real endpoint, only delete the dummy `return` in the `catch` — keep the `console.error`, don't restructure the method.
- JWT is stored in `localStorage` and attached by `authInterceptor` (functional `HttpInterceptorFn`).

## Common commands

Backend (run from inside a service folder, e.g. `AuthService/`):
```bash
dotnet build <Service>.slnx
dotnet test <Service>.Tests
dotnet ef migrations add <Name> --project <Service>.SqlMigration --startup-project <Service>.Api
dotnet ef database update --project <Service>.SqlMigration --startup-project <Service>.Api
```

Docker (pattern from `Commands/*.txt`, build context is the repo root):
```bash
docker buildx build --platform linux/amd64 -f <Service>/<Service>.Api/Dockerfile -t <service>:dev .
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management
```

Frontend (`FrontEndApp/`):
```bash
npm install --legacy-peer-deps
npm start        # ng serve, http://localhost:4200
npm run build    # ng build
npm test         # ng test (Vitest)
```

## Known gaps (relevant when picking up work here)

- `MatchingEngineService` is an unimplemented scaffold — no order book, no matching algorithm yet, and `TradingService`'s `OrderService` doesn't call it. This is the centerpiece still to be built (see performance rules above).
- `MatchingEngineService.Application`/`.Infrastructure` have no `DependencyInjection.cs` yet, so `Program.cs` doesn't wire up an `AddApplication()`/`AddInfrastructure()` call the way other services do — add those when the engine's actual logic is built.
