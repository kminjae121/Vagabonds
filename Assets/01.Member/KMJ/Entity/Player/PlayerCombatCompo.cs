using System.Collections;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float guideSpeed = 10f;
        [SerializeField] private float shortDashForce = 4f;
        [SerializeField] private float dashForce = 15f;
        
        public PlayerChargingCompo ChargingCompo { get; set; }

        private Entity _entity;
        private Rigidbody _rbCompo;
        private Coroutine _guidCoroutine;

        public void Initialize(Entity entity)
        {
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();

            _entity = entity;
            _rbCompo = entity.GetComponent<Rigidbody>();
        }

        public void ForceAttack()
        {
            GameObject enemy = ChargingCompo.GetEnemyObject();

            if (enemy != null)
            {
                GuidedAttack(enemy);
            }
            else
            {
                DashAttack();
            }

            ChargingCompo.EndCharging();
        }

        private void GuidedAttack(GameObject enemy)
        {
            if (_guidCoroutine != null)
            {
                StopCoroutine(_guidCoroutine);
            }

            _guidCoroutine = StartCoroutine(GuidTarget(enemy));
        }

        private void DashAttack()
        {
            if (_rbCompo == null) return;

            _rbCompo.linearVelocity = Vector3.zero;
            _rbCompo.AddForce(
                _entity.transform.forward * dashForce,
                ForceMode.Impulse);
        }

        private IEnumerator GuidTarget(GameObject target)
        {
            while (target != null &&
                   Vector3.Distance(_entity.transform.position, target.transform.position) > 0.3f)
            {
                Vector3 dir =
                    (target.transform.position - _entity.transform.position).normalized;

                _rbCompo.linearVelocity = dir * guideSpeed;

                yield return null;
            }

            _rbCompo.linearVelocity = Vector3.zero;
            
            _rbCompo.AddForce(
                _entity.transform.forward * shortDashForce,
                ForceMode.Impulse);
            _guidCoroutine = null;
        }
    }
}