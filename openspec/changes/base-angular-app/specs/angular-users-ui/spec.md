## ADDED Requirements

### Requirement: UsersComponent displays the list of users
The system SHALL provide a `UsersComponent` under `src/app/users/` that fetches the list of users from `UsersApiService.getUsers()` when initialized and renders the results as a list or table showing at minimum each user's username.

#### Scenario: Users are returned by the API
- **WHEN** the `UsersComponent` initializes and `GET /api/users` returns a non-empty array
- **THEN** each user's username is displayed in the list

#### Scenario: UsersComponent is accessible at /users
- **WHEN** an authenticated user navigates to `/users`
- **THEN** the `UsersComponent` is rendered and the users list is fetched

### Requirement: Loading state shown while fetch is in flight
The system SHALL display a loading indicator (text or spinner) from the moment the fetch begins until the response is received.

#### Scenario: Fetch is in progress
- **WHEN** the `UsersComponent` has initiated a fetch and has not yet received a response
- **THEN** a loading indicator is visible to the user

### Requirement: Error state shown on API failure
The system SHALL display a human-readable error message when `GET /api/users` returns a non-401 error response. The error state SHALL not leave the user with a blank or broken page.

#### Scenario: API returns a server error
- **WHEN** `GET /api/users` returns a 5xx response
- **THEN** an error message is displayed explaining that users could not be loaded

### Requirement: Empty state shown when no users exist
The system SHALL display a meaningful empty-state message when `GET /api/users` returns an empty array.

#### Scenario: API returns an empty array
- **WHEN** `GET /api/users` returns `[]`
- **THEN** a message such as "No users found" is displayed

### Requirement: Navigation link to /users in app layout
The system SHALL include a navigation element (link or nav bar) visible on all authenticated pages that allows the user to navigate to `/users`.

#### Scenario: Nav link is present
- **WHEN** an authenticated user is on any page
- **THEN** a link navigating to `/users` is visible in the layout
