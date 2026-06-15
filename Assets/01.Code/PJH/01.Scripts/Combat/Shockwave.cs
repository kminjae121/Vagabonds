using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using GondrLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.Combat
{
    public class Shockwave : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        [SerializeField] private AttackDataSO shockwaveAttackData;
        
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        
        private float _damage;
        private float _lifeTime;
        private float _scale;
        private float _startScale;

        public void Initialize(float lifeTime, float damage, float scale)
        {
            _lifeTime = lifeTime;
            _damage = damage;
            _startScale = _scale = scale;
        }
        
        private void Update()
        {
            SetShockwaveScale();
            CheckLifeTime();
        }

        private void SetShockwaveScale()
        {
            _scale += Time.deltaTime * 2;
            transform.localScale = new Vector3(_scale, 0, _scale);
        }
        
        private void CheckLifeTime()
        {
            _lifeTime -= Time.deltaTime;

            if (_lifeTime <= 0)
                DestroyShockwave();
        }

        private void DestroyShockwave()
        {
            if (_myPool != null)
                _myPool.Push(this);
            else
                Destroy(gameObject);
        }

        public void HandleShockwaveDamaged(Collider other)
        {
            if (other.gameObject.TryGetComponent(out IDamageable damageable))
            {
                var data = new DamageData
                {
                    damage = _damage,
                    damageType = shockwaveAttackData.damageType
                };

                damageable.ApplyDamage(data, other.transform.position, transform.forward, shockwaveAttackData, null);
            }
        }
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            transform.localScale = new Vector3(_startScale, 1, _startScale);
        }
    }
}