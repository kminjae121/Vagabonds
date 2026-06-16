using System.Collections;
using System.Collections.Generic;
using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using MovementTestCameraFollow = _Code.KDH.MovementTest.MovementTestCameraFollow;
using MovementCompo = _Code.KDH.EntityCompo.Move.PlayerMoveCompo;

namespace _Code.EntityCompo.Combat
{
    public class PlayerCombatCompo : MonoBehaviour, IEntityComponent
    {
        [Header("Attack Data")]
        [SerializeField] private AttackDataSO _normalAttackData;
        [SerializeField] private AttackDataSO _baldoAttackData;

        [Header("Damage")]
        [SerializeField] private float _normalAttackDamage = 50f;
        [SerializeField] private float _baldoAttackDamage = 999f;
        [SerializeField] private float _normalAttackDamageWindow = 0.16f;
        [SerializeField] private float _baldoDamageDelay = 0.06f;
        [SerializeField] private float _baldoEventDuration = 0.34f;

        [Header("Baldo Cone")]
        [SerializeField] private float _baldoRange = 6f;
        [SerializeField, Range(1f, 180f)] private float _baldoAngle = 100f;
        [SerializeField] private float _baldoOriginHeight = 1.1f;

        [Header("Baldo Aim Lock")]
        [SerializeField] private bool _enableBaldoAimLock = true;
        [SerializeField] private float _baldoAimLockRange = 30f;
        [SerializeField, Range(1f, 180f)] private float _baldoAimLockAngle = 180f;
        [SerializeField] private float _baldoAimLockRangePadding = 1.25f;
        [SerializeField] private float _baldoAimLockShakeForce = 0.25f;

        [Header("Baldo Air Dash")]
        [FormerlySerializedAs("_enableBaldoGuidedLunge")]
        [SerializeField] private bool _enableBaldoLockedAirDash = true;
        [FormerlySerializedAs("_enableBaldoForwardBurst")]
        [SerializeField] private bool _enableBaldoFallbackAirDash = true;
        [FormerlySerializedAs("_baldoForwardBurstSpeed")]
        [SerializeField] private float _baldoFallbackAirDashSpeed = 54f;
        [FormerlySerializedAs("_baldoForwardBurstDuration")]
        [SerializeField] private float _baldoFallbackAirDashDuration = 0.16f;
        [FormerlySerializedAs("_baldoGuidedLungeSpeed")]
        [SerializeField] private float _baldoLockedAirDashSpeed = 96f;
        [FormerlySerializedAs("_baldoGuidedLungeMaxDuration")]
        [SerializeField] private float _baldoLockedAirDashDuration = 0.18f;
        [SerializeField] private float _baldoLockedAirDashMinDuration = 0.06f;
        [SerializeField] private float _baldoLockedAirDashStopDistance = 1.25f;
        [FormerlySerializedAs("_baldoGuidedLungeVerticalLift")]
        [SerializeField] private float _baldoAirDashVerticalLift = 0.25f;
        [FormerlySerializedAs("_baldoGuidedLungeCapMultiplier")]
        [SerializeField] private float _baldoAirDashCapMultiplier = 9.5f;
        [FormerlySerializedAs("_baldoGuidedLungeCapDuration")]
        [SerializeField] private float _baldoAirDashCapDuration = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _baldoAirDashControlScale = 0f;
        [SerializeField] private float _baldoAirDashDamageProbeRange = 5.5f;
        [SerializeField, Range(1f, 180f)] private float _baldoAirDashDamageAngle = 115f;
        [SerializeField] private float _baldoLockedAirDashHitRadius = 2.2f;
        [SerializeField] private float _baldoLockedAirDashOvershootDistance = 1.2f;
        [SerializeField] private float _baldoAirDashPostHitCarryTime = 0.04f;

