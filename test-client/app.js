// SignalR connection
let connection = null;
let authToken = null;

// DOM elements
const userIdInput = document.getElementById('userId');
const roleInput = document.getElementById('role');
const getTokenButton = document.getElementById('getToken');
const tokenStatusElement = document.getElementById('tokenStatus');
const statusElement = document.getElementById('status');
const connectButton = document.getElementById('connect');
const disconnectButton = document.getElementById('disconnect');
const eventTypeInput = document.getElementById('eventType');
const eventDataFields = document.getElementById('eventDataFields');
const addFieldButton = document.getElementById('addField');
const jsonPreview = document.getElementById('jsonPreview');
const sendEventButton = document.getElementById('sendEvent');
const notificationList = document.getElementById('notificationList');
const filterFieldInput = document.getElementById('filterField');
const filterValueInput = document.getElementById('filterValue');
const eventTypesInput = document.getElementById('eventTypes');
const saveConfigButton = document.getElementById('saveConfig');
const connectGroupsInput = document.getElementById('connectGroups');
const loadHistoryButton = document.getElementById('loadHistory');
const historyList = document.getElementById('historyList');

// Get authentication token
async function getToken() {
    const userId = userIdInput.value.trim();
    const role = roleInput.value.trim();

    if (!userId || !role) {
        tokenStatusElement.textContent = 'Please enter both User ID and Role';
        return;
    }

    try {
        const response = await fetch('https://localhost:5000/api/auth/token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ userId, role })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        authToken = data.token;
        console.log('Your token:', authToken);
        tokenStatusElement.textContent = 'Token received';
        connectButton.disabled = false;
    } catch (err) {
        console.error('Error getting token:', err);
        tokenStatusElement.textContent = 'Failed to get token';
    }
}

// Connect to SignalR hub
async function connect() {
    try {
        console.log('Starting connection...');
        
        // Get groups from connectGroups input
        const groups = connectGroupsInput.value
            .split(',')
            .map(g => g.trim())
            .filter(g => g);
        
        // Add groups to the connection URL
        const url = new URL('https://localhost:5000/notificationHub');
        if (groups.length > 0) {
            url.searchParams.append('groups', groups.join(','));
        }
        
        // Update the connection URL
        connection = new signalR.HubConnectionBuilder()
            .withUrl(url.toString(), {
                accessTokenFactory: () => authToken,
                transport: signalR.HttpTransportType.WebSockets,
                skipNegotiation: true,
                logger: signalR.LogLevel.Debug
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount === 0) {
                        return 0;
                    }
                    return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                }
            })
            .configureLogging(signalR.LogLevel.Debug)
            .build();

        // Set up event handlers
        setupConnectionHandlers();

        // Start the connection
        await connection.start();
        console.log('Connected to SignalR hub');
        statusElement.textContent = 'Connected';
        updateButtonStates(true);
    } catch (err) {
        console.error('Error connecting to SignalR hub:', err);
        statusElement.textContent = 'Connection failed';
        
        // Log detailed error information
        if (err.message) {
            console.error('Error message:', err.message);
        }
        if (err.stack) {
            console.error('Error stack:', err.stack);
        }
        
        // Try to reconnect after a delay
        setTimeout(() => {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                console.log('Attempting to reconnect...');
                connect();
            }
        }, 5000);
    }
}

// Set up connection event handlers
function setupConnectionHandlers() {
    connection.onreconnecting(error => {
        console.log('Reconnecting:', error);
        statusElement.textContent = 'Reconnecting...';
        updateButtonStates(false);
    });

    connection.onreconnected(connectionId => {
        console.log('Reconnected:', connectionId);
        statusElement.textContent = 'Connected';
        updateButtonStates(true);
    });

    connection.onclose(error => {
        console.log('Connection closed:', error);
        statusElement.textContent = 'Disconnected';
        updateButtonStates(false);
    });

    // Handle notifications
    connection.on("ReceiveNotification", (notification) => {
        console.log("Received notification:", notification);
        displayNotification(notification);
    });

    // Handle history
    connection.on("ReceiveHistory", (history) => {
        console.log("Received history:", history);
        displayHistory(history);
    });

    // Log any other messages
    connection.on('*', (message) => {
        console.log('Received message:', message);
    });
}

// Initialize SignalR connection
function initializeConnection() {
    if (!authToken) {
        statusElement.textContent = 'Please get a token first';
        return;
    }

    connect();
}

// Disconnect from SignalR hub
async function disconnect() {
    try {
        await connection.stop();
        statusElement.textContent = 'Disconnected';
        updateButtonStates(false);
    } catch (err) {
        console.error('Error disconnecting from SignalR hub:', err);
    }
}

// Add new field to event data form
function addEventDataField() {
    const row = document.createElement('div');
    row.className = 'event-data-row';
    row.innerHTML = `
        <input type="text" class="key-input" placeholder="Key" />
        <input type="text" class="value-input" placeholder="Value" />
        <button type="button" class="btn-icon remove-field" title="Remove field">
            <i class="fa-solid fa-trash"></i>
        </button>
    `;
    
    // Add remove field handler
    row.querySelector('.remove-field').addEventListener('click', () => {
        row.remove();
        updateJsonPreview();
    });
    
    // Add input change handlers
    row.querySelectorAll('input').forEach(input => {
        input.addEventListener('input', updateJsonPreview);
    });
    
    eventDataFields.appendChild(row);
}

