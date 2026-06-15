using _Code.EntityCompo;
using Code.Entities;
using Code.Interfaces;
using UnityEngine;

namespace Code.Combat
{
    public class WeaponHolder : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private Weapon[] weapons;

        private Entity _entity;
        
        public void Initialize(Entity entity)
        {
            _entity = entity;
        }

        public void DropWeapons()
        {
            foreach (var weapon in weapons)
                weapon.Drop();
        }
    }
}