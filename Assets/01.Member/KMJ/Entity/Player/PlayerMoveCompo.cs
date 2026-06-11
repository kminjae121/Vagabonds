using UnityEngine;

namespace _Code.EntityCompo.Move
{
    public class PlayerMoveCompo : MonoBehaviour, IEntityComponent
    {
        [Header("Move")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Jump")]
        [SerializeField] private float _jumpPower = 5f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckDistance = 0.2f;

        private Rigidbody _rbCompo;
        private Entity _entity;
        private Vector3 _moveDir;

        public void Initialize(Entity entity)
        {
            _rbCompo = entity.GetComponentInChildren<Rigidbody>();
            _entity = entity;
        }

        public void SetMove(Vector2 dir)
        {
            _moveDir.x = dir.x;
            _moveDir.z = dir.y;
            _moveDir.Normalize();
        }

        public float GetMoveSpeed() => _moveSpeed;

        public void SetMoveSpeed(float moveSpeed)
            => _moveSpeed = moveSpeed;

        public void GravityZero() => _rbCompo.useGravity = false;

        private void FixedUpdate()
        {
            if (_moveDir != Vector3.zero)
                return;
            
            Vector3 worldDir = _entity.transform.TransformDirection(_moveDir);

            _rbCompo.linearVelocity = new Vector3(
                worldDir.x * _moveSpeed,
                _rbCompo.linearVelocity.y,
                worldDir.z * _moveSpeed
            );
        }

        public void Jump()
        {
            if (!IsGrounded())
                return;

            _rbCompo.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(
                _entity.transform.position,
                Vector3.down,
                _groundCheckDistance,
                _groundLayer
            );
        }
    }
}