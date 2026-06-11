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
            if (ChargingCompo.EndCharging())
            {
                Debug.Log("발도술!!!");
            }
        }
    }
}