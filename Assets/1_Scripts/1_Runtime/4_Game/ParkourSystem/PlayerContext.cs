using UnityEngine;

namespace RedGaint.Games.ParkourSystem
{
    [System.Serializable]
    public class PlayerContext
    {
        // Core components
        public Animator Animator;
        public CharacterController Controller;
        public PlayerInputReader Input;

        // Movement values
        public float MoveSpeed = 5f;
        public float JumpForce = 7f;
        public float Gravity = -9.81f;
        [SerializeField]
        public LayerMask GroundLayer;

        // State-related
        public bool CanDoubleJump;
        public bool IsGrounded;
        public float CoyoteTimer;

        // Position & Physics
        public Vector3 VerticalVelocity = Vector3.zero;
        public Vector3 MoveInputDirection = Vector3.zero;

        // Ground detection
        public Transform GroundCheck;
        public float GroundCheckRadius = 0.2f;

        // References
        public Transform Transform { get; set; }
        public AnimatorControllerWrapper AnimatorWrapper { get; private set; }
        [SerializeField]
        public EnvironmentScanner EnvironmentScanner;

        public void Init(PlayerStateMachine fsm)
        {
            // Rebuild wrapper if animator is set
            if (Animator != null)
                AnimatorWrapper = new AnimatorControllerWrapper(Animator);

            // Transform reference fallback
            if (Transform == null)
                Transform = fsm.transform;

            // Assign EnvironmentScanner if not set
            if (EnvironmentScanner == null)
                EnvironmentScanner = fsm.GetComponent<EnvironmentScanner>();
        }

        public void CheckGrounded()
        {
            if (GroundCheck == null)
            {
                Debug.LogWarning("GroundCheck transform is not set in PlayerContext.");
                return;
            }

            IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, GroundLayer);
            Debug.Log($"[PlayerContext] IsGrounded: {IsGrounded} at position {GroundCheck.position}");

            // Draw a debug ray downward to visualize the ground check
            Debug.DrawRay(GroundCheck.position, Vector3.down * GroundCheckRadius, IsGrounded ? Color.green : Color.red, 0.1f);

            if (IsGrounded && VerticalVelocity.y < 0)
                VerticalVelocity.y = -2f; // Keeps grounded
        }
    }
} 