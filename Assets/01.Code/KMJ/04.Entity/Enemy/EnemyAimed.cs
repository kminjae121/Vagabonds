using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace _Code.EntityCompo.Enemy
{
    public class EnemyAimed : MonoBehaviour
    {
        [SerializeField] private float _maxAimmingTime = 0.5f;

        public bool isAimmed { get; private set; }
        public bool isTarget { get; private set; }
        public UnityEvent OnAimmedThis;
        public float aimmingTime;

        private Coroutine _aimmingFalseCoroutine;

        private void Update()
        {
            if (!isAimmed)
                return;

            if (isTarget)
                return;

            aimmingTime += Time.deltaTime;
            if (aimmingTime < _maxAimmingTime)
                return;

            OnAimmedThis?.Invoke();
            aimmingTime = _maxAimmingTime;
            isTarget = true;
        }

        public void AimmingThis()
        {
            if (_aimmingFalseCoroutine != null)
            {
                StopCoroutine(_aimmingFalseCoroutine);
                _aimmingFalseCoroutine = null;
            }

            isAimmed = true;
        }

        public void StartCoroutineInScript()
        {
            if (_aimmingFalseCoroutine == null)
                _aimmingFalseCoroutine = StartCoroutine(AimmingFalse());
        }

        public IEnumerator AimmingFalse()
        {
            yield return new WaitForSeconds(0.35f);

            isAimmed = false;
            aimmingTime = 0f;
            isTarget = false;
            _aimmingFalseCoroutine = null;
        }
    }
}