        [Header("Baldo Reward")]
        [SerializeField] private int _baldoBloodStacksOnKill = 1;
        [SerializeField] private int _baldoMultiKillBonusStacks = 1;
        [SerializeField] private int _baldoMultiKillThreshold = 2;
        [SerializeField] private float _baldoKillShakeForce = 0.55f;
        [SerializeField] private float _baldoMultiKillShakeForce = 0.85f;
        [SerializeField] private float _baldoKillHitStopDuration = 0.045f;
        [SerializeField] private float _baldoMultiKillHitStopDuration = 0.075f;
        [SerializeField, Range(0f, 1f)] private float _baldoHitStopTimeScale = 0f;

        public UnityEvent BaldoStartEvent;
        public UnityEvent BaldoEndEvent;

        public PlayerChargingCompo ChargingCompo { get; set; }

        private EntityAnimator _animator;
        private DamageTriggerComponent _damageTriggerCompo;
        private EntityAnimatorTrigger _triggerCompo;
        private Entity _entity;
        private MovementCompo _movement;
        private MovementTestCameraFollow _cameraFollow;
        private PlayerAutoAimmingCompo _aimingCompo;
        private Rigidbody _rbCompo;
        private Coroutine _baldoCoroutine;
        private Coroutine _damageWindowCoroutine;

        private readonly struct BaldoAirDash
        {
            public BaldoAirDash(GameObject lockedTarget, Vector3 direction, float speed, float duration)
            {
                LockedTarget = lockedTarget;
                Direction = direction;
                Speed = speed;
                Duration = duration;
            }

            public GameObject LockedTarget { get; }
            public Vector3 Direction { get; }
            public float Speed { get; }
            public float Duration { get; }
            public bool HasLockedTarget => LockedTarget != null;
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _damageTriggerCompo = entity.GetUnitCompo<DamageTriggerComponent>();
            ChargingCompo = entity.GetUnitCompo<PlayerChargingCompo>();
            _animator = entity.GetUnitCompo<EntityAnimator>();
            _triggerCompo = entity.GetUnitCompo<EntityAnimatorTrigger>();
            _movement = entity.GetUnitCompo<MovementCompo>();
            _aimingCompo = entity.GetComponentInChildren<PlayerAutoAimmingCompo>();
            _rbCompo = entity.GetComponentInChildren<Rigidbody>();
            _cameraFollow = GetCameraFollow();

            if (_triggerCompo != null)
                _triggerCompo.OnBaldoAnimationEndTrigger += HandleBaldoAnimationEnd;
        }

        private void Update()
        {
            bool canLockTarget = ChargingCompo != null && ChargingCompo.IsSheathed;
            _aimingCompo?.ShootRayForCheckEnemy(canLockTarget);
        }

        private void OnDestroy()
        {
            if (_triggerCompo != null)
                _triggerCompo.OnBaldoAnimationEndTrigger -= HandleBaldoAnimationEnd;
        }

