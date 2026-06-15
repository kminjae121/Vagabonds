using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using Code.Entities;
using UnityEngine;

namespace Code.Combat
{
    public abstract class DamageCaster : MonoBehaviour
    {
        [SerializeField] protected LayerMask targetLayer;

        protected Entity _owner;

        public virtual void InitCaster(Entity owner)
        {
            _owner = owner;
        }

        public virtual void ApplyDamage(Transform target, DamageData damageData, Vector3 pos,
            Vector3 normal, AttackDataSO attackData)
        {
            if (target.TryGetComponent(out IDamageable damageable))
                damageable.ApplyDamage(damageData, pos, normal, attackData, _owner);
        }

        public abstract bool CastDamage(DamageData damageData, Vector3 pos, Vector3 dir, AttackDataSO attackData);
    }
}