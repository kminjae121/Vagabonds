using Code.Core.Events.Bus;
using System.Collections;
using UnityEngine;

namespace Code.Core.Managers
{
    public class HitStopManager : MonoBehaviour
    {
        [SerializeField] private float _maxDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _minTimeScale = 0f;

        private Coroutine _hitStopCoroutine;
        private float _baseTimeScale = 1f;
        private float _baseFixedDeltaTime;
        private bool _isHitStopping;

        private void Awake()
        {
            _baseFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnable()
        {
            Bus<HitStopEvent>.Subscribe(OnHitStopEvent);
        }

        private void OnDisable()
        {
            Bus<HitStopEvent>.Unsubscribe(OnHitStopEvent);
            StopHitStop();
        }

        private void OnHitStopEvent(HitStopEvent evt)
        {
            float duration = Mathf.Clamp(evt.Duration, 0f, _maxDuration);
            if (duration <= 0f)
                return;

            float timeScale = Mathf.Clamp(evt.TimeScale, _minTimeScale, 1f);
            if (_hitStopCoroutine != null)
            {
                if (!evt.OverrideCurrent)
                    return;

                StopCoroutine(_hitStopCoroutine);
            }

            if (!_isHitStopping)
            {
                _baseTimeScale = Time.timeScale;
                _baseFixedDeltaTime = Time.fixedDeltaTime;
            }

            _hitStopCoroutine = StartCoroutine(ProcessHitStop(duration, timeScale));
        }

        private IEnumerator ProcessHitStop(float duration, float timeScale)
        {
            _isHitStopping = true;
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = Mathf.Max(0.0001f, _baseFixedDeltaTime * Mathf.Max(timeScale, 0.01f));

            yield return new WaitForSecondsRealtime(duration);

            RestoreTime();
            _hitStopCoroutine = null;
        }

        private void StopHitStop()
        {
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                _hitStopCoroutine = null;
            }

            if (_isHitStopping)
                RestoreTime();
        }

        private void RestoreTime()
        {
            Time.timeScale = Mathf.Approximately(_baseTimeScale, 0f) ? 1f : _baseTimeScale;
            Time.fixedDeltaTime = _baseFixedDeltaTime > 0f ? _baseFixedDeltaTime : 0.02f;
            _isHitStopping = false;
        }
    }
}
