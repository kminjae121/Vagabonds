using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.EntityCompo.Combat
{
    public class PlayerChargingCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float _maxChargingTime = 0.7f;
        [SerializeField] private Image aimmingSlider;

        private EntityAnimator _animator;
        private float _chargeStartTime = -999f;
        private bool _isCharging;
        private bool _isSheathed;

        public bool IsCharging => _isCharging;
        public bool IsSheathed
        {
            get
            {
                UpdateSheathCompletion();
                return _isSheathed;
            }
        }

        public float SheathProgress => _maxChargingTime <= 0f
            ? 1f
            : GetSheathProgress();

        public void Initialize(Entity entity)
        {
            _animator = entity.GetUnitCompo<EntityAnimator>();
            ResetState();
        }

        private void Update()
        {
            UpdateSheathCompletion();

            if (aimmingSlider != null)
                aimmingSlider.fillAmount = SheathProgress;
        }

        public void Charging()
        {
            BeginCharging();
        }

        public void BeginCharging()
        {
            _isCharging = true;
            _isSheathed = false;
            _chargeStartTime = Time.time;

            if (aimmingSlider != null)
            {
                aimmingSlider.DOKill();
                aimmingSlider.fillAmount = 0f;
            }
        }

        public bool EndCharging()
        {
            return ConsumeSheathedAttack();
        }

        public bool ConsumeSheathedAttack()
        {
            if (IsSheathed)
            {
                NotifyAttackCommitted();
                return true;
            }

            NotifyAttackCommitted();
            return false;
        }

        public void NotifyAttackCommitted()
        {
            ResetState();
            _animator?.SetBoolean("IDLE");
        }

        public void ResetUI()
        {
            if (aimmingSlider != null)
            {
                aimmingSlider.DOKill();
                aimmingSlider.DOFillAmount(0f, 0.12f);
            }
        }

        private float GetSheathProgress()
        {
            if (_isSheathed)
                return 1f;

            if (!_isCharging)
                return 0f;

            return Mathf.Clamp01((Time.time - _chargeStartTime) / _maxChargingTime);
        }

        private void UpdateSheathCompletion()
        {
            if (_isSheathed || !_isCharging || SheathProgress < 1f)
                return;

            _isSheathed = true;
            _animator?.SetBoolean("NABDO");
        }

        private void ResetState()
        {
            _isCharging = false;
            _isSheathed = false;
            _chargeStartTime = -999f;
        }
    }
}
