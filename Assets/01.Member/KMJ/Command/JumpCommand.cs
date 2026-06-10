using _Code.EntityCompo.Move;
using UnityEngine;

namespace _Code.Command
{
    public class JumpCommand : MonoBehaviour, ICommand
    {
        private readonly PlayerMoveCompo movement;

        public JumpCommand(PlayerMoveCompo movement)
        {
            this.movement = movement;
        }

        public void Execute()
        {
            movement.Jump();
        }
    }
}