using _Code.EntityCompo.Move;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.MovementTest
{
    public class MovementTestCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _eyeHeight = 1.55f;
        [SerializeField] private float _mouseSensitivity = 0.09f;
        [SerializeField] private float _gamepadSensitivity = 180f;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;
        [SerializeField] private bool _lockCursorOnPlay = true;

        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            if (_target == null)
            {
                PlayerMoveCompo movement = FindFirstObjectByType<PlayerMoveCompo>();
                if (movement != null)
                    _target = movement.transform;
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
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
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
