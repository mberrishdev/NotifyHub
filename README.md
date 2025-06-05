# NotifyHub

NotifyHub is a real-time notification system built with ASP.NET Core and SignalR, designed to provide efficient and scalable group-based notifications. It's perfect for applications that need to send targeted notifications to specific user groups or roles.

![NotifyHub Demo](docs/demo.gif)

## 🌟 Features

- **Real-time Notifications**: Instant delivery using SignalR
- **Group-based Targeting**: Send notifications to specific user groups
- **Event History**: Track and retrieve notification history
- **Role-based Access**: Secure notifications with role-based permissions
- **Scalable Architecture**: Built with performance and scalability in mind
- **Modern UI**: Clean and responsive user interface
- **Easy Integration**: Simple API for sending and receiving notifications

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+ (for test client)
- Modern web browser

### Installation

1. Clone the repository:
```bash
git clone https://github.com/mberrishdev/NotifyHub.git
cd NotifyHub
```

2. Build and run the server:
```bash
dotnet build
dotnet run --project src/NotifyHub.Api
```

3. Start the test client:
```bash
cd test-client
npm install
npm start
```

4. Open your browser and navigate to `https://localhost:5000`

## 📖 Usage Guide

### Sending Notifications

```csharp
// Create a notification
var notification = new Notification
{
    Type = "UserJoined",
    Data = new { userId = "123", username = "John" },
    TargetGroups = new List<string> { "group1", "group2" }
};

// Send the notification
await _notificationService.SendNotificationAsync(notification);
```

### Receiving Notifications

```javascript
// Connect to the hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Handle incoming notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
});

// Connect with specific groups
await connection.start();
await connection.invoke("ConnectWithGroups", ["group1", "group2"]);
```

### Group Management

```csharp
// Subscribe to groups
await _groupService.SubscribeToGroupsAsync(userId, connectionId, groups);

// Unsubscribe from groups
await _groupService.UnsubscribeFromGroupsAsync(userId, connectionId, groups);
```

## 🏗️ Architecture

NotifyHub follows a clean architecture pattern:

```
NotifyHub/
├── src/
│   ├── NotifyHub.Api/           # API layer
│   ├── NotifyHub.Application/   # Business logic
│   └── NotifyHub.Domain/        # Domain models
└── test-client/                 # Test client application
```

### Key Components

- **NotificationHub**: SignalR hub for real-time communication
- **GroupSubscriptionService**: Manages group subscriptions
- **EventHistoryService**: Handles notification history
- **NotificationService**: Core notification logic

## 🔒 Security

- JWT-based authentication
- Role-based authorization
- Secure WebSocket connections
- Input validation and sanitization

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

## 📝 API Documentation

### Authentication

```http
POST /api/auth/token
Content-Type: application/json

{
    "userId": "string",
    "role": "string"
}
```

### Notifications

```http
POST /api/notifications
Authorization: Bearer {token}
Content-Type: application/json

{
    "type": "string",
    "data": object,
    "targetGroups": ["string"]
}
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [ASP.NET Core](https://dotnet.microsoft.com/)
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)
- [Font Awesome](https://fontawesome.com/)
- [Inter Font](https://rsms.me/inter/)

## 📞 Support

For support, email support@notifyhub.com or open an issue in the repository.

---

Made with ❤️ by [Your Name]

## Group-Based Notifications

NotifyHub now uses a flexible group-based notification system. Each event can be sent to specific groups, and users can subscribe to groups dynamically.

### Example Usage

1. Send a notification to specific groups:
```json
{
  "type": "task.created",
  "data": "{\"priority\": \"high\", \"message\": \"Important task!\"}",
  "targetGroups": ["admin", "developers"],
  "saveToHistory": true
}
```

2. Send a notification to all groups:
```json
{
  "type": "system.update",
  "data": "{\"version\": \"1.0.0\", \"message\": \"System updated\"}",
  "saveToHistory": true
}
```

## Event Filtering

The system supports filtering events based on their data. Filters can work with:

- JSON data
- Dictionary data
- Object properties

### Filter Examples

1. Filter by status:
```json
{
  "type": "order.created",
  "data": "{\"status\": \"completed\", \"orderId\": \"12345\"}"
}
```

2. Filter by numeric value:
```json
{
  "type": "payment.received",
  "data": "{\"amount\": 100.50, \"currency\": \"USD\"}"
}
```

## API Endpoints

### Authentication
- `POST /api/auth/token` - Get JWT token

### Events
- `POST /api/events` - Send a new event

## SignalR Hub

The application uses a SignalR hub for real-time communication:

- Hub URL: `/notificationHub`
- Client method: `ReceiveNotification`
- Authentication: JWT token required

## Security

- JWT-based authentication
- Role-based access control
- Secure WebSocket connections
- CORS configuration for development

## Development

### Project Structure
```
NotifyHub/
├── src/
│   ├── NotifyHub.Api/           # API project
│   ├── NotifyHub.Application/   # Application layer
│   └── NotifyHub.Infrastructure/# Infrastructure layer
└── test-client/                 # Test client
```

### Running Tests
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## User Notification Configuration

Users can configure their notification preferences through the test client interface. The configuration includes:

- `FilterField`: Field to filter incoming notifications
- `FilterValue`: Value to match against the filter field
- `EventTypes`: Comma-separated list of event types to receive

### Example User Configuration

```json
{
  "filterField": "priority",
  "filterValue": "high",
  "eventTypes": ["task.created", "order.completed", "system.alert"]
}
```

This configuration will:
- Only show notifications where the `priority` field equals `high`
- Only receive notifications of type `task.created`, `order.completed`, or `system.alert`

## Testing the System

### Step 1: Connect to SignalR

1. Open `http://localhost:5500` in your browser
2. In the Authentication section:
   - Enter a User ID (e.g., "1")
   - Enter a Role (e.g., "admin")
   - Click "Get Token"
   - You should see "Token received" in the status
3. Click "Connect" to establish the SignalR connection
   - You should see "Connected" in the status

### Step 2: Configure User Notifications

1. In the Notification Configuration section:
   - Enter a Filter Field (e.g., "priority")
   - Enter a Filter Value (e.g., "high")
   - Enter Event Types (e.g., "task.created,order.completed")
   - Click "Save Configuration"
   - You should see a success message

### Step 3: Send Test Events

In the test client:

1. For group-specific notification with priority:
   - Event Type: `task.created`
   - Event Data: `{"priority": "high", "message": "Important task!"}`
   - Target Groups: `admin,developers`

2. For broadcast notification:
   - Event Type: `system.broadcast`
   - Event Data: `{"message": "System broadcast!"}`

The notifications will now include:
- Timestamp
- Event Type
- Username of the sender
- Event Data

## Troubleshooting

1. If you can't connect to SignalR:
   - Check that the API is running
   - Verify you have a valid token
   - Check the browser console for errors

2. If you don't receive notifications:
   - Verify you're connected to SignalR
   - Check that you've subscribed to the correct groups
   - Ensure the event type matches your configuration
   - Check the API logs for any errors

3. If you get CORS errors:
   - Make sure you're accessing the test client through `http://localhost:5500`
   - Verify the API is running on `https://localhost:5000`

