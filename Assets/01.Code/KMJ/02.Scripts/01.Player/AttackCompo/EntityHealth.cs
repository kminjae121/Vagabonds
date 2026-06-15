using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using _Code.EntityCompo.BloodFlower;
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

        [SerializeField] private BloodFlowerSystem _bloodFlowerSystem = null;
            
        public UnityEvent OnMinusHealthEvent;
        
        private Entity _entity;
        private ActionData _actionData;
        private EntityStatCompo _statCompo;

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _actionData = entity.GetUnitCompo<ActionData>();
            _statCompo = entity.GetUnitCompo<EntityStatCompo>();
        }
        
        public void AfterInitialize()
        {
            maxHealth = currentHealth = _statCompo.SubscribeStat(
                hpStat, HandleMaxHPChanged, 10f);
        }

        private void OnDestroy()
        {
            _statCompo.UnSubscribeStat(hpStat, HandleMaxHPChanged);
        }

        private void HandleMaxHPChanged(StatSO stat, float currentValue, float previousValue)
        {
            float changed = currentValue - previousValue; //얼마만큼 변했는지를 측정
            maxHealth = currentValue;
            
            currentHealth = changed > 0 ?
                Mathf.Clamp(currentHealth + changed, 0, maxHealth) 
                : Mathf.Clamp(currentHealth, 0, maxHealth);
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
            
            if (_bloodFlowerSystem != null)
            {
                _bloodFlowerSystem.GetDamage(damageData.damage);
            }
            else
            {
                return;
            }
        }
    }
}