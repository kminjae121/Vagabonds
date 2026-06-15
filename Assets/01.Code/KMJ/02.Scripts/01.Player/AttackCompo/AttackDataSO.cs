using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using UnityEngine;

namespace _01.Member.KMJ._02.Scripts._01.Player.AttackCompo
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "SO/Combat/AttackData", order = 0)]
    public class AttackDataSO : ScriptableObject
    {
        public DamageType damageType = DamageType.MELEE;
        
        public string attackName;
        public float damageMultiplier = 1f; 
        public float damageIncrease = 0;  
        public bool isPowerAttack;
        public float impulseForce;
        
        private void OnEnable()
        {
            attackName = this.name;
        }
    }
}