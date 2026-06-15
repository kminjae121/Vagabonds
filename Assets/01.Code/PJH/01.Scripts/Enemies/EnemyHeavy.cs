using Code.Combat;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.Enemies
{
    public class EnemyHeavy : CommonEnemy
    {
        [SerializeField] private PoolItemSO shockwaveItem;
        [SerializeField] private float shockwaveLifeTime = 3f;
        [SerializeField] private float shockwaveDamage = 10f;
        [SerializeField] private float shockwaveStartScale = 2.5f;
        
        [Inject] private PoolManagerMono _poolManager;

        public void SpawnShockWave()
        {
            var shockwave = _poolManager.Pop<Shockwave>(shockwaveItem);
            shockwave.Initialize(shockwaveLifeTime, shockwaveDamage, shockwaveStartScale);
            shockwave.transform.position = transform.position;;
        }
    }
}