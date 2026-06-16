using _Code.KDH.EntityCompo.Move;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.KDH.MovementTest
{
    public class MovementTestCameraFollow : MonoBehaviour
    {
        [SerializeField] private PlayerMoveCompo _movement;
        [SerializeField] private Transform _target;
        [SerializeField] private float _eyeHeight = 1.55f;
        [SerializeField] private float _mouseSensitivity = 0.09f;
        [SerializeField] private float _gamepadSensitivity = 180f;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;
        [SerializeField] private bool _lockCursorOnPlay = true;
        [SerializeField] private float _strafeTiltAngle = 3.5f;
        [SerializeField] private float _wallRideTiltAngle = 8f;
        [SerializeField] private float _wallKickTiltPunchAngle = 5f;
        [SerializeField] private float _tiltSharpness = 12f;
        [SerializeField] private float _wallKickTiltSharpness = 18f;
        [SerializeField] private float _slideEyeHeightDrop = 0.42f;
        [SerializeField] private float _slidePitchKickAngle = 4f;
        [SerializeField] private float _slideTiltAngle = 4f;
        [SerializeField] private float _slideFovKick = 4.5f;
        [SerializeField] private float _slideCameraEnterSharpness = 18f;
        [SerializeField] private float _slideCameraExitSharpness = 6f;
        [SerializeField] private float _speedFovKick = 0f;
        [SerializeField] private float _fovSharpness = 5.5f;
        [SerializeField] private float _maxFovChangePerSecond = 18f;
        [SerializeField] private bool _smoothVerticalFollow;
        [SerializeField] private float _verticalFollowSharpness = 14f;
        [SerializeField] private float _verticalSnapDistance = 4f;

        private Camera _camera;
        private float _baseFieldOfView;
        private Vector3 _smoothedPosition;
        private float _yaw;
        private float _pitch;
        private float _roll;
        private float _slideCameraBlend;
        private bool _hasSmoothedPosition;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                enabled = false;
                return;
            }

            _baseFieldOfView = _camera.fieldOfView;

            if (_movement == null)
                _movement = FindFirstObjectByType<PlayerMoveCompo>();

            if (_target == null && _movement != null)
                _target = _movement.transform;

            if (_target == null)
            {
                _movement = FindFirstObjectByType<PlayerMoveCompo>();
                if (_movement != null)
                    _target = _movement.transform;
            }

            if (_target != null)
            {
                _yaw = _target.eulerAngles.y;
                _smoothedPosition = _target.position + Vector3.up * _eyeHeight;
                _hasSmoothedPosition = true;
            }
        }

        private void OnEnable()
        {
            if (_lockCursorOnPlay && Application.isPlaying)
                LockCursor();
        }

        private void OnDisable()
        {
            if (_lockCursorOnPlay && Application.isPlaying)
                UnlockCursor();

            if (_camera != null && _baseFieldOfView > 0f)
                _camera.fieldOfView = _baseFieldOfView;

            _slideCameraBlend = 0f;
        }

        private void Update()
        {
            if (_target == null)
                return;

            UpdateCursorLock();
            UpdateLookRotation();
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            SyncYawFromTarget();

            float slideBlend = GetSmoothedSlideBlend(_movement != null ? _movement.SlideCameraBlend : 0f);
            float eyeHeight = _eyeHeight - _slideEyeHeightDrop * slideBlend;
            Vector3 desiredPosition = _target.position + Vector3.up * eyeHeight;

            transform.position = GetSmoothedCameraPosition(desiredPosition);
            UpdateCameraRoll(slideBlend);
            UpdateCameraFov(slideBlend);
            transform.rotation = Quaternion.Euler(_pitch + _slidePitchKickAngle * slideBlend, _yaw, _roll);
        }

        private Vector3 GetSmoothedCameraPosition(Vector3 desiredPosition)
        {
            if (!_smoothVerticalFollow)
            {
                _smoothedPosition = desiredPosition;
                _hasSmoothedPosition = true;
                return desiredPosition;
            }

            if (!_hasSmoothedPosition)
            {
                _smoothedPosition = desiredPosition;
                _hasSmoothedPosition = true;
                return _smoothedPosition;
            }

            _smoothedPosition.x = desiredPosition.x;
            _smoothedPosition.z = desiredPosition.z;

            float verticalDelta = Mathf.Abs(desiredPosition.y - _smoothedPosition.y);
            if (verticalDelta >= _verticalSnapDistance)
            {
                _smoothedPosition.y = desiredPosition.y;
                return _smoothedPosition;
            }

            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, _verticalFollowSharpness) * Time.deltaTime);
            _smoothedPosition.y = Mathf.Lerp(_smoothedPosition.y, desiredPosition.y, blend);
            return _smoothedPosition;
        }

        private void UpdateCursorLock()
        {
            if (!_lockCursorOnPlay || !Application.isPlaying)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                UnlockCursor();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                LockCursor();
        }

        private void UpdateLookRotation()
        {
            SyncYawFromTarget();

            Vector2 lookDelta = Vector2.zero;

            if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
                lookDelta += Mouse.current.delta.ReadValue() * _mouseSensitivity;

            if (Gamepad.current != null)
                lookDelta += Gamepad.current.rightStick.ReadValue() * (_gamepadSensitivity * Time.deltaTime);

            if (lookDelta.sqrMagnitude <= Mathf.Epsilon)
                return;

            _yaw += lookDelta.x;
            _pitch = Mathf.Clamp(_pitch - lookDelta.y, _minPitch, _maxPitch);

            _target.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        public void LookAtWorldPoint(Vector3 worldPoint)
        {
            if (_target == null)
                return;

            Vector3 toPoint = worldPoint - transform.position;
            if (toPoint.sqrMagnitude <= 0.0001f)
                return;

            Vector3 planarDirection = Vector3.ProjectOnPlane(toPoint, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                _yaw = Quaternion.LookRotation(planarDirection.normalized, Vector3.up).eulerAngles.y;
                _target.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }

            Vector3 direction = toPoint.normalized;
            float targetPitch = -Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
            _pitch = Mathf.Clamp(targetPitch, _minPitch, _maxPitch);
        }

        private void SyncYawFromTarget()
        {
            if (_target == null)
                return;

            float targetYaw = _target.eulerAngles.y;
            if (Mathf.Abs(Mathf.DeltaAngle(_yaw, targetYaw)) > 0.05f)
                _yaw = targetYaw;
        }

        private float GetSmoothedSlideBlend(float targetBlend)
        {
            targetBlend = Mathf.Clamp01(targetBlend);
            float sharpness = targetBlend > _slideCameraBlend
                ? _slideCameraEnterSharpness
                : _slideCameraExitSharpness;
            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * Time.deltaTime);
            _slideCameraBlend = Mathf.Lerp(_slideCameraBlend, targetBlend, blend);

            if (Mathf.Abs(_slideCameraBlend - targetBlend) <= 0.001f)
                _slideCameraBlend = targetBlend;

            return _slideCameraBlend;
        }

        private void UpdateCameraRoll(float slideBlend)
        {
            float targetRoll = 0f;
            float sharpness = _tiltSharpness;

            if (_movement != null && _target != null)
            {
                targetRoll += -_movement.MoveInput.x * _strafeTiltAngle;
                targetRoll += -_movement.MoveInput.x * _slideTiltAngle * slideBlend;

                if (_movement.IsWallRiding || _movement.IsWallKickReady)
                    targetRoll += GetWallSideTilt(_wallRideTiltAngle);

                if (_movement.WallKickFeedbackRemainingTime > 0f)
                {
                    targetRoll += GetWallSideTilt(_wallKickTiltPunchAngle);
                    sharpness = _wallKickTiltSharpness;
                }
            }

            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * Time.deltaTime);
            _roll = Mathf.Lerp(_roll, targetRoll, blend);
        }

        private void UpdateCameraFov(float slideBlend)
        {
            if (_camera == null || _baseFieldOfView <= 0f)
                return;

            float targetFov = _baseFieldOfView + _slideFovKick * slideBlend;
            if (slideBlend > 0.01f && _speedFovKick > 0f && _movement != null)
            {
                float speedBlend = Mathf.InverseLerp(
                    _movement.EffectiveMaxSpeed,
                    Mathf.Max(_movement.EffectiveMaxSpeed + 0.01f, _movement.CurrentSpeedCap),
                    _movement.CurrentHorizontalSpeed);
                targetFov += _speedFovKick * speedBlend * slideBlend;
            }

            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, _fovSharpness) * Time.deltaTime);
            float smoothedFov = Mathf.Lerp(_camera.fieldOfView, targetFov, blend);

            if (_maxFovChangePerSecond > 0f)
            {
                float maxStep = _maxFovChangePerSecond * Time.deltaTime;
                _camera.fieldOfView = Mathf.MoveTowards(_camera.fieldOfView, smoothedFov, maxStep);
                return;
            }

            _camera.fieldOfView = smoothedFov;
        }

        private float GetWallSideTilt(float angle)
        {
            if (_movement == null || _target == null || _movement.WallNormal.sqrMagnitude <= Mathf.Epsilon)
                return 0f;

            float side = Vector3.Dot(_movement.WallNormal.normalized, _target.right);
            if (Mathf.Abs(side) <= 0.01f)
                return 0f;

            return -Mathf.Sign(side) * angle;
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
