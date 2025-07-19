using UnityEngine;

namespace RedGaint.ParkourSystem
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

        public void Init(PlayerStateMachine fsm)
        {
            // Rebuild wrapper if animator is set
            if (Animator != null)
                AnimatorWrapper = new AnimatorControllerWrapper(Animator);

            // Transform reference fallback
            if (Transform == null)
                Transform = fsm.transform;
        }

        public void CheckGrounded()
        {
            if (GroundCheck == null)
            {
                Debug.LogWarning("GroundCheck transform is not set in PlayerContext.");
                return;
            }

            IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, GroundLayer);

            if (IsGrounded && VerticalVelocity.y < 0)
                VerticalVelocity.y = -2f; // Keeps grounded
        }
    }
}