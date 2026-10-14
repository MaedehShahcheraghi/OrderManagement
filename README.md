# Order Management Service

Order management API using .NET 10, Clean Architecture, EF Core SQL Server, JWT authorization, MediatR, and background Worker hosting.

## Projects

- Domain: order aggregate, inventory model, validation and status history
- Application: order commands, queries, pagination, authorization checks
- Infrastructure.Persistence: EF Core, SQL Server, migrations and seed data
- Infrastructure.Identity: ASP.NET Core Identity, JWT and permissions
- API: HTTP endpoints, authentication, rate limiting and documentation
- Worker: hosted process; RabbitMQ is outside this task's scope

## Local setup

Install .NET 10 and configure SQL Server. Development configuration is stored in User Secrets, not tracked appsettings files.

```powershell
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost;Database=OrderManagementDB;Trusted_Connection=True;TrustServerCertificate=True" --project src/OrderManagement.Api

$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:SigningKey" "$key" --project src/OrderManagement.Api

dotnet restore OrderManagement.slnx
dotnet build OrderManagement.slnx
```

Run the sales database migration with EF Core CLI, using `OrderDbContext` and the Persistence project. Apply the Identity migration separately using `IdentityDBContext` in the Identity project after creating the initial Identity migration. The Identity database should be prepared before attempting to seed login users.

```powershell
dotnet ef database update --project src/OrderManagement.Infrastructure.Persistence --startup-project src/OrderManagement.Api --context OrderDbContext
dotnet run --project src/OrderManagement.Api --launch-profile http
dotnet test OrderManagement.slnx
```

The API listens on `http://localhost:5080`, with `GET /health/live` for liveness. The Swagger PR enables `http://localhost:5080/swagger` in Development.

## Authentication

`POST /api/auth/login` accepts an email and password for an existing ASP.NET Core Identity user. The response contains an `accessToken`. Send `Authorization: Bearer <accessToken>` to the protected endpoints. `GET /api/auth/me` returns the authenticated account's roles and customer ID.

The codebase contains `IdentitySeeder`, but the bootstrap needs to be executed against a migrated Identity database to create login users. Do not commit passwords or access tokens. The previously committed SQL password must be rotated.

## Orders

- `POST /api/orders`: create an order; pricing is loaded from persisted products
- `POST /api/orders/bulk`: create up to 500 orders per transaction
- `GET /api/orders/{id}`: retrieve order details
- `GET /api/orders`: filter by customer, status and created date; offset pagination
- `GET /api/orders/after`: cursor pagination
- `POST /api/orders/{id}/confirm`: atomically deduct inventory and confirm
- `POST /api/orders/{id}/ship`: ship confirmed order
- `POST /api/orders/{id}/deliver`: deliver shipped order
- `DELETE /api/orders/{id}`: soft delete an order

Permissions are enforced on endpoints. Admins can manage orders; normal users are scoped to their associated customer when creating or reading orders. Inventory decrements are executed conditionally in a database transaction.

## Validation and outstanding work

Domain xUnit tests cover status ordering, price snapshots, duplicate items, soft deletion, and stock rules. Run `dotnet test OrderManagement.slnx`.

Database-backed integration testing, EF Core migration execution for Identity and API smoke testing with live JWT credentials still require verification. The repository previously contained mismatched DTOs and missing application features, so run the full build/test suite on the integrated branches before submission. The batched EF Core bulk write is not a provider-native bulk copy and should not be described as equivalent to `SqlBulkCopy`. RabbitMQ and Docker are intentionally out of scope.

See `docs/development-startup.md` for details.
