## ADDED Requirements

### Requirement: Angular CLI proxy forwards /api/* to the .NET backend in development
The system SHALL include a `proxy.conf.json` at the `webapp-angular/` root that configures the Angular CLI dev server to forward all requests matching `/api/*` to the .NET backend URL, eliminating CORS issues during local development.

#### Scenario: Developer runs npm start
- **WHEN** a developer runs `npm start` in `webapp-angular/`
- **THEN** requests to `/api/*` are proxied to the configured .NET backend URL without CORS errors in the browser

#### Scenario: proxy.conf.json is referenced in angular.json
- **WHEN** the Angular CLI dev server starts
- **THEN** `proxy.conf.json` is picked up automatically via the `proxyConfig` entry in `angular.json`

### Requirement: Dockerfile builds and serves the Angular app
The system SHALL provide a `webapp-angular/Dockerfile` using a multi-stage build: a `node:lts-alpine` build stage that runs `npm run build`, and an `nginx:alpine` serve stage that copies the compiled output and serves it.

#### Scenario: Docker image builds successfully
- **WHEN** `docker build` is run in `webapp-angular/`
- **THEN** the image is built without errors

#### Scenario: Container serves the app on port 80
- **WHEN** the Docker container is started
- **THEN** the Angular app is accessible on port 80 inside the container

### Requirement: nginx serves the Angular SPA with API proxy and fallback
The system SHALL provide a `webapp-angular/nginx.conf` that: serves static files from the build output directory, proxies `/api/*` requests to the `api` service (in Docker Compose), and falls back to `index.html` for all other paths to support Angular client-side routing.

#### Scenario: Direct navigation to a non-root route
- **WHEN** a user navigates directly to a URL such as `/users` in a browser pointing at the nginx container
- **THEN** `index.html` is returned and Angular's client-side router renders the correct component

#### Scenario: API request through nginx container
- **WHEN** the browser makes a request to `/api/users` through the nginx container
- **THEN** the request is forwarded to the `api` service and the response is returned to the browser

### Requirement: Docker Compose includes a frontend-angular service
The system SHALL add a `frontend-angular` service to `docker-compose.yml` that builds from `webapp-angular/Dockerfile`, exposes the app on a host port distinct from the React frontend's port, and declares a dependency on the `api` service.

#### Scenario: docker compose up starts both frontends and the API
- **WHEN** a developer runs `docker compose up`
- **THEN** the `api`, `frontend` (React), and `frontend-angular` services all start without errors
