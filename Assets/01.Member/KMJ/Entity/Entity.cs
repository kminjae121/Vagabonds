using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Member.KMJ.Entity
{
    public class Entity : MonoBehaviour
    {
        protected Dictionary<Type, IEntityComponent> _components = new();

        protected virtual void Awake()
        {
            AddComponent();
            InitUnitComponents();
        }

        private void AddComponent()
        {
            _components = GetComponentsInChildren<IEntityComponent>()
                .ToDictionary(compo => compo.GetType());
        }

        private void InitUnitComponents()
        {
            foreach (var component in _components.Values)
                component.Initialize(this);
        }
        
        public T GetUnitCompo<T>() where T : class, IEntityComponent
        {
            return _components.GetValueOrDefault(typeof(T)) as T;
        }
    }
}