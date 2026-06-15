using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "RotateByTrigger", story: "[Movement] rotate to [Target] by [Trigger]", category: "Action", id: "b4cc90cf38d52790fb30d386c15605a5")]
    public partial class RotateByTriggerAction : Action
    {
        [SerializeReference] public BlackboardVariable<NavMovement> Movement;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<EntityAnimatorTrigger> Trigger;

        private bool _isRotate;
        
        protected override Status OnStart()
        {
            Trigger.Value.OnManualRotationTrigger += HandleManualRotation;
            
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_isRotate)
                Movement.Value.LookAtTarget(Target.Value.position);
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            Trigger.Value.OnManualRotationTrigger -= HandleManualRotation;
        }

        private void HandleManualRotation(bool isRotate) => _isRotate = isRotate;
    }
}