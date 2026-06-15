using _Code.EntityCompo;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo
{
    public class EntityAnimator : MonoBehaviour, IEntityComponent
    {
        public UnityEvent<Vector3, Quaternion> OnAnimatorMoveEvent;
        [field: SerializeField] public Animator animator {get; private set;}

        public bool ApplyRootMotion
        {
            get => animator.applyRootMotion;
            set => animator.applyRootMotion = value;
        }
        
        private _Code.EntityCompo.Entity _entity;

        public void Initialize(_Code.EntityCompo.Entity entity)
        {
            _entity = entity;
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            OnAnimatorMoveEvent?.Invoke(animator.deltaPosition, animator.deltaRotation);
        }
        
        public void HandleDeadEvent()
        {
            animator.enabled = false;
        }

        public void SetAllBoolParamFalse()
        {
            if (animator == null) return;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(param.name, false);
                }
            }
        }

        public void SetParam(int hash, float value) => animator.SetFloat(hash, value);
        public void SetParam(int hash, bool value) => animator.SetBool(hash, value);
        public void SetParam(int hash, int value) => animator.SetInteger(hash, value);
        public void SetParam(int hash) => animator.SetTrigger(hash);

        public void SetParam(int hash, float value, float dampTime)
            => animator.SetFloat(hash, value, dampTime, Time.deltaTime);

        public void SetAnimatorOff()
        {
            animator.enabled = false;
        }
        
    }
}