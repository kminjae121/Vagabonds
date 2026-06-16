using System;
using System.Collections;
using _01.Code.KMJ.Entity;
using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float guideSpeed = 10f;
        [SerializeField] private float shortDashForce = 4f;
        [SerializeField] private float shotDashUpForce = 4f;
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private CinemachineCamera playerCam;

        public UnityEvent BaldoStartEvent;
        public UnityEvent BaldoEndEvent;
        
        public PlayerChargingCompo ChargingCompo { get; set; }

        private EntityAnimator animator;
        private DamageTriggerComponent _damageTriggerCompo;
        private EntityAnimatorTrigger _triggerCompo;
        private Coroutine _speedCorountine;
        private Entity _entity;
        private Rigidbody _rbCompo;
        private Coroutine _guidCoroutine;

        public void Initialize(Entity entity)
        {
            _entity = entity;
            
            _damageTriggerCompo = entity.GetUnitCompo<DamageTriggerComponent>();
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();
            animator = entity.GetUnitCompo<EntityAnimator>();
            _triggerCompo = entity.GetUnitCompo<EntityAnimatorTrigger>();
            
            _rbCompo = entity.GetComponent<Rigidbody>();
            _triggerCompo.OnBaldoAnimationEndTrigger += HandleBaldoAniamtionEnd;
        }

        private void HandleBaldoAniamtionEnd()
        {
            animator.SetBoolean("IDLE");
        }

        public void ForceAttack()
        {
            GameObject enemy = ChargingCompo.GetEnemyObject();

            if (enemy != null)
            {
                GuidedAttack(enemy);
            }
            else if(ChargingCompo.EndCharging())
            {
                DashAttack();
            }
            
            ChargingCompo.ResetUI();
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
            
            animator.SetBoolean("BALDO");

            _speedCorountine = StartCoroutine(BaldoEvent());

            _rbCompo.AddForce(
                _entity.transform.forward * dashForce,
                ForceMode.Impulse);
            _rbCompo.AddForce(
                _entity.transform.up * shotDashUpForce,
                ForceMode.Impulse);
        }

        private IEnumerator GuidTarget(GameObject target)
        {
            bool isUseAnimation = false;
            BaldoStartEvent?.Invoke();
            
            while (target != null && Vector3.Distance(_entity.transform.position, target.transform.position) > 0.3f)
            {
                Vector3 dir = (target.transform.position - _entity.transform.position).normalized;

                _rbCompo.linearVelocity = dir * guideSpeed;

                if (Vector3.Distance(_entity.transform.position, target.transform.position) <= 2f && !isUseAnimation)
                {
                    animator.SetBoolean("BALDO");
                    isUseAnimation = true;
                }

                yield return null;
            }

            BaldoEndEvent?.Invoke();
            _damageTriggerCompo.GiveDamageForTarget(target);
            _rbCompo.linearVelocity = Vector3.zero;
            
            _rbCompo.AddForce(
                _entity.transform.forward * shortDashForce,
                ForceMode.Impulse);
            _rbCompo.AddForce(
                _entity.transform.up * shotDashUpForce,
                ForceMode.Impulse);
            
            _guidCoroutine = null;
        }

        private IEnumerator BaldoEvent()
        {
            BaldoStartEvent?.Invoke();
            yield return new WaitForSeconds(0.6f);
            BaldoEndEvent?.Invoke();
        }
    }
}