using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles Idle, Walk, Run, Sprint, Crouch logic
    public class GroundedState : PlayerState
    {
        public GroundedState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Set animator parameters for grounded
            if (context.AnimatorWrapper != null)
            {
                context.AnimatorWrapper.SetGrounded(true);
                context.AnimatorWrapper.SetFalling(false);
            }
            context.CanDoubleJump = true;
            context.CoyoteTimer = 0.2f; // Grace period after landing
        }

        public override void Update()
        {
            // Check if still grounded
            context.CheckGrounded();
            if (!context.IsGrounded)
            {
                fsm.ChangeState(new FallingState(fsm, context));
                return;
            }

            // Handle jump input
            if (context.Input != null && context.Input.JumpPressed)
            {
                fsm.ChangeState(new JumpingState(fsm, context));
                return;
            }

            // Handle crouch input (if implemented)
            if (context.Input != null && context.Input.CrouchPressed)
            {
                fsm.ChangeState(new CrouchingState(fsm, context));
                return;
            }

            // Camera-relative movement
            Vector2 moveInput = context.Input != null ? context.Input.MoveInput : Vector2.zero;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveDirection = Vector3.zero;
            if (input.sqrMagnitude > 0.01f)
            {
                // Camera-relative
                Transform cam = Camera.main != null ? Camera.main.transform : null;
                if (cam != null)
                {
                    Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 camRight = cam.right;
                    moveDirection = (camForward * input.z + camRight * input.x).normalized;

                    // Smoothly rotate character to face move direction
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    context.Transform.rotation = Quaternion.Slerp(context.Transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
                else
                {
                    moveDirection = input.normalized;
                }
            }

            // Determine speed (walk/run/sprint)
            float speed = context.MoveSpeed;
            if (context.Input != null && context.Input.SprintHeld)
                speed *= 1.5f; // Sprint multiplier (adjust as needed)

            // Apply movement
            Vector3 velocity = moveDirection * speed;
            velocity.y = context.Gravity * Time.deltaTime; // Stick to ground
            context.Controller.Move(velocity * Time.deltaTime);

            // Update animator parameters
            if (context.AnimatorWrapper != null)
            {
                float moveAmount = input.magnitude;
                context.AnimatorWrapper.SetMoveAmount(moveAmount);
                // Add more animator parameter updates as needed
            }
        }
    }
} 