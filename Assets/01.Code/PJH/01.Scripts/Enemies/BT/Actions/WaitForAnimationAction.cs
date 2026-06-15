using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "WaitForAnimation", story: "Wait for [Trigger] end", category: "Action", id: "af492792317a7fb7dc5d6aa6a11fcc00")]
    public partial class WaitForAnimationAction : Action
    {
        [SerializeReference] public BlackboardVariable<EntityAnimatorTrigger> Trigger;

        private bool _isTriggered;
        
        protected override Status OnStart()
        {
            _isTriggered = false;
            Trigger.Value.OnAnimationEndTrigger += HandleAnimationEndTrigger;
            
            return Status.Running;
        }
        
        protected override Status OnUpdate()
        {
            return _isTriggered ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            Trigger.Value.OnAnimationEndTrigger -= HandleAnimationEndTrigger;
        }
        
        private void HandleAnimationEndTrigger() => _isTriggered = true;
    }
}