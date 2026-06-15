using _Code.EntityCompo;
using UnityEngine;

namespace Code.Entities
{
    [CreateAssetMenu(fileName = "Entity Finder", menuName = "SO/Entity Finder", order = 0)]
    public class EntityFinderSO : ScriptableObject
    {
        public Entity Target { get; private set; }

        public void SetTarget(Entity target) => Target = target;
    }
}