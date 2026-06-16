using System.Collections.Generic;
using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using Code.Core.Events.Bus;
using Code.Core.Stats;
using UnityEngine;

namespace _01.Member.KMJ._02.Scripts._01.Player.AttackCompo
{
    public readonly struct DamageHitResult
    {
        public static readonly DamageHitResult NotApplied = new(null, DamageResult.NotApplied, Vector3.zero);

        public DamageHitResult(GameObject targetObject, DamageResult damageResult, Vector3 hitPoint)
        {
            TargetObject = targetObject;
            DamageResult = damageResult;
            HitPoint = hitPoint;
        }

        public GameObject TargetObject { get; }
        public DamageResult DamageResult { get; }
        public Vector3 HitPoint { get; }
        public bool WasApplied => DamageResult.WasApplied;
        public bool WasFatal => DamageResult.WasFatal;
    }

    public class DamageTriggerComponent : MonoBehaviour, IEntityComponent
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
        [SerializeField] private float _hitShakeForce = 0.3f;

        private readonly HashSet<IDamageable> _damagedTargets = new();
        private DamageData damageData = new();
        private Entity _owner;
        private Collider _thisCollider;
        private GameObject _target;
        private AttackDataSO _activeAttackData;
        private float _activeFixedDamage = -1f;

        public void Initialize(Entity entity)
        {
            _owner = entity;
            _thisCollider = GetComponent<Collider>();
            if (_thisCollider != null)
                _thisCollider.enabled = false;
        }

        private void Start()
        {
            damageData = BuildDamageData(null, -1f);
        }

        public void StartTrigger(GameObject target)
        {
            StartTrigger(target, null, -1f);
        }

        public void StartTrigger(GameObject target, AttackDataSO attackData, float fixedDamage = -1f)
        {
            _damagedTargets.Clear();
            SetTarget(target);
            SetActiveAttack(attackData, fixedDamage);

            if (_thisCollider != null)
                _thisCollider.enabled = true;
        }

        public void EndTrigger()
        {
            if (_thisCollider != null)
                _thisCollider.enabled = false;

            SetActiveAttack(null, -1f);
        }

        public DamageHitResult GiveDamageForTarget(GameObject target)
        {
            return GiveDamageForTarget(target, null, -1f);
        }

        public DamageHitResult GiveDamageForTarget(GameObject target, AttackDataSO attackData, float fixedDamage = -1f)
        {
            SetTarget(target);
            SetActiveAttack(attackData, fixedDamage);

            if (_target == null)
                return DamageHitResult.NotApplied;

            return TryApplyDamage(_target, _target.transform.position, _activeAttackData, _activeFixedDamage);
        }

        public void ClearDamagedTargets()
        {
            _damagedTargets.Clear();
        }

        public List<DamageHitResult> GiveDamageInCone(
            Vector3 origin,
            Vector3 forward,
            float range,
            float angle,
            AttackDataSO attackData,
            float fixedDamage = -1f,
            bool clearDamagedTargets = true)
        {
            if (clearDamagedTargets)
                _damagedTargets.Clear();

            SetActiveAttack(attackData, fixedDamage);

            List<DamageHitResult> results = new();
            if (range <= 0f)
                return results;

            Vector3 normalizedForward = forward.sqrMagnitude > Mathf.Epsilon
                ? forward.normalized
                : transform.forward;
            float halfAngle = Mathf.Clamp(angle, 0f, 360f) * 0.5f;
            Collider[] hits = Physics.OverlapSphere(origin, range, whatIsEnemy, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                Vector3 targetPoint = hit.bounds.center;
                Vector3 toTarget = targetPoint - origin;
                if (toTarget.sqrMagnitude <= 0.0001f)
                    toTarget = normalizedForward;

                if (Vector3.Angle(normalizedForward, toTarget.normalized) > halfAngle)
                    continue;

                DamageHitResult result = TryApplyDamage(hit.gameObject, targetPoint, _activeAttackData, _activeFixedDamage);
                if (result.WasApplied)
                    results.Add(result);
            }

            return results;
        }

        private void SetTarget(GameObject target)
        {
            _target = target;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & whatIsEnemy) == 0)
                return;

            TryApplyDamage(other.gameObject, other.transform.position, _activeAttackData, _activeFixedDamage);
        }

        private DamageHitResult TryApplyDamage(GameObject target, Vector3 hitPoint, AttackDataSO attackData, float fixedDamage)
        {
            if (target == null || _owner == null || !TryGetDamageable(target, out IDamageable damageable))
                return DamageHitResult.NotApplied;

            if (!_damagedTargets.Add(damageable))
                return DamageHitResult.NotApplied;

            AttackDataSO effectiveAttackData = attackData != null ? attackData : weaponAtkData;
            damageData = BuildDamageData(effectiveAttackData, fixedDamage);
            if (_hitShakeForce > 0f)
                Bus<CamShakeEvent>.Raise(new CamShakeEvent(_hitShakeForce));

            DamageResult damageResult = damageable.ApplyDamage(damageData, hitPoint, _owner.transform.forward, effectiveAttackData, _owner);
            return damageResult.WasApplied
                ? new DamageHitResult(target, damageResult, hitPoint)
                : DamageHitResult.NotApplied;
        }

        private static bool TryGetDamageable(GameObject target, out IDamageable damageable)
        {
            if (target.TryGetComponent(out damageable))
                return true;

            damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
                return true;

            damageable = target.GetComponentInChildren<IDamageable>();
            return damageable != null;
        }

        private void SetActiveAttack(AttackDataSO attackData, float fixedDamage)
        {
            _activeAttackData = attackData;
            _activeFixedDamage = fixedDamage;
        }

        private DamageData BuildDamageData(AttackDataSO attackData, float fixedDamage)
        {
            DamageData result = new();
            if (statCompo != null && atkDamageStat != null)
                result.damage = statCompo.GetStat(atkDamageStat).Value;

            if (fixedDamage >= 0f)
                result.damage = fixedDamage;

            if (attackData != null)
            {
                result.damage = result.damage * attackData.damageMultiplier + attackData.damageIncrease;
                result.damageType = attackData.damageType;
            }

            return result;
        }
    }
}
