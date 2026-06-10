using System;
using UnityEngine;

namespace _01.Member.KMJ.Entity.Player
{
    public class PlayerMoveCompo : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float _moveSpeed;
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

        public float GetMoveSpeed()
            => _moveSpeed;

        public void SetMoveSpeed(float moveSpeed)
            => _moveSpeed = moveSpeed;

        private void FixedUpdate()
        {
            Vector3 worldDir = _entity.transform.TransformDirection(_moveDir);

            _rbCompo.linearVelocity = new Vector3(
                worldDir.x * _moveSpeed,
                _rbCompo.linearVelocity.y,
                worldDir.z * _moveSpeed
            );
        }

        public void Jump()
        {
            
        }
    }
}