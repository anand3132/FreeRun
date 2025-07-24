using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles climbing up ledges or walls
    public class ClimbingState : PlayerState
    {
        private float climbDuration = 1.0f; // Duration of the climb
        private float climbTimer = 0f;
        private Vector3 startPos;
        private Vector3 endPos;
        private bool useRootMotion = false;
        private bool toHanging = false;

        public ClimbingState(PlayerStateMachine fsm, PlayerContext context, Vector3 climbEndPos, bool rootMotion = false, bool hangAfter = false) : base(fsm, context)
        {
            endPos = climbEndPos;
            useRootMotion = rootMotion;
            toHanging = hangAfter;
        }

        public override void Enter()
        {
            // Trigger climb animation
            // Example: context.AnimatorWrapper.TriggerClimb();
            startPos = context.Transform.position;
            climbTimer = 0f;
        }

        public override void Update()
        {
            climbTimer += Time.deltaTime;
            float t = Mathf.Clamp01(climbTimer / climbDuration);

            if (!useRootMotion)
            {
                // Manually interpolate position up the ledge
                context.Transform.position = Vector3.Lerp(startPos, endPos, t);
            }
            // If using root motion, let the animation drive the movement

            // Optionally, check for animation event or just use timer
            if (t >= 1.0f)
            {
                if (toHanging)
                    fsm.ChangeState(new HangingState(fsm, context));
                else
                    fsm.ChangeState(new GroundedState(fsm, context));
            }
        }
    }
} 