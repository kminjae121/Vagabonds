using _Code.EntityCompo.Combat;

namespace _Code.Command
{
    public class AttackCommand : ICommand
    {
        private readonly PlayerCombatCompo _combatCompo;

        public AttackCommand(PlayerCombatCompo combatCompo)
        {
            _combatCompo = combatCompo;
        }

        public void Execute()
        {
            _combatCompo.ChargingCompo?.Charging();
        }

        public void ExecuteEnd()
        {
            _combatCompo?.ForceAttack();
        }
    }
}
