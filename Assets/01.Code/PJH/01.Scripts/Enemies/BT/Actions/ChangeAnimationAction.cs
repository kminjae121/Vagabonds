using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ChangeAnimation", story: "[MainAnimator] change [oldAnim] to [newAnim]", category: "Action", id: "48b287f905b1e897d99d0518aa1d377b")]
    public partial class ChangeAnimationAction : Action
    {
        [SerializeReference] public BlackboardVariable<EntityAnimator> MainAnimator;
        [SerializeReference] public BlackboardVariable<string> OldAnim;
        [SerializeReference] public BlackboardVariable<string> NewAnim;

        protected override Status OnStart()
        {
            var oldHash = Animator.StringToHash(OldAnim.Value);
            var newHash = Animator.StringToHash(NewAnim.Value);
        
            MainAnimator.Value.SetParam(oldHash, false);
            MainAnimator.Value.SetParam(newHash, true);

            OldAnim.Value = NewAnim.Value;
        
            return Status.Success;
        }
    }
}