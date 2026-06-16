using UnityEngine;
using UnityEngine.Serialization;

namespace _Code.EntityCompo.Move
{
    public class PlayerMoveCompo : MonoBehaviour, IEntityComponent
    {
        private const int MaxLateralContactNormals = 4;

        public enum HopMode
        {
            None,
            Timed,
            AutoRepeat
        }

        [Header("1. Base Movement")]
        [FormerlySerializedAs("_groundMaxSpeed")]
        [SerializeField] private float _baseMaxSpeed = 10.5f;
        [SerializeField] private float _groundAcceleration = 110f;
        [SerializeField] private float _groundFriction = 5.5f;
        [SerializeField] private float _groundStopSpeed = 1f;

        [Header("2. Bunnyhop Momentum")]
        [FormerlySerializedAs("_maxBhopSpeedMultiplier")]
        [SerializeField] private float _bhopSpeedMultiplier = 1.65f;
        [FormerlySerializedAs("_jumpHorizontalRetention")]
        [SerializeField, Range(0f, 1.2f)] private float _timedJumpHorizontalRetention = 1f;
        [SerializeField, Range(0f, 1.2f)] private float _autoJumpHorizontalRetention = 0.88f;
        [SerializeField] private int _landingFrictionSkipFrames;
        [SerializeField] private bool _autoBhopWhenJumpHeld = true;
        [SerializeField] private bool _skipFrictionOnBufferedJump = true;

        [Header("3. Air Steering")]
        [SerializeField] private float _airAcceleration = 58f;
        [FormerlySerializedAs("_airControl")]
        [SerializeField] private float _airControlResponsiveness = 6f;
        [SerializeField] private float _airWishSpeedMultiplier = 1f;
        [SerializeField, Range(0f, 2f)] private float _forwardAirAccelerationScale = 0.7f;
        [SerializeField, Range(0f, 2f)] private float _strafeAirAccelerationScale = 0.95f;
        [SerializeField] private float _minimumSpeedForStrafeScaling = 7f;
        [SerializeField] private float _smallSteerBonusAngle = 10f;
        [SerializeField] private float _fullStrafeBonusAngle = 38f;
        [SerializeField, Range(0f, 1f)] private float _lateralInputStrafeInfluence = 0.45f;

        [Header("4. Jump and Gravity")]
        [SerializeField] private float _jumpHeight = 2.35f;
        [SerializeField] private float _gravity = 32f;
        [SerializeField] private float _jumpBufferTime = 0.16f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpGroundingLockoutTime = 0.1f;

        [Header("5. Combat Momentum")]
        [SerializeField] private float _killImpulseSpeed = 9.5f;
        [SerializeField] private float _killImpulseVerticalLift = 1.25f;
        [SerializeField] private float _multiKillImpulseMultiplier = 1.25f;
        [SerializeField] private float _combatMomentumSpeedCapMultiplier = 3.6f;
        [SerializeField] private float _combatMomentumCapDuration = 1.25f;

        [Header("6. Blood Speed Bonus")]
        [SerializeField] private float _bloodSpeedBonusPerStack = 0.03f;
        [SerializeField] private int _maxBloodStacksForMovement = 12;

        [Header("7. Wall Kick")]
        [SerializeField] private bool _enableWallKick = true;
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private float _wallCheckDistance = 1.35f;
        [SerializeField] private float _wallCheckRadius = 0.22f;
        [SerializeField] private float _wallCheckHeight = 1.05f;
        [SerializeField] private float _wallKickMinSpeed = 0f;
        [SerializeField] private float _wallKickCoyoteTime = 0.22f;
        [SerializeField] private float _wallKickDetachCooldown = 0.55f;
        [SerializeField] private float _sameWallReattachCooldown = 0.75f;
        [SerializeField] private float _sameWallReattachMinDistance = 2.25f;
        [SerializeField] private float _sameWallReattachApproachSpeed = 0.8f;
        [SerializeField] private float _wallKickReturnControlDampingTime = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _wallKickReturnControlScale = 0f;
        [SerializeField] private float _wallRideAwaySpeedThreshold = 0.25f;
        [SerializeField] private float _wallContactVelocityTrim = 0.95f;
        [SerializeField] private float _wallRideGravity = 18f;
        [SerializeField] private float _wallRideMaxFallSpeed = 7f;
        [SerializeField] private float _wallRideMaxRiseSpeed = 1.5f;
        [SerializeField] private float _wallRideUpwardBrake = 70f;
        [SerializeField] private float _wallRideAcceleration = 42f;
        [SerializeField] private float _wallRideMaxSpeed = 13f;
        [SerializeField] private float _wallKickMinimumRideTime = 0.08f;
        [SerializeField] private float _wallKickHorizontalImpulse = 18f;
        [SerializeField] private float _wallKickForwardImpulse = 10f;
        [SerializeField, Range(0f, 1.2f)] private float _wallKickForwardRetention = 0.75f;
        [SerializeField] private float _wallKickVerticalVelocity = 23.5f;
        [SerializeField] private float _wallKickMinimumExitSpeed = 24f;
        [SerializeField, Range(0f, 1f)] private float _wallKickViewAssist = 0.25f;
        [SerializeField] private float _wallKickSpeedCapMultiplier = 3.6f;
        [SerializeField] private float _wallKickMomentumCapDuration = 0.35f;
        [SerializeField] private float _wallKickFeedbackTime = 0.2f;
        [SerializeField] private int _maxAirWallKicks;
        [SerializeField] private bool _requireNewWallForRepeatKick = false;
        [SerializeField, Range(0f, 1f)] private float _sameWallNormalDotThreshold = 0.85f;

