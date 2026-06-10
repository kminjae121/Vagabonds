using _Code.EntityCompo;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class PlayerChargingCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float _maxChargingTime;
        
        private bool _isCharging;
        private float _chargingSec;
        
        public void Initialize(Entity entity)
        {
            
        }
        
        private void Update()
        {
            if (_isCharging)
                _maxChargingTime += Time.deltaTime;
        }

        public void Charging()
        {
            _maxChargingTime = 0;
            _isCharging = true;
        }

        public bool EndCharging()
        {
            _isCharging = false;

            return _maxChargingTime >= _chargingSec;
        }
    }
}