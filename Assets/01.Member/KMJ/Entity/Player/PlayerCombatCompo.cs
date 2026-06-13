using System.Collections;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        public PlayerChargingCompo ChargingCompo { get; set; }
        
        private Entity _entity;
        private Coroutine _guidCoroutine;
        
        public void Initialize(Entity entity)
        {
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();
            _entity = entity;
        }

        public void ForceAttack()
        {
            if (ChargingCompo.GetEnemyObject())
            {
                GuidedAttack(ChargingCompo.GetEnemyObject());
            }
            
            if (ChargingCompo.EndCharging())
            {    
            }
        }

        private void GuidedAttack(GameObject enemy)
        {
            if (_guidCoroutine != null)
            {
                StopCoroutine(_guidCoroutine);
                _guidCoroutine = null;
            }
            _guidCoroutine = StartCoroutine(GuidTarget(enemy));
        }

        private void DashAttack()
        {
            
        }

        private IEnumerator GuidTarget(GameObject target)
        {
            while (Vector3.Distance(_entity.transform.position, target.transform.position) > 0.5)
            {
                Vector3.MoveTowards(_entity.transform.position, target.transform.position, 0.1f);                
                yield return null;
            }
        }
    }
}