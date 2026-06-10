using _01.Member.KMJ.Entity.Player;
using UnityEngine;

namespace _01.Member.KMJ.Command
{
    public class JumpCommand : MonoBehaviour
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