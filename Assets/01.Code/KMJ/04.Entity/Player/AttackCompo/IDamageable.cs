using _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo;
using _Code.EntityCompo;
using UnityEngine;

namespace _01.Member.KMJ._02.Scripts._01.Player.AttackCompo
{
    public readonly struct DamageResult
    {
        public static readonly DamageResult NotApplied = new(false, false, null);

        public DamageResult(bool wasApplied, bool wasFatal, Entity targetEntity)
        {
            WasApplied = wasApplied;
            WasFatal = wasFatal;
            TargetEntity = targetEntity;
        }

        public bool WasApplied { get; }
        public bool WasFatal { get; }
        public Entity TargetEntity { get; }
    }

    public interface IDamageable
    {
        public DamageResult ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer);
    }
}
