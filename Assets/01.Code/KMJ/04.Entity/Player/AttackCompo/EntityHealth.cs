using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using _Code.EntityCompo.Combat;
using Code.Core.Stats;
using Code.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Entities
{
    public class EntityHealth : MonoBehaviour, IEntityComponent, IDamageable, IAfterInitialize
    {
        [SerializeField] private StatSO hpStat;
        [SerializeField] private float maxHealth;
        [SerializeField] private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        
        public delegate void OnHealthChanged(float current, float max);

        public event OnHealthChanged OnHealthChangedEvent;
            
        public UnityEvent OnMinusHealthEvent;
        
        private Entity _entity;
        private ActionData _actionData;
        private EntityStatCompo _statCompo;

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _actionData = entity.GetEntityCompo<ActionData>();
            _statCompo = entity.GetEntityCompo<EntityStatCompo>();
        }
        
        public void AfterInitialize()
        {
            maxHealth = currentHealth = _statCompo.GetStat(hpStat).Value;
        }

        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer)
        {
            if (_entity.IsDead)
                return;
            
            _actionData.HitNormal = hitNormal;
            _actionData.HitPoint = hitPoint;
            _actionData.HitByPowerAttack = attackData.isPowerAttack;
            _actionData.LastDamageData = damageData; 

            currentHealth = Mathf.Clamp(currentHealth - damageData.damage, 0, maxHealth);

            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
                _entity.OnDeathEvent?.Invoke();

            OnMinusHealthEvent?.Invoke();
            _entity.OnHitEvent?.Invoke();
        }
    }
}