        [Header("8. Slide")]
        [SerializeField] private bool _enableSlide = true;
        [SerializeField] private float _slideDuration = 0.55f;
        [SerializeField] private float _slideCooldown = 0.25f;
        [SerializeField] private float _slideMinStartSpeed = 7f;
        [SerializeField] private float _slideStartBoost = 3f;
        [SerializeField] private float _slideFriction = 1.25f;
        [SerializeField] private float _slideSpeedCapMultiplier = 1.85f;
        [SerializeField] private float _slideMomentumCapDuration = 0.25f;
        [SerializeField] private float _slideSteerResponsiveness = 3.5f;
        [SerializeField, Range(0f, 1f)] private float _slideInputControl = 0.35f;

        [Header("9. Collision Smoothing")]
        [SerializeField] private bool _useLowFrictionColliderMaterial = true;
        [SerializeField, Range(0f, 1f)] private float _lateralContactMaxNormalY = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _lateralCollisionSlideStrength = 1f;
        [SerializeField] private float _lateralCollisionSlideGraceTime = 0.08f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.32f;
        [SerializeField] private float _groundCheckDistance = 0.24f;
        [SerializeField, Range(0f, 1f)] private float _minGroundNormalY = 0.6f;

        private Rigidbody _rbCompo;
        private CapsuleCollider _capsuleCollider;
        private Entity _entity;
        private Vector2 _moveInput;
        private float _lastJumpRequestTime = -999f;
        private float _lastGroundedTime = -999f;
        private float _ignoreGroundUntilTime = -999f;
        private float _combatMomentumCapUntilTime = -999f;
        private float _combatMomentumCapMultiplier = 1f;
        private int _frictionSkipFrames;
        private int _bloodStacks;
        private HopMode _pendingHopMode;
        private HopMode _lastConsumedHopMode;
        private Vector3 _wallNormal;
        private Vector3 _wallForward;
        private Vector3 _lastWallKickNormal;
        private Vector3 _lastWallKickPosition;
        private Vector3 _slideDirection;
        private float _lastWallContactTime = -999f;
        private float _lastWallKickTime = -999f;
        private float _wallRideStartTime = -999f;
        private float _wallKickMomentumCapUntilTime = -999f;
        private float _wallKickCooldownUntil = -999f;
        private float _sameWallReattachLockedUntilTime = -999f;
        private float _slideEndTime = -999f;
        private float _nextSlideTime = -999f;
        private float _slideMomentumCapUntilTime = -999f;
        private readonly Vector3[] _lateralContactNormals = new Vector3[MaxLateralContactNormals];
        private PhysicsMaterial _lowFrictionMaterial;
        private float _lastLateralContactTime = -999f;
        private int _lateralContactCount;
        private int _airWallKickCount;
        private int _timedHopCount;
        private int _autoRepeatHopCount;
        private int _wallRideEnterCount;
        private int _slideCount;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _isTouchingWall;
        private bool _isWallRiding;
        private bool _jumpHeld;
        private bool _slideHeld;
        private bool _slideRequested;

        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => _rbCompo != null ? _rbCompo.linearVelocity : Vector3.zero;
        public int BloodStacks => _bloodStacks;
        public float BloodSpeedMultiplier => GetBloodSpeedMultiplier();
        public float EffectiveMaxSpeed => GetEffectiveBaseSpeed();
        public float MaxBhopSpeed => GetBhopSpeedCap();
        public float CurrentSpeedCap => GetCurrentHorizontalSpeedCap();
        public float CombatMomentumRemainingTime => Mathf.Max(0f, _combatMomentumCapUntilTime - Time.time);
        public HopMode LastConsumedHopMode => _lastConsumedHopMode;
        public Vector2 MoveInput => _moveInput;
        public bool IsTouchingWall => _isTouchingWall;
        public bool IsWallRiding => _isWallRiding;
        public bool IsSliding => IsSlideActive();
        public bool IsWallKickReady => _isWallRiding && CanUseWallKickCount();
        public float WallKickGraceRemainingTime => Mathf.Max(0f, _wallKickCoyoteTime - (Time.time - _lastWallContactTime));
        public float WallKickFeedbackRemainingTime => Mathf.Max(0f, _wallKickFeedbackTime - (Time.time - _lastWallKickTime));
        public float WallKickReturnDampingRemainingTime => ShouldDampenWallKickReturnControl()
            ? Mathf.Max(0f, _wallKickReturnControlDampingTime - (Time.time - _lastWallKickTime))
            : 0f;
        public float SameWallReattachDistanceRemaining => GetSameWallReattachDistanceRemaining();
        public float SlideRemainingTime => IsSlideActive() ? Mathf.Max(0f, _slideEndTime - Time.time) : 0f;
        public int AirWallKickCount => _airWallKickCount;
        public int TimedHopCount => _timedHopCount;
        public int AutoRepeatHopCount => _autoRepeatHopCount;
        public int WallRideEnterCount => _wallRideEnterCount;
        public int SlideCount => _slideCount;
        public Vector3 WallNormal => _wallNormal;
        public float CurrentHorizontalSpeed
        {
            get
            {
                Vector3 velocity = Velocity;
                velocity.y = 0f;
                return velocity.magnitude;
            }
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _rbCompo = entity.GetComponentInChildren<Rigidbody>();
            _capsuleCollider = entity.GetComponentInChildren<CapsuleCollider>();

            if (_rbCompo == null)
            {
                Debug.LogError($"{nameof(PlayerMoveCompo)} requires a Rigidbody in the player hierarchy.", this);
                enabled = false;
                return;
            }

            _rbCompo.useGravity = false;
            _rbCompo.freezeRotation = true;
            _rbCompo.interpolation = RigidbodyInterpolation.Interpolate;
            _rbCompo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rbCompo.solverIterations = Mathf.Max(_rbCompo.solverIterations, 10);
            _rbCompo.solverVelocityIterations = Mathf.Max(_rbCompo.solverVelocityIterations, 10);

            ApplyLowFrictionColliderMaterial();
        }

