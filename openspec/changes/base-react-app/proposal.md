## Why

This template currently provides only a .NET Web API backend. To make it a full-stack template that developers can use as a starting point for modern web applications, it needs a companion React frontend that's pre-wired to the existing API (auth, users, health endpoints).

## What Changes

- Add a `MyBuyingList.Web` React application (Vite + React + TypeScript) under a new `webapp/` directory at the repo root
- Pre-configure API client pointing to the .NET backend (configurable base URL via environment variable)
- Include authentication flow: login form, JWT token storage, refresh token handling
- Include a basic user list page that calls the existing `GET /api/users` endpoint
- Add a health check status indicator on the home/dashboard page
- Provide Docker Compose entry and dev-proxy configuration so the React app proxies API calls to avoid CORS issues in development # TODO REVIEW

## Capabilities

### New Capabilities
- `react-app-scaffold`: Base Vite + React + TypeScript project structure with routing (React Router), state management (Zustand or React Context), and ESLint/Prettier configuration
- `api-client`: Typed API client (using `fetch` or `axios`) wired to the backend, with JWT attach and automatic token refresh on 401
- `auth-ui`: Login page and auth context/provider that stores access token and triggers refresh token flow
- `users-ui`: Basic users list page consuming `GET /api/users`
- `dev-proxy`: Vite dev server proxy config + Docker Compose service entry for the React app # TODO REVIEW

### Modified Capabilities

## Impact

- New top-level `webapp/` directory (does not affect existing .NET solution or CI)
- `.github/workflows/deploy.yml` may need a new job for building/deploying the frontend
- `docker-compose.yml` (if it exists) will be updated to include the frontend service
- No changes to existing API contracts, database schema, or .NET code
