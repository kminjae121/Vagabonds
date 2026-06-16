using System;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.Combat
{
    public class BloodDecal : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }

        public GameObject GameObject => gameObject;

        private Pool _myPool;


        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
        }
    }
}