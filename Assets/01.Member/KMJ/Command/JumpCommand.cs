using _Code.EntityCompo.Move;

namespace _Code.Command
{
    public class JumpCommand : ICommand
    {
        private readonly PlayerMoveCompo _movement;

        public JumpCommand(PlayerMoveCompo movement)
        {
            _movement = movement;
        }

        public void Execute()
        {
            _movement.Jump();
        }
    }
}
