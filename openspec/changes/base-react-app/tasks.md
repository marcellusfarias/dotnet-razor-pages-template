## 1. Project Scaffold

- [ ] 1.1 Create `webapp/` directory and initialise Vite + React + TypeScript project (`npm create vite@latest`)
- [ ] 1.2 Install dependencies: `axios`, `react-router-dom`
- [ ] 1.3 Create feature directory structure: `src/api/`, `src/auth/`, `src/users/`, `src/components/`
- [ ] 1.4 Add `.env.example` with `VITE_API_BASE_URL=http://localhost:5000`
- [ ] 1.5 Configure ESLint with `@eslint/js` recommended + `@typescript-eslint/recommended`
- [ ] 1.6 Verify `npm run dev` and `npm run build` succeed

## 2. API Client

- [ ] 2.1 Create `src/api/client.ts` — axios instance with `baseURL` from `VITE_API_BASE_URL`
- [ ] 2.2 Add request interceptor to attach `Authorization: Bearer <token>` from localStorage
- [ ] 2.3 Add response interceptor to handle 401: refresh token → retry once → redirect to `/login` on second 401
- [ ] 2.4 Create `src/api/types.ts` with `User`, `LoginRequest`, `AuthResponse` TypeScript types
- [ ] 2.5 Create `src/api/auth.ts` with `login(username, password)` and `refresh(refreshToken)` functions
- [ ] 2.6 Create `src/api/users.ts` with `getUsers()` function returning `Promise<User[]>`

## 3. Auth UI

- [ ] 3.1 Create `src/auth/AuthContext.tsx` — context with `user`, `login()`, `logout()` and localStorage token persistence
- [ ] 3.2 Create `src/auth/useAuth.ts` — convenience hook wrapping `useContext(AuthContext)`
- [ ] 3.3 Create `src/auth/LoginPage.tsx` — form with username/password fields, submit handler, error display
- [ ] 3.4 Create `src/components/ProtectedRoute.tsx` — redirects to `/login` if unauthenticated
- [ ] 3.5 Wire `AuthContext.Provider` at the app root in `src/App.tsx`
- [ ] 3.6 Add React Router routes: `/login` → `LoginPage`, `/` and `/users` wrapped in `ProtectedRoute`

## 4. Users UI

- [ ] 4.1 Create `src/users/UsersPage.tsx` — fetches `getUsers()` on mount, renders list/table of usernames
- [ ] 4.2 Add loading state (spinner or text) while fetch is in flight
- [ ] 4.3 Add error state with human-readable message on non-401 API failure
- [ ] 4.4 Add empty state message ("No users found") when API returns empty array
- [ ] 4.5 Add navigation link to `/users` in app layout/navbar

## 5. Dev Proxy & Docker

- [ ] 5.1 Configure Vite dev server proxy in `vite.config.ts`: `/api/*` → `VITE_API_BASE_URL`
- [ ] 5.2 Create `webapp/Dockerfile` — multi-stage: `node:alpine` build stage + `nginx:alpine` serve stage
- [ ] 5.3 Create `webapp/nginx.conf` — serve static files, proxy `/api/*` to `api` service, fallback to `index.html` for SPA routing
- [ ] 5.4 Add `frontend` service to `docker-compose.yml` (build from `webapp/`, port 80, depends on `api`)
- [ ] 5.5 Verify `docker compose up` starts both services without errors

## 6. Verification

- [ ] 6.1 Manually test login flow against running .NET backend
- [ ] 6.2 Manually test users list page with real data
- [ ] 6.3 Manually test token refresh (expire/clear access token, confirm silent refresh)
- [ ] 6.4 Manually test logout clears state and redirects
- [ ] 6.5 Run `npm run build` to confirm production build is clean
