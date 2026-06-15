using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsInDetect", story: "[Self] check detectRange from [Target]", category: "Conditions", id: "07ab29378bdb7dd93d2505b303583389")]
    public partial class IsInDetectCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Enemy> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        public override bool IsTrue()
        {
            if (Target.Value == null)
                return false;

            float distance = Vector3.Distance(Self.Value.transform.position, Target.Value.position);

            return distance < Self.Value.detectRange;
        }
    }
}