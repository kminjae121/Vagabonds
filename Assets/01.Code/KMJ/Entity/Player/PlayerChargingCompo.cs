using _Code.EntityCompo;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.EntityCompo.Combat
{
    public class PlayerChargingCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private PlayerAutoAimmingCompo aimmingCompo;
        [SerializeField] private float _maxChargingTime = 1f;
        [SerializeField] private Image aimmingSlider;

        private bool _isCharging;
        private float _chargingSec;

        public void Initialize(Entity entity)
        {
        }

        private void Update()
        {
            aimmingCompo.ShootRayForCheckEnemy(_isCharging);
            
            if (_isCharging == false)
                return;
            
            _chargingSec += Time.deltaTime;
            
            aimmingSlider.fillAmount = _chargingSec / _maxChargingTime;

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

        public void ResetUI() => aimmingSlider.DOFillAmount(0, 0.3f);

        public GameObject GetEnemyObject() => aimmingCompo.aimingObject;
    }
}