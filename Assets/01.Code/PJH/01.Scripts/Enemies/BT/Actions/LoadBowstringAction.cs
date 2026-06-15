using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "LoadBowstring", story: "[Self] load to bowstring", category: "Action", id: "1c0019e6c5dec6cab8ba0f56c201a06e")]
    public partial class LoadBowstringAction : Action
    {
        [SerializeReference] public BlackboardVariable<EnemyArcher> Self;

        protected override Status OnStart()
        {
            if (Self.Value == null)
                return Status.Failure;
            
            Self.Value.LoadBowstring();
            return Status.Success;
        }
    }
}