// Update JSON preview
function updateJsonPreview() {
    const data = {};
    eventDataFields.querySelectorAll('.event-data-row').forEach(row => {
        const key = row.querySelector('.key-input').value.trim();
        const value = row.querySelector('.value-input').value.trim();
        if (key) {
            // Try to parse value as JSON if it looks like JSON
            try {
                if (value.startsWith('{') || value.startsWith('[')) {
                    data[key] = JSON.parse(value);
                } else if (value === 'true') {
                    data[key] = true;
                } else if (value === 'false') {
                    data[key] = false;
                } else if (!isNaN(value)) {
                    data[key] = Number(value);
                } else {
                    data[key] = value;
                }
            } catch {
                data[key] = value;
            }
        }
    });
    
    jsonPreview.textContent = JSON.stringify(data, null, 2);
}

// Get event data as JSON string
function getEventData() {
    const data = {};
    eventDataFields.querySelectorAll('.event-data-row').forEach(row => {
        const key = row.querySelector('.key-input').value.trim();
        const value = row.querySelector('.value-input').value.trim();
        if (key) {
            try {
                if (value.startsWith('{') || value.startsWith('[')) {
                    data[key] = JSON.parse(value);
                } else if (value === 'true') {
                    data[key] = true;
                } else if (value === 'false') {
                    data[key] = false;
                } else if (!isNaN(value)) {
                    data[key] = Number(value);
                } else {
                    data[key] = value;
                }
            } catch {
                data[key] = value;
            }
        }
    });
    return JSON.stringify(data);
}

// Send test event
async function sendEvent() {
    const eventType = eventTypeInput.value.trim();
    const eventData = getEventData();

    // Get target groups from input
    const targetGroupsInput = document.getElementById('targetGroups');
    const targetGroups = targetGroupsInput.value
        .split(',')
        .map(group => group.trim())
        .filter(group => group);

    // Get save to history preference
    const saveToHistoryInput = document.getElementById('saveToHistory');
    const saveToHistory = saveToHistoryInput.checked;

    try {
        const response = await fetch('https://localhost:5000/api/events', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                type: eventType,
                data: eventData,
                saveToHistory: saveToHistory,
                targetGroups: targetGroups
            })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(JSON.stringify(errorData, null, 2));
        }

        console.log('Event sent successfully');
    } catch (err) {
        console.error('Error sending event:', err);
        alert('Failed to send event: ' + err.message);
    }
}

// Display notification in the UI
function displayNotification(notification) {
    const notificationItem = document.createElement('div');
    notificationItem.className = 'notification-item';
    notificationItem.innerHTML = `
        <div class="timestamp">${new Date(notification.timestamp).toLocaleString()}</div>
        <div class="type">${notification.type}</div>
        <div class="data">${formatJson(notification.data)}</div>
    `;
    notificationList.insertBefore(notificationItem, notificationList.firstChild);
}

// Display history in the UI
function displayHistory(history) {
    const historyList = document.getElementById('historyList');
    if (!historyList) return;

    // Clear existing history
    historyList.innerHTML = '';

    if (!history || history.length === 0) {
        historyList.innerHTML = '<div class="no-events">No historical events found</div>';
        return;
    }

    // Add each event to the history list
    history.forEach(event => {
        const historyItem = document.createElement('div');
        historyItem.className = 'history-item';
        historyItem.innerHTML = `
            <div class="timestamp">${new Date(event.timestamp).toLocaleString()}</div>
            <div class="type">${event.type}</div>
            <div class="data">${formatJson(event.data)}</div>
        `;
        historyList.appendChild(historyItem);
    });
}

// Format JSON for display
function formatJson(json) {
    try {
        const obj = typeof json === 'string' ? JSON.parse(json) : json;
        return JSON.stringify(obj, null, 2);
    } catch (e) {
        return json;
    }
}

// Update button states based on connection status
function updateButtonStates(isConnected) {
    connectButton.disabled = isConnected;
    disconnectButton.disabled = !isConnected;
    sendEventButton.disabled = !isConnected;
}

// Load event history
async function loadHistory() {
    try {
        const response = await fetch('https://localhost:5000/api/events/history', {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const events = await response.json();
        displayHistory(events);
    } catch (err) {
        console.error('Error loading history:', err);
        alert('Failed to load event history');
    }
}

// Event listeners
getTokenButton.addEventListener('click', getToken);
connectButton.addEventListener('click', () => {
    initializeConnection();
    connect();
});
disconnectButton.addEventListener('click', disconnect);
sendEventButton.addEventListener('click', sendEvent);
loadHistoryButton.addEventListener('click', loadHistory);
addFieldButton.addEventListener('click', addEventDataField);

// Initial setup
connectButton.disabled = true;
addEventDataField(); // Add initial field 