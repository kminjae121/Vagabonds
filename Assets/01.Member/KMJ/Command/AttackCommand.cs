using _Code.EntityCompo.Combat;
using UnityEngine;

namespace _Code.Command
{
    public class AttackCommand : MonoBehaviour, ICommand
    {
        private PlayerCombatCompo _combatCompo;

        public AttackCommand(PlayerCombatCompo combatCompo)
        {
            _combatCompo = combatCompo;
        }

        public void Execute()
        {
            _combatCompo.ChargingCompo.Charging();
        }

        public void ExecuteEnd()
        {
            _combatCompo.ForceAttack();
        }
    }
}