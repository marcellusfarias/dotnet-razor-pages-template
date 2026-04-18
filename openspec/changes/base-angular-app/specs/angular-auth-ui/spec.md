## ADDED Requirements

### Requirement: AuthService manages session state and token persistence
The system SHALL provide an `AuthService` under `src/app/auth/` that exposes a `Signal<User | null>` representing the currently authenticated user. `AuthService` SHALL persist the raw access token and refresh token to `localStorage` on login and remove them on logout. On application startup, `AuthService` SHALL restore session state from `localStorage` if valid tokens are present.

#### Scenario: User logs in successfully
- **WHEN** `AuthService.login(username, password)` is called and the backend returns a valid `AuthResponse`
- **THEN** the access token and refresh token are stored in `localStorage`, and the current-user signal is updated with the decoded user claims

#### Scenario: User logs out
- **WHEN** `AuthService.logout()` is called
- **THEN** access token and refresh token are removed from `localStorage`, and the current-user signal is set to `null`

#### Scenario: Session restored on page refresh
- **WHEN** the application loads and valid tokens exist in `localStorage`
- **THEN** the current-user signal is initialized with the decoded user claims, and the user is not redirected to the login page

### Requirement: LoginComponent provides username/password form
The system SHALL provide a `LoginComponent` under `src/app/auth/` that renders a form with username and password fields and a submit button. On successful submission, the component SHALL call `AuthService.login()` and navigate to the `/users` route. On failure, the component SHALL display a human-readable error message.

#### Scenario: Valid credentials submitted
- **WHEN** the user enters valid credentials and clicks Submit
- **THEN** the login API is called, the session is established, and the user is redirected to `/users`

#### Scenario: Invalid credentials submitted
- **WHEN** the user enters invalid credentials and clicks Submit
- **THEN** an error message is displayed and the user remains on the login page

#### Scenario: Login form is accessible at /login
- **WHEN** the user navigates to `/login`
- **THEN** the `LoginComponent` is rendered

### Requirement: AuthGuard protects authenticated routes
The system SHALL provide a functional `AuthGuard` (`CanActivateFn`) under `src/app/auth/` that reads `AuthService` and returns `true` if the user is authenticated, or redirects to `/login` if not.

#### Scenario: Authenticated user accesses protected route
- **WHEN** an authenticated user navigates to a protected route (e.g., `/users`)
- **THEN** the route is activated and the target component is rendered

#### Scenario: Unauthenticated user accesses protected route
- **WHEN** an unauthenticated user navigates to a protected route
- **THEN** the router redirects to `/login`
