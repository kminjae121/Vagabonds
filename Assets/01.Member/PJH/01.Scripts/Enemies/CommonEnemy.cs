using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo.Combat;
using Code.Core.GameEvent;
using Map;
using UnityEngine;

namespace Code.Enemies
{
    public class CommonEnemy : Enemy
    {
        [field: SerializeField] public GameEventChannelSO PlayerChannel { get; private set; }
        [field: SerializeField] public bool IsBattleState { get; set; }

        private StateChangeEvent _stateChangeChannel;
        private ActionData _actionData;
        
        protected override void Awake()
        {
            base.Awake();
            _actionData = GetUnitCompo<ActionData>();
        }

        protected override void Start()
        {
            base.Start();

            _stateChangeChannel = GetBlackboardVariable<StateChangeEvent>("StateChannel").Value;
            OnDeathEvent.AddListener(MapOpen.Instance.GetOpenCnt);
        }

        public void HandleChildAnimatorMove(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            transform.position += deltaPosition;
            transform.rotation = deltaRotation * transform.rotation;
        }
        
        public void SetBattleState()
        {
            if (IsBattleState || IsDead)
                return;

            IsBattleState = true;
            
            var stateVariable = GetBlackboardVariable<EnemyState>("CurrentState");
            
            if (stateVariable != null && stateVariable.Value != EnemyState.HIT)
                _stateChangeChannel.SendEventMessage(EnemyState.CHASE);
        }

        public void HandleDeadEvent()
        {
            if (IsDead)
                return;

            IsDead = true;
            _stateChangeChannel.SendEventMessage(EnemyState.DEAD);
            
            const float force = -100f;
        }
    }
}