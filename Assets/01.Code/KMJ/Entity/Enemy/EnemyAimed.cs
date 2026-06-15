using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Code.EntityCompo.Enemy
{

    public class EnemyAimed : MonoBehaviour
    {
        public bool isAimmed { get; private set; } = false;
        public bool isTarget { get; private set; } = false;
        public UnityEvent OnAimmedThis;

        private Coroutine aimmingFalseCoroutine;


        public float aimmingTime = 0;
        private float _maxAimmingTime = 0.2f;
        
        private void Update()
        {
            if (isAimmed)
            {
                OnAimmedThis?.Invoke();
                aimmingTime += Time.deltaTime;

                if (aimmingTime >= _maxAimmingTime)
                {
                    aimmingTime = _maxAimmingTime;
                    isTarget = true;
                }
            }
        }

        public void AimmingThis()
        {
            isAimmed = true;
        }

        public void StartCoroutineInScript()
        {
            if (aimmingFalseCoroutine == null)
            {
                aimmingFalseCoroutine = StartCoroutine(AimmingFalse());
            }
        }
        

        public IEnumerator AimmingFalse()
        {
            yield return new WaitForSeconds(0.35f);
        
            isAimmed = false;
            aimmingTime = 0;
            isTarget = false;
        
            aimmingFalseCoroutine = null;
        }
    }
}