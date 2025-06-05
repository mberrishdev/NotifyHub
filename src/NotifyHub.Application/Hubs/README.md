# NotifyHub SignalR Hubs

This directory contains the SignalR hubs that handle real-time communication in the NotifyHub system.

## NotificationHub

The main hub for handling real-time notifications and group management.

### Methods

#### Server Methods

1. **OnConnectedAsync**
   ```csharp
   public override async Task OnConnectedAsync()
   ```
   - Handles new client connections
   - Processes group subscriptions from query parameters
   - Called automatically when a client connects
   - Requires JWT authentication

2. **OnDisconnectedAsync**
   ```csharp
   public override async Task OnDisconnectedAsync(Exception? exception)
   ```
   - Handles client disconnections
   - Cleans up group subscriptions
   - Called automatically when a client disconnects
   - Handles both normal disconnects and errors

3. **SubscribeToGroups**
   ```csharp
   public async Task SubscribeToGroups(List<string> groups)
   ```
   - Allows clients to subscribe to specific groups
   - Updates group membership in real-time
   - Requires authentication
   - Returns success/failure status

4. **UnsubscribeFromGroups**
   ```csharp
   public async Task UnsubscribeFromGroups(List<string> groups)
   ```
   - Allows clients to unsubscribe from specific groups
   - Removes group membership
   - Requires authentication
   - Returns success/failure status

5. **ConfigureUserNotifications**
   ```csharp
   public async Task ConfigureUserNotifications(UserNotificationConfig config)
   ```
   - Configures user-specific notification settings
   - Updates notification preferences
   - Requires authentication
   - Supports filtering and event type preferences

### Client Methods

1. **ReceiveNotification**
   ```javascript
   connection.on("ReceiveNotification", (notification) => {
       // Handle incoming notification
   });
   ```
   - Receives real-time notifications
   - Contains notification type, data, and metadata
   - Triggered when a matching notification is sent

2. **ReceiveHistory**
   ```javascript
   connection.on("ReceiveHistory", (history) => {
       // Handle history updates
   });
   ```
   - Receives event history updates
   - Contains chronological list of events
   - Triggered on history requests

### Usage Examples

#### Connecting to the Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => authToken
    })
    .withAutomaticReconnect()
    .build();

await connection.start();
```

#### Subscribing to Groups
```javascript
// Subscribe to specific groups
await connection.invoke("SubscribeToGroups", ["group1", "group2"]);

// Unsubscribe from groups
await connection.invoke("UnsubscribeFromGroups", ["group1"]);
```

#### Configuring Notifications
```javascript
const config = {
    filterField: "priority",
    filterValue: "high",
    eventTypes: ["task.created", "order.completed"]
};

await connection.invoke("ConfigureUserNotifications", config);
```

### Security

- All hub methods require JWT authentication
- Group access is controlled by role-based permissions
- Input validation is performed on all methods
- Connection state is tracked and managed

### Error Handling

The hub implements comprehensive error handling:
- Connection errors are logged and reported
- Method invocation errors return detailed messages
- Authentication failures are properly handled
- Group subscription errors are caught and reported

### Best Practices

1. **Connection Management**
   - Use automatic reconnection
   - Handle connection state changes
   - Implement proper error handling
   - Monitor connection health

2. **Group Management**
   - Subscribe only to necessary groups
   - Unsubscribe when no longer needed
   - Handle group subscription errors
   - Monitor group membership

3. **Notification Configuration**
   - Set appropriate filters
   - Configure event type preferences
   - Update configuration as needed
   - Handle configuration errors

### Dependencies

- Microsoft.AspNetCore.SignalR
- Microsoft.AspNetCore.Authentication.JwtBearer
- NotifyHub.Application.Interfaces
- NotifyHub.Application.Models 