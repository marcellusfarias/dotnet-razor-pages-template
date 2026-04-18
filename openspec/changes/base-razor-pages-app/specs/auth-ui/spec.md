## ADDED Requirements

### Requirement: Login page accepts credentials
The `Pages/Auth/Login.cshtml` page SHALL render a form with username and password fields and a submit button.

#### Scenario: Login form is rendered
- **WHEN** an unauthenticated user navigates to `/auth/login`
- **THEN** a form is displayed with `username`, `password` fields and a submit button

### Requirement: Successful login stores tokens in encrypted session cookie
On a successful `POST /api/auth/login` response, the page model SHALL store the access token and refresh token in an ASP.NET Core Data Protection-encrypted authentication cookie and redirect the user to `/`.

#### Scenario: Valid credentials result in authenticated session
- **WHEN** the user submits valid credentials on the login page
- **THEN** the API returns 200 with tokens, the cookie is set, and the user is redirected to `/`

### Requirement: Failed login displays an error message
When the API returns a non-2xx response (e.g., 401 Unauthorized), the login page SHALL redisplay with an error message and SHALL NOT store any cookie.

#### Scenario: Invalid credentials show error
- **WHEN** the user submits invalid credentials
- **THEN** the login page is redisplayed with a visible error message and no auth cookie is set

### Requirement: Unauthenticated users are redirected to login
Any Razor Page that requires authentication SHALL redirect unauthenticated users to `/auth/login`.

#### Scenario: Protected page redirects to login
- **WHEN** an unauthenticated user navigates to a protected page (e.g., `/users`)
- **THEN** they are redirected to `/auth/login`

### Requirement: Logout clears the session cookie
A logout action SHALL delete the authentication cookie and redirect the user to `/auth/login`.

#### Scenario: Logout clears session
- **WHEN** the user navigates to `/auth/logout` (or triggers a logout action)
- **THEN** the auth cookie is deleted and the user is redirected to `/auth/login`
