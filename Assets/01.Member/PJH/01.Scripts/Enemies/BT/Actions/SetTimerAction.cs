using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SetTimer", story: "Set Time to [Timer]", category: "Action", id: "55df30eb856cf9a1f53010c8099e5c40")]
    public partial class SetTimerAction : Action
    {
        [SerializeReference] public BlackboardVariable<float> Timer;

        protected override Status OnStart()
        {
            Timer.Value = Time.time;
        
            return Status.Success;
        }
    }
}