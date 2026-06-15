using Code.Enemies;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/StateChangeEvent")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "StateChangeEvent", message: "Set [NextState]", category: "Events", id: "715377e8216943d9d0c6827ead9f5001")]
public sealed partial class StateChangeEvent : EventChannel<EnemyState> { }

