## Context

The existing template exposes a .NET Web API with JWT authentication (access + refresh tokens), a users resource, and a health endpoint. There is no frontend. Developers cloning this template to build a full-stack app must wire up a UI from scratch every time.

This design adds a React app inside a `webapp/` directory at the repo root. The app is deliberately thin — it demonstrates how to integrate with the existing backend, not how to build a full product.

## Goals / Non-Goals

**Goals:**
- Scaffold a working Vite + React + TypeScript app in `webapp/`
- Provide a typed API client that attaches JWT tokens and handles refresh automatically
- Provide login/logout UI backed by the real `/api/auth` endpoints
- Provide a users list page backed by the real `/api/users` endpoint
- Provide a dev-proxy so the Vite dev server forwards `/api/*` to the .NET backend
- Add a Docker Compose service for the frontend

**Non-Goals:**
- Production-ready UI/UX design (no component library, no accessibility audit)
- Full CRUD for users (read-only list is sufficient as a pattern demo)
- State management library (React Context is sufficient for this scope)
- End-to-end tests (covered by existing .NET integration tests for the API)
- SSR / Next.js (Vite SPA is the simplest approach)

## Decisions

### 1. Vite over Create React App
Vite is the current standard for new React projects — faster dev server, native ESM, built-in proxy configuration. CRA is effectively unmaintained.

### 2. TypeScript
Consistent with the .NET template's emphasis on type safety. All new code in `webapp/` uses `.tsx`/`.ts`.

### 3. API client: native `fetch` with a thin wrapper over `axios`
Alternatives considered:
- **`axios`**: widely used, interceptors make token refresh ergonomic — **chosen**
- **`fetch` only**: no interceptors means retry/refresh logic must be wired manually — rejected for higher boilerplate
- **React Query / TanStack Query**: adds value for caching but is out of scope for a minimal template

The API client module wraps `axios`, attaches the bearer token from local storage on every request, and uses a response interceptor to detect 401s, call `POST /api/auth/refresh`, retry the original request once, and redirect to login if refresh also fails.

### 4. Auth state: React Context + localStorage
Alternatives considered:
- **Zustand**: lightweight but an extra dependency
- **sessionStorage**: cleared on tab close — bad UX for a "remember me" pattern
- **HttpOnly cookie (handled server-side)**: more secure but requires backend changes — out of scope

React Context holds the decoded access token claims and expiry. `localStorage` persists the raw tokens so the user stays logged in across page refreshes.

### 5. Routing: React Router v6
Standard choice, no meaningful alternatives at this scale.

### 6. Directory layout
```
webapp/
  src/
    api/          # axios client + typed endpoint functions
    auth/         # AuthContext, useAuth hook, login page
    users/        # UsersPage component
    components/   # shared UI (HealthBadge, ProtectedRoute)
    App.tsx
    main.tsx
  .env.example    # VITE_API_BASE_URL=http://localhost:5000
  vite.config.ts  # proxy config
  Dockerfile      # multi-stage: build → nginx serve
```

### 7. Docker Compose
A new `frontend` service builds from `webapp/Dockerfile` and depends on the existing `api` service. Dev workflow uses `docker compose up` to run both.

## Risks / Trade-offs

- **localStorage token storage** → XSS risk. Mitigation: this is a template demonstrating the pattern; production apps should evaluate HttpOnly cookies. A comment in the code flags this. # TODO REVIEW
- **Single-retry refresh loop** → if the refresh endpoint itself returns 401 (expired refresh token), the client redirects to login without infinite looping. This must be tested manually.
- **Vite proxy only works in dev** → production builds go through nginx reverse-proxy config in the Dockerfile. The nginx config must be kept in sync with the .NET route structure.

## Migration Plan

No migration needed — this is purely additive. The `webapp/` directory is independent of the .NET solution. Existing consumers of the template are unaffected.

To adopt as a template user:
1. `cd webapp && npm install`
2. Copy `.env.example` → `.env.local` and set `VITE_API_BASE_URL`
3. `npm run dev`

## Open Questions

- Should the Dockerfile use `node:alpine` for the build stage? (Prefer smallest image — yes, unless build tooling requires glibc.)
- Should ESLint use `eslint-config-airbnb` or the lighter `@eslint/js` recommended config? (Airbnb adds opinions beyond scope — use recommended + `@typescript-eslint/recommended`.)
