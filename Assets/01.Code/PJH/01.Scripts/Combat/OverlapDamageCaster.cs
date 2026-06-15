using System.Collections.Generic;
using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using UnityEngine;

namespace Code.Combat
{
    public class OverlapDamageCaster : DamageCaster
    {
        [SerializeField] private float castRadius;
        [SerializeField] private int maxColliderCnt = 1;

        private Collider[] _colliders;
        private HashSet<Transform> _hitObjects;

        public override void InitCaster(Entity owner)
        {
            base.InitCaster(owner);

            _colliders = new Collider[maxColliderCnt];
            _hitObjects = new HashSet<Transform>(maxColliderCnt);
        }

        public void StartCasting()
        {
            _hitObjects.Clear();
        }

        public override bool CastDamage(DamageData damageData, Vector3 pos, Vector3 dir, AttackDataSO attackData)
        {
            int cnt = Physics.OverlapSphereNonAlloc(transform.position, castRadius, _colliders, targetLayer);
            
            for (int i = 0; i < cnt; ++i)
            {
                Transform target = _colliders[i].transform;
                
                if (_hitObjects.Contains(target) || _hitObjects.Count >= maxColliderCnt)
                    continue;

                _hitObjects.Add(target);

                Vector3 normal = (_owner.transform.position - target.position).normalized;
                
                ApplyDamage(target, damageData, transform.position, normal, attackData);
            }

            return cnt > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, castRadius);
        }
    }
}