        private void OnValidate()
        {
            _normalAttackDamage = Mathf.Max(0f, _normalAttackDamage);
            _baldoAttackDamage = Mathf.Max(0f, _baldoAttackDamage);
            _normalAttackDamageWindow = Mathf.Max(0f, _normalAttackDamageWindow);
            _baldoDamageDelay = Mathf.Max(0f, _baldoDamageDelay);
            _baldoEventDuration = Mathf.Max(_baldoDamageDelay, _baldoEventDuration);
            _baldoRange = Mathf.Max(0f, _baldoRange);
            _baldoOriginHeight = Mathf.Max(0f, _baldoOriginHeight);
            _baldoAimLockRange = Mathf.Max(_baldoRange, _baldoAimLockRange);
            _baldoAimLockRangePadding = Mathf.Max(0f, _baldoAimLockRangePadding);
            _baldoAimLockShakeForce = Mathf.Max(0f, _baldoAimLockShakeForce);
            _baldoFallbackAirDashSpeed = Mathf.Max(0f, _baldoFallbackAirDashSpeed);
            _baldoFallbackAirDashDuration = Mathf.Max(0f, _baldoFallbackAirDashDuration);
            _baldoLockedAirDashSpeed = Mathf.Max(0f, _baldoLockedAirDashSpeed);
            _baldoLockedAirDashDuration = Mathf.Max(0f, _baldoLockedAirDashDuration);
            _baldoLockedAirDashMinDuration = Mathf.Clamp(_baldoLockedAirDashMinDuration, 0f, _baldoLockedAirDashDuration);
            _baldoLockedAirDashStopDistance = Mathf.Max(0f, _baldoLockedAirDashStopDistance);
            _baldoAirDashVerticalLift = Mathf.Max(0f, _baldoAirDashVerticalLift);
            _baldoAirDashCapMultiplier = Mathf.Max(1f, _baldoAirDashCapMultiplier);
            _baldoAirDashCapDuration = Mathf.Max(0f, _baldoAirDashCapDuration);
            _baldoAirDashControlScale = Mathf.Clamp01(_baldoAirDashControlScale);
            _baldoAirDashDamageProbeRange = Mathf.Max(0f, _baldoAirDashDamageProbeRange);
            _baldoLockedAirDashHitRadius = Mathf.Max(0f, _baldoLockedAirDashHitRadius);
            _baldoLockedAirDashOvershootDistance = Mathf.Max(0f, _baldoLockedAirDashOvershootDistance);
            _baldoAirDashPostHitCarryTime = Mathf.Max(0f, _baldoAirDashPostHitCarryTime);
            _baldoEventDuration = Mathf.Max(_baldoEventDuration, _baldoLockedAirDashDuration, _baldoFallbackAirDashDuration);
            _baldoBloodStacksOnKill = Mathf.Max(0, _baldoBloodStacksOnKill);
            _baldoMultiKillBonusStacks = Mathf.Max(0, _baldoMultiKillBonusStacks);
            _baldoMultiKillThreshold = Mathf.Max(2, _baldoMultiKillThreshold);
            _baldoKillShakeForce = Mathf.Max(0f, _baldoKillShakeForce);
            _baldoMultiKillShakeForce = Mathf.Max(_baldoKillShakeForce, _baldoMultiKillShakeForce);
            _baldoKillHitStopDuration = Mathf.Max(0f, _baldoKillHitStopDuration);
            _baldoMultiKillHitStopDuration = Mathf.Max(_baldoKillHitStopDuration, _baldoMultiKillHitStopDuration);
        }

        public void HandleAttackPressed()
        {
            if (_entity != null && _entity.IsDead)
                return;

            ChargingCompo?.BeginCharging();
        }

        public void HandleAttackReleased()
        {
            ForceAttack();
        }

        public void ForceAttack()
        {
            if (_entity != null && _entity.IsDead)
                return;

            bool isSheathed = ChargingCompo != null && ChargingCompo.EndCharging();
            if (isSheathed)
                _aimingCompo?.ShootRayForCheckEnemy(true);

            GameObject aimLockTarget = isSheathed ? GetBaldoAimLockTarget() : null;
            ChargingCompo?.ResetUI();

            if (isSheathed)
                BaldoAttack(aimLockTarget);
            else
                NormalAttack();
        }

        private void HandleBaldoAnimationEnd()
        {
            _animator?.SetBoolean("IDLE");
        }

        private void NormalAttack()
        {
            _animator?.SetBoolean("BALDO");
            StartDamageWindow(null, _normalAttackDamageWindow, _normalAttackData, _normalAttackDamage);
        }

        private void BaldoAttack(GameObject aimLockTarget)
        {
            _animator?.SetBoolean("BALDO");

            if (_baldoCoroutine != null)
                StopCoroutine(_baldoCoroutine);

            _baldoCoroutine = StartCoroutine(BaldoSequence(aimLockTarget));
        }

