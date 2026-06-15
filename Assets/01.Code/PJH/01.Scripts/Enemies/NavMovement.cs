using _Code.EntityCompo;
using Code.Core.Stats;
using Code.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Enemies
{
    public class NavMovement : MonoBehaviour, IEntityComponent, IAfterInitialize
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float stopOffset = 0.05f;
        [SerializeField] private float rotateSpeed = 10f;
        [SerializeField] private bool isUpdateRotation;
        [SerializeField] private LayerMask whatIsWall;
        [SerializeField] private StatSO moveSpeedStat;

        private Entity _entity;
        private EntityStatCompo _statCompo;
        private Transform _lookAtTrm;
        private float _speedMultiplier = 1f;

        public bool IsArrived => !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + stopOffset;
        public float RemainDistance => agent.pathPending ? -1 : agent.remainingDistance;
        public Vector3 Velocity => agent.velocity;

        public bool IsUpdateRotation
        {
            get => agent.updateRotation;
            
            set => agent.updateRotation = value;
        }

        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            
            set
            {
                _speedMultiplier = value;
                agent.speed = _statCompo.GetStat(moveSpeedStat).Value * _speedMultiplier;
            }
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _statCompo = entity.GetUnitCompo<EntityStatCompo>();
            agent.updateRotation = isUpdateRotation;
        }

        public void AfterInitialize()
        {
            agent.speed = _statCompo.SubscribeStat(moveSpeedStat, HandleMoveSpeedChanged, 1f);
        }

        private void OnDestroy()
        {
            _statCompo.UnSubscribeStat(moveSpeedStat, HandleMoveSpeedChanged);
        }

        private void Update()
        {
            if (_lookAtTrm != null)
                LookAtTarget(_lookAtTrm.position);
            else if (agent.hasPath && !agent.isStopped)
                LookAtTarget(agent.steeringTarget);
        }

        public Quaternion LookAtTarget(Vector3 target, bool isSmooth = false)
        {
            var dir = target - _entity.transform.position;
            dir.y = 0;

            var lookRotation = Quaternion.LookRotation(dir.normalized);

            if (isSmooth)
                _entity.transform.rotation = Quaternion.Slerp(_entity.transform.rotation,
                    lookRotation, Time.deltaTime * rotateSpeed);
            else
                _entity.transform.rotation = lookRotation;

            return lookRotation;
        }
        
        public void SetLookAtTarget(Transform target)
        {
            _lookAtTrm = target;
            IsUpdateRotation = _lookAtTrm == null;
        }       
        
        public void SetStop(bool isStop) => agent.isStopped = isStop;
        public void SetVelocity(Vector3 velocity) => agent.velocity = velocity;
        public void SetSpeed(float speed) => agent.speed = speed;
        public void SetDestination(Vector3 destination) => agent.SetDestination(destination);
        public void WarpToPosition(Vector3 pos) => agent.Warp(pos);
        
        private void HandleMoveSpeedChanged(StatSO stat, float currentValue, float previousValue)
        {
            agent.speed = currentValue * _speedMultiplier;
        }
    }
}