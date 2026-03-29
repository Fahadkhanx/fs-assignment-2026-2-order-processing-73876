# SportsStore - Distributed Order Processing Platform

A simple e-commerce platform with microservices architecture. Built with .NET 10, React, and Blazor.

---

## System Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  React Admin    │     │  Blazor Portal  │     │   Order API     │
│   (Port 3000)   │     │  (Port 5187)    │     │  (Port 5138)    │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────────────┴───────────────────────┘
                                 │
                    ┌────────────┴────────────┐
                    │     Rabbit MQ   
                    └────────────┬────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌────────▼────────┐     ┌────────▼────────┐     ┌────────▼────────┐
│   Inventory     │     │    Payment      │     │    Shipping     │
│  (Port 5139)    │     │  (Port 5140)    │     │  (Port 5141)    │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

**4 Microservices:**
1. **OrderAPI** - Main API, handles orders and checkout
2. **InventoryService** - Manages product stock
3. **PaymentService** - Processes payments with Stripe
4. **ShippingService** - Manages shipments

---

## Event Flow (Order Processing)

```
1. Customer clicks "Checkout" in Blazor
         │
         ▼
2. OrderAPI creates order (Status: Submitted)
         │
         ▼
3. OrderAPI sends OrderSubmittedEvent → InventoryService
         │
         ▼
4. InventoryService checks stock
   ├─ If available: InventoryConfirmedEvent → PaymentService
   └─ If not: Order cancelled
         │
         ▼
5. PaymentService processes payment with Stripe
   ├─ If success: PaymentApprovedEvent → ShippingService
   └─ If failed: Order marked PaymentFailed
         │
         ▼
6. ShippingService creates shipment
         │
         ▼
7. Order status: Shipped → Delivered
```

---

## How to Run

### Option 1: Docker (Recommended - Single Container)

```bash
# Build the image
docker build -f Dockerfile.single -t sportsstore .

# Run the container
docker run -d -p 3000:3000 -p 5187:5187 -p 5138-5141:5138-5141 sportsstore
```

**Access the app:**
- React Admin: http://localhost:3000
- Blazor Portal: http://localhost:5187
- Order API: http://localhost:5138

### Option 2: Manual Run

**Step 1: Start all services**
```bash
# Terminal 1: OrderAPI
cd SportsStore.OrderAPI
dotnet run --urls "http://localhost:5138"

# Terminal 2: InventoryService
cd SportsStore.InventoryService
dotnet run --urls "http://localhost:5139"

# Terminal 3: PaymentService
cd SportsStore.PaymentService
dotnet run --urls "http://localhost:5140"

# Terminal 4: ShippingService
cd SportsStore.ShippingService
dotnet run --urls "http://localhost:5141"
```

**Step 2: Start frontends**
```bash
# Terminal 5: Blazor
cd SportsStore.Blazor
dotnet run --urls "http://localhost:5187"

# Terminal 6: React Admin
cd SportsStore.Admin
npm install
npm run dev
```

---

## Service Responsibilities

| Service | Port | What it does |
|---------|------|--------------|
| **OrderAPI** | 5138 | Main API. Handles checkout, orders, products. Uses CQRS pattern. |
| **InventoryService** | 5139 | Manages stock levels. Checks availability before payment. |
| **PaymentService** | 5140 | Processes payments using Stripe. Handles test cards. |
| **ShippingService** | 5141 | Creates shipments and tracks delivery status. |
| **React Admin** | 3000 | Admin dashboard to manage orders, inventory, payments. |
| **Blazor Portal** | 5187 | Customer-facing shop to browse products and checkout. |

---

## Assumptions and Limitations

### Assumptions
- SQLite database used (simple, no setup needed)
- Stripe test mode (no real payments)
- Single container deployment (all services in one Docker image)
- In-memory messaging (RabbitMQ disabled in single container)

### Limitations
- No real RabbitMQ in single container (services communicate via HTTP)
- SQLite not suitable for high traffic production
- No user authentication (simple demo app)
- Inventory check is basic (no reservation expiry)
- Payment is simulated (Stripe test cards only)

---

## Technology Stack

### Backend
- .NET 10 - Web API microservices
- Entity Framework Core with SQLite
- MediatR - CQRS implementation
- MassTransit - RabbitMQ messaging
- AutoMapper - Object mapping
- Serilog - Structured logging

### Frontend
- Blazor WebAssembly - Customer portal
- React 18 + Vite - Admin dashboard (JavaScript)
- Bootstrap 5 - UI styling

## Project Structure

```
SportsStore/
├── SportsStore.Shared/           
├── SportsStore.OrderAPI/         
├── SportsStore.InventoryService/
├── SportsStore.PaymentService/  
├── SportsStore.ShippingService/  
├── SportsStore.Blazor/           
├── SportsStore.Admin/           
├── SportsStore.Tests/           
├── SportsStore.IntegrationTests/ 
├── SportsStore.OrderAPI.Tests/   
├── SportsStore.InventoryService.Tests/
├── SportsStore.PaymentService.Tests/
├── SportsStore.ShippingService.Tests/
├── docker-compose.yml            
├── .dockerignore                
└── .github/workflows/ci.yml     
```

## Microservices

### 1. OrderAPI (Port 5138)
Main API for orders and checkout.

**Endpoints:**
```
POST   /api/orders/checkout      - Create new order
GET    /api/orders               - List all orders
GET    /api/orders/{id}          - Get order by ID
GET    /api/orders/dashboard     - Dashboard summary
GET    /api/products             - List products
```

### 2. InventoryService (Port 5139)
Manages product stock.

**Endpoints:**
```
GET    /api/inventory            - List inventory
GET    /api/inventory/{id}       - Get by product ID
```

### 3. PaymentService (Port 5140)
Processes payments with Stripe.

**Endpoints:**
```
GET    /api/payment/transactions - List transactions
GET    /api/payment/test-cards   - List test cards
```

### 4. ShippingService (Port 5141)
Manages shipments.

**Endpoints:**
```
GET    /api/shipping/shipments   - List shipments
POST   /api/shipping/shipments/{id}/dispatch - Dispatch
POST   /api/shipping/shipments/{id}/deliver  - Deliver
```

