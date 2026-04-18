## 1. Project Scaffold

- [ ] 1.1 Create `webapp-angular/` directory and generate an Angular 17+ standalone app with the Angular CLI (`ng new webapp-angular --standalone --routing --style=css --skip-git`)
- [ ] 1.2 Create feature directory structure: `src/app/api/`, `src/app/auth/`, `src/app/users/`
- [ ] 1.3 Create `src/environments/environment.ts` and `src/environments/environment.prod.ts` exporting `apiBaseUrl`
- [ ] 1.4 Add `.env.example` documenting how to set `NG_APP_API_BASE_URL` (or equivalent)
- [ ] 1.5 Configure ESLint with `@angular-eslint` and `@typescript-eslint/recommended`; verify `npm run lint` exits 0
- [ ] 1.6 Verify `npm start` and `npm run build` succeed on the bare scaffold

## 2. API Client

- [ ] 2.1 Create `src/app/api/types.ts` with `User`, `LoginRequest`, and `AuthResponse` TypeScript interfaces
- [ ] 2.2 Create `src/app/api/auth-api.service.ts` with `login(username, password)` and `refresh(refreshToken)` methods using `HttpClient`
- [ ] 2.3 Create `src/app/api/users-api.service.ts` with `getUsers()` method returning `Observable<User[]>`
- [ ] 2.4 Create `src/app/auth/auth.interceptor.ts` (functional `HttpInterceptorFn`) that attaches `Authorization: Bearer <token>` from `localStorage` on every outgoing request
- [ ] 2.5 Extend `auth.interceptor.ts` to handle 401: call refresh → update tokens → retry original request once; redirect to `/login` if refresh also fails
- [ ] 2.6 Register `AuthInterceptor` via `provideHttpClient(withInterceptors([authInterceptor]))` in `app.config.ts`

## 3. Auth UI

- [ ] 3.1 Create `src/app/auth/auth.service.ts` with a `Signal<User | null>` for current user, `login()`, `logout()`, and startup token-restore logic
- [ ] 3.2 Create `src/app/auth/login/login.component.ts` — reactive form with username and password fields, submit handler, and error display
- [ ] 3.3 Create `src/app/auth/auth.guard.ts` as a functional `CanActivateFn` that redirects to `/login` if unauthenticated
- [ ] 3.4 Add routes in `app.routes.ts`: `/login` → `LoginComponent`; `/` and `/users` protected by `AuthGuard`
- [ ] 3.5 Wire navigation bar into `AppComponent` with a logout button and a link to `/users`
- [ ] 3.6 Manually test login with valid credentials → redirected to `/users`; invalid credentials → error message shown

## 4. Users UI

- [ ] 4.1 Create `src/app/users/users.component.ts` — fetches `getUsers()` on init and renders a list/table of usernames
- [ ] 4.2 Add loading state (spinner or text) while the fetch is in flight
- [ ] 4.3 Add error state with a human-readable message on non-401 API failure
- [ ] 4.4 Add empty state message ("No users found") when the API returns an empty array
- [ ] 4.5 Manually test the users list page against the running .NET backend

## 5. Dev Proxy & Docker

- [ ] 5.1 Create `webapp-angular/proxy.conf.json` forwarding `/api/*` to the .NET backend; wire it into `angular.json` via `proxyConfig`
- [ ] 5.2 Create `webapp-angular/Dockerfile` — multi-stage: `node:lts-alpine` build stage + `nginx:alpine` serve stage
- [ ] 5.3 Create `webapp-angular/nginx.conf` — serve static files, proxy `/api/*` to `api` service, SPA fallback to `index.html`
- [ ] 5.4 Add `frontend-angular` service to `docker-compose.yml` (build from `webapp-angular/`, distinct host port, depends on `api`)
- [ ] 5.5 Verify `docker compose up` starts `api`, `frontend`, and `frontend-angular` without errors

## 6. Verification

- [ ] 6.1 Manually test login flow against running .NET backend
- [ ] 6.2 Manually test users list page with real data
- [ ] 6.3 Manually test token refresh (expire/clear access token, confirm silent refresh and request retry)
- [ ] 6.4 Manually test logout clears state and redirects to `/login`
- [ ] 6.5 Manually test direct navigation to `/users` returns `index.html` from nginx (SPA fallback)
- [ ] 6.6 Run `npm run build` to confirm the production build is clean
- [ ] 6.7 Run `npm run lint` to confirm no ESLint violations
