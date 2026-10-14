# Development startup and configuration

The API uses `ValidateOnStart()` for JWT configuration. If `Jwt:SigningKey` is absent or too short, .NET throws an `OptionsValidationException` from `Microsoft.Extensions.Options.StartupValidator`. That framework class is **not** the source of the bug: fix the application configuration, not the framework code.

## Configure local development

Run from the solution root:

```powershell
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost;Database=OrderManagementDB;Trusted_Connection=True;TrustServerCertificate=True" --project src/OrderManagement.Api

$keyBytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($keyBytes)
$signingKey = [Convert]::ToBase64String($keyBytes)
dotnet user-secrets set "Jwt:SigningKey" "$signingKey" --project src/OrderManagement.Api
```

Adjust the SQL Server connection string for your environment. `UserSecretsId` is configured in the API project file; no need to run `user-secrets init` again. ASP.NET Core automatically loads User Secrets while running in Development. Do **not** commit secrets to git. The previous SQL password was committed; rotate it because removing it from the latest version does not erase git history.

```powershell
dotnet restore OrderManagement.slnx
dotnet build OrderManagement.slnx
dotnet run --project src/OrderManagement.Api --launch-profile http
```

## Known issues in the source snapshot

The `develop` branch at `e294c6ac` includes registrations for `IBulkOrderWriteRepository` and `BulkOrderWriteRepository`, but neither type is present in that committed tree. It also contains an `IOrderReadRepository` contract and `OrderReadRepository` implementation that reference DTO namespaces/types not present in the corresponding committed files. Several order handlers mentioned in local runtime errors are not committed to this branch. Synchronize the full working source before treating runtime DI errors from those handlers as reproducible against this snapshot.

The Identity project's tracked directory currently begins with a space (`src/ OrderManagement.Infrastructure.Identity`). Rename it consistently in a separate change to avoid broken project references. Do not copy/recreate business features merely to silence dependency injection errors.

## API pipeline

Security services are registered by `AddApiSecurity()`, which provides `ICurrentUser` and `IHttpContextAccessor`. `AddInfrastructure()` registers the order persistence dependencies. The API now calls `UseRateLimiter()`, `UseAuthentication()`, `UseAuthorization()` and maps `MapAuthEndpoints()` after configuring services.

If a DI exception still names missing services, compare the local branch and local uncommitted files with the commit under test; also confirm the exact startup project and clean/rebuild.


## Swagger UI and JWT testing

When running with the `Development` environment, Swagger UI is available at:

- `http://localhost:5080/swagger`
- OpenAPI JSON: `http://localhost:5080/swagger/v1/swagger.json`

Swagger is deliberately not enabled in Production. The API uses `Swashbuckle.AspNetCore` 10.2.3 and defines an HTTP Bearer/JWT security scheme.

1. Run the API with `dotnet run --project src/OrderManagement.Api --launch-profile http`.
2. Use `POST /api/auth/login` to obtain an access token for an existing user. The Identity users must already exist (the current snapshot registers `IdentitySeeder` but does not call it during startup).
3. In Swagger UI, select **Authorize**, paste only the `accessToken` value (without the `Bearer ` prefix), then authorize.
4. Call `GET /api/auth/me` to verify JWT authentication. Other endpoints appear only when they are actually mapped in the running API.

The global OpenAPI security requirement only documents authentication; it does **not** bypass or implement role/permission checks. `POST /api/auth/login` remains anonymous at runtime.

**Do not paste access tokens or passwords into issues or Git commits.** The current tracked repository snapshot has missing order implementation types and cannot be claimed as fully verified until the entire solution builds cleanly.
