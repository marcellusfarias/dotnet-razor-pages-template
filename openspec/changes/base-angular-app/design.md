## Context

The existing template provides a .NET Web API backend with JWT authentication (access + refresh tokens), a users resource, and a health endpoint. A React frontend (`webapp/`) was added in `base-react-app`. This design adds a parallel Angular frontend under `webapp-angular/` that demonstrates the same integration patterns using Angular's conventions — giving teams a choice of frontend stack without changing any backend code.

## Goals / Non-Goals

**Goals:**
- Scaffold a working Angular 17+ standalone-components app in `webapp-angular/`
- Use Angular's built-in `HttpClient` with an `HttpInterceptor` for JWT attachment and token refresh
- Provide login/logout UI backed by the real `/api/auth` endpoints
- Provide a users list page backed by the real `/api/users` endpoint
- Protect routes with an Angular `AuthGuard`
- Provide Angular CLI proxy config for development and a Dockerfile + Docker Compose entry for production-like runs

**Non-Goals:**
- Production-ready UI/UX or component library (no Angular Material, no accessibility audit)
- Full CRUD for users (read-only list is sufficient as a pattern demo)
- Server-Side Rendering (Angular Universal / SSR)
- Unit or e2e tests for the Angular app (covered by existing .NET integration tests for the API layer)
- Duplicate or replace the React frontend — both coexist independently

## Decisions

### 1. Angular 17+ with Standalone Components
Angular 17+ made standalone components the default. NgModules are legacy boilerplate. Using standalone components keeps the scaffold minimal, idiomatic, and aligned with where the Angular ecosystem is heading. The Angular CLI `--standalone` flag is no longer needed — it is the default.

### 2. Angular's built-in HttpClient over axios
Alternatives considered:
- **axios**: familiar, but bypasses Angular's DI system and `HttpInterceptor` pipeline — rejected
- **native fetch**: no interceptors; refresh/retry logic must be wired manually — rejected
- **Angular HttpClient**: ships with the framework, integrates natively with DI, interceptors, and RxJS — **chosen**

Token refresh is handled in an `AuthInterceptor` (functional interceptor, Angular 15+ style): on 401, call `POST /api/auth/refresh`, update stored tokens, and replay the original request once. If the refresh also fails, redirect to `/login`.

### 3. Auth state: Angular Signal-based `AuthService` + localStorage
Alternatives considered:
- **NgRx**: mature, but heavy for a minimal template — rejected
- **BehaviorSubject (RxJS)**: idiomatic pre-Angular-16, still works, but Signals are the modern default
- **sessionStorage**: cleared on tab close — bad UX for a persistent session pattern
- **HttpOnly cookies (server-side)**: more secure but requires backend changes — out of scope

`AuthService` exposes a `Signal<User | null>` for the current user. Raw tokens are persisted to `localStorage` so the session survives page refresh. A comment in the service flags the XSS trade-off.

### 4. Routing and guards: Angular Router + functional `AuthGuard`
Angular 15+ supports `canActivate` as a plain function (`CanActivateFn`), eliminating the need for a guard class. The guard reads `AuthService` and redirects to `/login` if no valid token is present.

### 5. Directory layout
```
webapp-angular/
  src/
    app/
      api/              # typed services wrapping HttpClient
        auth.service.ts
        users.service.ts
      auth/             # AuthService, AuthInterceptor, LoginComponent, auth.guard.ts
      users/            # UsersComponent
      app.component.ts
      app.routes.ts
    environments/
      environment.ts        # apiBaseUrl: '' (proxied in dev)
      environment.prod.ts   # apiBaseUrl: read from window or build var
  proxy.conf.json           # Angular CLI dev proxy: /api → .NET backend
  .env.example              # NG_APP_API_BASE_URL=http://localhost:5000
  Dockerfile                # multi-stage: node:alpine build → nginx:alpine serve
  nginx.conf                # static files + /api proxy + SPA fallback
```

### 6. Docker Compose
A `frontend-angular` service builds from `webapp-angular/Dockerfile` and depends on the existing `api` service. It runs alongside the React frontend on a different port so both can be tested independently.

## Risks / Trade-offs

- **localStorage token storage** → XSS risk. Mitigation: this is a pattern demo; production apps should evaluate HttpOnly cookies. A comment in `AuthService` flags this explicitly.
- **Single-retry refresh loop** → if the refresh token is also expired, the interceptor redirects to `/login` without infinite looping. Manual testing required (no automated e2e tests in scope).
- **Angular CLI proxy only works in dev** → production builds go through the nginx reverse-proxy in the Dockerfile. The nginx config must match the .NET route structure — same trade-off as the React frontend.
- **Signals API is Angular 16+** → minimum Angular version for this scaffold is 17 (current LTS). Older versions are not supported.

## Migration Plan

No migration needed — purely additive. `webapp-angular/` is independent of the .NET solution and of `webapp/`. Existing consumers of the template are unaffected.

To adopt as a template user:
1. `cd webapp-angular && npm install`
2. Copy `.env.example` → `.env.local` and set `NG_APP_API_BASE_URL`
3. `npm start` (runs Angular CLI dev server with proxy)

## Open Questions

- Should `webapp-angular/` use `@angular/material` for minimal styling? (Adds an extra dependency but improves demo aesthetics — decision deferred to implementation; default is no component library.)
- Should the Dockerfile build stage pin a specific Node version, or use `node:lts-alpine`? (Prefer `node:lts-alpine` for automatic LTS tracking without manual updates.)