        private IEnumerator BaldoSequence(GameObject aimLockTarget)
        {
            float sequenceStartTime = Time.time;
            BaldoStartEvent?.Invoke();

            List<DamageHitResult> hitResults = new();
            bool usedAirDash = TryGetBaldoAirDash(aimLockTarget, out BaldoAirDash airDash);
            if (usedAirDash)
                yield return RunBaldoAirDash(airDash, hitResults);
            else if (_baldoDamageDelay > 0f)
                yield return new WaitForSeconds(_baldoDamageDelay);

            if (!usedAirDash && hitResults.Count == 0)
                hitResults.AddRange(ExecuteBaldoDamage(aimLockTarget));

            ApplyBaldoRewards(hitResults, aimLockTarget);

            float elapsed = Time.time - sequenceStartTime;
            float remainingDuration = Mathf.Max(0f, _baldoEventDuration - elapsed);
            if (remainingDuration > 0f)
                yield return new WaitForSeconds(remainingDuration);

            BaldoEndEvent?.Invoke();
            _aimingCompo?.SetEnemyNull();
            _baldoCoroutine = null;
        }

        private IReadOnlyList<DamageHitResult> ExecuteBaldoDamage(GameObject aimLockTarget)
        {
            if (_damageTriggerCompo == null)
                return System.Array.Empty<DamageHitResult>();

            Transform actor = GetActorTransform();
            Vector3 origin = actor.position + Vector3.up * _baldoOriginHeight;
            Vector3 damageForward = actor.forward;
            float damageRange = _baldoRange;

            if (TryGetAimLockDirection(aimLockTarget, out Vector3 lockDirection, out float lockDistance, false))
            {
                LockAimToTarget(aimLockTarget, lockDirection);
                damageForward = lockDirection;
                damageRange = Mathf.Max(_baldoRange, lockDistance + _baldoAimLockRangePadding);

                if (_baldoAimLockShakeForce > 0f)
                    Bus<CamShakeEvent>.Raise(new CamShakeEvent(_baldoAimLockShakeForce));
            }

            return _damageTriggerCompo.GiveDamageInCone(
                origin,
                damageForward,
                damageRange,
                _baldoAngle,
                _baldoAttackData,
                _baldoAttackDamage);
        }

        private void ApplyBaldoRewards(IReadOnlyList<DamageHitResult> hitResults, GameObject aimLockTarget)
        {
            int killCount = CountFatalHits(hitResults);
            if (killCount <= 0)
                return;

            int stackGain = _baldoBloodStacksOnKill;
            if (killCount >= _baldoMultiKillThreshold)
                stackGain += _baldoMultiKillBonusStacks;

            _movement?.AddBloodStacks(stackGain);
            _movement?.ApplyKillImpulse(GetBaldoRewardDirection(hitResults, aimLockTarget), killCount);

            bool isMultiKill = killCount >= _baldoMultiKillThreshold;
            float shakeForce = isMultiKill ? _baldoMultiKillShakeForce : _baldoKillShakeForce;
            if (shakeForce > 0f)
                Bus<CamShakeEvent>.Raise(new CamShakeEvent(shakeForce));

            float hitStopDuration = isMultiKill ? _baldoMultiKillHitStopDuration : _baldoKillHitStopDuration;
            if (hitStopDuration > 0f)
                Bus<HitStopEvent>.Raise(new HitStopEvent(hitStopDuration, _baldoHitStopTimeScale));
        }

        private int CountFatalHits(IReadOnlyList<DamageHitResult> hitResults)
        {
            if (hitResults == null)
                return 0;

            int killCount = 0;
            for (int i = 0; i < hitResults.Count; i++)
            {
                if (hitResults[i].WasFatal)
                    killCount++;
            }

            return killCount;
        }

        private Vector3 GetBaldoRewardDirection(IReadOnlyList<DamageHitResult> hitResults, GameObject aimLockTarget)
        {
            Transform actor = GetActorTransform();
            Vector3 origin = actor.position + Vector3.up * _baldoOriginHeight;

            if (TryGetAimLockDirection(aimLockTarget, out Vector3 lockDirection, out _, false))
                return lockDirection;

            Vector3 averageKillPoint = Vector3.zero;
            int killCount = 0;

            if (hitResults != null)
            {
                for (int i = 0; i < hitResults.Count; i++)
                {
                    DamageHitResult result = hitResults[i];
                    if (!result.WasFatal)
                        continue;

                    averageKillPoint += result.HitPoint;
                    killCount++;
                }
            }

            if (killCount <= 0)
                return actor.forward;

            Vector3 direction = averageKillPoint / killCount - origin;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : actor.forward;
        }

