const crypto = require('crypto');

module.exports = {
    generateAllocationId: () => crypto.randomUUID()
};