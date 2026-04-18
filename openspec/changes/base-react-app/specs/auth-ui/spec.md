## ADDED Requirements

### Requirement: AuthContext provides authentication state app-wide
The system SHALL provide an `AuthContext` (via `src/auth/AuthContext.tsx`) that exposes: current user claims (or `null`), `login(username, password)` action, and `logout()` action. The context SHALL be provided at the app root so all components can consume it.

#### Scenario: Unauthenticated state on cold load with no stored token
- **WHEN** the app loads and no token is found in localStorage
- **THEN** `AuthContext` exposes `user: null`

#### Scenario: Authenticated state restored from localStorage on page refresh
- **WHEN** the app loads and a valid access token exists in localStorage
- **THEN** `AuthContext` exposes the decoded user claims without requiring a new login

### Requirement: Login page collects credentials and authenticates
The system SHALL provide a `/login` route rendering a login form with username and password fields and a submit button. On submit, it SHALL call `POST /api/auth/login` via the API client. On success, tokens SHALL be stored and the user redirected to `/`. On failure, an error message SHALL be displayed.

#### Scenario: Successful login redirects to home
- **WHEN** a user submits valid credentials on the login page
- **THEN** tokens are stored, `AuthContext` updates to authenticated state, and the browser navigates to `/`

#### Scenario: Invalid credentials shows error message
- **WHEN** a user submits invalid credentials
- **THEN** an error message is displayed on the login page and the user remains on `/login`

### Requirement: Protected routes redirect unauthenticated users to login
The system SHALL provide a `ProtectedRoute` component that wraps routes requiring authentication. If the user is not authenticated, it SHALL redirect to `/login` preserving the intended destination.

#### Scenario: Unauthenticated user accessing a protected route is redirected
- **WHEN** an unauthenticated user navigates to a protected URL (e.g., `/users`)
- **THEN** the browser redirects to `/login`

#### Scenario: Authenticated user accesses a protected route normally
- **WHEN** an authenticated user navigates to a protected URL
- **THEN** the route renders the intended component

### Requirement: Logout clears tokens and redirects to login
The system SHALL provide a logout action in `AuthContext`. Calling `logout()` SHALL remove tokens from localStorage, clear auth state, and navigate to `/login`.

#### Scenario: Logout clears state and redirects
- **WHEN** `logout()` is called
- **THEN** localStorage tokens are removed, `user` becomes `null`, and the browser navigates to `/login`
