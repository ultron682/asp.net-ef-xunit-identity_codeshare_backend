A backend that provides collaborative code writing with other people. 

The frontend is located at: https://github.com/ultron682/react-signalR_codeshare_frontend

A quick look at the functionalities:

![intro](https://github.com/user-attachments/assets/dfd341d9-ebf8-4b43-a211-25e953620973)

# CodeShareBackend
CodeShareBackend is a backend application developed using ASP.NET Core that supports user authentication, code snippet sharing, collaborative code editing, and account management functionalities. It provides a RESTful API for managing user accounts, code snippets, and handles email-based operations like user registration, login, and email confirmation.

## Technologies:
- **ASP.NET Core 8 Web API:** Core framework for building RESTful services.
- **Entity Framework Core:** For interacting with the database and managing entities.
- **MailKit:** For sending emails (e.g., confirmation emails).
- **Microsoft Identity:** For user authentication, password management, and security.
- **SignalR:** For real-time communication between clients for collaborative code editing.
- **JWT (JSON Web Tokens):** For user authentication and authorization.
- **Swagger:** API documentation and testing interface.


## Features

### 1. User Authentication and Authorization
- User Registration: Allows users to register by providing an email, username, and password.
- Login: Authenticated users can log in using their email and password.
- JWT Token Generation: Issues JWT tokens for secure API access after successful login.
- Email Confirmation: Sends a confirmation email to verify user registration.
- Authorization: Protects API endpoints using JWT-based authorization.

### 2. Code Snippet Management
- Add Code Snippet: Users can create and save code snippets with various programming languages.
- Delete Code Snippet: Users can delete their own code snippets by unique identifier (UniqueId).
- List Code Snippets: Fetches all code snippets created by the authenticated user.

### 3. Collaborative Code Editing with SignalR (Real-time Updates)
CodeShareHub is a SignalR Hub that allows users to collaboratively edit and update code snippets in real-time. It provides the following features:

- Join Code Snippet Room: Users can join a unique document editing session, represented by a code snippet's unique identifier.
- Real-time Code Updates: Users can collaboratively edit a code snippet in real-time, with changes immediately reflected to other users in the session.
- Document Management: Supports multiple users editing the same code snippet while ensuring only authorized users can make changes.
- Snippet Property Changes: Owners of a snippet can update properties like the selected programming language or set the snippet as read-only.
- Connection Management: Manages user connections and group memberships to keep track of which users are editing which documents.

### 4. Account Management
- Account Information: Retrieve user profile details such as email and username.
- Update Username: Users can update their display name (nickname).
#### Email-Based Operations:
- Send a confirmation email after registration.
- Handle email confirmation links for account verification.

### 5. Email Service Integration
- The backend uses MailKit for sending emails.
- It handles operations like sending account verification emails.
- Configurable SMTP settings through appsettings.json.

### 6. Security
- JWT Authentication: Securely manages user sessions with token-based authentication.
- Password Hashing: Protects user credentials using Identity framework.

## Models
- UserCodeShare: Inherits from IdentityUser, stores user-specific data.
- CodeSnippet: Represents individual code snippets with language, user association, and expiration information.
- ProgLanguage: Stores programming languages associated with snippets.
- MailData: Stores email information for sending emails via MailKit.
- MailSettings: Holds the SMTP server settings.

## API Endpoints

### Authentication and User Management:

- POST /account/register: Register a new user.

- POST /account/login: User login and JWT token generation.

- GET /account: Retrieve authenticated user's profile info.

- PATCH /account/nickname: Update user's nickname.

### Code Snippet Management:
- POST /snippet: Create a new code snippet.

- DELETE /snippet/{UniqueId}: Delete a snippet by unique ID.

- GET /snippet: Get all code snippets for the authenticated user.

### Real-time Collaboration with SignalR (CodeShareHub):
- JoinToDocument: Join a real-time code editing session for a specific snippet.

- PushUpdate: Send real-time code changes to other connected users.

- ChangeCodeSnippetProperty: Update code snippet properties such as programming language or read-only status.

### Email Verification:
- POST /account/send-confirmation-email: Sends an email confirmation link.

- GET /account/confirm: Confirms user email using a token.

### The API is documented using Swagger. Access the Swagger interface at:
http://localhost:5555/swagger