        private void OnValidate()
        {
            _baseMaxSpeed = Mathf.Max(0f, _baseMaxSpeed);
            _groundAcceleration = Mathf.Max(0f, _groundAcceleration);
            _groundFriction = Mathf.Max(0f, _groundFriction);
            _groundStopSpeed = Mathf.Max(0f, _groundStopSpeed);
            _bhopSpeedMultiplier = Mathf.Max(1f, _bhopSpeedMultiplier);
            _landingFrictionSkipFrames = Mathf.Max(0, _landingFrictionSkipFrames);
            _airAcceleration = Mathf.Max(0f, _airAcceleration);
            _airControlResponsiveness = Mathf.Max(0f, _airControlResponsiveness);
            _airWishSpeedMultiplier = Mathf.Max(0f, _airWishSpeedMultiplier);
            _forwardAirAccelerationScale = Mathf.Max(0f, _forwardAirAccelerationScale);
            _strafeAirAccelerationScale = Mathf.Max(0f, _strafeAirAccelerationScale);
            _minimumSpeedForStrafeScaling = Mathf.Max(0f, _minimumSpeedForStrafeScaling);
            _smallSteerBonusAngle = Mathf.Max(0f, _smallSteerBonusAngle);
            _fullStrafeBonusAngle = Mathf.Max(_smallSteerBonusAngle + 0.01f, _fullStrafeBonusAngle);
            _jumpHeight = Mathf.Max(0f, _jumpHeight);
            _gravity = Mathf.Max(0f, _gravity);
            _jumpBufferTime = Mathf.Max(0f, _jumpBufferTime);
            _coyoteTime = Mathf.Max(0f, _coyoteTime);
            _jumpGroundingLockoutTime = Mathf.Max(0f, _jumpGroundingLockoutTime);
            _killImpulseSpeed = Mathf.Max(0f, _killImpulseSpeed);
            _killImpulseVerticalLift = Mathf.Max(0f, _killImpulseVerticalLift);
            _multiKillImpulseMultiplier = Mathf.Max(1f, _multiKillImpulseMultiplier);
            _combatMomentumSpeedCapMultiplier = Mathf.Max(_bhopSpeedMultiplier, _combatMomentumSpeedCapMultiplier);
            _combatMomentumCapDuration = Mathf.Max(0f, _combatMomentumCapDuration);
            _bloodSpeedBonusPerStack = Mathf.Max(0f, _bloodSpeedBonusPerStack);
            _maxBloodStacksForMovement = Mathf.Max(0, _maxBloodStacksForMovement);
            _wallCheckDistance = Mathf.Max(0f, _wallCheckDistance);
            _wallCheckRadius = Mathf.Max(0f, _wallCheckRadius);
            _wallCheckHeight = Mathf.Max(0f, _wallCheckHeight);
            _wallKickMinSpeed = Mathf.Max(0f, _wallKickMinSpeed);
            _wallKickCoyoteTime = Mathf.Max(0f, _wallKickCoyoteTime);
            _wallKickDetachCooldown = Mathf.Max(0f, _wallKickDetachCooldown);
            _sameWallReattachCooldown = Mathf.Max(0f, _sameWallReattachCooldown);
            _sameWallReattachMinDistance = Mathf.Max(0f, _sameWallReattachMinDistance);
            _sameWallReattachApproachSpeed = Mathf.Max(0f, _sameWallReattachApproachSpeed);
            _wallKickReturnControlDampingTime = Mathf.Max(0f, _wallKickReturnControlDampingTime);
            _wallKickReturnControlScale = Mathf.Clamp01(_wallKickReturnControlScale);
            _wallRideAwaySpeedThreshold = Mathf.Max(0f, _wallRideAwaySpeedThreshold);
            _wallContactVelocityTrim = Mathf.Clamp01(_wallContactVelocityTrim);
            _wallRideGravity = Mathf.Max(0f, _wallRideGravity);
            _wallRideMaxFallSpeed = Mathf.Max(0f, _wallRideMaxFallSpeed);
            _wallRideMaxRiseSpeed = Mathf.Max(0f, _wallRideMaxRiseSpeed);
            _wallRideUpwardBrake = Mathf.Max(0f, _wallRideUpwardBrake);
            _wallRideAcceleration = Mathf.Max(0f, _wallRideAcceleration);
            _wallRideMaxSpeed = Mathf.Max(0f, _wallRideMaxSpeed);
            _wallKickMinimumRideTime = Mathf.Max(0f, _wallKickMinimumRideTime);
            _wallKickHorizontalImpulse = Mathf.Max(0f, _wallKickHorizontalImpulse);
            _wallKickForwardImpulse = Mathf.Max(0f, _wallKickForwardImpulse);
            _wallKickForwardRetention = Mathf.Max(0f, _wallKickForwardRetention);
            _wallKickVerticalVelocity = Mathf.Max(0f, _wallKickVerticalVelocity);
            _wallKickMinimumExitSpeed = Mathf.Max(0f, _wallKickMinimumExitSpeed);
            _wallKickViewAssist = Mathf.Clamp01(_wallKickViewAssist);
            _wallKickSpeedCapMultiplier = Mathf.Max(_bhopSpeedMultiplier, _wallKickSpeedCapMultiplier);
            _wallKickMomentumCapDuration = Mathf.Max(0f, _wallKickMomentumCapDuration);
            _wallKickFeedbackTime = Mathf.Max(0f, _wallKickFeedbackTime);
            _maxAirWallKicks = Mathf.Max(0, _maxAirWallKicks);
            _slideDuration = Mathf.Max(0f, _slideDuration);
            _slideCooldown = Mathf.Max(0f, _slideCooldown);
            _slideMinStartSpeed = Mathf.Max(0f, _slideMinStartSpeed);
            _slideStartBoost = Mathf.Max(0f, _slideStartBoost);
            _slideFriction = Mathf.Max(0f, _slideFriction);
            _slideSpeedCapMultiplier = Mathf.Max(_bhopSpeedMultiplier, _slideSpeedCapMultiplier);
            _slideMomentumCapDuration = Mathf.Max(0f, _slideMomentumCapDuration);
            _slideSteerResponsiveness = Mathf.Max(0f, _slideSteerResponsiveness);
            _slideInputControl = Mathf.Clamp01(_slideInputControl);
            _lateralContactMaxNormalY = Mathf.Clamp01(_lateralContactMaxNormalY);
            _lateralCollisionSlideStrength = Mathf.Clamp01(_lateralCollisionSlideStrength);
            _lateralCollisionSlideGraceTime = Mathf.Max(0f, _lateralCollisionSlideGraceTime);
        }

