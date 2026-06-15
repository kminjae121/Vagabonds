using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetRootMotion", story: "Set root motion [MainAnimator] [Value]", category: "Action", id: "8a76fec3c2f7a4755299d6b6def52697")]
public partial class SetRootMotionAction : Action
{
    [SerializeReference] public BlackboardVariable<EntityAnimator> MainAnimator;
    [SerializeReference] public BlackboardVariable<bool> Value;

    protected override Status OnStart()
    {
        if (MainAnimator.Value == null)
            return Status.Failure;

        MainAnimator.Value.ApplyRootMotion = Value.Value;
        return Status.Success;
    }
}

