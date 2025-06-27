const axios = require('axios');
const config = require('../config/env');
const { generateAllocationId } = require('../utils/helpers');
const authService = require('./auth');

const activeAllocations = {}; // sessionId → { localId, unityId, timestamp }

module.exports = {
    allocateServer: async (sessionId) => {
        if (!sessionId) {
            throw new Error('Missing sessionId in request.');
        }

        const token = await authService.getUnityAccessToken();
        console.log(`Access Token: ${token}`);
        console.log(`--------------------------------------------------------------------------------------------------------------------`);
        console.log(`--------------------------------------------------------------------------------------------------------------------`);
        const localAllocationId = generateAllocationId();
        console.log(`Allocation ID: ${localAllocationId}`);
        console.log(`--------------------------------------------------------------------------------------------------------------------`);
        const allocationPayload = {
            allocationId: localAllocationId,
            buildConfigurationId: parseInt(config.UNITY_BUILD_CONFIG_ID),
            payload: "string",
            regionId: config.UNITY_REGION_ID,
            restart: true,
            sessionId
        };

        const response = await axios.post(
            `https://multiplay.services.api.unity.com/v1/allocations/projects/${config.UNITY_PROJECT_ID}/environments/${config.UNITY_ENVIRONMENT_ID}/fleets/${config.UNITY_FLEET_ID}/allocations`,
            allocationPayload,
            {
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        const allocationIdFromUnity = response.data.allocationId;

        activeAllocations[sessionId] = {
            localId: localAllocationId,
            unityId: allocationIdFromUnity,
            timestamp: Date.now()
        };

        return {
            sessionId,
            localAllocationId,
            unityAllocationId: allocationIdFromUnity
        };
    },

    getAllocation: (sessionId) => {
        const entry = activeAllocations[sessionId];
        if (!entry) {
            throw new Error("No allocation found for this session.");
        }

        return {
            sessionId,
            localAllocationId: entry.localId,
            unityAllocationId: entry.unityId,
            allocatedAt: new Date(entry.timestamp).toISOString()
        };
    }
};