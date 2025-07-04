const express = require('express');
const router = express.Router();
const allocationService = require('../services/allocation');
const authService = require('../services/auth');

router.post('/allocate-server', async (req, res) => {
    const { sessionId } = req.body;
    
    try {
        const result = await allocationService.allocateServer(sessionId);
        res.json(result);
    } catch (err) {
        console.error("❌ Allocation failed:", err.message);
        res.status(500).json({ error: err.message || 'Failed to allocate server.' });
    }
});

router.get('/allocations/:sessionId', (req, res) => {
    const { sessionId } = req.params;
    
    try {
        const result = allocationService.getAllocation(sessionId);
        res.json(result);
    } catch (err) {
        res.status(404).json({ error: err.message });
    }
});

router.get('/get-token', async (req, res) => {
    try {
        const token = await authService.getUnityAccessToken();
        res.json({ accessToken: token });
    } catch (err) {
        console.error("❌ Token fetch failed:", err.message);
        res.status(500).json({ error: 'Failed to get access token.' });
    }
});

module.exports = router;