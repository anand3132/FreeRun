using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles hanging from ledges
    public class HangingState : PlayerState
    {
        public HangingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Trigger hanging animation
            // Example: context.AnimatorWrapper.TriggerHang();
        }

        public override void Update()
        {
            // Check for input to climb up, drop, or move along the ledge
            if (context.Input != null)
            {
                // Climb up (e.g., press up or jump)
                if (context.Input.JumpPressed || context.Input.MoveInput.y > 0.5f)
                {
                    // You would pass the climb end position here
                    fsm.ChangeState(new ClimbingState(fsm, context, context.Transform.position + context.Transform.up * 1.5f));
                    return;
                }
                // Drop from ledge (e.g., press down)
                if (context.Input.MoveInput.y < -0.5f)
                {
                    fsm.ChangeState(new FallingState(fsm, context));
                    return;
                }
                // Move along ledge (e.g., left/right input)
                // TODO: Implement ledge movement logic if needed
            }

            // Optionally, check if still hanging (e.g., ledge detection)
            // If not, transition to FallingState
        }
    }
} 