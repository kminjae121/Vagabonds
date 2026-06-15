using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SetMove", story: "[Movement] isStop set to [newValue]", category: "Action", id: "ee94d021b029f9fcd638574078ea2946")]
    public partial class SetMoveAction : Action
    {
        [SerializeReference] public BlackboardVariable<NavMovement> Movement;
        [SerializeReference] public BlackboardVariable<bool> NewValue;

        protected override Status OnStart()
        {
            if (Movement.Value == null)
                return Status.Failure;
        
            Movement.Value.SetStop(NewValue.Value);
        
            if (NewValue.Value)
                Movement.Value.SetDestination(Movement.Value.transform.position);

            return Status.Success;
        }
    }
}