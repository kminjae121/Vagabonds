using System;
using _Code.EntityCompo;
using Code.Interfaces;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetComponents", story: "Get Components from [Self]", category: "Action/GetCompo", id: "f0d4e8492854d568f57b909ba3a7aa8e")]
    public partial class GetComponentsAction : Action
    {
        [SerializeReference] public BlackboardVariable<Enemy> Self;

        protected override Status OnStart()
        {
            var enemy = Self.Value;
            var varList = enemy.BTAgent.BlackboardReference.Blackboard.Variables;

            foreach (var variable in varList)
            {
                if (!typeof(IEntityComponent).IsAssignableFrom(variable.Type))
                    continue;

                SetVariable(enemy, variable.Name, enemy.GetCompo(variable.Type));
            }

            return Status.Success;
        }

        private void SetVariable<T>(Enemy enemy, string variableName, T component)
        {
            Debug.Assert(component != null, 
                $"{variableName} 컴포넌트가 {enemy.gameObject.name}에 존재하는지 체크해 주세요.");

            if (enemy.BTAgent.GetVariable(variableName, out var target))
                target.ObjectValue = component;
        }
    }
}