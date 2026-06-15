using System;
using System.Collections;
using _01.Code.KMJ.Entity;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float guideSpeed = 10f;
        [SerializeField] private float shortDashForce = 4f;
        [SerializeField] private float shotDashUpForce = 4f;
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private DamageTriggerComponent _damageTriggerCompo;
        [SerializeField] private SpeedShader speedShader;
        
        public PlayerChargingCompo ChargingCompo { get; set; }

        private Coroutine _speedCorountine;
        private Entity _entity;
        private Rigidbody _rbCompo;
        private Coroutine _guidCoroutine;

        public void Initialize(Entity entity)
        {
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();

            _entity = entity;
            _rbCompo = entity.GetComponent<Rigidbody>();
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

            _speedCorountine = StartCoroutine(SpeedShaderSetActive());

            _rbCompo.AddForce(
                _entity.transform.forward * dashForce,
                ForceMode.Impulse);
            _rbCompo.AddForce(
                _entity.transform.up * shotDashUpForce,
                ForceMode.Impulse);
        }

        private IEnumerator GuidTarget(GameObject target)
        {
            speedShader.SetMaskSize(0.6f);
            while (target != null &&
                   Vector3.Distance(_entity.transform.position, target.transform.position) > 0.3f)
            {
                Vector3 dir =
                    (target.transform.position - _entity.transform.position).normalized;

                _rbCompo.linearVelocity = dir * guideSpeed;

                yield return null;
            }

            speedShader.SetMaskSize(1.2f);
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

        private IEnumerator SpeedShaderSetActive()
        {
            speedShader.SetMaskSize(0.6f);
            yield return new WaitForSeconds(0.8f);
            
            speedShader.SetMaskSize(1.2f);
        }
    }
}