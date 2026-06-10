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

        private MoveCommand moveCommand;
        private JumpCommand jumpCommand;
        private AttackCommand attackCommand;

        private void Awake()
        {
            moveCommand = new MoveCommand(movement);
            jumpCommand = new JumpCommand(movement);
            attackCommand = new AttackCommand(combat);
        }

        private void OnEnable()
        {
            inputReader.MoveEvent += moveCommand.Execute;
            inputReader.JumpKeyEvent += jumpCommand.Execute;
        }

        private void OnDisable()
        {
            inputReader.MoveEvent -= moveCommand.Execute;
            inputReader.JumpKeyEvent -= jumpCommand.Execute;
        }
    }
}