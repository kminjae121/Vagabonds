using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
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
        private EntityAnimator _animator;

        private bool _isCharging;
        private float _chargingSec;

        public void Initialize(Entity entity)
        {
            _animator = entity.GetEntityCompo<EntityAnimator>();
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
            _animator.SetBoolean("NABDO");
        }

        public bool EndCharging()
        {
            _isCharging = false;
            _animator.SetBoolean("IDLE");
            return _chargingSec >= _maxChargingTime;
        }

        public void ResetUI() => aimmingSlider.DOFillAmount(0, 0.3f);

        public GameObject GetEnemyObject() => aimmingCompo.aimingObject;
    }
}