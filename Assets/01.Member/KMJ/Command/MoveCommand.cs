using _Code.EntityCompo.Move;
using UnityEngine;

namespace _Code.Command
{
    public class MoveCommand : ICommand<Vector2>
    {
        private readonly PlayerMoveCompo _movement;

        public MoveCommand(PlayerMoveCompo movement)
        {
            _movement = movement;
        }

        public void Execute(Vector2 direction)
        {
            _movement.SetMove(direction);
        }
    }
}
