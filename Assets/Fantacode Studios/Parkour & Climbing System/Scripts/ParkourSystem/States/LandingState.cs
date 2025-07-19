using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class LandingState : PlayerState
    {
        private float _landingDuration;
        private float _timer;

        public LandingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            context.AnimatorWrapper.TriggerLand(); // ✅ Use wrapper instead of SetTrigger

            _landingDuration = 0.25f;
            _timer = _landingDuration;

            Debug.Log("LandingState Entered");
        }

        public override void Exit()
        {
            // Optional cleanup — usually unnecessary for triggers unless you're force-resetting
            // context.AnimatorWrapper.ResetTrigger("Land"); // Not needed if trigger auto-resets in Animator
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                fsm.ChangeState(new GroundedState(fsm, context));
                return;
            }

            ApplyDampedHorizontalMovement();
        }

        private void ApplyDampedHorizontalMovement()
        {
            Vector2 input = context.Input.MoveInput;
            Vector3 inputDir = new Vector3(input.x, 0, input.y);

            Vector3 move = context.Transform.TransformDirection(inputDir.normalized) * (context.MoveSpeed * 0.5f);
            move.y = context.Gravity * Time.deltaTime; // gravity to keep grounded

            context.Controller.Move(move * Time.deltaTime);
        }
    }
}