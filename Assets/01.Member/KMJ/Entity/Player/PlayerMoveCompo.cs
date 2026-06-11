using UnityEngine;
using UnityEngine.Serialization;

namespace _Code.EntityCompo.Move
{
    public class PlayerMoveCompo : MonoBehaviour, IEntityComponent
    {
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
        [SerializeField] private float _bhopSpeedMultiplier = 2.5f;
        [FormerlySerializedAs("_jumpHorizontalRetention")]
        [SerializeField, Range(0f, 1.2f)] private float _timedJumpHorizontalRetention = 1f;
        [SerializeField, Range(0f, 1.2f)] private float _autoJumpHorizontalRetention = 0.92f;
        [SerializeField] private int _landingFrictionSkipFrames;
        [SerializeField] private bool _autoBhopWhenJumpHeld = true;
        [SerializeField] private bool _skipFrictionOnBufferedJump = true;

        [Header("3. Air Steering")]
        [SerializeField] private float _airAcceleration = 72f;
        [FormerlySerializedAs("_airControl")]
        [SerializeField] private float _airControlResponsiveness = 7f;
        [SerializeField] private float _airWishSpeedMultiplier = 1f;
        [SerializeField, Range(0f, 2f)] private float _forwardAirAccelerationScale = 0.45f;
        [SerializeField, Range(0f, 2f)] private float _strafeAirAccelerationScale = 1.15f;
        [SerializeField] private float _minimumSpeedForStrafeScaling = 7f;
        [SerializeField] private float _smallSteerBonusAngle = 6f;
        [SerializeField] private float _fullStrafeBonusAngle = 28f;
        [SerializeField, Range(0f, 1f)] private float _lateralInputStrafeInfluence = 0.75f;

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
        [SerializeField] private float _wallCheckDistance = 0.75f;
        [SerializeField] private float _wallCheckHeight = 1.05f;
        [SerializeField] private float _wallKickMinSpeed = 6.5f;
        [SerializeField] private float _wallKickCoyoteTime = 0.16f;
        [SerializeField] private float _wallKickDetachCooldown = 0.18f;
        [SerializeField] private float _wallContactVelocityTrim = 0.65f;
        [SerializeField] private float _wallSlideGravity = 14f;
        [SerializeField] private float _wallSlideMaxFallSpeed = 6f;
        [SerializeField] private float _wallKickHorizontalImpulse = 14f;
        [SerializeField] private float _wallKickForwardImpulse = 6f;
        [SerializeField, Range(0f, 1f)] private float _wallKickForwardRetention = 0.45f;
        [SerializeField] private float _wallKickVerticalVelocity = 9f;
        [SerializeField] private float _wallKickSpeedCapMultiplier = 2.7f;
        [SerializeField] private int _maxAirWallKicks = 2;
        [SerializeField] private bool _requireNewWallForRepeatKick = true;
        [SerializeField, Range(0f, 1f)] private float _sameWallNormalDotThreshold = 0.85f;

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
        private float _lastWallContactTime = -999f;
        private float _wallKickCooldownUntil = -999f;
        private int _airWallKickCount;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _isTouchingWall;
        private bool _jumpHeld;

        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => _rbCompo != null ? _rbCompo.linearVelocity : Vector3.zero;
        public int BloodStacks => _bloodStacks;
        public float BloodSpeedMultiplier => GetBloodSpeedMultiplier();
        public float EffectiveMaxSpeed => GetEffectiveBaseSpeed();
        public float MaxBhopSpeed => GetBhopSpeedCap();
        public float CurrentSpeedCap => GetCurrentHorizontalSpeedCap();
        public float CombatMomentumRemainingTime => Mathf.Max(0f, _combatMomentumCapUntilTime - Time.time);
        public HopMode LastConsumedHopMode => _lastConsumedHopMode;
        public bool IsTouchingWall => _isTouchingWall;
        public bool IsWallKickReady => IsWallContactAvailable() && CanUseWallKickCount();
        public float WallKickGraceRemainingTime => Mathf.Max(0f, _wallKickCoyoteTime - (Time.time - _lastWallContactTime));
        public int AirWallKickCount => _airWallKickCount;
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
            _wallCheckHeight = Mathf.Max(0f, _wallCheckHeight);
            _wallKickMinSpeed = Mathf.Max(0f, _wallKickMinSpeed);
            _wallKickCoyoteTime = Mathf.Max(0f, _wallKickCoyoteTime);
            _wallKickDetachCooldown = Mathf.Max(0f, _wallKickDetachCooldown);
            _wallContactVelocityTrim = Mathf.Clamp01(_wallContactVelocityTrim);
            _wallSlideGravity = Mathf.Max(0f, _wallSlideGravity);
            _wallSlideMaxFallSpeed = Mathf.Max(0f, _wallSlideMaxFallSpeed);
            _wallKickHorizontalImpulse = Mathf.Max(0f, _wallKickHorizontalImpulse);
            _wallKickForwardImpulse = Mathf.Max(0f, _wallKickForwardImpulse);
            _wallKickForwardRetention = Mathf.Clamp01(_wallKickForwardRetention);
            _wallKickVerticalVelocity = Mathf.Max(0f, _wallKickVerticalVelocity);
            _wallKickSpeedCapMultiplier = Mathf.Max(_bhopSpeedMultiplier, _wallKickSpeedCapMultiplier);
            _maxAirWallKicks = Mathf.Max(0, _maxAirWallKicks);
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

