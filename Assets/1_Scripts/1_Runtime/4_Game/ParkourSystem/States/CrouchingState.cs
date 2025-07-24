using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles crouch logic and transitions to Grounded or other states
    public class CrouchingState : PlayerState
    {
        public CrouchingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Set animator parameters for crouch
            if (context.AnimatorWrapper != null)
            {
                context.AnimatorWrapper.SetCrouchType(1f); // 1 = crouching
            }
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

            // If crouch is released, return to GroundedState
            if (context.Input != null && !context.Input.CrouchPressed)
            {
                if (context.AnimatorWrapper != null)
                    context.AnimatorWrapper.SetCrouchType(0f); // 0 = not crouching
                fsm.ChangeState(new GroundedState(fsm, context));
                return;
            }

            // Camera-relative crouch movement
            Vector2 moveInput = context.Input != null ? context.Input.MoveInput : Vector2.zero;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveDirection = Vector3.zero;
            if (input.sqrMagnitude > 0.01f)
            {
                Transform cam = Camera.main != null ? Camera.main.transform : null;
                if (cam != null)
                {
                    Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 camRight = cam.right;
                    moveDirection = (camForward * input.z + camRight * input.x).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    context.Transform.rotation = Quaternion.Slerp(context.Transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
                else
                {
                    moveDirection = input.normalized;
                }
            }

            // Crouch movement speed (slower than normal)
            float speed = context.MoveSpeed * 0.5f;
            Vector3 velocity = moveDirection * speed;
            velocity.y = context.Gravity * Time.deltaTime;
            context.Controller.Move(velocity * Time.deltaTime);

            // Update animator parameters
            if (context.AnimatorWrapper != null)
            {
                float moveAmount = input.magnitude;
                context.AnimatorWrapper.SetMoveAmount(moveAmount);
            }
        }
    }
} 