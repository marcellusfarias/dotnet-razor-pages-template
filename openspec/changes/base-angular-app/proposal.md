## Why

The template already ships a base React frontend (`base-react-app`). To make it useful for teams that prefer Angular, a parallel Angular frontend should exist that demonstrates the same integration patterns (auth, JWT refresh, users list) using Angular's idioms — so developers can pick the frontend stack that matches their team's expertise.

## What Changes

- Add a `webapp-angular/` directory at the repo root with an Angular 17+ standalone-components application
- Pre-configure Angular's built-in `HttpClient` to point to the .NET backend (base URL via environment variable)
- Include authentication flow: login form, JWT access-token storage, automatic refresh-token handling via an `HttpInterceptor`
- Include a basic users list page consuming the existing `GET /api/users` endpoint
- Add an Angular `AuthGuard` that protects routes requiring a valid session
- Provide Angular dev server proxy configuration so `/api/*` requests are forwarded to the .NET backend (no CORS issues in development)
- Add a Dockerfile and Docker Compose service entry for the Angular app

## Capabilities

### New Capabilities
- `angular-app-scaffold`: Angular CLI project scaffold with standalone components, Angular Router, and ESLint configuration under `webapp-angular/`
- `angular-api-client`: Typed service wrapping Angular's `HttpClient`, with an `HttpInterceptor` that attaches `Authorization: Bearer` headers and handles 401 → refresh → retry
- `angular-auth-ui`: Login component, `AuthService` (token storage + decode), and `AuthGuard` protecting routes that require authentication
- `angular-users-ui`: Users list component consuming `GET /api/users`, with loading, error, and empty states
- `angular-dev-proxy`: Angular CLI proxy config for development + Dockerfile (multi-stage) + Docker Compose service entry

### Modified Capabilities

## Impact

- New top-level `webapp-angular/` directory — no effect on the existing .NET solution, `webapp/` (React), or CI
- `docker-compose.yml` updated to include the new `frontend-angular` service
- No changes to existing API contracts, database schema, or .NET code
- `.github/workflows/deploy.yml` may need a new job for building/deploying the Angular frontend (out of scope for this change)