        public void ApplyKillImpulse(Vector3 direction, int killCount = 1)
        {
            ApplyCombatImpulse(direction, killCount);
        }

        public void ApplyCombatImpulse(Vector3 direction, int killCount = 1)
        {
            if (_rbCompo == null)
                return;

            Vector3 impulseDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (impulseDirection.sqrMagnitude <= Mathf.Epsilon)
                impulseDirection = _entity != null ? _entity.transform.forward : transform.forward;

            impulseDirection.Normalize();
            ActivateCombatMomentumCap();

            float killMultiplier = killCount > 1 ? _multiKillImpulseMultiplier : 1f;
            Vector3 velocity = _rbCompo.linearVelocity;
            Vector3 horizontalVelocity = GetHorizontalVelocity(velocity);
            horizontalVelocity += impulseDirection * (_killImpulseSpeed * killMultiplier);
            horizontalVelocity = ClampHorizontalSpeed(horizontalVelocity, GetCurrentHorizontalSpeedCap());

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            velocity.y = Mathf.Max(velocity.y, _killImpulseVerticalLift);
            _rbCompo.linearVelocity = velocity;
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

            Vector3 wishDirection = GetWishDirection();
            float wishSpeed = EffectiveMaxSpeed;
            UpdateWallContactState(horizontalVelocity, wishDirection);

            if (_isGrounded)
            {
                if (ShouldApplyGroundFriction())
                    horizontalVelocity = ApplyFriction(horizontalVelocity, deltaTime);

                horizontalVelocity = Accelerate(horizontalVelocity, wishDirection, wishSpeed, _groundAcceleration, deltaTime);
                verticalVelocity = Mathf.Min(verticalVelocity, -1f);
            }
            else
            {
                float airAcceleration = _airAcceleration * GetAirAccelerationScale(horizontalVelocity, wishDirection);
                horizontalVelocity = Accelerate(horizontalVelocity, wishDirection, wishSpeed * _airWishSpeedMultiplier, airAcceleration, deltaTime);
                horizontalVelocity = ApplyAirControl(horizontalVelocity, wishDirection, deltaTime);
                ApplyAirborneGravity(ref horizontalVelocity, ref verticalVelocity, deltaTime);
            }

            if (CanConsumeWallKick())
            {
                ConsumeWallKick(ref horizontalVelocity, ref verticalVelocity);
            }
            else if (CanConsumeJump())
            {
                _lastConsumedHopMode = _pendingHopMode;
                horizontalVelocity = ApplyJumpMomentumRetention(horizontalVelocity, _pendingHopMode);
                verticalVelocity = Mathf.Sqrt(2f * _gravity * _jumpHeight);
                _lastJumpRequestTime = -999f;
                _lastGroundedTime = -999f;
                _ignoreGroundUntilTime = Time.time + _jumpGroundingLockoutTime;
                _isGrounded = false;
                _frictionSkipFrames = _landingFrictionSkipFrames;
                _pendingHopMode = HopMode.None;
            }

            horizontalVelocity = ClampHorizontalSpeed(horizontalVelocity, GetCurrentHorizontalSpeedCap());
            _rbCompo.linearVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
            _wasGrounded = _isGrounded;
        }

