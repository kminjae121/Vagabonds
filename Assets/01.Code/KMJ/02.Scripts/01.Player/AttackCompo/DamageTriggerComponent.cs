using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using Code.Core.Stats;
using UnityEngine;

namespace _01.Member.KMJ._02.Scripts._01.Player.AttackCompo
{
    public class DamageTriggerComponent : MonoBehaviour,IEntityComponent
    {
        [Space(5)]
        [Header("EnemyLayer")]
        [SerializeField] private LayerMask whatIsEnemy;
        
        [Space(5)]
        [Header("AttackData")]
        [SerializeField] private AttackDataSO weaponAtkData;
            
        [Header("Stat")]
        [SerializeField] private  StatSO atkDamageStat;

        [SerializeField] private EntityStatCompo statCompo;

        [SerializeField] private DamageType _damageType;
            
        private float _atkDamage;

        private Entity _owner;
        
        private DamageData damageData;

        private Collider _thisCollider;
        public void Initialize(Entity entity)
        {
            damageData.damage = statCompo.GetStat(atkDamageStat).Value;
            damageData.damageType = _damageType;
            _thisCollider = GetComponent<Collider>();
        }

        public void StartTrigger()
        {
            _thisCollider.enabled = true;
        }

        public void EndTrigger()
        {
            _thisCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & whatIsEnemy) != 0)
            {
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    damageable.ApplyDamage(damageData, other.transform.position, _owner.transform.forward, weaponAtkData,
                        _owner);
                }
            }
        }
    }
}