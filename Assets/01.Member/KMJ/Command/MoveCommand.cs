using _01.Member.KMJ.Entity.Player;
using UnityEngine;

namespace _01.Member.KMJ.Command
{
    public class MoveCommand : MonoBehaviour
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