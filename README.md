# AIDEN API

A thin HTTP adapter over AIDEN engine contracts. This .NET service exposes a REST API that forwards requests to the TypeScript engine and translates responses into typed C# models.

## Contract Story: Schema-First

**The engine publishes JSON Schemas** (`review-result.schema.json`, `gate-decision.schema.json`) in `aiden-engine/contracts/`. Those schemas are the single source of truth for all inter-service contracts.

**C# models are derived from those schemas, not from an npm package dependency.** There is no `npm install` in this repo. The models in `AidenApi/Models/` are hand-written C# records that mirror the engine's JSON Schema definitions. When a schema changes upstream, the corresponding C# record is updated to match.

**In production, this service is a sidecar or gateway** that forwards requests to the TypeScript engine and translates responses. All business logic — policy evaluation, gate decisions, scoring — lives in the engine. This API is a typed HTTP surface over those capabilities.

## How Models Map to Engine Schemas

| C# Model | Engine Schema | Description |
|---|---|---|
| `GateDecision` | `gate-decision.schema.json` | Outcome of policy gate evaluation |
| `ReviewResult` | `review-result.schema.json` | Full review with per-section assurance profiles |
| `SectionProfile` | Embedded in `review-result` | Per-domain assurance scores and rule tallies |
| `AutonomyBudget` | Embedded in `gate-decision` | Consumed/remaining autonomy budget |
| `Proposal` | Engine internal | Proposal metadata and YAML content |
| `LifecycleState` | Engine internal | Proposal state machine with transition history |
| `CopilotGapResult` | Engine internal | Gap analysis from copilot module |
| `PreflightResult` | Engine internal | Readiness check before formal review |

See the [aiden-engine](../aiden-engine) repo for the canonical contract definitions.

## Architecture

```
┌──────────────┐       ┌──────────────────┐       ┌────────────────┐
│   Client     │──────▶│   AIDEN API      │──────▶│  AIDEN Engine   │
│  (Browser,   │ HTTP  │   (.NET 8)       │ HTTP  │  (TypeScript)   │
│   CLI, CI)   │◀──────│   Sidecar/GW     │◀──────│  Policy Engine  │
└──────────────┘       └──────────────────┘       └────────────────┘
```

## Endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/proposals` | Create a proposal from YAML |
| `GET` | `/api/proposals/{id}` | Retrieve a proposal |
| `POST` | `/api/proposals/{id}/transition` | Trigger lifecycle state transition |
| `POST` | `/api/proposals/{id}/review` | Trigger full policy review |
| `GET` | `/api/proposals/{id}/review` | Get latest review result |
| `POST` | `/api/proposals/{id}/copilot/gaps` | Run copilot gap analysis |
| `POST` | `/api/proposals/{id}/copilot/preflight` | Run preflight readiness check |
| `GET` | `/health` | Health check |

## Development

```bash
# Restore and build
dotnet restore AidenApi.sln
dotnet build AidenApi.sln

# Run locally (defaults to http://localhost:5000)
dotnet run --project AidenApi

# Configure engine URL
ENGINE_URL=http://localhost:3000 dotnet run --project AidenApi
```

Swagger UI is available at `/swagger` in development mode.

## Docker

```bash
docker build -t aiden-api .
docker run -p 5000:5000 -e ENGINE_URL=http://engine:3000 aiden-api
```
