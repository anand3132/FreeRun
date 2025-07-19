using UnityEngine;
using UnityEngine.InputSystem;

namespace RedGaint.ParkourSystem
{
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool FirePressed { get; private set; }
        public bool DashPressed { get; private set; }

        private FreeRunInputAction inputActions;

        private void Awake()
        {
            inputActions = new FreeRunInputAction();
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.ParkourControls.Move.performed += OnMove;
            inputActions.ParkourControls.Move.canceled += OnMove;

            inputActions.ParkourControls.Jump.performed += OnJump;
            inputActions.ParkourControls.Jump.canceled += OnJump;

            inputActions.ParkourControls.Fire.performed += OnFire;
            inputActions.ParkourControls.Fire.canceled += OnFire;

            inputActions.ParkourControls.Dash.performed += OnDash;
            inputActions.ParkourControls.Dash.canceled += OnDash;
        }

        private void OnDisable()
        {
            inputActions.ParkourControls.Move.performed -= OnMove;
            inputActions.ParkourControls.Move.canceled -= OnMove;

            inputActions.ParkourControls.Jump.performed -= OnJump;
            inputActions.ParkourControls.Jump.canceled -= OnJump;

            inputActions.ParkourControls.Fire.performed -= OnFire;
            inputActions.ParkourControls.Fire.canceled -= OnFire;

            inputActions.ParkourControls.Dash.performed -= OnDash;
            inputActions.ParkourControls.Dash.canceled -= OnDash;

            inputActions.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            JumpPressed = context.ReadValueAsButton();
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            FirePressed = context.ReadValueAsButton();
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            DashPressed = context.ReadValueAsButton();
        }
    }
}
