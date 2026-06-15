using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using Code.Combat;
using Code.Core.Stats;
using UnityEngine;

namespace Code.Enemies
{
    public class MeleeEnemyAttackCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private AttackDataSO attackData;
        [SerializeField] private StatSO meleeDamageStat;
        [SerializeField] private OverlapDamageCaster[] casters;
        
        private Entity _entity;
        private EntityStatCompo _statCompo;
        private EntityAnimatorTrigger _trigger;
        private DamageData _currentDamageData;
        private bool _isActive;
        
        public void Initialize(Entity entity)
        {
            _entity = entity;
            _statCompo = entity.GetUnitCompo<EntityStatCompo>();
            _trigger = entity.GetUnitCompo<EntityAnimatorTrigger>();

            casters = entity.GetComponentsInChildren<OverlapDamageCaster>(true);

            foreach (var caster in casters)
                caster.InitCaster(entity);
        }

        public void AfterInitialize()
        {
            meleeDamageStat = _statCompo.GetStat(meleeDamageStat);
            _trigger.OnDamageToggleTrigger += SetDamageCaster;
        }

        private void OnDestroy()
        {
            _trigger.OnDamageToggleTrigger -= SetDamageCaster;
        }

        private void SetDamageCaster(bool isActive)
        {
            _isActive = isActive;

            if (!isActive)
                return;
            
            foreach (var caster in casters)
                caster.StartCasting();

            _currentDamageData = new DamageData
            {
                damage = meleeDamageStat.Value,
                damageType = attackData.damageType
            };
        }

        private void FixedUpdate()
        {
            if (!_isActive)
                return;
            
            foreach (var caster in casters)
            {
                caster.CastDamage(_currentDamageData, transform.position,
                    transform.forward, attackData);
            }
        }
    }
}