using _Code.EntityCompo.Move;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.MovementTest
{
    public class MovementTestDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerMoveCompo _movement;
        [SerializeField] private MovementTestCourseController _course;
        [SerializeField] private bool _enableDebugShortcuts = true;
        [SerializeField] private Rect _rect = new(16f, 16f, 500f, 360f);

        private GUIStyle _style;

        private void Awake()
        {
            if (_movement == null)
                _movement = FindFirstObjectByType<PlayerMoveCompo>();

            if (_course == null)
                _course = FindFirstObjectByType<MovementTestCourseController>();
        }

        private void Update()
        {
            if (!_enableDebugShortcuts || _movement == null || Keyboard.current == null)
                return;

            if (Keyboard.current.bKey.wasPressedThisFrame)
                _movement.AddBloodStacks();

            if (Keyboard.current.nKey.wasPressedThisFrame)
                _movement.ClearBloodStacks();

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                Vector3 direction = Camera.main != null
                    ? Camera.main.transform.forward
                    : _movement.transform.forward;

                _movement.ApplyKillImpulse(direction);
            }
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 18,
                    padding = new RectOffset(12, 12, 10, 10)
                };

                _style.normal.textColor = Color.white;
            }

            string text = _movement == null
                ? "MovementTest\nPlayerMoveCompo not found."
                : BuildHudText();

            GUI.Box(_rect, text, _style);
        }

        private string BuildHudText()
        {
            string courseText = _course == null
                ? "Course: not found\n"
                : $"Time: {_course.ElapsedTime:00.00}s / Finish: {(_course.IsFinished ? $"{_course.FinishTime:00.00}s" : "--")}\n" +
                  $"Max: {_course.MaxSpeed:00.00} / Avg: {_course.AverageSpeed:00.00} / Dist: {_course.DistanceTravelled:000.0}m\n" +
                  $"Run: {GetRunStateText()}\n";

            return $"MovementTest Course\n" +
                   courseText +
                   $"Speed: {_movement.CurrentHorizontalSpeed:00.00}\n" +
                   $"Base: {_movement.EffectiveMaxSpeed:00.00} / Cap: {_movement.CurrentSpeedCap:00.00}\n" +
                   $"Blood: {_movement.BloodStacks} ({_movement.BloodSpeedMultiplier:0.00}x)\n" +
                   $"Combat Boost: {_movement.CombatMomentumRemainingTime:0.00}s\n" +
                   $"Wall Kick: {(_movement.IsWallKickReady ? "Ready" : "--")} / Grace: {_movement.WallKickGraceRemainingTime:0.00}s / Count: {_movement.AirWallKickCount}\n" +
                   $"Last Hop: {_movement.LastConsumedHopMode} / Grounded: {_movement.IsGrounded}\n" +
                   $"Mouse: look / WASD: move\n" +
                   $"Space tap: timed / hold: auto\n" +
                   $"K: impulse / B: blood +1 / N: clear\n" +
                   $"R: reset course / Esc: cursor";
        }

        private string GetRunStateText()
        {
            if (_course == null)
                return "Unknown";

            if (_course.IsFinished)
                return "Finished";

            return _course.HasStarted ? "Running" : "Ready";
        }
    }
}
