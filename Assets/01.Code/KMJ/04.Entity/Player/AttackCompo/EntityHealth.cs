using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using _Code.EntityCompo.Combat;
using Code.Core.Events.Bus;
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
        [SerializeField] private bool _raiseHitStopOnDeath = true;
        [SerializeField] private float _deathHitStopDuration = 0.06f;
        [SerializeField, Range(0f, 1f)] private float _deathHitStopTimeScale = 0f;

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
            if (_statCompo != null && hpStat != null)
                maxHealth = _statCompo.GetStat(hpStat).Value;

            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = maxHealth;
        }

        public DamageResult ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer)
        {
            if (_entity != null && _entity.IsDead)
                return DamageResult.NotApplied;

            if (_actionData != null)
            {
                _actionData.HitNormal = hitNormal;
                _actionData.HitPoint = hitPoint;
                _actionData.HitByPowerAttack = attackData != null && attackData.isPowerAttack;
                _actionData.LastDamageData = damageData;
            }

            currentHealth = Mathf.Clamp(currentHealth - damageData.damage, 0, maxHealth);

            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);

            OnMinusHealthEvent?.Invoke();
            _entity?.OnHitEvent?.Invoke();

            if (currentHealth > 0)
                return new DamageResult(true, false, _entity);

            if (_entity != null)
                _entity.IsDead = true;

            if (_raiseHitStopOnDeath)
                Bus<HitStopEvent>.Raise(new HitStopEvent(_deathHitStopDuration, _deathHitStopTimeScale));

            _entity?.OnDeathEvent?.Invoke();
            return new DamageResult(true, true, _entity);
        }
    }
}
