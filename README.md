# 🔐 NP-AuthService

[![CI](https://github.com/JuanDeRe/NP-AuthService/actions/workflows/ci.yml/badge.svg)](https://github.com/JuanDeRe/NP-AuthService/actions/workflows/ci.yml)

Authentication microservice of [Nightwatch Protocol](https://github.com/Nightwatch-Protocol/nightwatch-protocol), a real-time asymmetric multiplayer horror game.

## What it does
- **Registers** players (`POST /auth/register`) and **logs them in** (`POST /auth/login`)
- Hashes passwords with ASP.NET Core `PasswordHasher`
- Issues **signed JWTs** (issuer, audience, expiration configurable) that Matchmaking and the Realtime Gateway rely on
- Persists users in **Amazon DynamoDB** (table auto-created at startup; local DynamoDB supported for development)
- Rejects duplicate usernames with a dedicated exception
- Publishes **operational metrics** to CloudWatch (`Observability/`)
- Exposes `/health` for load-balancer health checks

## What was achieved
Identity is established once at login and trusted across services: the same JWT signing configuration is shared with the Gateway, which validates it on every WebSocket connection. The service is stateless and deployed behind an ALB in an Auto Scaling Group.

## Tech stack
C# · .NET 9 · ASP.NET Core · JWT Bearer · DynamoDB (AWS SDK) · CloudWatch · Docker · GitHub Actions

## Run locally
```bash
# DynamoDB Local on :8000 (default dev config uses UseLocalMode=true)
dotnet run
```

| Setting (env var) | Description |
|---|---|
| `Jwt__Key` | **Required in any real environment.** The value in `appsettings.json` is a dev-only placeholder |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes` | Token settings |
| `DynamoDb__TableName`, `DynamoDb__ServiceUrl`, `DynamoDb__UseLocalMode` | User table settings |
| `Cors__AllowedOrigins__0` | Allowed web-client origin |

## Notes / limitations
- No automated tests in this repo yet (CI builds the project).
- Roles are stored (`Player`); role-based authorization is not enforced on endpoints.

## Part of Nightwatch Protocol
[Main repo](https://github.com/Nightwatch-Protocol/nightwatch-protocol) · [Unity Client](https://github.com/JuanDeRe/Nightwatch-Protocol-Unity-Client) · **Auth** · [Matchmaking](https://github.com/JuanDeRe/NP-MatchmakingService) · [Gateway](https://github.com/JuanDeRe/NP-RealtimeGateway) · [Game Service](https://github.com/JuanDeRe/NP-GameService) · [Terraform](https://github.com/JuanDeRe/NP-Terraform)
