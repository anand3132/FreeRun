using UnityEngine;
using UnityEngine.InputSystem;

namespace RedGaint.Games.ParkourSystem
{
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchPressed { get; private set; }

        private FreeRunInputAction inputActions;

        private void Awake()
        {
            inputActions = new FreeRunInputAction();
            inputActions.Enable();

            inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            inputActions.Player.Jump.performed += ctx => JumpPressed = true;
            inputActions.Player.Jump.canceled += ctx => JumpPressed = false;

            inputActions.Player.Sprint.performed += ctx => SprintHeld = true;
            inputActions.Player.Sprint.canceled += ctx => SprintHeld = false;

            inputActions.Player.Crouch.performed += ctx => CrouchPressed = true;
            inputActions.Player.Crouch.canceled += ctx => CrouchPressed = false;
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
    }
} 