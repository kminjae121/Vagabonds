using _Code.Command;
using _Code.EntityCompo.Combat;
using _Code.KDH.EntityCompo.Move;
using UnityEngine;
using PlayerInput = _00.CORE._02.Scripts.Input.PlayerInput;

namespace _Code.KDH.Command
{
    public class PlayerCommandBinder : MonoBehaviour
    {
        [SerializeField] private PlayerInput inputReader;
        [SerializeField] private PlayerMoveCompo movement;
        [SerializeField] private PlayerCombatCompo combat;

        private AttackCommand _attackCommand;

        private void Awake()
        {
            _attackCommand = new AttackCommand(combat);
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                Debug.LogError($"{nameof(PlayerCommandBinder)} requires a PlayerInput asset.", this);
                enabled = false;
                return;
            }

            inputReader.MoveEvent += OnMove;
            inputReader.JumpKeyEvent += OnJump;
            inputReader.SlidingEvent += OnSlide;
            inputReader.ChargingEvent += _attackCommand.Execute;
            inputReader.ChargingAttackEvent += _attackCommand.ExecuteEnd;
        }

        private void Update()
        {
            if (inputReader != null && movement != null)
                movement.SetJumpHeld(inputReader.IsJumpPressed());
        }

        private void OnDisable()
        {
            if (inputReader == null)
                return;

            inputReader.MoveEvent -= OnMove;
            inputReader.JumpKeyEvent -= OnJump;
            inputReader.SlidingEvent -= OnSlide;
            inputReader.ChargingEvent -= _attackCommand.Execute;
            inputReader.ChargingAttackEvent -= _attackCommand.ExecuteEnd;
        }

        private void OnMove(Vector2 moveInput)
        {
            if (movement != null)
                movement.SetMove(moveInput);
        }

        private void OnJump()
        {
            if (movement != null)
                movement.Jump();
        }

        private void OnSlide(bool isHeld)
        {
            if (movement != null)
                movement.SetSlideHeld(isHeld);
        }
    }
}
