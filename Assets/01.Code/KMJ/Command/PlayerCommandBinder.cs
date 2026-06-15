using _Code.EntityCompo.Combat;
using _Code.EntityCompo.Move;
using UnityEngine;
using PlayerInput = _00.CORE._02.Scripts.Input.PlayerInput;

namespace _Code.Command
{
    public class PlayerCommandBinder : MonoBehaviour
    {
        [SerializeField] private PlayerInput inputReader;
        [SerializeField] private PlayerMoveCompo movement;
        [SerializeField] private PlayerCombatCompo combat;

        private MoveCommand _moveCommand;
        private JumpCommand _jumpCommand;
        private AttackCommand _attackCommand;

        private void Awake()
        {
            _moveCommand = new MoveCommand(movement);
            _jumpCommand = new JumpCommand(movement);
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

            inputReader.MoveEvent += _moveCommand.Execute;
            inputReader.JumpKeyEvent += _jumpCommand.Execute;
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

            inputReader.MoveEvent -= _moveCommand.Execute;
            inputReader.JumpKeyEvent -= _jumpCommand.Execute;
            inputReader.ChargingEvent -= _attackCommand.Execute;
            inputReader.ChargingAttackEvent -= _attackCommand.ExecuteEnd;
        }
    }
}
