using System;
using _Code.EntityCompo;
using Code.Interfaces;
using UnityEngine;

namespace _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo
{
    public class EntityAnimatorTrigger : MonoBehaviour, IEntityComponent
    {
        public event Action OnAnimationEndTrigger;
        public event Action OnAttackVFXTrigger;
        public event Action<bool> OnManualRotationTrigger;
        public event Action OnDamageCastTrigger;
        public event Action<bool> OnDamageToggleTrigger;
        public event Action OnCastSkillTrigger;
        
        private _Code.EntityCompo.Entity _entity;
        
        public void Initialize(_Code.EntityCompo.Entity entity)
        {
            _entity = entity;
        }

        private void AnimationEnd() =>  OnAnimationEndTrigger?.Invoke();
        private void PlayAttackVFX() => OnAttackVFXTrigger?.Invoke();
        private void StartManualRotation() => OnManualRotationTrigger?.Invoke(true);
        private void StopManualRotation() => OnManualRotationTrigger?.Invoke(false);
        private void DamageCast() => OnDamageCastTrigger?.Invoke();
        private void StartDamageCast() => OnDamageToggleTrigger?.Invoke(true);
        private void StopDamageCast() => OnDamageToggleTrigger?.Invoke(false);
        private void CastSkill() => OnCastSkillTrigger?.Invoke();
    }
}