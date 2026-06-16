using System;
using System.Collections.Generic;
using System.Linq;
using Code.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace _Code.EntityCompo
{
    public class Entity : MonoBehaviour
    {
        public UnityEvent OnHitEvent;
        public UnityEvent OnDeathEvent;

        public bool IsDead { get; set; } = false;
        
        protected Dictionary<Type, IEntityComponent> _components = new();

        protected Dictionary<Type, Component> _casingComponents = new();

        protected virtual void Awake()
        {
            AddComponent();
            InitUnitComponents();
            AfterInitializeComponents();
        }

        private void AddComponent()
        {
            _components = GetComponentsInChildren<IEntityComponent>()
                .ToDictionary(compo => compo.GetType());
        }
        public IEntityComponent GetCompo(Type type)
            => _components.GetValueOrDefault(type);

        private void AfterInitializeComponents()
        {
            _components.Values.OfType<IAfterInitialize>()
                .ToList().ForEach(component => component.AfterInitialize());
        }

        private void InitUnitComponents()
        {
            foreach (var component in _components.Values)
                component.Initialize(this);
        }
        
        public virtual void EntityDestroy()
        {
            Destroy(gameObject);
        }
        
        public T GetUnitCompo<T>() where T : class, IEntityComponent
        {
            return _components.GetValueOrDefault(typeof(T)) as T;
        }
    }
}