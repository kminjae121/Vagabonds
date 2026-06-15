using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsInAttack", story: "[Self] check in attack from [Target]", category: "Conditions", id: "25de407efce03d51a467ec6cdde07aac")]
    public partial class IsInAttackCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Enemy> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        public override bool IsTrue()
        {
            var distance = Vector3.Distance(Self.Value.transform.position, Target.Value.position);

            return distance < Self.Value.attackRange;
        }
    }
}