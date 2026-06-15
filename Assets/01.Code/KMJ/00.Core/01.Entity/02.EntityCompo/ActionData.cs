using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using Code.Interfaces;
using UnityEngine;

namespace _Code.EntityCompo.Combat
{
    public class ActionData : MonoBehaviour, IEntityComponent
    {
        public Vector3 HitPoint { get; set; }
        public Vector3 HitNormal { get; set; }
        public bool HitByPowerAttack { get; set; }
        public DamageData LastDamageData { get; set; } 

        private Entity _entity;
        public void Initialize(Entity entity)
        {
            _entity = entity;
        }
        
    }
}