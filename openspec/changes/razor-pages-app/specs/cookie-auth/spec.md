## ADDED Requirements

### Requirement: Login with HttpOnly cookie
The system SHALL authenticate users via a POST to the `/login` page. On successful authentication, the system SHALL issue an HttpOnly, SameSite=Strict, Secure cookie containing the user's identity and permission claims. The cookie SHALL NOT be accessible to JavaScript. Development environments use a local TLS certificate so the Secure flag is always enforced.

#### Scenario: Successful login
- **WHEN** a user submits valid credentials on the login page
- **THEN** the system sets an authentication cookie and redirects to the home page (users list)

#### Scenario: Invalid credentials
- **WHEN** a user submits incorrect username or password
- **THEN** the system redisplays the login page with an error message and does NOT set a cookie

#### Scenario: Already authenticated user visits login page
- **WHEN** an authenticated user navigates to `/login`
- **THEN** the system redirects them to the home page

### Requirement: Session sliding expiration
The authentication cookie SHALL use sliding expiration. The cookie TTL SHALL reset on each authenticated request as long as the user remains active.

#### Scenario: Active user session renewal
- **WHEN** an authenticated user makes a request within the sliding window
- **THEN** the response contains a refreshed cookie with a new expiration timestamp

#### Scenario: Inactive session expiry
- **WHEN** no authenticated request is made for longer than the configured expiration period
- **THEN** the cookie expires and the user is redirected to the login page on the next request

### Requirement: Logout clears the authentication cookie
The system SHALL provide a logout mechanism that deletes the authentication cookie and ends the session.

#### Scenario: User logs out
- **WHEN** a user submits the logout form (POST to `/logout`)
- **THEN** the system deletes the authentication cookie and redirects to the login page

#### Scenario: Unauthenticated access after logout
- **WHEN** a user attempts to access a protected page after logging out
- **THEN** the system redirects them to `/login`

### Requirement: Unauthenticated access redirects to login
The system SHALL redirect unauthenticated requests to any protected page to the `/login` page.

#### Scenario: Unauthenticated page access
- **WHEN** an unauthenticated user navigates to any protected Razor Page
- **THEN** the system redirects to `/login?returnUrl=<original-path>`

#### Scenario: Post-login redirect to local URL
- **WHEN** a user logs in after being redirected from a protected page with a valid local `returnUrl`
- **THEN** the system redirects them to the original `returnUrl` using `LocalRedirect` or equivalent validation

#### Scenario: Post-login redirect with external returnUrl is rejected
- **WHEN** the `returnUrl` parameter contains an external or non-local URL (e.g. `https://evil.com`, `//evil.com`)
- **THEN** the system ignores the `returnUrl` and redirects to the default page instead

#### Scenario: Post-login redirect with missing returnUrl
- **WHEN** no `returnUrl` parameter is present
- **THEN** the system redirects to the default page after login

### Requirement: Account lockout enforced during cookie login
The system SHALL honour the existing account lockout mechanism. A locked account SHALL be rejected at login even with correct credentials.

#### Scenario: Locked account login attempt
- **WHEN** a user whose account is locked submits credentials (even correct ones)
- **THEN** the system redisplays the login page with a lockout message and does NOT set a cookie

#### Scenario: Lockout after repeated failures
- **WHEN** a user exceeds the configured maximum failed login attempts
- **THEN** the account is locked for the configured duration and subsequent login attempts are rejected

### Requirement: Permission and identity claims embedded in cookie
The system SHALL embed the following claims in the cookie payload at login so that authorization checks work without a database round-trip per request:
- The authenticated user's permission claims (e.g. `UserCreate`, `UserDelete`)
- The authenticated user's ID (for ownership checks)
- An `isAdmin` claim when the user holds the Administrator role

#### Scenario: Authorized user accesses permitted page
- **WHEN** an authenticated user with the required permission accesses a Razor Page
- **THEN** access is granted based on claims in the cookie

#### Scenario: Authenticated user without permission is forbidden
- **WHEN** an authenticated user lacks the required permission for a page
- **THEN** the system returns a 403 Forbidden response or redirects to an access-denied page

#### Scenario: isAdmin claim present for administrator users
- **WHEN** a user with the Administrator role logs in
- **THEN** the issued cookie contains the `isAdmin` claim

#### Scenario: isAdmin claim absent for non-administrator users
- **WHEN** a user without the Administrator role logs in
- **THEN** the issued cookie does NOT contain the `isAdmin` claim

### Requirement: Data Protection key persistence
The system SHALL configure ASP.NET Core Data Protection to persist encryption keys to a durable location. Keys SHALL NOT be stored only in memory, as this would invalidate all active sessions on every application restart or redeploy.

#### Scenario: Sessions survive application restart
- **WHEN** the application is restarted or redeployed
- **THEN** existing authenticated sessions remain valid and users are not logged out

#### Scenario: Keys are not lost on container restart
- **WHEN** the Docker container is stopped and restarted
- **THEN** previously issued authentication cookies are still accepted

## REMOVED Requirements

### Requirement: JWT bearer token authentication
**Reason**: Replaced by HttpOnly cookie authentication. JWT bearer tokens are not appropriate for server-rendered applications and require client-side token management.
**Migration**: No migration path — this is a breaking architectural change. Consumers of the WebAPI template should use the separate WebAPI template if they need JWT.

### Requirement: Refresh token rotation
**Reason**: Eliminated by cookie sliding expiration. The cookie middleware handles session renewal automatically on each request.
**Migration**: Drop the `RefreshTokens` table via an EF Core migration. Remove `RefreshToken` entity, `RefreshTokenCleanupService`, and `RefreshTokenOptions`.
