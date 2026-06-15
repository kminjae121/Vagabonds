using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using Code.Entities;
using UnityEngine;

namespace _01.Member.KMJ._02.Scripts._01.Player.AttackCompo
{
    public interface IDamageable
    {
        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer);
    }
}