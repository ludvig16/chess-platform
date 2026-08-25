# Chess Platform
A real-time multiplayer chess platform built with React and ASP.NET Core.

The project features a custom-built chessboard, real-time multiplayer communication using SignalR, and a server-side game clock implemented using a background worker and priority queue.

The backend acts as the authoritative source of game state. Moves and clock events are validated and processed server-side.

### Database
- Entity Framework Core
- PostgreSQL

Game state, players, moves, clocks, and game termination information are persisted in the database.

### REST API
The backend exposes a REST API for authentication, user management, and game management. Users can create an account and authenticate using JWTs.
```
POST /api/auth/register
POST /api/auth/login
```
The returned JWT is used to authenticate subsequent API requests and SignalR connections.

### Games
Games can be created through the REST API:
```
POST /api/games
```

The architecture deliberately uses both REST and SignalR, with each handling a different responsibility:
```
REST API
   │
   ├── Register
   ├── Login
   ├── Create game
   ├── Fetch users
   └── Fetch game history
       
SignalR
   │
   ├── Join game
   ├── Make move
   └── Receive real-time game state
```

### Real-Time Multiplayer
Games use SignalR to synchronize connected clients in real time.

When a player joins a game, their SignalR connection is added to a game-specific group:

```
game:{gameId}
```

Both players connected to the same game therefore receive updates through the same SignalR group.

Game events such as moves and game-state changes are broadcast to the group so that both clients remain synchronized.

### Server-Side Game Clock
The game clock is handled on the server rather than relying solely on client-side timers.

When a turn starts, the backend tracks when the turn expires and schedules a clock event.

The game clock uses a priority queue to process upcoming expiration events efficiently.

Using the server as the source of truth prevents clients from independently determining when a game has timed out.

The worker also verifies that a timeout event still corresponds to the current turn expiration before terminating the game. This prevents stale clock events from ending a game after the clock has already been updated.

### Configuration
The backend requires configuration for the database and authentication.
This is stored in an .env file at "src/ChessPlatform.Api"

The .env file includes the following:
```
ConnectionStrings__DefaultConnection=VALUE

JwtSettings__Issuer=VALUE
JwtSettings__Audience=VALUE
JwtSettings__Key=VALUE
```
