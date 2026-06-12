using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        public PlayerChargingCompo ChargingCompo { get; set; }
        
        public void Initialize(Entity entity)
        {
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();
        }

        public void ForceAttack()
        {
            if (ChargingCompo.GetEnemyObject())
            {
                GuidedAttack(ChargingCompo.GetEnemyObject());
            }
            
            if (ChargingCompo.EndCharging())
            {
                Debug.Log("발도술");    
            }
        }

        private void GuidedAttack(GameObject enemy)
        {
                
        }

        private void DashAttack()
        {
            
        }
    }
}