using UnityEngine;

namespace RedGaint.Games.ParkourSystem
{
    public class EnvironmentScanner : MonoBehaviour
    {
        [Header("Beam Detection Settings")]
        public LayerMask beamLayer;
        public float beamCheckDistance = 0.5f;

        [Header("Ledge Detection Settings")]
        public LayerMask ledgeLayer;
        public float ledgeCheckDistance = 1.0f;
        public float ledgeCheckHeight = 1.5f;

        [Header("Climbable Detection Settings")]
        public LayerMask climbableLayer;
        public float climbCheckDistance = 1.0f;
        public float climbCheckHeight = 1.5f;

        [Header("Vaultable Detection Settings")]
        public LayerMask vaultableLayer;
        public float vaultCheckDistance = 1.0f;
        public float vaultCheckHeight = 1.0f;

        // Checks if the player is currently on a beam (balance surface)
        public bool IsOnBeam(Transform groundCheck)
        {
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, beamCheckDistance, beamLayer))
            {
                // Optionally check tag or component here
                return true;
            }
            return false;
        }

        // Checks if there is a ledge in front of the player
        public bool IsNearLedge(Transform origin)
        {
            RaycastHit hit;
            Vector3 start = origin.position + Vector3.up * ledgeCheckHeight;
            if (Physics.Raycast(start, origin.forward, out hit, ledgeCheckDistance, ledgeLayer))
            {
                // Optionally check tag or component here
                return true;
            }
            return false;
        }

        // Checks if there is a climbable surface in front of the player
        public bool IsNearClimbable(Transform origin)
        {
            RaycastHit hit;
            Vector3 start = origin.position + Vector3.up * climbCheckHeight;
            if (Physics.Raycast(start, origin.forward, out hit, climbCheckDistance, climbableLayer))
            {
                // Optionally check tag or component here
                return true;
            }
            return false;
        }

        // Checks if there is a vaultable obstacle in front of the player
        public bool IsVaultableAhead(Transform origin)
        {
            RaycastHit hit;
            Vector3 start = origin.position + Vector3.up * vaultCheckHeight;
            if (Physics.Raycast(start, origin.forward, out hit, vaultCheckDistance, vaultableLayer))
            {
                // Optionally check tag or component here
                return true;
            }
            return false;
        }

        // Add more methods for other environment checks as needed
    }
} 