        public void SetMove(Vector2 dir)
        {
            _moveInput = Vector2.ClampMagnitude(dir, 1f);
        }

        public void SetJumpHeld(bool isHeld)
        {
            _jumpHeld = isHeld;

            if (_autoBhopWhenJumpHeld && isHeld)
                RequestJump(HopMode.AutoRepeat);
        }

        public void Jump()
        {
            RequestJump(HopMode.Timed);
        }

        public void SetSlideHeld(bool isHeld)
        {
            _slideHeld = isHeld;

            if (isHeld)
                _slideRequested = true;
        }

        public float GetMoveSpeed() => EffectiveMaxSpeed;

        public void SetMoveSpeed(float moveSpeed)
        {
            _baseMaxSpeed = Mathf.Max(0f, moveSpeed);
        }

        public void GravityZero()
        {
            if (_rbCompo != null)
                _rbCompo.useGravity = false;
        }

        public void SetBloodStacks(int stackCount)
        {
            _bloodStacks = Mathf.Max(0, stackCount);
        }

        public void AddBloodStacks(int amount = 1)
        {
            if (amount <= 0)
                return;

            SetBloodStacks(_bloodStacks + amount);
        }

        public void ClearBloodStacks()
        {
            _bloodStacks = 0;
        }

        public void ResetMovementStats()
        {
            _lastConsumedHopMode = HopMode.None;
            _timedHopCount = 0;
            _autoRepeatHopCount = 0;
            _wallRideEnterCount = 0;
            _slideCount = 0;
            _airWallKickCount = 0;
            _lastWallKickTime = -999f;
            _lastWallKickNormal = Vector3.zero;
            _lastWallKickPosition = Vector3.zero;
            _slideEndTime = -999f;
            _nextSlideTime = -999f;
            _slideMomentumCapUntilTime = -999f;
        }

        public void ApplyKillImpulse(Vector3 direction, int killCount = 1)
        {
            ApplyCombatImpulse(direction, killCount);
        }

        public void ApplyCombatImpulse(Vector3 direction, int killCount = 1)
        {
            if (_rbCompo == null)
                return;

            ActivateCombatMomentumCap();
            MomentumModifier.ApplyCombatImpulse(
                _rbCompo,
                _entity != null ? _entity.transform : transform,
                direction,
                killCount,
                _killImpulseSpeed,
                _killImpulseVerticalLift,
                _multiKillImpulseMultiplier,
                GetCurrentHorizontalSpeedCap());
        }

        private void OnCollisionStay(Collision collision)
        {
            if (_lateralCollisionSlideStrength <= 0f)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                if (Mathf.Abs(normal.y) > _lateralContactMaxNormalY)
                    continue;

                normal.y = 0f;
                if (normal.sqrMagnitude <= Mathf.Epsilon)
                    continue;

                AddLateralContactNormal(normal.normalized);
            }
        }

        private void FixedUpdate()
        {
            if (_rbCompo == null || _entity == null)
                return;

            float deltaTime = Time.fixedDeltaTime;
            Vector3 velocity = _rbCompo.linearVelocity;
            Vector3 horizontalVelocity = GetHorizontalVelocity(velocity);
            float verticalVelocity = velocity.y;

            if (_autoBhopWhenJumpHeld && _jumpHeld)
                RequestJump(HopMode.AutoRepeat);

            UpdateGroundState();

            Vector3 wishDirection = DampenWallKickReturnControl(GetWishDirection(), out float wishControlScale);
            float wishSpeed = EffectiveMaxSpeed;
            ConsumeSlideRequest(ref horizontalVelocity, wishDirection);
            UpdateWallContactState(horizontalVelocity);

            if (_isGrounded)
            {
                if (IsSlideActive())
                {
                    horizontalVelocity = ApplySlideMovement(horizontalVelocity, wishDirection, deltaTime);
                }
                else if (ShouldApplyGroundFriction())
                {
                    horizontalVelocity = ApplyFriction(horizontalVelocity, deltaTime);

                    horizontalVelocity = Accelerate(horizontalVelocity, wishDirection, wishSpeed, _groundAcceleration, deltaTime);
                }
                else
                {
                    horizontalVelocity = Accelerate(horizontalVelocity, wishDirection, wishSpeed, _groundAcceleration, deltaTime);
                }

                verticalVelocity = Mathf.Min(verticalVelocity, -1f);
            }
            else
            {
                if (_isWallRiding)
                {
                    horizontalVelocity = ApplyWallRideMovement(horizontalVelocity, wishDirection, deltaTime);
                    ApplyWallRideGravity(ref verticalVelocity, deltaTime);
                }
                else
                {
                    float airAcceleration = _airAcceleration * GetAirAccelerationScale(horizontalVelocity, wishDirection) * wishControlScale;
                    horizontalVelocity = Accelerate(horizontalVelocity, wishDirection, wishSpeed * _airWishSpeedMultiplier, airAcceleration, deltaTime);
                    horizontalVelocity = ApplyAirControl(horizontalVelocity, wishDirection, deltaTime, wishControlScale);
                    verticalVelocity -= _gravity * deltaTime;
                }
            }

            if (CanConsumeWallKick())
            {
                ConsumeWallKick(ref horizontalVelocity, ref verticalVelocity);
            }
            else if (CanConsumeJump())
            {
                _lastConsumedHopMode = _pendingHopMode;
                RegisterConsumedHop(_lastConsumedHopMode);
                horizontalVelocity = ApplyJumpMomentumRetention(horizontalVelocity, _pendingHopMode);
                verticalVelocity = Mathf.Sqrt(2f * _gravity * _jumpHeight);
                _lastJumpRequestTime = -999f;
                _lastGroundedTime = -999f;
                _ignoreGroundUntilTime = Time.time + _jumpGroundingLockoutTime;
                _isGrounded = false;
                _slideEndTime = -999f;
                _frictionSkipFrames = _landingFrictionSkipFrames;
                _pendingHopMode = HopMode.None;
                UpdateWallContactState(horizontalVelocity);
            }

            horizontalVelocity = ApplyLateralCollisionSlide(horizontalVelocity);
            horizontalVelocity = ClampHorizontalSpeed(horizontalVelocity, GetCurrentHorizontalSpeedCap());
            _rbCompo.linearVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
            ClearLateralContactNormals();
            _wasGrounded = _isGrounded;
        }

