using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles turning and quick turn logic
    public class TurningState : PlayerState
    {
        private float turnDuration = 0.3f; // Duration of the turn animation
        private float turnTimer = 0f;
        private Quaternion startRotation;
        private Quaternion targetRotation;
        private bool isQuickTurn = false;

        public TurningState(PlayerStateMachine fsm, PlayerContext context, bool quickTurn = false) : base(fsm, context)
        {
            isQuickTurn = quickTurn;
        }

        public override void Enter()
        {
            // Trigger turn animation if needed
            // Example: context.AnimatorWrapper.TriggerTurn();
            startRotation = context.Transform.rotation;
            targetRotation = startRotation * Quaternion.Euler(0, 180f, 0); // 180° turn
            turnTimer = 0f;
        }

        public override void Update()
        {
            turnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(turnTimer / turnDuration);
            context.Transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Optionally, check for animation event or just use timer
            if (t >= 1.0f)
            {
                fsm.ChangeState(new GroundedState(fsm, context));
            }
        }
    }
} 