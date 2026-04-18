## ADDED Requirements

### Requirement: Vite dev server proxies /api/* to the .NET backend
`vite.config.ts` SHALL configure a dev server proxy that forwards all requests matching `/api/*` to the URL defined by `VITE_API_BASE_URL` (defaulting to `http://localhost:5000`). This avoids CORS issues during local development.

#### Scenario: API call in dev is proxied to the backend
- **WHEN** the Vite dev server is running and a request to `/api/users` is made from the browser
- **THEN** the request is forwarded to `{VITE_API_BASE_URL}/api/users` and the response is returned to the browser

### Requirement: Docker Compose includes a frontend service
`docker-compose.yml` at the repo root SHALL include a `frontend` service that builds from `webapp/Dockerfile`, exposes port 5173 (dev) or 80 (production nginx), and declares a dependency on the `api` service.

#### Scenario: docker compose up starts both services
- **WHEN** `docker compose up` is run from the repo root
- **THEN** both the `api` (.NET) service and the `frontend` (React) service start without errors

### Requirement: Production Dockerfile serves the built app via nginx
`webapp/Dockerfile` SHALL use a multi-stage build: first stage installs dependencies and runs `npm run build`; second stage copies `dist/` into an nginx image and serves it on port 80. The nginx config SHALL proxy `/api/*` requests to the `api` service by hostname.

#### Scenario: Docker image builds and serves the app
- **WHEN** `docker build -t frontend webapp/` is run
- **THEN** the image is created without errors and running it serves the React app on port 80
