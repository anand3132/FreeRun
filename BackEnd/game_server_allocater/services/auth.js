const axios = require('axios');
const config = require('../config/env');
const { generateAllocationId } = require('../utils/helpers');

let cachedToken = null;
let tokenExpiry = null;

module.exports = {
    getUnityAccessToken: async () => {
        const now = Date.now();
        if (cachedToken && tokenExpiry && now < tokenExpiry) {
            return cachedToken;
        }

        const url = `https://services.api.unity.com/auth/v1/token-exchange` +
                    `?projectId=${config.UNITY_PROJECT_ID}` +
                    `&environmentId=${config.UNITY_ENVIRONMENT_ID}`;

        const credentials = `${config.UNITY_CLIENT_ID}:${config.UNITY_CLIENT_SECRET}`;
        const encoded = Buffer.from(credentials).toString('base64');

        const response = await axios.post(url, {}, {
            headers: {
                Authorization: `Basic ${encoded}`,
                'Content-Type': 'application/json'
            }
        });

        cachedToken = response.data.accessToken;
        tokenExpiry = now + (response.data.expires_in * 1000) - 10000;

        return cachedToken;
    }
};