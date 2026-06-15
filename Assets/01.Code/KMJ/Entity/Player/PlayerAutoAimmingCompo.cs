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

        private Player _player;
        private float currentAimmingTime = 0f;
        [field: SerializeField] public GameObject aimingObject { get; set; }

        private EnemyAimUI _aimUI;
        private Transform defaultTarget;
        private bool isLockedOn = false;
        
        public float sphereRadius = 0.5f;
        public float maxDistance = 100f;

        public void Initialize(Entity entity)
        {
            _player = entity as Player;
            _aimUI = aimUI.GetComponent<EnemyAimUI>();
        }

        public void ShootRayForCheckEnemy()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.SphereCast(ray, sphereRadius, out hit, maxDistance, whatIsEnemy))
            {
                if (hit.transform.gameObject != null)
                {
                    uiImage.color = Color.white;
                    SetUIActive(true);
                    CheckIsTimeOver(hit);
                }
            }
            else if (aimingObject != null)
            {               
                if (aimingObject.TryGetComponent(out EnemyAimed aimed))
                {
                    SetUIActive(false);
                    uiImage.color = Color.white;
                    uiImage.sprite = baseImage;
                    SetEnemyNull();
                    aimed.StartCoroutineInScript();
                }
            }
            else
            {
                uiImage.color = Color.white;
                uiImage.sprite = baseImage;
                SetUIActive(false);
            }
        }


        private void CheckIsTimeOver(RaycastHit hit)
        {
            if (hit.transform.gameObject.TryGetComponent(out EnemyAimed aimed))
            {
                aimed.AimmingThis();

                if (aimed.isTarget)
                {
                    _aimUI._isBoosted = true;
                    uiImage.color = uiRGBColor;
                    uiImage.sprite = aimImage;
                    aimingObject = hit.collider.gameObject;
                }
            }
        }

        public void SetUIActive(bool isActive)
        {
            aimUI.SetActive(isActive);
        }

        public void SetEnemyNull()
        {
            aimingObject = null;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            

            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(ray.origin, sphereRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxDistance);
        }
    }
}