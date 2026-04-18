## ADDED Requirements

### Requirement: Dockerfile builds and runs the Razor Pages app
`MyBuyingList.Web/Dockerfile` SHALL use a multi-stage build: a build stage (`mcr.microsoft.com/dotnet/sdk:8.0`) and a runtime stage (`mcr.microsoft.com/dotnet/aspnet:8.0-alpine`). The resulting image SHALL run the app on port 80.

#### Scenario: Docker image builds successfully
- **WHEN** `docker build -f MyBuyingList.Web/Dockerfile .` is executed from the repo root
- **THEN** the build succeeds and produces a runnable image

#### Scenario: Container serves the app
- **WHEN** the container is started and port 80 is accessible
- **THEN** navigating to the container's address returns the Razor Pages application

### Requirement: Docker Compose includes the web service
`docker-compose.yml` (or an equivalent compose file) SHALL include a `web` service that builds from `MyBuyingList.Web/Dockerfile`, depends on the `api` service, and maps a host port to container port 80.

#### Scenario: Compose starts both services
- **WHEN** `docker compose up` is executed
- **THEN** both the `api` and `web` services start without errors

#### Scenario: Web service can reach API service by name
- **WHEN** the `web` container makes an HTTP request to the API using the compose service name (e.g., `http://api:5000`)
- **THEN** the request succeeds (network is shared within the compose project)

### Requirement: BackendApi:BaseUrl is set in Docker Compose
The `web` service in `docker-compose.yml` SHALL set the `BackendApi__BaseUrl` environment variable to point to the `api` service (e.g., `http://api:5000`).

#### Scenario: Environment variable overrides default base URL
- **WHEN** the `web` container starts with `BackendApi__BaseUrl=http://api:5000`
- **THEN** all backend API calls from the web app target that address
