using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Unity Log", story: "Unity log [Message] to the console", category: "Action", id: "ef185e934faa6e093252f6d9bffa99f5")]
    public partial class UnityLogAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> Message;

        protected override Status OnStart()
        {
            return Status.Success;
        }
    }
}