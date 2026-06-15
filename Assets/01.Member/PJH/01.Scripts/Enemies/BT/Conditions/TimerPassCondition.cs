using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "TimerPass", story: "Check [Timer] pass [Second]", category: "Conditions", id: "974a746e8a6e8b48f1c6a060a42f4b9e")]
public partial class TimerPassCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> Timer;
    [SerializeReference] public BlackboardVariable<float> Second;

    public override bool IsTrue()
    {
        return Timer + Second < Time.time;
    }
}
