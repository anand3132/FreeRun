const express = require('express');
const config = require('./config/env');
const authService = require('./services/auth');
const apiRoutes = require('./routes/api');

const app = express();
app.use(express.json());

// Register routes
app.use('/', apiRoutes);

// Start the server
app.listen(config.PORT, async () => {
    console.log(`✅ Server running at http://localhost:${config.PORT}`);
    try {
        await authService.getUnityAccessToken();
        console.log("🔑 Unity access token is ready.");
    } catch (err) {
        console.error("❌ Token fetch failed at startup:", err.message);
    }
});