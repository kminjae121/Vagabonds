using System;
using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using Code.Core.Events.Bus;
using Code.Core.Stats;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] private StatSO atkDamageStat;
        [SerializeField] private EntityStatCompo statCompo;

        public UnityEvent DamageEvent;
        private DamageData damageData = new();
        private Entity _owner;
        private Collider _thisCollider;
        private GameObject _target;
        
        private float _atkDamage;
        public void Initialize(Entity entity)
        {
            _owner = entity;
            _thisCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            damageData.damage = statCompo.GetStat(atkDamageStat).Value;
        }

        public void StartTrigger(GameObject target)
        {
            _thisCollider.enabled = true;

            SetTarget(target);
        }

        private void SetTarget(GameObject target)
        {
            if (target == null)
                _target = null;
            else
                _target = target;
        }

        public void EndTrigger()
        {
            _thisCollider.enabled = false;
        }

        public void GiveDamageForTarget(GameObject target)
        {
            SetTarget(target);
            
            if (_target.TryGetComponent(out IDamageable damageable))                 
            {
                DamageEvent?.Invoke();
                Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.3f));
                damageable.ApplyDamage(damageData, _target.transform.position, _owner.transform.forward, weaponAtkData,
                    _owner);
            }
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