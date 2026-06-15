using Code.Enemies;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SpawnShockwave", story: "[Self] spawn shockwave", category: "Action", id: "9a521e1c3926e917beb2f39142c86397")]
public partial class SpawnShockwaveAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyHeavy> Self;

    protected override Status OnStart()
    {
        if (Self.Value == null)
            return Status.Failure;

        Self.Value.SpawnShockWave();
        
        return Status.Success;
    }
}

