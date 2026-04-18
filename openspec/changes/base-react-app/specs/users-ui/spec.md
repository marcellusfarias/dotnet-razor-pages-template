## ADDED Requirements

### Requirement: Users list page fetches and displays all users
The system SHALL provide a `/users` route (protected) that calls `GET /api/users` on mount and renders the result as a list or table. Each row SHALL display at minimum the user's username.

#### Scenario: Users are displayed on successful fetch
- **WHEN** an authenticated user navigates to `/users` and the API returns a 200 with a list of users
- **THEN** each user's username is visible on the page

#### Scenario: Empty state is shown when no users exist
- **WHEN** an authenticated user navigates to `/users` and the API returns an empty array
- **THEN** a message such as "No users found" is displayed

### Requirement: Loading state is shown while fetching
The page SHALL display a loading indicator while the `GET /api/users` request is in flight.

#### Scenario: Loading indicator appears during fetch
- **WHEN** the users page mounts and the API request has not yet resolved
- **THEN** a loading indicator (e.g., spinner or "Loading…" text) is visible

### Requirement: Error state is shown on fetch failure
If `GET /api/users` returns a non-2xx response (other than 401, which is handled by the API client interceptor), the page SHALL display a human-readable error message.

#### Scenario: Error message appears on API failure
- **WHEN** `GET /api/users` returns a 500
- **THEN** an error message is displayed and the list is not rendered
