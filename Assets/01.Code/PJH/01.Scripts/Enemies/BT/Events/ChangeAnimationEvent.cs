using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ChangeAnimationEvent")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ChangeAnimationEvent", message: "change to [NextAnimation]", category: "Events", id: "ed41707e30b74897b8ec10dacb8de0ff")]
public sealed partial class ChangeAnimationEvent : EventChannel<string> { }

