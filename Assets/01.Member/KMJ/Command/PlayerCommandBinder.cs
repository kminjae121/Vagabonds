using _01.Member.KMJ.Entity.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = _00.CORE._02.Scripts.Input.PlayerInput;

namespace _01.Member.KMJ.Command
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