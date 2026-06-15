using _Code.EntityCompo;
using Code.Entities;
using GondrLib.ObjectPool.RunTime;
using Unity.Behavior;
using UnityEngine;

namespace Code.Enemies
{
    public abstract class Enemy : Entity, IPoolable
    {
        [field: SerializeField] public EntityFinderSO PlayerFinder { get; private set; }
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }

        public BehaviorGraphAgent BTAgent { get; private set; }
        public GameObject GameObject => gameObject;

        public float detectRange;
        public float attackRange;

        protected Pool _myPool;
        
        protected virtual void Start()
        {
            BTAgent = GetComponent<BehaviorGraphAgent>();
            
            var target = GetBlackboardVariable<Transform>("Target");

            if (target == null)
            {
                return;
            }

            target.Value = PlayerFinder.Target.transform;
        }

        public BlackboardVariable<T> GetBlackboardVariable<T>(string key)
            => BTAgent.GetVariable(key, out BlackboardVariable<T> result) ? result : null;
        
        public override void EntityDestroy()
        {
            if (_myPool != null)
                _myPool.Push(this);
            else
                base.EntityDestroy();
        }
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}