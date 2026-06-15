using _Code.EntityCompo.Move;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.MovementTest
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

        private float _yaw;
        private float _pitch;
        private float _roll;

        private void Awake()
        {
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
                _yaw = _target.eulerAngles.y;
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

            transform.position = _target.position + Vector3.up * _eyeHeight;
            UpdateCameraRoll();
            transform.rotation = Quaternion.Euler(_pitch, _yaw, _roll);
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

        private void UpdateCameraRoll()
        {
            float targetRoll = 0f;
            float sharpness = _tiltSharpness;

            if (_movement != null && _target != null)
            {
                targetRoll += -_movement.MoveInput.x * _strafeTiltAngle;

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
