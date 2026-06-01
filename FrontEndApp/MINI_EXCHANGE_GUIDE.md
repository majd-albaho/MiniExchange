# MiniExchange — Angular 21 Crypto Exchange Platform

> A full-featured mini crypto-exchange SPA built with **Angular 21**, standalone components, signals, Angular Material, and a microservices-ready API layer. Containerized with Docker for Azure deployment.

---

## 📋 Table of Contents

1. [Tech Stack](#tech-stack)
2. [Project Structure](#project-structure)
3. [Features](#features)
4. [Getting Started](#getting-started)
5. [Environment Configuration](#environment-configuration)
6. [API Integration Guide](#api-integration-guide)
7. [Docker & Azure Deployment](#docker--azure-deployment)
8. [Page-by-Page Reference](#page-by-page-reference)
9. [How to Add a New Feature](#how-to-add-a-new-feature)
10. [How to Swap Dummy Data for Real APIs](#how-to-swap-dummy-data-for-real-apis)
11. [Architecture Decisions](#architecture-decisions)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | Angular 21 (Standalone Components) |
| State Management | Angular Signals |
| UI Components | Angular Material 19 |
| Charts | lightweight-charts (TradingView) |
| QR Codes | angularx-qrcode |
| HTTP | Angular HttpClient + functional interceptors |
| Auth | JWT (localStorage) |
| Styling | SCSS + CSS Custom Properties (dark/light theme) |
| Container | Docker + nginx |
| CI Target | Azure Container Apps / Azure App Service |

---

## Project Structure

```
mini-exchange/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── guards/
│   │   │   │   └── auth.guard.ts          # authGuard + guestGuard
│   │   │   ├── interceptors/
│   │   │   │   └── auth.interceptor.ts    # Attaches Bearer token to every request
│   │   │   ├── models/
│   │   │   │   ├── user.model.ts
│   │   │   │   ├── wallet.model.ts
│   │   │   │   ├── transaction.model.ts
│   │   │   │   └── trade.model.ts
│   │   │   └── services/
│   │   │       ├── auth.service.ts        # Login, register, token management
│   │   │       ├── wallet.service.ts      # Wallet overview, send, receive
│   │   │       ├── trade.service.ts       # Pairs, order book, candles, orders
│   │   │       ├── transaction.service.ts # Transaction history + filters
│   │   │       ├── user.service.ts        # Profile, password, 2FA, PIN, language
│   │   │       ├── market.service.ts      # Market tickers
│   │   │       ├── notification.service.ts# Toast notifications (signal-based)
│   │   │       └── theme.service.ts       # Dark/light theme toggle
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   │   ├── login/                 # Login page (email + password + 2FA)
│   │   │   │   └── signup/                # Multi-step signup (stepper)
│   │   │   ├── home/                      # Dashboard page
│   │   │   ├── wallet/
│   │   │   │   ├── wallet.component.ts    # Asset table + overview
│   │   │   │   ├── send-dialog/           # Send crypto modal
│   │   │   │   └── receive-dialog/        # Receive crypto modal (QR code)
│   │   │   ├── trade/
│   │   │   │   ├── trade.component.ts     # Main trade page
│   │   │   │   ├── trade-chart/           # Candlestick chart (lightweight-charts)
│   │   │   │   ├── order-book/            # Live order book with depth bars
│   │   │   │   └── spot-trading/          # Buy/Sell form (limit & market)
│   │   │   ├── transactions/              # Transaction history (tabs + filters)
│   │   │   └── settings/                  # Profile, security, language, account
│   │   ├── shared/
│   │   │   └── components/
│   │   │       ├── main-layout/           # App shell (sidebar + navbar + outlet)
│   │   │       ├── navbar/                # Top bar with ticker, theme, user menu
│   │   │       ├── sidebar/               # Collapsible navigation sidebar
│   │   │       ├── notifications-toast/   # Animated toast notifications
│   │   │       ├── page-header/           # Reusable page title component
│   │   │       ├── stat-card/             # Metric card (value + % change)
│   │   │       └── loading-spinner/       # Centered spinner with overlay option
│   │   ├── app.routes.ts                  # Lazy-loaded routes + guards
│   │   ├── app.config.ts                  # App bootstrap providers
│   │   └── app.ts                         # Root component (applies theme on init)
│   ├── environments/
│   │   ├── environment.ts                 # Production API base URLs
│   │   └── environment.development.ts     # Development API base URLs
│   ├── styles.scss                        # Global styles + CSS vars
│   └── index.html
├── Dockerfile                             # Multi-stage build
├── nginx.conf                             # SPA routing + gzip + security headers
├── docker-compose.yml                     # Frontend + backend service stubs
└── .dockerignore
```

---

## Features

### ✅ Implemented Pages

| Page | Route | Description |
|---|---|---|
| Login | `/auth/login` | Email + password + optional 2FA code |
| Sign Up | `/auth/signup` | 3-step stepper: account info → password → confirm |
| Home | `/home` | Portfolio stats, quick actions, asset breakdown, live tickers, recent transactions |
| Wallet | `/wallet` | Asset table with prices, 24h change, send/receive modals |
| Send | (dialog) | Network selection, recipient address, amount, fee preview, PIN confirmation |
| Receive | (dialog) | QR code, copy address, network tips, memo support |
| Trade | `/trade` | Pair selector, candlestick chart, order book with depth, spot buy/sell (limit+market) |
| Transactions | `/transactions` | Tabbed history, status/asset/date filters, CSV export, pagination |
| Settings | `/settings` | Profile edit, change password, 2FA setup (QR), transaction PIN, language (EN/FR), account/logout |

### ✅ Cross-cutting

- **Dark/Light theme** — CSS custom properties, toggled from navbar, persisted in localStorage
- **Toast notifications** — signal-based, animated (success/error/info/warning)
- **Auth guard** — protects all main routes, redirects to `/auth/login`
- **Guest guard** — redirects logged-in users away from auth pages
- **JWT interceptor** — attaches `Authorization: Bearer <token>` automatically
- **Dummy data fallback** — every `catch` block returns realistic dummy data so the app works without live APIs

---

## Getting Started

### Prerequisites

- Node.js 22+
- npm 9+
- Angular CLI 21 (`npm install -g @angular/cli@latest`)

### Run Locally

```bash
# Navigate to the project
cd D:\SourceCode\Github\MiniExchange\mini-exchange

# Install dependencies
npm install --legacy-peer-deps

# Start development server
npm start
# or
ng serve --open
```

App will be available at **http://localhost:4200**

> **Default demo login**: any email/password — the catch block returns a dummy user automatically.

---

## Environment Configuration

Edit `src/environments/environment.development.ts` for local development:

```typescript
export const environment = {
  production: false,
  apiBase: {
    auth:         'https://your-auth-service/api/v1',
    wallet:       'https://your-wallet-service/api/v1',
    trade:        'https://your-trade-service/api/v1',
    transactions: 'https://your-tx-service/api/v1',
    market:       'https://your-market-service/api/v1',
    user:         'https://your-user-service/api/v1',
  },
};
```

For production edit `src/environments/environment.ts`.

---

## API Integration Guide

Each service in `src/app/core/services/` follows this exact pattern:

```typescript
async methodName(params): Promise<ResponseType> {
  try {
    // 1. Real API call
    const result = await firstValueFrom(
      this.http.get<ResponseType>(`${this.baseUrl}/endpoint`, { params })
    );
    return result;
  } catch (err) {
    // 2. Log error (remove dummy data once API is ready)
    console.error('[ServiceName] methodName error:', err);

    // 3. Dummy data fallback — DELETE this block when your API is live
    return { /* dummy data */ };
  }
}
```

### When your ASP.NET Core microservices are ready:

1. Update the base URLs in `src/environments/environment.ts`
2. Find each service method that has dummy data in the `catch` block
3. Remove the dummy return statement (keep the `console.error`)
4. The try block already makes the real HTTP call — nothing else needs to change

### Expected API Endpoints

| Service | Endpoint | Method | Description |
|---|---|---|---|
| Auth | `POST /api/v1/auth/login` | POST | Login → returns `{ accessToken, refreshToken, expiresIn, user }` |
| Auth | `POST /api/v1/auth/register` | POST | Register new user |
| Auth | `POST /api/v1/auth/refresh` | POST | Refresh JWT token |
| Wallet | `GET /api/v1/wallet/{userId}` | GET | Full wallet overview |
| Wallet | `GET /api/v1/wallet/{userId}/receive` | GET | Deposit address for symbol+network |
| Wallet | `POST /api/v1/wallet/send` | POST | Submit withdrawal |
| Market | `GET /api/v1/tickers` | GET | All market tickers |
| Market | `GET /api/v1/pairs` | GET | All tradeable pairs |
| Market | `GET /api/v1/orderbook/{symbol}` | GET | Order book |
| Market | `GET /api/v1/candles/{symbol}?interval=1h` | GET | OHLCV candles |
| Trade | `POST /api/v1/orders` | POST | Place order |
| Trade | `GET /api/v1/orders/{userId}/open` | GET | Open orders |
| Trade | `DELETE /api/v1/orders/{orderId}` | DELETE | Cancel order |
| Transaction | `GET /api/v1/transactions/{userId}` | GET | Paginated transaction history |
| User | `PUT /api/v1/users/{userId}/profile` | PUT | Update profile |
| User | `PUT /api/v1/users/{userId}/password` | PUT | Change password |
| User | `POST /api/v1/users/{userId}/2fa/setup` | POST | Init 2FA → returns QR + secret |
| User | `POST /api/v1/users/{userId}/2fa/verify` | POST | Verify 2FA code |
| User | `POST /api/v1/users/{userId}/2fa/disable` | POST | Disable 2FA |
| User | `POST /api/v1/users/{userId}/pin` | POST | Set transaction PIN |
| User | `PUT /api/v1/users/{userId}/language` | PUT | Update preferred language |

---

## Docker & Azure Deployment

### Build & Run Locally with Docker

```bash
cd D:\SourceCode\Github\MiniExchange\mini-exchange

# Build image
docker build -t mini-exchange-frontend:latest .

# Run container
docker run -d -p 4200:80 --name mini-exchange mini-exchange-frontend:latest

# Or use docker-compose
docker-compose up -d
```

App will be available at **http://localhost:4200**

### Push to Azure Container Registry

```bash
# Login to ACR
az acr login --name <your-acr-name>

# Tag the image
docker tag mini-exchange-frontend:latest <your-acr-name>.azurecr.io/mini-exchange-frontend:latest

# Push
docker push <your-acr-name>.azurecr.io/mini-exchange-frontend:latest
```

### Deploy to Azure Container Apps

```bash
az containerapp create \
  --name mini-exchange-frontend \
  --resource-group <your-rg> \
  --environment <your-env> \
  --image <your-acr-name>.azurecr.io/mini-exchange-frontend:latest \
  --target-port 80 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3
```

### Deploy to Azure App Service

```bash
az webapp create \
  --resource-group <your-rg> \
  --plan <your-plan> \
  --name mini-exchange-app \
  --deployment-container-image-name <your-acr-name>.azurecr.io/mini-exchange-frontend:latest
```

---

## Page-by-Page Reference

### Home (`/home`)
- `HomeComponent` in `src/app/features/home/home.component.ts`
- Loads wallet overview, market tickers, and 5 recent transactions in parallel (`Promise.all`)
- To add a widget: add a new `div` section and inject the required service

### Wallet (`/wallet`)
- `WalletComponent` — asset table, search, send/receive buttons
- `SendDialogComponent` — opened via `MatDialog`, handles network selection + fee + PIN
- `ReceiveDialogComponent` — shows QR using `angularx-qrcode`, loads address from API

### Trade (`/trade`)
- `TradeComponent` — pair selector bar, pair info stats, layout container
- `TradeChartComponent` — wraps `lightweight-charts`, supports interval switching
- `OrderBookComponent` — depth visualization with animated bars
- `SpotTradingComponent` — buy/sell forms, limit/market toggle, % fill buttons

### Transactions (`/transactions`)
- `TransactionsComponent` — tabs map to transaction types, filter bar, expandable rows, CSV export

### Settings (`/settings`)
- `SettingsComponent` — sidebar navigation between Profile / Security / Language / Account tabs
- Security tab includes full 2FA setup flow with QR code

---

## How to Add a New Feature

1. **Create the component file:**
   ```
   src/app/features/<feature-name>/<feature-name>.component.ts
   ```

2. **Add a route in `app.routes.ts`** inside the protected children array:
   ```typescript
   {
     path: 'feature-name',
     loadComponent: () =>
       import('./features/feature-name/feature-name.component').then(m => m.FeatureNameComponent),
   },
   ```

3. **Add a nav item in `sidebar.component.ts`:**
   ```typescript
   { label: 'Feature', icon: 'icon_name', route: '/feature-name' },
   ```

4. **Create a service** in `src/app/core/services/feature.service.ts` following the try/catch pattern.

---

## How to Swap Dummy Data for Real APIs

Each service method in `src/app/core/services/` looks like this:

```typescript
async getData(): Promise<DataType> {
  try {
    // ← This already calls your real API
    return await firstValueFrom(this.http.get<DataType>(...));
  } catch (err) {
    console.error('[Service] error:', err);
    // ↓ DELETE everything below this line when your endpoint is ready
    return dummyData;
  }
}
```

**Steps:**
1. Set the correct URL in `src/environments/environment.ts`
2. Remove the dummy `return` inside the `catch` block of that method
3. Keep `console.error` for logging
4. Test — the `try` block was always making the real call

---

## Architecture Decisions

| Decision | Rationale |
|---|---|
| Standalone components | No NgModules — simpler, tree-shakeable, Angular 17+ best practice |
| Signals for state | Built-in Angular reactivity, no RxJS boilerplate for simple state |
| Functional guards | Cleaner, no class injection needed, Angular 15+ recommended |
| Functional interceptors | Same as guards — simpler than class-based |
| Lazy-loaded routes | Reduces initial bundle size; each page is a separate chunk |
| try/catch + dummy data | Allows full UI development without live APIs; easy to remove later |
| CSS Custom Properties | Single source of truth for theming; no runtime overhead |
| JWT in localStorage | Simple for a mini-exchange; for production consider httpOnly cookies |
| nginx multi-stage Docker | Minimal final image (~25MB); handles SPA routing automatically |