        private GameObject GetBaldoAimLockTarget()
        {
            if (!_enableBaldoAimLock || _aimingCompo == null || !_aimingCompo.HasTarget)
                return null;

            GameObject target = _aimingCompo.CurrentTarget;
            return TryGetAimLockDirection(target, out _, out _) ? target : null;
        }

        private bool TryGetBaldoAirDash(GameObject aimLockTarget, out BaldoAirDash airDash)
        {
            Transform actor = GetActorTransform();

            if (_enableBaldoLockedAirDash && TryGetAimLockDirection(aimLockTarget, out Vector3 lockDirection, out float lockDistance, false))
            {
                LockAimToTarget(aimLockTarget, lockDirection);
                airDash = new BaldoAirDash(
                    aimLockTarget,
                    lockDirection,
                    _baldoLockedAirDashSpeed,
                    GetLockedAirDashDuration(lockDistance));
                return true;
            }

            if (_enableBaldoFallbackAirDash)
            {
                airDash = new BaldoAirDash(null, actor.forward, _baldoFallbackAirDashSpeed, _baldoFallbackAirDashDuration);
                return true;
            }

            airDash = default;
            return false;
        }

        private IEnumerator RunBaldoAirDash(BaldoAirDash airDash, List<DamageHitResult> hitResults)
        {
            if (airDash.Speed <= 0f || airDash.Duration <= 0f)
                yield break;

            Vector3 dashDirection = GetDashDirection(airDash.Direction);
            FaceActorToDirection(dashDirection);

            if (airDash.HasLockedTarget && _baldoAimLockShakeForce > 0f)
                Bus<CamShakeEvent>.Raise(new CamShakeEvent(_baldoAimLockShakeForce));

            _damageTriggerCompo?.ClearDamagedTargets();

            bool releasedAimLock = false;
            float endTime = Time.time + airDash.Duration;
            while (Time.time < endTime)
            {
                ApplyAirDashVelocity(dashDirection, airDash.Speed);

                bool hitThisFrame = CollectBaldoAirDashHits(dashDirection, hitResults)
                                    || CollectLockedTargetAirDashHit(airDash, dashDirection, hitResults);

                if (hitThisFrame)
                {
                    if (!releasedAimLock)
                    {
                        ReleaseAimLock();
                        releasedAimLock = true;
                    }

                    if (airDash.HasLockedTarget)
                        endTime = Mathf.Min(endTime, Time.time + _baldoAirDashPostHitCarryTime);
                }

                if (ShouldStopLockedAirDashAfterPass(airDash, dashDirection))
                    break;

                yield return new WaitForFixedUpdate();
            }

            bool hitOnExit = CollectBaldoAirDashHits(dashDirection, hitResults)
                             || CollectLockedTargetAirDashHit(airDash, dashDirection, hitResults);

            if (hitOnExit && !releasedAimLock)
                ReleaseAimLock();
        }

        private bool CollectBaldoAirDashHits(Vector3 dashDirection, List<DamageHitResult> hitResults)
        {
            if (_damageTriggerCompo == null)
                return false;

            Transform actor = GetActorTransform();
            Vector3 origin = actor.position + Vector3.up * _baldoOriginHeight;
            float damageRange = Mathf.Max(_baldoRange, _baldoAirDashDamageProbeRange);
            float damageAngle = Mathf.Max(_baldoAngle, _baldoAirDashDamageAngle);
            List<DamageHitResult> results = _damageTriggerCompo.GiveDamageInCone(
                origin,
                dashDirection,
                damageRange,
                damageAngle,
                _baldoAttackData,
                _baldoAttackDamage,
                false);

            if (results.Count <= 0)
                return false;

            hitResults.AddRange(results);
            return true;
        }

