using System;
using GondrLib.Dependencies;
using UnityEngine;

namespace GondrLib.ObjectPool.RunTime
{
    [Provide]
    public class PoolManagerMono : MonoBehaviour,IDependencyProvider
    {
        [SerializeField] private PoolManagerSO poolManager;

        private void Awake()
        {
            poolManager.InitializePool(transform);
        }

        public T Pop<T>(PoolItemSO item) where T : IPoolable
        {
            return (T)poolManager.Pop(item);
        }
        
        public void Push(IPoolable item)
        {
            poolManager.Push(item);
        }
    }
}