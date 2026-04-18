## ADDED Requirements

### Requirement: Users list page displays all users
The `Pages/Users/Index.cshtml` page SHALL display a list of users retrieved from `GET /api/users` via `BackendApiClient`.

#### Scenario: Authenticated user sees user list
- **WHEN** an authenticated user navigates to `/users`
- **THEN** the page displays a table or list of users returned by the API

### Requirement: Users list page requires authentication
The users list page SHALL be accessible only to authenticated users.

#### Scenario: Unauthenticated access redirects to login
- **WHEN** an unauthenticated user navigates to `/users`
- **THEN** they are redirected to `/auth/login`

### Requirement: Users list shows user identifiers
Each user entry in the list SHALL display at least the user's username or display name.

#### Scenario: User data is rendered
- **WHEN** the API returns a non-empty list of users
- **THEN** each user's username is visible on the page

### Requirement: Empty state is handled gracefully
If the API returns an empty user list, the page SHALL display an appropriate empty-state message instead of an empty table.

#### Scenario: No users returns friendly message
- **WHEN** the API returns an empty users array
- **THEN** the page displays a message such as "No users found"

### Requirement: API error is handled gracefully
If `BackendApiClient` throws or the API returns an error response, the page SHALL display an error message and SHALL NOT crash.

#### Scenario: API failure shows error message
- **WHEN** the backend API is unavailable or returns a 5xx error
- **THEN** the page displays a user-friendly error message
