using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "RotateToTarget", story: "[Self] rotate to [Target] with [Movement] , Threshold : [RotationThreshold] , Smooth : [IsSmooth]", category: "Action", id: "e678029f0224c22aa39acf2290574228")]
    public partial class RotateToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<Enemy> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<NavMovement> Movement;
        [SerializeReference] public BlackboardVariable<float> RotationThreshold;
        [SerializeReference] public BlackboardVariable<bool> IsSmooth;
        
        protected override Status OnUpdate()
        {
            return LookTargetSmoothly() ? Status.Success : Status.Running;
        }

        private bool LookTargetSmoothly()
        {
            var targetRotation = Movement.Value.LookAtTarget(Target.Value.position, IsSmooth.Value);
            return Quaternion.Angle(targetRotation, Self.Value.transform.rotation) < RotationThreshold.Value;
        }
    }
}