        private void RequestJump(HopMode hopMode)
        {
            if (hopMode == HopMode.AutoRepeat && IsJumpBuffered() && _pendingHopMode == HopMode.Timed)
                return;

            _lastJumpRequestTime = Time.time;
            _pendingHopMode = hopMode;
        }

        private Vector3 GetWishDirection()
        {
            Vector3 localInput = new Vector3(_moveInput.x, 0f, _moveInput.y);
            if (localInput.sqrMagnitude <= Mathf.Epsilon)
                return Vector3.zero;

            return _entity.transform.TransformDirection(localInput).normalized;
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

        private void UpdateWallContactState(Vector3 horizontalVelocity, Vector3 wishDirection)
        {
            if (_isGrounded || !_enableWallKick || Time.time < _wallKickCooldownUntil)
            {
                ClearWallTouch();
                return;
            }

            if (horizontalVelocity.magnitude < _wallKickMinSpeed)
            {
                ClearWallTouch();
                return;
            }

            if (!TryFindKickableWall(out RaycastHit wallHit))
            {
                ClearWallTouch();
                return;
            }

            _isTouchingWall = true;
            _wallNormal = wallHit.normal;
            _wallForward = GetWallForward(_wallNormal, wishDirection);
            _lastWallContactTime = Time.time;
        }

        private bool TryFindKickableWall(out RaycastHit wallHit)
        {
            Vector3 origin = _entity.transform.position + Vector3.up * _wallCheckHeight;
            Vector3 forward = _entity.transform.forward;
            Vector3 right = _entity.transform.right;
            wallHit = default;
            bool foundWall = false;
            float closestDistance = float.MaxValue;

            TryWallRay(origin, forward, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, right, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, -right, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, (forward + right).normalized, ref wallHit, ref foundWall, ref closestDistance);
            TryWallRay(origin, (forward - right).normalized, ref wallHit, ref foundWall, ref closestDistance);

            return foundWall;
        }

        private void TryWallRay(Vector3 origin, Vector3 direction, ref RaycastHit bestHit, ref bool foundWall, ref float closestDistance)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, _wallCheckDistance, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
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

        private Vector3 GetWallForward(Vector3 wallNormal, Vector3 wishDirection)
        {
            Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal).normalized;
            Vector3 referenceDirection = wishDirection.sqrMagnitude > Mathf.Epsilon
                ? wishDirection
                : _entity.transform.forward;

            if (Vector3.Dot(wallForward, referenceDirection) < 0f)
                wallForward = -wallForward;

            return wallForward;
        }

        private void ApplyAirborneGravity(ref Vector3 horizontalVelocity, ref float verticalVelocity, float deltaTime)
        {
            if (!IsWallContactAvailable())
            {
                verticalVelocity -= _gravity * deltaTime;
                return;
            }

            float intoWallSpeed = Vector3.Dot(horizontalVelocity, -_wallNormal);
            if (intoWallSpeed > 0f)
                horizontalVelocity += _wallNormal * (intoWallSpeed * _wallContactVelocityTrim);

            verticalVelocity = Mathf.Max(verticalVelocity - _wallSlideGravity * deltaTime, -_wallSlideMaxFallSpeed);
        }

        private bool CanConsumeWallKick()
        {
            return IsJumpBuffered()
                   && _pendingHopMode == HopMode.Timed
                   && IsWallContactAvailable()
                   && CanUseWallKickCount()
                   && IsNewWallForRepeatKick();
        }