        private void RequestJump(HopMode hopMode)
        {
            if (hopMode == HopMode.AutoRepeat && IsJumpBuffered() && _pendingHopMode == HopMode.Timed)
                return;

            _lastJumpRequestTime = Time.time;
            _pendingHopMode = hopMode;
        }

        private void RegisterConsumedHop(HopMode hopMode)
        {
            if (hopMode == HopMode.Timed)
            {
                _timedHopCount++;
                return;
            }

            if (hopMode == HopMode.AutoRepeat)
                _autoRepeatHopCount++;
        }

        private void ConsumeSlideRequest(ref Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            if (!_slideRequested)
                return;

            _slideRequested = false;
            if (!CanStartSlide(horizontalVelocity, wishDirection))
                return;

            StartSlide(ref horizontalVelocity, wishDirection);
        }

        private bool CanStartSlide(Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            return _enableSlide
                   && _isGrounded
                   && Time.time >= _nextSlideTime
                   && !IsSlideActive()
                   && (horizontalVelocity.sqrMagnitude > 0.25f || wishDirection.sqrMagnitude > Mathf.Epsilon);
        }

        private void StartSlide(ref Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            _slideDirection = GetSlideStartDirection(horizontalVelocity, wishDirection);

            float startSpeed = Mathf.Max(horizontalVelocity.magnitude + _slideStartBoost, _slideMinStartSpeed);
            float slideCap = EffectiveMaxSpeed * _slideSpeedCapMultiplier;
            horizontalVelocity = ClampHorizontalSpeed(_slideDirection * startSpeed, slideCap);

            _slideEndTime = Time.time + _slideDuration;
            _nextSlideTime = Time.time + _slideDuration + _slideCooldown;
            _slideMomentumCapUntilTime = Time.time + _slideDuration + _slideMomentumCapDuration;
            _slideCount++;
            _frictionSkipFrames = 0;
        }

        private Vector3 ApplySlideMovement(Vector3 horizontalVelocity, Vector3 wishDirection, float deltaTime)
        {
            if (!IsSlideActive())
                return horizontalVelocity;

            if (wishDirection.sqrMagnitude > Mathf.Epsilon && _slideInputControl > 0f)
            {
                float steer = (1f - Mathf.Exp(-_slideSteerResponsiveness * deltaTime)) * _slideInputControl;
                _slideDirection = Vector3.Slerp(_slideDirection, wishDirection, steer).normalized;

                float speed = horizontalVelocity.magnitude;
                if (speed > Mathf.Epsilon)
                    horizontalVelocity = Vector3.Slerp(horizontalVelocity.normalized, _slideDirection, steer).normalized * speed;
            }

            return MovementMotor.ApplyFriction(horizontalVelocity, 0f, _slideFriction, deltaTime);
        }

        private Vector3 GetSlideStartDirection(Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            if (horizontalVelocity.sqrMagnitude > 0.25f)
                return horizontalVelocity.normalized;

            if (wishDirection.sqrMagnitude > Mathf.Epsilon)
                return wishDirection.normalized;

            return _entity.transform.forward;
        }

        private bool IsSlideActive()
        {
            return _enableSlide && _isGrounded && Time.time < _slideEndTime;
        }

        private Vector3 GetWishDirection()
        {
            Vector3 localInput = new Vector3(_moveInput.x, 0f, _moveInput.y);
            if (localInput.sqrMagnitude <= Mathf.Epsilon)
                return Vector3.zero;

            return _entity.transform.TransformDirection(localInput).normalized;
        }

        private Vector3 DampenWallKickReturnControl(Vector3 wishDirection, out float controlScale)
        {
            controlScale = 1f;

            if (!ShouldDampenWallKickReturnControl() || wishDirection.sqrMagnitude <= Mathf.Epsilon)
                return wishDirection;

            Vector3 exitDirection = _lastWallKickNormal.normalized;
            Vector3 returnDirection = -exitDirection;
            float returnAmount = Vector3.Dot(wishDirection, returnDirection);
            if (returnAmount <= 0f)
                return wishDirection;

            float elapsed = Time.time - _lastWallKickTime;
            float recovery = _wallKickReturnControlDampingTime > 0f
                ? Mathf.Clamp01(elapsed / _wallKickReturnControlDampingTime)
                : 1f;
            float returnScale = Mathf.Lerp(_wallKickReturnControlScale, 1f, recovery);

            Vector3 returnComponent = returnDirection * returnAmount;
            Vector3 freeComponent = wishDirection - returnComponent;
            Vector3 dampedDirection = freeComponent + returnComponent * returnScale;
            dampedDirection.y = 0f;

            controlScale = Mathf.Clamp01(dampedDirection.magnitude);
            return controlScale > Mathf.Epsilon ? dampedDirection / controlScale : Vector3.zero;
        }