        private bool CollectLockedTargetAirDashHit(BaldoAirDash airDash, Vector3 dashDirection, List<DamageHitResult> hitResults)
        {
            if (!airDash.HasLockedTarget || _damageTriggerCompo == null)
                return false;

            if (!TryGetLockedTargetOffset(airDash, dashDirection, out float forwardDistance, out float lateralDistance))
                return false;

            bool isInsideForwardWindow = forwardDistance <= _baldoAirDashDamageProbeRange
                                         && forwardDistance >= -_baldoLockedAirDashOvershootDistance;
            if (!isInsideForwardWindow || lateralDistance > _baldoLockedAirDashHitRadius)
                return false;

            DamageHitResult result = _damageTriggerCompo.GiveDamageForTarget(
                airDash.LockedTarget,
                _baldoAttackData,
                _baldoAttackDamage);

            if (!result.WasApplied)
                return false;

            hitResults.Add(result);
            return true;
        }

        private void ApplyAirDashVelocity(Vector3 direction, float speed)
        {
            if (_movement != null)
            {
                _movement.ApplyCombatBurst(
                    direction,
                    speed,
                    _baldoAirDashVerticalLift,
                    _baldoAirDashCapMultiplier,
                    _baldoAirDashCapDuration,
                    true,
                    Time.fixedDeltaTime * 3f,
                    _baldoAirDashControlScale);
                return;
            }

            if (_rbCompo == null)
                return;

            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= Mathf.Epsilon)
                planarDirection = GetActorTransform().forward;

            planarDirection.Normalize();
            Vector3 velocity = _rbCompo.linearVelocity;
            velocity.x = planarDirection.x * speed;
            velocity.z = planarDirection.z * speed;
            velocity.y = Mathf.Max(velocity.y, _baldoAirDashVerticalLift);
            _rbCompo.linearVelocity = velocity;
        }

