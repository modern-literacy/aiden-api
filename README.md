# AIDEN API

A schema-first .NET integration surface for AIDEN.

This service is intentionally thin. It forwards to the TypeScript engine, preserves typed models for consumers that want a .NET boundary, and keeps business logic in the engine where the policy lock, scoring, gate logic, and bounded assistive runtime actually live.

## Position in the system
- **Not the decision core** — deterministic authority stays in `aiden-engine`
- **Not a separate agent runtime** — it is a typed HTTP surface
- **Yes, an intentional integration layer** — useful for consumers that need OpenAPI, typed C# models, or a gateway pattern

## Contract story
- The engine publishes the canonical schemas in `aiden-engine/contracts/`
- The API mirrors those contracts with typed C# models
- Schema changes should be reflected here explicitly rather than hidden behind opaque package drift

## Architecture
```text
Client -> AIDEN API (.NET) -> AIDEN Engine (TypeScript)
```

## Endpoints
| Method | Path | Description |
|---|---|---|
| POST | `/api/proposals` | Create a proposal from YAML |
| GET | `/api/proposals/{id}` | Retrieve a proposal |
| POST | `/api/proposals/{id}/transition` | Trigger a lifecycle transition |
| POST | `/api/proposals/{id}/review` | Trigger a review |
| GET | `/api/proposals/{id}/review` | Get latest review result |
| POST | `/api/proposals/{id}/copilot/gaps` | Proposal-shaping gap analysis |
| POST | `/api/proposals/{id}/copilot/preflight` | Readiness check before formal review |
| GET | `/health` | Health check |

## Development
```bash
dotnet restore AidenApi.sln
dotnet build AidenApi.sln
dotnet run --project AidenApi
```
