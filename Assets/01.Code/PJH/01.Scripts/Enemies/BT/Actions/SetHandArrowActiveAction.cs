using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SetHandArrowActive", story: "[Self] set active arrow in hand [IsActive]", category: "Action", id: "2f4d755710671b818a0cb5c990ec01e6")]
    public partial class SetHandArrowActiveAction : Action
    {
        [SerializeReference] public BlackboardVariable<EnemyArcher> Self;
        [SerializeReference] public BlackboardVariable<bool> IsActive;

        protected override Status OnStart()
        {
            if (Self.Value == null)
                return Status.Failure;
        
            Self.Value.SetActiveArrowInHand(IsActive.Value);
        
            return Status.Success;
        }
    }
}