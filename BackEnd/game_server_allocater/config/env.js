require('dotenv').config();

module.exports = {
    PORT: process.env.PORT || 3000,
    UNITY_PROJECT_ID: process.env.UNITY_PROJECT_ID,
    UNITY_ENVIRONMENT_ID: process.env.UNITY_ENVIRONMENT_ID,
    UNITY_CLIENT_ID: process.env.UNITY_CLIENT_ID,
    UNITY_CLIENT_SECRET: process.env.UNITY_CLIENT_SECRET,
    UNITY_BUILD_CONFIG_ID: process.env.UNITY_BUILD_CONFIG_ID,
    UNITY_REGION_ID: process.env.UNITY_REGION_ID,
    UNITY_FLEET_ID: process.env.UNITY_FLEET_ID
};