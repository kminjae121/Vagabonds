using UnityEngine;

namespace Code.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private Collider weaponCollider;
        [SerializeField] private Rigidbody weaponRigid;

        private void Awake()
        {
            weaponCollider.enabled = false;
            weaponRigid.isKinematic = true;
        }

        public void Drop()
        {
            weaponCollider.enabled = true;
            weaponRigid.isKinematic = false;
            transform.SetParent(null);
        }
    }
}