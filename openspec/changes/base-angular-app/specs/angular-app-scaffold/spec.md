## ADDED Requirements

### Requirement: Angular project exists under webapp-angular/
The system SHALL contain an Angular 17+ application scaffolded with the Angular CLI under a `webapp-angular/` directory at the repo root. The application SHALL use standalone components (no NgModules) and TypeScript throughout.

#### Scenario: Project structure is valid
- **WHEN** a developer runs `npm install && npm start` in `webapp-angular/`
- **THEN** the Angular CLI dev server starts without errors and the app is accessible in a browser

#### Scenario: Production build succeeds
- **WHEN** a developer runs `npm run build` in `webapp-angular/`
- **THEN** the build completes without errors and outputs static files to `webapp-angular/dist/`

### Requirement: Feature directory structure
The application SHALL organize source code by feature under `src/app/`: `api/` for HTTP services, `auth/` for authentication concerns, and `users/` for the users feature.

#### Scenario: Directories are present after scaffold
- **WHEN** the repository is cloned and the Angular project is inspected
- **THEN** `src/app/api/`, `src/app/auth/`, and `src/app/users/` directories exist

### Requirement: ESLint is configured
The project SHALL include an ESLint configuration using `@angular-eslint` and `@typescript-eslint/recommended` rules with no material violations on the generated scaffold.

#### Scenario: Lint passes on generated code
- **WHEN** a developer runs `npm run lint` in `webapp-angular/`
- **THEN** the command exits with code 0

### Requirement: Environment variable for API base URL
The application SHALL read the backend API base URL from an Angular environment file (`environments/environment.ts`) so it can be overridden per environment without code changes.

#### Scenario: Default environment file is present
- **WHEN** the repository is cloned
- **THEN** `webapp-angular/src/environments/environment.ts` exists and exports an `apiBaseUrl` property

#### Scenario: Example env file is present
- **WHEN** the repository is cloned
- **THEN** `webapp-angular/.env.example` (or equivalent documentation) documents how to set the API base URL
