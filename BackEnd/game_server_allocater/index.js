// index.js
require('dotenv').config();               // Load variables from .env file
const express = require('express');       // Lightweight web framework
const axios = require('axios');           // HTTP client

const app = express();
app.use(express.json());                  // To parse incoming JSON bodies

const PORT = process.env.PORT || 3000;

// In-memory cache for access token and allocation mapping
let cachedToken = null;
let tokenExpiry = null;

let activeAllocations = {}; // Stores lobbyId → { allocationId, timestamp }

/**
 * Fetches and caches Unity access token using client credentials.
 */
async function getUnityAccessToken() {
    const now = Date.now();

    // Return cached token if it's still valid
    if (cachedToken && tokenExpiry && now < tokenExpiry) {
        return cachedToken;
    }

    console.log("🔐 Requesting Unity access token via token-exchange...");

    const url = `https://services.api.unity.com/auth/v1/token-exchange` +
                `?projectId=${process.env.UNITY_PROJECT_ID}` +
                `&environmentId=${process.env.UNITY_ENVIRONMENT_ID}`;

    const credentials = `${process.env.UNITY_CLIENT_ID}:${process.env.UNITY_CLIENT_SECRET}`;
    const encoded = Buffer.from(credentials).toString('base64');

    const response = await axios.post(url, {}, {
        headers: {
            Authorization: `Basic ${encoded}`,
            'Content-Type': 'application/json'
        }
    });

    cachedToken = response.data.access_token;
    tokenExpiry = now + (response.data.expires_in * 1000) - 10000; // renew 10s early

    console.log("✅ Access token received.");
    return cachedToken;
}

/**
 * Allocates a Unity Multiplay server for the given lobby.
 */
app.post('/allocate-server', async (req, res) => {
    const { lobbyId } = req.body;

    if (!lobbyId) {
        return res.status(400).json({ error: 'Missing lobbyId in request.' });
    }

    console.log(`📥 Received allocation request for lobby: ${lobbyId}`);

    try {
        const token = await getUnityAccessToken();

        const payload = {
            sessionId: lobbyId
        };

        const response = await axios.post(
            `https://multiplay.services.api.unity.com/v1/allocations/projects/${process.env.UNITY_PROJECT_ID}/environments/${process.env.UNITY_ENVIRONMENT_ID}/fleets/${process.env.UNITY_FLEET_ID}/allocations`,
            payload,
            {
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        const allocationId = response.data.allocationId;
        console.log("✅ Server allocated. Allocation ID:", allocationId);

        // Store the allocation for future reference
        activeAllocations[lobbyId] = {
            allocationId: allocationId,
            timestamp: Date.now()
        };

        res.json({ lobbyId, allocationId });

    } catch (err) {
        console.error("❌ Allocation failed:", err.response?.data || err.message);
        res.status(500).json({ error: 'Failed to allocate server.' });
    }
});

/**
 * Retrieves stored allocation ID for a lobby.
 */
app.get('/allocations/:lobbyId', (req, res) => {
    const { lobbyId } = req.params;
    const entry = activeAllocations[lobbyId];

    if (!entry) {
        return res.status(404).json({ error: "No allocation found for this lobby." });
    }

    res.json({
        lobbyId,
        allocationId: entry.allocationId,
        allocatedAt: new Date(entry.timestamp).toISOString()
    });
});

/**
 * Test route: Manually get the Unity token.
 */
app.get('/get-token', async (req, res) => {
    try {
        const token = await getUnityAccessToken();
        res.json({ access_token: token });
    } catch (err) {
        console.error("❌ Token fetch failed:", err.response?.data || err.message);
        res.status(500).json({ error: 'Failed to get access token.' });
    }
});

/**
 * Start the backend server
 */
app.listen(PORT, async () => {
    console.log(`✅ Server running at http://localhost:${PORT}`);
    try {
        const token = await getUnityAccessToken();
        console.log("🔑 Initial access token fetched successfully.");
    } catch (err) {
        console.error("❌ Error fetching token at startup:", err.response?.data || err.message);
    }
});
