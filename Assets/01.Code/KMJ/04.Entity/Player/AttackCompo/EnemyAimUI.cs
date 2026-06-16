using UnityEngine;

namespace _Code.EntityCompo.Enemy
{
    public class EnemyAimUI : MonoBehaviour
    {
        [field: SerializeField] public GameObject targetingUI { get; set; }
    
        public float normalSpeed = 100f;    
        public float boostedSpeed = 500f;  
        public float smoothTime = 2f;

        public bool _isBoosted { get; set; } = false;      
        private float _currentSpeed;

        private void Awake()
        {
        }

        void Start()
        {
            _currentSpeed = normalSpeed;
        }

        private void Update()
        {
            TurningAimming();
        }

        public void TurningAimming()
        {
            float targetSpeed = _isBoosted ? boostedSpeed : normalSpeed;
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * smoothTime);
        
            targetingUI.transform.Rotate(targetingUI.transform.rotation.x, targetingUI.transform.rotation.y, _currentSpeed * Time.deltaTime, Space.Self);
        }
    }
}