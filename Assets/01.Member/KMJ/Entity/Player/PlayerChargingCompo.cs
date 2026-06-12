using _Code.EntityCompo;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerChargingCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private PlayerAutoAimmingCompo aimmingCompo;
        [SerializeField] private float _maxChargingTime = 3f;

        private bool _isCharging;
        private float _chargingSec;

        public void Initialize(Entity entity)
        {
        }

        private void Update()
        {
            if (_isCharging == false)
                return;

            aimmingCompo.ShootRayForCheckEnemy();
            
            _chargingSec += Time.deltaTime;

            if (_chargingSec > _maxChargingTime)
                _chargingSec = _maxChargingTime;
        }

        public void Charging()
        {
            _chargingSec = 0f;
            _isCharging = true;
        }

        public bool EndCharging()
        {
            _isCharging = false;

            return _chargingSec >= _maxChargingTime;
        }

        public GameObject GetEnemyObject() => aimmingCompo.aimingObject;
    }
}