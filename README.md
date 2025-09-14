# E-Commerce Microservices Platform

This repository contains an intermediate-level e-commerce platform built with a polyglot microservices architecture (Java Spring Boot + .NET 8 + Angular) and RabbitMQ for asynchronous messaging.

## Services and Ports

- Auth Service (Spring Boot, PostgreSQL): http://localhost:8081
- Product Service (Spring Boot, MySQL): http://localhost:8082
- Order Service (Spring Boot, PostgreSQL): http://localhost:8083
- Cart Service (.NET 8, Redis): http://localhost:8084
- Payment Service (.NET 8, SQLite): http://localhost:8085
- API Gateway (Spring Cloud Gateway): http://localhost:8080
- RabbitMQ UI: http://localhost:15672 (guest/guest)

## Core Flows

- Login with email/password → receive JWT (Auth Service)
- Products browsing (Product Service)
- Cart operations (Cart Service) with TTL rules:
  - Default: 24h on add/update
  - On order placed: extend TTL = 7 days
  - On payment confirmed: optional enhancement to remove TTL (indefinite)
- Place Order (Order Service, JWT required)
- Pay Order (Payment Service, JWT required) → emits payment.success or payment.failed
- Order Service updates status based on payment events

## Prerequisites

- Java 17+
- .NET 8 SDK
- RabbitMQ (Windows service or Docker)
- Redis (Memurai/WSL/Docker)
- PostgreSQL (for Auth + Order)
- MySQL (for Product)
- Node.js LTS + Angular CLI (for frontend) — optional until you run the Angular app

## Quick Start (without Docker)

1) Start RabbitMQ
- Enable management plugin (done once):
  - Use "RabbitMQ Command Prompt (sbin dir)":
    - `rabbitmq-plugins.bat enable rabbitmq_management`
    - `rabbitmq-service.bat start`
- UI: http://localhost:15672 (guest/guest)

2) Start Databases
- PostgreSQL: create `authdb` and `orderdb`
- MySQL: create `productdb`

3) Run Java services (from repo root)
- Auth: `./mvnw.cmd -pl auth-service spring-boot:run`
- Product: `./mvnw.cmd -pl product-service spring-boot:run`
- Order: `./mvnw.cmd -pl order-service spring-boot:run`
- Gateway: `./mvnw.cmd -pl gateway-service spring-boot:run`

4) Run .NET services
- Cart (needs Redis at `localhost:6379`):
  - `cd cart-service`
  - `dotnet restore && dotnet run`
- Payment (SQLite file `payments.db` auto-created):
  - `cd payment-service`
  - `dotnet restore && dotnet run`

5) Frontend (Angular)
- Requires Node.js LTS and Angular CLI: `npm i -g @angular/cli`
- Scaffold command (to run once):
  - `ng new frontend --routing --style=scss`
  - `cd frontend`
  - `npm i primeng primeicons @angular/animations`
- Configure API base as `http://localhost:8080` (Gateway) in `src/environments/*`.
- Implement pages: Login/Register, Products, Cart, Checkout (skeletons listed below).
- Run: `ng serve -o`

> If you prefer, I can commit a ready-made Angular app into `frontend/` in a follow-up step after you confirm Node is installed.

## Configuration

- JWT secret: shared across services
  - Auth: `auth-service/src/main/resources/application.yml`
  - Order: `order-service/src/main/resources/application.yml`
  - Gateway: `gateway-service/src/main/resources/application.yml`
  - .NET services (Cart/Payment): `appsettings.json`
- RabbitMQ:
  - Order publishes `order.created` to exchange `order.events`
  - Product consumes `order.created`
  - Payment publishes `payment.success` / `payment.failed` to exchange `payment.events`
  - Order consumes payment events and updates order status
- Redis (Cart): key format `cart:{userId}` with TTL logic

## Postman Testing

1) Login (Auth)
- POST `http://localhost:8081/api/auth/login`
- Body:
```
{
  "email": "john@example.com",
  "password": "MyPlainPassword123!"
}
```
- Get `token` from response.

2) Create Product (Product)
- POST `http://localhost:8082/api/products`
- Body:
```
{
  "name": "Laptop X",
  "description": "15-inch, 16GB RAM",
  "price": 1299.99,
  "stock": 10,
  "category": "Laptops"
}
```

3) Cart operations (Cart, JWT required)
- POST `http://localhost:8084/api/cart/items`
- Headers: `Authorization: Bearer <token>`
- Body:
```
{
  "productId": 101,
  "quantity": 2,
  "price": 49.99
}
```
- GET `http://localhost:8084/api/cart` (with Authorization header)

4) Create Order (Order, JWT required)
- POST `http://localhost:8083/api/orders`
- Headers: Authorization
- Body:
```
{
  "userId": 1,
  "totalAmount": 149.97,
  "status": "CREATED",
  "items": [
    { "productId": 101, "quantity": 1, "price": 99.99 },
    { "productId": 102, "quantity": 1, "price": 49.98 }
  ]
}
```

5) Pay Order (Payment, JWT required)
- POST `http://localhost:8085/api/payments`
- Body:
```
{ "orderId": 1, "userId": 1, "amount": 149.97, "simulateFailure": false }
```
- Effect:
  - `payment.success` → Order status becomes `PAID`
  - `payment.failed` → Order status becomes `CANCELLED`

## Swagger / API Docs

- Java services (enable springdoc-openapi):
  - Auth: `http://localhost:8081/swagger-ui.html`
  - Product: `http://localhost:8082/swagger-ui.html`
  - Order: `http://localhost:8083/swagger-ui.html`
- .NET services (Swashbuckle):
  - Cart: `http://localhost:8084/swagger`
  - Payment: `http://localhost:8085/swagger`

> If any Swagger UI is missing, I will add springdoc dependencies and config to each Java service.

## Troubleshooting

- Maven cache lock:
  - Close Java/maven processes, delete folder under `~/.m2/repository` and re-run with `-U`.
- Redis not running:
  - Install Memurai, or use WSL Ubuntu `redis-server`, or Docker.
- JWT 401 at Gateway:
  - Ensure `Authorization: Bearer <token>` header is present and the secret matches across services.
- RabbitMQ UI not visible:
  - Restart service after enabling management plugin.

## Next Enhancements

- Service Discovery (Eureka)
- Cart: remove TTL on payment success (consumer on `payment.success`)
- Angular: full UI polish and store management
- CI/CD: GitHub Actions to build and test services

---

If you want me to add a ready-made Angular app into `frontend/` now, let me know whether Node.js + Angular CLI are installed; I will commit the code and instructions to run it.
