using _Code.EntityCompo.Combat;
using UnityEngine;

namespace _Code.Command
{
    public class AttackCommand : MonoBehaviour, ICommand
    {
        private PlayerCombatCompo combatCompo;
        
        public AttackCommand(PlayerCombatCompo combatCompo)
        {
            this.combatCompo = combatCompo;
        }
        public void Execute()
        {
            
        }
    }
}