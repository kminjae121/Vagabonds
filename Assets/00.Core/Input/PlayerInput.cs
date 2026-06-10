using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _00.CORE._02.Scripts.Input
{
    [CreateAssetMenu(menuName = "SO/Player")]
    public class PlayerInput : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;

        [Header("Input Values")]
        public Vector2 MoveValue;
        private bool jumpInput = false;

        #region Events
        
        public Action<bool> SlidingEvent { get; set; }
        
        public Action<Vector2> MoveEvent { get; set; }
        public Action JumpKeyEvent { get; set; }
        public Action SlideEvent { get; set; }
        public Action ChargingEvent { get; set; }
        public Action ChargingAttackEvent { get; set; }
        public Action BarrierEndEvent { get; set; }
        public Action BarrierEvent { get; set; }
        public Action DashEvent { get; set; }

        #endregion
        
        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
            }

            _controls.Player.SetCallbacks(this);
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls?.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveValue = context.ReadValue<Vector2>();
            
            MoveEvent?.Invoke(MoveValue);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                ChargingEvent?.Invoke();
            }
            else if (context.canceled)
            {
                ChargingAttackEvent?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
        }

        public void OnCharging(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                BarrierEvent?.Invoke();
            }
            else if (context.canceled)
            {
                BarrierEndEvent?.Invoke();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                DashEvent?.Invoke();
            }
        }

        public void OnSliding(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                SlidingEvent?.Invoke(true);
            }
            else if (context.canceled)
            {
                SlidingEvent?.Invoke(false);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                JumpKeyEvent?.Invoke();
            }
            
            if (context.started)
            {
                jumpInput = true;
            }
            else if (context.canceled)
            {
                jumpInput = false;
            }
        }
        
        public bool IsJumpPressed()
        {
            return jumpInput;
        }
    }
}