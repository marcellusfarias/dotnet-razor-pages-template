## ADDED Requirements

### Requirement: Login page
The system SHALL provide a `/login` Razor Page with a form accepting username and password. The page SHALL display validation errors inline. The page SHALL be accessible to unauthenticated users.

#### Scenario: Login page renders for unauthenticated user
- **WHEN** an unauthenticated user navigates to `/login`
- **THEN** a form with username and password fields is displayed

#### Scenario: Login form submission with empty fields
- **WHEN** a user submits the login form with missing username or password
- **THEN** the page redisplays with field-level validation messages

### Requirement: Users list page
The system SHALL provide a `/users` Razor Page that lists all users with pagination. The page SHALL require the `UserGetAll` permission.

#### Scenario: Authorized user views users list
- **WHEN** an authenticated user with `UserGetAll` permission navigates to `/users`
- **THEN** a paginated list of users is displayed with username, email, and active status

#### Scenario: Pagination controls
- **WHEN** the total number of users exceeds the configured page size
- **THEN** next/previous navigation links are rendered

#### Scenario: Unauthorized user is forbidden
- **WHEN** an authenticated user without `UserGetAll` permission navigates to `/users`
- **THEN** the system returns a 403 response

### Requirement: Create user page
The system SHALL provide a `/users/create` Razor Page with a form for creating a new user. The page SHALL require the `UserCreate` permission.

#### Scenario: Authorized user creates a valid user
- **WHEN** an authenticated user with `UserCreate` permission submits a valid create form
- **THEN** the user is created and the system redirects to the users list

#### Scenario: Create user with validation errors
- **WHEN** a user submits the create form with invalid data (e.g., duplicate username, weak password)
- **THEN** the page redisplays with specific validation error messages

### Requirement: Change password page
The system SHALL provide a `/users/{id}/change-password` Razor Page. The page SHALL require the `UserUpdate` permission.

#### Scenario: Authorized user changes password successfully
- **WHEN** an authenticated user with `UserUpdate` permission submits a valid current password and matching new passwords
- **THEN** the password is updated and the system redirects to the users list

#### Scenario: Incorrect current password
- **WHEN** the submitted current password does not match the stored password
- **THEN** the page redisplays with an error message

### Requirement: Delete user page
The system SHALL provide a `/users/{id}/delete` Razor Page with a confirmation form. The page SHALL require the `UserDelete` permission. The business rule that the admin user cannot be deleted SHALL be enforced.

#### Scenario: Authorized user deletes a non-admin user
- **WHEN** an authenticated user with `UserDelete` permission confirms deletion of a non-admin user
- **THEN** the user is deactivated and the system redirects to the users list

#### Scenario: Attempt to delete admin user
- **WHEN** a user attempts to delete the admin user
- **THEN** the page redisplays with an error message explaining the operation is not allowed

### Requirement: Logout action
The system SHALL provide a POST-only `/logout` Razor Page handler that clears the authentication cookie and redirects to `/login`. The handler SHALL be protected by an anti-forgery token.

#### Scenario: Authenticated user logs out
- **WHEN** an authenticated user submits the logout form
- **THEN** the cookie is cleared and the user is redirected to `/login`

### Requirement: Anti-forgery protection on all state-changing pages
All Razor Page POST handlers that change state (login, logout, create, delete, change password) SHALL validate the anti-forgery token. Requests without a valid token SHALL be rejected with a 400 response.

#### Scenario: Valid anti-forgery token accepted
- **WHEN** a form is submitted with a valid anti-forgery token
- **THEN** the handler processes the request normally

#### Scenario: Missing or invalid anti-forgery token rejected
- **WHEN** a POST is submitted without a valid anti-forgery token (e.g., a forged cross-site request)
- **THEN** the system returns a 400 Bad Request and does NOT process the request

### Requirement: Minimal HTML with no external CSS or JS dependencies
All Razor Pages SHALL render semantic HTML. The template SHALL NOT depend on any external CDN, CSS framework, or JavaScript build pipeline.

#### Scenario: Page renders without CDN resources
- **WHEN** any page is rendered with network access blocked
- **THEN** the page is fully functional (form submission works, validation messages are visible)

### Requirement: Security response headers
The system SHALL include the following HTTP security headers on all responses:
- `X-Frame-Options: DENY` — prevents the app from being embedded in iframes (clickjacking protection)
- `X-Content-Type-Options: nosniff` — prevents browsers from MIME-sniffing responses away from the declared content type
- `Referrer-Policy: strict-origin-when-cross-origin` — limits referrer information sent on navigation

#### Scenario: Security headers present on page response
- **WHEN** any page is requested
- **THEN** the response includes `X-Frame-Options`, `X-Content-Type-Options`, and `Referrer-Policy` headers with the values above

#### Scenario: Security headers present on error response
- **WHEN** the system returns an error response (4xx, 5xx)
- **THEN** the same security headers are included

### Requirement: Insecure Direct Object Reference (IDOR) protection on user pages
Pages that operate on a specific user by `{id}` (change-password, delete) SHALL enforce two layers of access control:

1. **Existence check** — if the `{id}` does not correspond to an existing user, the system SHALL return 404.
2. **Ownership check** — the authenticated user may only operate on their own `{id}`, unless they carry an `isAdmin` claim, in which case they may operate on any existing user. A non-admin operating on another user's `{id}` SHALL be rejected with 403.

The `isAdmin` claim SHALL be embedded in the authentication cookie at login alongside the permission claims.

#### Scenario: Admin operates on any existing user
- **WHEN** an authenticated user with the `isAdmin` claim submits a request for any existing `{id}`
- **THEN** the operation is processed normally

#### Scenario: Non-admin operates on their own record
- **WHEN** an authenticated user without the `isAdmin` claim submits a request where `{id}` matches their own user ID
- **THEN** the operation is processed normally

#### Scenario: Non-admin attempts to operate on another user's record
- **WHEN** an authenticated user without the `isAdmin` claim submits a request where `{id}` does not match their own user ID
- **THEN** the system returns a 403 Forbidden response

#### Scenario: Non-existent user id returns 404
- **WHEN** an authorized user submits a request with an `{id}` that does not exist
- **THEN** the system returns a 404 Not Found response

### Requirement: Production error page does not leak internal details
In non-development environments, the system SHALL display a generic error page for unhandled exceptions. Stack traces, exception messages, and internal system details SHALL NOT be exposed to the client.

#### Scenario: Unhandled exception in production
- **WHEN** an unhandled exception occurs in a non-development environment
- **THEN** the system renders a generic error page with no internal details

#### Scenario: Developer exception page in development
- **WHEN** an unhandled exception occurs in the development environment
- **THEN** the system renders the full developer exception page with stack trace

## REMOVED Requirements

### Requirement: REST API controllers
**Reason**: Replaced by Razor Pages. External API consumption is out of scope for this template.
**Migration**: No migration path — consumers needing a REST API should use the WebAPI template.

### Requirement: Swagger / OpenAPI documentation
**Reason**: No API endpoints remain. Swagger has no purpose in a server-rendered-only application.
**Migration**: Remove Swashbuckle NuGet packages and all Swagger configuration from `Services.cs` and `ConfigureApp.cs`.
