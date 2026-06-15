using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MoveToTarget", story: "[Movement] move to [Target]", category: "Action", id: "8af313bf49a5a7e10b3aa8a6cc27273c")]
    public partial class MoveToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<NavMovement> Movement;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        protected override Status OnStart()
        {
            if (Movement.Value == null || Target.Value == null)
                return Status.Failure;
            
            Movement.Value.SetDestination(Target.Value.position);
            return Status.Success;
        }
    }
}