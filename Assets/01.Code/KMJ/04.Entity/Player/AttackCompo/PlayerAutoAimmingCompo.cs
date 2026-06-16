using _Code.EntityCompo.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.EntityCompo.Combat
{
    public class PlayerAutoAimmingCompo : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsEnemy;
        [SerializeField] private GameObject aimUI;
        [SerializeField] private Image uiImage;
        [SerializeField] private Sprite baseImage;
        [SerializeField] private Sprite aimImage;
        [SerializeField] private Color uiRGBColor;
        [SerializeField] private EnemyAimUI _aimUI;
        [SerializeField] private float _targetMemoryTime = 0.45f;

        [field: SerializeField] public GameObject aimingObject { get; set; }

        public float sphereRadius = 0.5f;
        public float maxDistance = 100f;
        public GameObject CurrentTarget => aimingObject;
        public bool HasTarget => aimingObject != null && Time.time <= _lastTargetSeenTime + _targetMemoryTime;

        private float _lastTargetSeenTime = -999f;

        public void Initialize(Entity entity)
        {
        }

        public void ShootRayForCheckEnemy(bool canLockTarget)
        {
            if (!canLockTarget || Camera.main == null)
            {
                ClearAimFeedback(false);
                return;
            }

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, maxDistance, whatIsEnemy, QueryTriggerInteraction.Collide)
                && TryGetAimedEnemy(hit.collider.gameObject, out EnemyAimed aimed))
            {
                SetUIActive(true);
                aimed.AimmingThis();

                if (uiImage != null)
                    uiImage.color = Color.red;

                aimingObject = aimed.gameObject;
                _lastTargetSeenTime = Time.time;

                if (!aimed.isTarget)
                    return;

                if (_aimUI != null)
                    _aimUI._isBoosted = true;

                if (uiImage != null)
                {
                    uiImage.color = uiRGBColor;
                    uiImage.sprite = aimImage;
                }
                return;
            }

            ClearAimFeedback(true);
        }

        public void SetUIActive(bool isActive)
        {
            if (aimUI != null)
                aimUI.SetActive(isActive);
        }

        public void SetEnemyNull()
        {
            aimingObject = null;
            _lastTargetSeenTime = -999f;
        }

        private void ClearAimFeedback(bool allowMemory)
        {
            if (allowMemory && HasTarget)
                return;

            if (aimingObject != null && TryGetAimedEnemy(aimingObject, out EnemyAimed aimed))
                aimed.StartCoroutineInScript();

            aimingObject = null;
            _lastTargetSeenTime = -999f;

            if (uiImage != null)
            {
                uiImage.color = Color.black;
                uiImage.sprite = baseImage;
            }

            SetUIActive(false);
        }

        private static bool TryGetAimedEnemy(GameObject target, out EnemyAimed aimed)
        {
            if (target.TryGetComponent(out aimed))
                return true;

            aimed = target.GetComponentInParent<EnemyAimed>();
            if (aimed != null)
                return true;

            aimed = target.GetComponentInChildren<EnemyAimed>();
            return aimed != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (Camera.main == null)
                return;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ray.origin, sphereRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxDistance);
        }
    }
}