        private Vector3 GetDashDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= Mathf.Epsilon)
                planarDirection = GetActorTransform().forward;

            return planarDirection.sqrMagnitude > Mathf.Epsilon
                ? planarDirection.normalized
                : Vector3.forward;
        }

        private float GetLockedAirDashDuration(float lockDistance)
        {
            if (_baldoLockedAirDashSpeed <= 0f)
                return _baldoLockedAirDashDuration;

            float travelDistance = Mathf.Max(0f, lockDistance - _baldoLockedAirDashStopDistance);
            float distanceDuration = travelDistance / _baldoLockedAirDashSpeed;
            return Mathf.Clamp(distanceDuration, _baldoLockedAirDashMinDuration, _baldoLockedAirDashDuration);
        }

        private bool ShouldStopLockedAirDashAfterPass(BaldoAirDash airDash, Vector3 dashDirection)
        {
            return TryGetLockedTargetOffset(airDash, dashDirection, out float forwardDistance, out _)
                   && forwardDistance < -_baldoLockedAirDashOvershootDistance;
        }

        private bool TryGetLockedTargetOffset(BaldoAirDash airDash, Vector3 dashDirection, out float forwardDistance, out float lateralDistance)
        {
            forwardDistance = 0f;
            lateralDistance = 0f;

            if (!airDash.HasLockedTarget || airDash.LockedTarget == null)
                return false;

            Vector3 origin = GetActorTransform().position + Vector3.up * _baldoOriginHeight;
            Vector3 toTarget = GetTargetPoint(airDash.LockedTarget) - origin;
            forwardDistance = Vector3.Dot(toTarget, dashDirection);
            lateralDistance = (toTarget - dashDirection * forwardDistance).magnitude;
            return true;
        }

        private void ReleaseAimLock()
        {
            _aimingCompo?.SetEnemyNull();
        }

        private void FaceActorToDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= Mathf.Epsilon)
                return;

            GetActorTransform().rotation = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
        }

        private void LockAimToTarget(GameObject target, Vector3 direction)
        {
            FaceActorToDirection(direction);
            MovementTestCameraFollow cameraFollow = GetCameraFollow();
            if (cameraFollow == null || target == null)
                return;

            cameraFollow.LookAtWorldPoint(GetTargetPoint(target));
        }

        private MovementTestCameraFollow GetCameraFollow()
        {
            if (_cameraFollow != null)
                return _cameraFollow;

            if (Camera.main != null && Camera.main.TryGetComponent(out MovementTestCameraFollow cameraFollow))
            {
                _cameraFollow = cameraFollow;
                return _cameraFollow;
            }

            _cameraFollow = FindFirstObjectByType<MovementTestCameraFollow>();
            return _cameraFollow;
        }

        private bool TryGetAimLockDirection(GameObject target, out Vector3 direction, out float distance, bool requireForwardAngle = true)
        {
            direction = Vector3.zero;
            distance = 0f;

            if (!_enableBaldoAimLock || target == null)
                return false;

            Entity targetEntity = target.GetComponentInParent<Entity>();
            if (targetEntity != null && targetEntity.IsDead)
                return false;

            Transform actor = GetActorTransform();
            Vector3 origin = actor.position + Vector3.up * _baldoOriginHeight;
            Vector3 targetPoint = GetTargetPoint(target);
            Vector3 toTarget = targetPoint - origin;
            distance = toTarget.magnitude;

            if (distance <= 0.001f)
            {
                direction = actor.forward;
                return true;
            }

            if (distance > _baldoAimLockRange)
                return false;

            direction = toTarget / distance;
            return !requireForwardAngle || Vector3.Angle(actor.forward, direction) <= _baldoAimLockAngle * 0.5f;
        }

        private static Vector3 GetTargetPoint(GameObject target)
        {
            if (target.TryGetComponent(out Collider collider))
                return collider.bounds.center;

            collider = target.GetComponentInChildren<Collider>();
            if (collider != null)
                return collider.bounds.center;

            collider = target.GetComponentInParent<Collider>();
            return collider != null ? collider.bounds.center : target.transform.position;
        }

        private void StartDamageWindow(GameObject target, float duration, AttackDataSO attackData, float fixedDamage)
        {
            if (_damageWindowCoroutine != null)
            {
                StopCoroutine(_damageWindowCoroutine);
                _damageTriggerCompo?.EndTrigger();
            }

            _damageWindowCoroutine = StartCoroutine(DamageWindow(target, duration, attackData, fixedDamage));
        }

        private IEnumerator DamageWindow(GameObject target, float duration, AttackDataSO attackData, float fixedDamage)
        {
            if (_damageTriggerCompo == null)
                yield break;

            _damageTriggerCompo.StartTrigger(target, attackData, fixedDamage);
            if (target != null)
                _damageTriggerCompo.GiveDamageForTarget(target, attackData, fixedDamage);

            yield return new WaitForSeconds(Mathf.Max(0f, duration));

            _damageTriggerCompo.EndTrigger();
            _damageWindowCoroutine = null;
        }

        private Transform GetActorTransform()
        {
            return _entity != null ? _entity.transform : transform;
        }

        private void OnDrawGizmosSelected()
        {
            Transform actor = _entity != null ? _entity.transform : transform;
            Vector3 origin = actor.position + Vector3.up * Mathf.Max(0f, _baldoOriginHeight);
            Vector3 forward = actor.forward;
            Vector3 left = Quaternion.AngleAxis(-_baldoAngle * 0.5f, Vector3.up) * forward;
            Vector3 right = Quaternion.AngleAxis(_baldoAngle * 0.5f, Vector3.up) * forward;

            Gizmos.color = new Color(1f, 0.15f, 0.05f, 0.7f);
            Gizmos.DrawWireSphere(origin, _baldoRange);
            Gizmos.DrawLine(origin, origin + left.normalized * _baldoRange);
            Gizmos.DrawLine(origin, origin + right.normalized * _baldoRange);
        }
    }
}
