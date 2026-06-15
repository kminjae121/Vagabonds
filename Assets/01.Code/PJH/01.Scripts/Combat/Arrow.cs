using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _01.Member.KMJ._02.Scripts._01.Player.AttackCompo;
using _Code.EntityCompo;
using Code.Entities;
using GondrLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.Entities.Combat
{
    public class Arrow : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }

        [SerializeField] private AttackDataSO arrowAttackData;
        
        public GameObject GameObject => gameObject;
        
        private Rigidbody _rigid;
        private Pool _myPool;
        private Entity _owner;
        
        private float _damage;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }
        
        public void Initialize(Vector3 dir, float speed, float damage, Entity owner)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            _rigid.linearVelocity = dir * speed;
            _damage = damage;
            _owner = owner;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.gameObject.TryGetComponent(out Entity entity) && entity == _owner)
                return;

            if (other.gameObject.CompareTag("Player") && other.gameObject.TryGetComponent(out IDamageable damageable))
            {
                var data = new DamageData
                {
                    damage = _damage,
                    damageType = arrowAttackData.damageType
                };

                damageable.ApplyDamage(data, other.transform.position, transform.forward, arrowAttackData, null);
            }
            
            if (_myPool != null)
                _myPool.Push(this);
            else
                Destroy(gameObject);
        }
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
        
        public void ResetItem()
        {
            _rigid.linearVelocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }
    }
}