        private bool ShouldDampenWallKickReturnControl()
        {
            return !_isGrounded
                   && _wallKickReturnControlDampingTime > 0f
                   && Time.time - _lastWallKickTime <= _wallKickReturnControlDampingTime
                   && _lastWallKickNormal.sqrMagnitude > Mathf.Epsilon;
        }

        private bool ShouldApplyGroundFriction()
        {
            if (_frictionSkipFrames > 0)
            {
                _frictionSkipFrames--;
                return false;
            }

            return !ShouldSkipGroundFriction();
        }

        private void UpdateGroundState()
        {
            if (Time.time < _ignoreGroundUntilTime || _rbCompo.linearVelocity.y > 0.1f)
            {
                _isGrounded = false;
                return;
            }

            Vector3 origin = GetGroundCheckOrigin(out float radius);
            bool grounded = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                _groundCheckDistance,
                GetGroundLayerMask(),
                QueryTriggerInteraction.Ignore
            );

            _isGrounded = grounded && hit.normal.y >= _minGroundNormalY;

            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
                _airWallKickCount = 0;
                _lastWallKickNormal = Vector3.zero;

                if (!_wasGrounded)
                    _frictionSkipFrames = _landingFrictionSkipFrames;
            }
        }

        private void UpdateWallContactState(Vector3 horizontalVelocity)
        {
            bool wasWallRiding = _isWallRiding;

            if (_isGrounded || !_enableWallKick || Time.time < _wallKickCooldownUntil)
            {
                ClearWallTouch();
                return;
            }

            if (_wallKickMinSpeed > 0f && horizontalVelocity.magnitude < _wallKickMinSpeed)
            {
                ClearWallTouch();
                return;
            }

            if (!TryFindKickableWall(out RaycastHit wallHit))
            {
                ClearWallTouch();
                return;
            }

            if (!CanAttachToWall(wallHit.normal, horizontalVelocity))
            {
                ClearWallTouch();
                return;
            }

            _isTouchingWall = true;
            _isWallRiding = true;
            _wallNormal = wallHit.normal;
            _wallForward = GetWallForward(_wallNormal, GetWallTravelReference(_wallNormal, horizontalVelocity));
            _lastWallContactTime = Time.time;

            if (!wasWallRiding)
            {
                _wallRideStartTime = Time.time;
                _wallRideEnterCount++;
                ClearBufferedTimedJump();
            }
        }

        private bool TryFindKickableWall(out RaycastHit wallHit)
        {
            wallHit = default;
            bool foundWall = false;
            float closestDistance = float.MaxValue;

            Vector3 basePosition = _entity.transform.position;
            float lowerHeight = Mathf.Max(0.35f, _wallCheckHeight - 0.45f);
            float upperHeight = _wallCheckHeight + 0.45f;

            TryWallRaysFrom(basePosition + Vector3.up * _wallCheckHeight, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRaysFrom(basePosition + Vector3.up * lowerHeight, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRaysFrom(basePosition + Vector3.up * upperHeight, ref wallHit, ref foundWall, ref closestDistance);

            return foundWall;
        }

        private void TryWallRaysFrom(Vector3 origin, ref RaycastHit wallHit, ref bool foundWall, ref float closestDistance)
        {
            Vector3 forward = _entity.transform.forward;
            Vector3 right = _entity.transform.right;

            TryWallRay(origin, forward, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, right, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, -right, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, (forward + right).normalized, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, (forward - right).normalized, ref wallHit, ref foundWall, ref closestDistance);
        }

        private void TryWallRay(Vector3 origin, Vector3 direction, ref RaycastHit bestHit, ref bool foundWall, ref float closestDistance)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            RaycastHit hit;
            bool hitWall = _wallCheckRadius > 0f
                ? Physics.SphereCast(origin, _wallCheckRadius, direction, out hit, _wallCheckDistance, GetWallLayerMask(), QueryTriggerInteraction.Ignore)
                : Physics.Raycast(origin, direction, out hit, _wallCheckDistance, GetWallLayerMask(), QueryTriggerInteraction.Ignore);

            if (!hitWall)
                return;

            if (!IsKickableWall(hit) || hit.distance >= closestDistance)
                return;

            bestHit = hit;
            foundWall = true;
            closestDistance = hit.distance;
        }

        private bool IsKickableWall(RaycastHit hit)
        {
            return Mathf.Abs(hit.normal.y) <= 0.2f;
        }

        private bool CanAttachToWall(Vector3 wallNormal, Vector3 horizontalVelocity)
        {
            Vector3 normalizedWallNormal = wallNormal.normalized;

            if (IsSameAsLastKickedWall(normalizedWallNormal))
            {
                if (Time.time < _sameWallReattachLockedUntilTime)
                    return false;

                if (GetLastWallKickOutwardDistance() < _sameWallReattachMinDistance)
                    return false;

                float approachSpeed = Vector3.Dot(horizontalVelocity, -normalizedWallNormal);
                if (_sameWallReattachApproachSpeed > 0f && approachSpeed < _sameWallReattachApproachSpeed)
                    return false;
            }

            float awaySpeed = Vector3.Dot(horizontalVelocity, normalizedWallNormal);
            return awaySpeed <= _wallRideAwaySpeedThreshold;
        }

        private bool IsSameAsLastKickedWall(Vector3 normalizedWallNormal)
        {
            return _lastWallKickNormal.sqrMagnitude > Mathf.Epsilon
                   && Vector3.Dot(normalizedWallNormal, _lastWallKickNormal.normalized) >= _sameWallNormalDotThreshold;
        }

        private float GetLastWallKickOutwardDistance()
        {
            if (_lastWallKickNormal.sqrMagnitude <= Mathf.Epsilon)
                return float.MaxValue;

            Vector3 fromKick = _entity.transform.position - _lastWallKickPosition;
            return Mathf.Max(0f, Vector3.Dot(fromKick, _lastWallKickNormal.normalized));
        }

        private float GetSameWallReattachDistanceRemaining()
        {
            if (_lastWallKickNormal.sqrMagnitude <= Mathf.Epsilon)
                return 0f;

            return Mathf.Max(0f, _sameWallReattachMinDistance - GetLastWallKickOutwardDistance());
        }

        private Vector3 GetWallForward(Vector3 wallNormal, Vector3 wishDirection)
        {
            return WallMove.GetWallForward(wallNormal, wishDirection, _entity.transform);
        }

        private Vector3 GetWallTravelReference(Vector3 wallNormal, Vector3 horizontalVelocity)
        {
            return WallMove.GetTravelReference(wallNormal, horizontalVelocity, _entity.transform);
        }

        private Vector3 ApplyWallRideMovement(Vector3 horizontalVelocity, Vector3 wishDirection, float deltaTime)
        {
            return WallMove.ApplyRideMovement(
                horizontalVelocity,
                wishDirection,
                _wallNormal,
                _wallContactVelocityTrim,
                _wallRideMaxSpeed,
                _wallRideAcceleration,
                deltaTime);
        }

        private void ApplyWallRideGravity(ref float verticalVelocity, float deltaTime)
        {
            WallMove.ApplyRideGravity(
                ref verticalVelocity,
                _wallRideGravity,
                _wallRideMaxFallSpeed,
                _wallRideMaxRiseSpeed,
                _wallRideUpwardBrake,
                deltaTime);
        }

        private bool CanConsumeWallKick()
        {
            return IsJumpBuffered()
                   && _pendingHopMode == HopMode.Timed
                   && _isWallRiding
                   && Time.time - _wallRideStartTime >= _wallKickMinimumRideTime
                   && _lastJumpRequestTime >= _wallRideStartTime
                   && CanUseWallKickCount();
        }

        private void ClearBufferedTimedJump()
        {
            if (_pendingHopMode != HopMode.Timed)
                return;

            _lastJumpRequestTime = -999f;
            _pendingHopMode = HopMode.None;
        }

        private void ConsumeWallKick(ref Vector3 horizontalVelocity, ref float verticalVelocity)
        {
            Vector3 wallNormal = _wallNormal.normalized;
            horizontalVelocity = WallMove.BuildKickVelocity(
                wallNormal,
                _wallForward,
                horizontalVelocity,
                _entity.transform,
                _wallKickHorizontalImpulse,
                _wallKickForwardImpulse,
                _wallKickForwardRetention,
                _wallKickViewAssist,
                _wallKickMinimumExitSpeed);
            horizontalVelocity = ClampHorizontalSpeed(horizontalVelocity, EffectiveMaxSpeed * _wallKickSpeedCapMultiplier);
            verticalVelocity = Mathf.Max(verticalVelocity, _wallKickVerticalVelocity);

            _lastWallKickNormal = wallNormal;
            _lastWallKickPosition = _entity.transform.position;
            _lastWallKickTime = Time.time;
            _wallKickMomentumCapUntilTime = Mathf.Max(_wallKickMomentumCapUntilTime, Time.time + _wallKickMomentumCapDuration);
            _sameWallReattachLockedUntilTime = Mathf.Max(_sameWallReattachLockedUntilTime, Time.time + _sameWallReattachCooldown);
            _airWallKickCount++;
            ClearWallTouch();
            _lastJumpRequestTime = -999f;
            _pendingHopMode = HopMode.None;
            _ignoreGroundUntilTime = Time.time + _jumpGroundingLockoutTime;
            _wallKickCooldownUntil = Time.time + _wallKickDetachCooldown;
        }

        private bool IsWallContactAvailable()
        {
            return Time.time - _lastWallContactTime <= _wallKickCoyoteTime
                   && _wallNormal.sqrMagnitude > Mathf.Epsilon
                   && Time.time >= _wallKickCooldownUntil;
        }

        private bool CanUseWallKickCount()
        {
            return _maxAirWallKicks <= 0 || _airWallKickCount < _maxAirWallKicks;
        }

        private bool IsNewWallForRepeatKick()
        {
            if (!_requireNewWallForRepeatKick || _airWallKickCount == 0 || _lastWallKickNormal.sqrMagnitude <= Mathf.Epsilon)
                return true;

            return Vector3.Dot(_wallNormal.normalized, _lastWallKickNormal.normalized) < _sameWallNormalDotThreshold;
        }

        private void ClearWallTouch()
        {
            _isTouchingWall = false;
            _isWallRiding = false;
            _wallRideStartTime = -999f;

            if (Time.time - _lastWallContactTime <= _wallKickCoyoteTime)
                return;

            _wallNormal = Vector3.zero;
            _wallForward = Vector3.zero;
        }

        private Vector3 GetGroundCheckOrigin(out float radius)
        {
            radius = _groundCheckRadius;

            if (_capsuleCollider == null)
                return _entity.transform.position + Vector3.up * (_groundCheckRadius + 0.05f);

            Vector3 scale = _capsuleCollider.transform.lossyScale;
            float capsuleRadius = _capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float capsuleHeight = Mathf.Max(_capsuleCollider.height * Mathf.Abs(scale.y), capsuleRadius * 2f);

            radius = Mathf.Min(_groundCheckRadius, capsuleRadius * 0.96f);
            Vector3 center = _capsuleCollider.transform.TransformPoint(_capsuleCollider.center);
            float lowerSphereOffset = capsuleHeight * 0.5f - capsuleRadius;

            return center + Vector3.down * lowerSphereOffset;
        }

        private int GetGroundLayerMask()
        {
            return _groundLayer.value == 0 ? Physics.DefaultRaycastLayers : _groundLayer.value;
        }

        private int GetWallLayerMask()
        {
            return _wallLayer.value == 0 ? Physics.DefaultRaycastLayers : _wallLayer.value;
        }

        private Vector3 ApplyJumpMomentumRetention(Vector3 horizontalVelocity, HopMode hopMode)
        {
            float retention = hopMode == HopMode.AutoRepeat
                ? _autoJumpHorizontalRetention
                : _timedJumpHorizontalRetention;

            return horizontalVelocity * retention;
        }

        private Vector3 ApplyFriction(Vector3 horizontalVelocity, float deltaTime)
        {
            return MovementMotor.ApplyFriction(horizontalVelocity, _groundStopSpeed, _groundFriction, deltaTime);
        }

        private static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float wishSpeed, float acceleration, float deltaTime)
        {
            return MovementMotor.Accelerate(currentVelocity, wishDirection, wishSpeed, acceleration, deltaTime);
        }

        private Vector3 ApplyAirControl(Vector3 horizontalVelocity, Vector3 wishDirection, float deltaTime, float controlScale = 1f)
        {
            return MovementMotor.ApplyAirControl(horizontalVelocity, wishDirection, _airControlResponsiveness, deltaTime, controlScale);
        }

        private Vector3 ApplyLateralCollisionSlide(Vector3 horizontalVelocity)
        {
            if (_lateralContactCount == 0 || Time.time - _lastLateralContactTime > _lateralCollisionSlideGraceTime)
                return horizontalVelocity;

            Vector3 result = horizontalVelocity;
            for (int i = 0; i < _lateralContactCount; i++)
            {
                Vector3 normal = _lateralContactNormals[i];
                float intoSurfaceSpeed = Vector3.Dot(result, -normal);
                if (intoSurfaceSpeed > 0f)
                    result += normal * (intoSurfaceSpeed * _lateralCollisionSlideStrength);
            }

            return result;
        }

        private void AddLateralContactNormal(Vector3 normal)
        {
            for (int i = 0; i < _lateralContactCount; i++)
            {
                if (Vector3.Dot(_lateralContactNormals[i], normal) > 0.94f)
                    return;
            }

            if (_lateralContactCount >= MaxLateralContactNormals)
                return;

            _lateralContactNormals[_lateralContactCount] = normal;
            _lateralContactCount++;
            _lastLateralContactTime = Time.time;
        }

        private void ClearLateralContactNormals()
        {
            _lateralContactCount = 0;
        }

        private void ApplyLowFrictionColliderMaterial()
        {
            if (!_useLowFrictionColliderMaterial || _capsuleCollider == null)
                return;

            _capsuleCollider.sharedMaterial = GetLowFrictionMaterial();
        }

        private PhysicsMaterial GetLowFrictionMaterial()
        {
            if (_lowFrictionMaterial != null)
                return _lowFrictionMaterial;

            _lowFrictionMaterial = new PhysicsMaterial("Vagabond Low Friction")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                hideFlags = HideFlags.DontSave
            };

            return _lowFrictionMaterial;
        }

        private float GetAirAccelerationScale(Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            if (wishDirection.sqrMagnitude <= Mathf.Epsilon)
                return 0f;

            if (horizontalVelocity.magnitude < _minimumSpeedForStrafeScaling)
                return 1f;

            float steerAngle = Vector3.Angle(horizontalVelocity, wishDirection);
            float angleStrafeFactor = Mathf.InverseLerp(_smallSteerBonusAngle, _fullStrafeBonusAngle, steerAngle);
            float inputStrafeFactor = Mathf.Clamp01(Mathf.Abs(_moveInput.x) * _lateralInputStrafeInfluence);
            float strafeFactor = Mathf.Max(angleStrafeFactor, inputStrafeFactor);

            return Mathf.Lerp(_forwardAirAccelerationScale, _strafeAirAccelerationScale, strafeFactor);
        }

        private bool ShouldSkipGroundFriction()
        {
            return _skipFrictionOnBufferedJump && IsJumpBuffered() && _pendingHopMode == HopMode.Timed;
        }

        private bool IsJumpBuffered()
        {
            return Time.time - _lastJumpRequestTime <= _jumpBufferTime;
        }

        private bool CanConsumeJump()
        {
            bool canUseGround = _isGrounded || Time.time - _lastGroundedTime <= _coyoteTime;

            return IsJumpBuffered() && canUseGround;
        }

        private void ActivateCombatMomentumCap()
        {
            _combatMomentumCapMultiplier = Mathf.Max(_combatMomentumCapMultiplier, _combatMomentumSpeedCapMultiplier);
            _combatMomentumCapUntilTime = Mathf.Max(_combatMomentumCapUntilTime, Time.time + _combatMomentumCapDuration);
        }

        private float GetEffectiveBaseSpeed()
        {
            return MomentumModifier.GetEffectiveBaseSpeed(_baseMaxSpeed, _bloodStacks, _bloodSpeedBonusPerStack, _maxBloodStacksForMovement);
        }

        private float GetBloodSpeedMultiplier()
        {
            return MomentumModifier.GetBloodSpeedMultiplier(_bloodStacks, _bloodSpeedBonusPerStack, _maxBloodStacksForMovement);
        }

        private float GetBhopSpeedCap()
        {
            return MomentumModifier.GetBhopSpeedCap(GetEffectiveBaseSpeed(), _bhopSpeedMultiplier);
        }

        private float GetCurrentHorizontalSpeedCap()
        {
            return MomentumModifier.GetCurrentHorizontalSpeedCap(
                GetEffectiveBaseSpeed(),
                _bhopSpeedMultiplier,
                _combatMomentumCapMultiplier,
                _combatMomentumCapUntilTime,
                _wallKickSpeedCapMultiplier,
                _wallKickMomentumCapUntilTime,
                _slideSpeedCapMultiplier,
                _slideMomentumCapUntilTime,
                Time.time);
        }

        private static Vector3 GetHorizontalVelocity(Vector3 velocity)
        {
            return MovementMotor.GetHorizontalVelocity(velocity);
        }

        private static Vector3 ClampHorizontalSpeed(Vector3 horizontalVelocity, float maxSpeed)
        {
            return MovementMotor.ClampHorizontalSpeed(horizontalVelocity, maxSpeed);
        }
    }
}
