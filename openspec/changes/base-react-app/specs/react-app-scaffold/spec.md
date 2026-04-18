## ADDED Requirements

### Requirement: Vite + React + TypeScript project exists under webapp/
The system SHALL provide a `webapp/` directory at the repository root containing a fully initialised Vite + React + TypeScript project. The project SHALL include `package.json`, `tsconfig.json`, `vite.config.ts`, ESLint configuration, and a working `npm run dev` / `npm run build` script.

#### Scenario: Dev server starts successfully
- **WHEN** a developer runs `npm run dev` inside `webapp/`
- **THEN** the Vite dev server starts without errors on the configured port (default 5173)

#### Scenario: Production build succeeds
- **WHEN** a developer runs `npm run build` inside `webapp/`
- **THEN** a production bundle is emitted to `webapp/dist/` without errors

### Requirement: Project structure follows feature-based layout
The `src/` directory SHALL be organised by feature: `api/`, `auth/`, `users/`, `components/`. Each feature folder SHALL contain only code relevant to that feature.

#### Scenario: Feature folders are present after scaffold
- **WHEN** the `webapp/src/` directory is inspected
- **THEN** it contains `api/`, `auth/`, `users/`, and `components/` subdirectories

### Requirement: Environment variable for API base URL
The app SHALL read the backend base URL from the `VITE_API_BASE_URL` environment variable. A `.env.example` file SHALL be present documenting this variable. The app SHALL NOT hardcode any localhost URLs.

#### Scenario: .env.example is present and documents the variable
- **WHEN** `.env.example` is read
- **THEN** it contains `VITE_API_BASE_URL=http://localhost:5000` as an example value

#### Scenario: Missing VITE_API_BASE_URL falls back to empty string proxy
- **WHEN** `VITE_API_BASE_URL` is not set and the Vite proxy is configured
- **THEN** API calls are proxied to the Vite dev server's own origin (relative path)