        private void ConsumeWallKick(ref Vector3 horizontalVelocity, ref float verticalVelocity)
        {
            Vector3 wallForward = _wallForward.sqrMagnitude > Mathf.Epsilon
                ? _wallForward
                : GetWallForward(_wallNormal, horizontalVelocity);

            Vector3 alongWallVelocity = Vector3.Project(horizontalVelocity, wallForward);
            float retainedForwardSpeed = Mathf.Max(Vector3.Dot(alongWallVelocity, wallForward) * _wallKickForwardRetention, _wallKickForwardImpulse);

            horizontalVelocity = (_wallNormal.normalized * _wallKickHorizontalImpulse) + (wallForward.normalized * retainedForwardSpeed);
            horizontalVelocity = ClampHorizontalSpeed(horizontalVelocity, EffectiveMaxSpeed * _wallKickSpeedCapMultiplier);
            verticalVelocity = Mathf.Max(verticalVelocity, _wallKickVerticalVelocity);

            _lastWallKickNormal = _wallNormal;
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
            float speed = horizontalVelocity.magnitude;
            if (speed <= Mathf.Epsilon)
                return Vector3.zero;

            float control = speed < _groundStopSpeed ? _groundStopSpeed : speed;
            float drop = control * _groundFriction * deltaTime;
            float nextSpeed = Mathf.Max(speed - drop, 0f);

            return horizontalVelocity * (nextSpeed / speed);
        }

        private static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float wishSpeed, float acceleration, float deltaTime)
        {
            if (wishDirection.sqrMagnitude <= Mathf.Epsilon || wishSpeed <= 0f)
                return currentVelocity;

            float currentSpeed = Vector3.Dot(currentVelocity, wishDirection);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f)
                return currentVelocity;

            float accelSpeed = Mathf.Min(acceleration * wishSpeed * deltaTime, addSpeed);
            return currentVelocity + wishDirection * accelSpeed;
        }

        private Vector3 ApplyAirControl(Vector3 horizontalVelocity, Vector3 wishDirection, float deltaTime)
        {
            if (wishDirection.sqrMagnitude <= Mathf.Epsilon || horizontalVelocity.sqrMagnitude <= 0.01f)
                return horizontalVelocity;

            float speed = horizontalVelocity.magnitude;
            float steer = 1f - Mathf.Exp(-_airControlResponsiveness * deltaTime);
            Vector3 blendedDirection = Vector3.Slerp(horizontalVelocity.normalized, wishDirection, steer).normalized;

            return blendedDirection * speed;
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
            return _baseMaxSpeed * GetBloodSpeedMultiplier();
        }

        private float GetBloodSpeedMultiplier()
        {
            int countedStacks = _maxBloodStacksForMovement > 0
                ? Mathf.Min(_bloodStacks, _maxBloodStacksForMovement)
                : _bloodStacks;

            return 1f + countedStacks * _bloodSpeedBonusPerStack;
        }

        private float GetBhopSpeedCap()
        {
            return GetEffectiveBaseSpeed() * _bhopSpeedMultiplier;
        }

        private float GetCurrentHorizontalSpeedCap()
        {
            float capMultiplier = _bhopSpeedMultiplier;
            if (Time.time < _combatMomentumCapUntilTime)
                capMultiplier = Mathf.Max(capMultiplier, _combatMomentumCapMultiplier);

            return GetEffectiveBaseSpeed() * capMultiplier;
        }

        private static Vector3 GetHorizontalVelocity(Vector3 velocity)
        {
            return new Vector3(velocity.x, 0f, velocity.z);
        }

        private static Vector3 ClampHorizontalSpeed(Vector3 horizontalVelocity, float maxSpeed)
        {
            if (maxSpeed <= 0f)
                return Vector3.zero;

            if (horizontalVelocity.sqrMagnitude <= maxSpeed * maxSpeed)
                return horizontalVelocity;

            return horizontalVelocity.normalized * maxSpeed;
        }
    }
}
