using System;

namespace _01.Member.KMJ._00.Core._01.Entity._02.EntityCompo
{
    [Flags]
    public enum DamageType
    {
        None = 0, MELEE = 1, RANGE = 2, MAGIC = 4
    }
    
    public struct DamageData
    {
        public float damage;
        public bool isCritical;
        public DamageType damageType;
        //데미지에 관련된 모든 것을 저장하는 구조체
    }
}