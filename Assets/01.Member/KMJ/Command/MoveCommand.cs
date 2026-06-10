using _Code.EntityCompo.Move;
using UnityEngine;

namespace _Code.Command
{
    public class MoveCommand : MonoBehaviour, ICommand<Vector2>
    {
        private readonly PlayerMoveCompo movement;

        public MoveCommand(PlayerMoveCompo movement)
        {
            this.movement = movement;
        }

        public void Execute(Vector2 direction)
        {
            movement.SetMove(direction);
        }
    }
}