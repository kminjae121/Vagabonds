using UnityEngine;

namespace _Code.KDH.EntityCompo.Move
{
    internal static class MomentumModifier
    {
        public static float GetBloodSpeedMultiplier(int bloodStacks, float bloodSpeedBonusPerStack, int maxBloodStacksForMovement)
        {
            int countedStacks = maxBloodStacksForMovement > 0
                ? Mathf.Min(bloodStacks, maxBloodStacksForMovement)
                : bloodStacks;

            return 1f + countedStacks * bloodSpeedBonusPerStack;
        }

        public static float GetEffectiveBaseSpeed(float baseMaxSpeed, int bloodStacks, float bloodSpeedBonusPerStack, int maxBloodStacksForMovement)
        {
            return baseMaxSpeed * GetBloodSpeedMultiplier(bloodStacks, bloodSpeedBonusPerStack, maxBloodStacksForMovement);
        }

        public static float GetBhopSpeedCap(float effectiveBaseSpeed, float bhopSpeedMultiplier)
        {
            return effectiveBaseSpeed * bhopSpeedMultiplier;
        }

        public static float GetCurrentHorizontalSpeedCap(
            float effectiveBaseSpeed,
            float bhopSpeedMultiplier,
            float combatMomentumCapMultiplier,
            float combatMomentumCapUntilTime,
            float wallKickSpeedCapMultiplier,
            float wallKickMomentumCapUntilTime,
            float slideSpeedCapMultiplier,
            float slideMomentumCapUntilTime,
            float currentTime)
        {
            float capMultiplier = bhopSpeedMultiplier;
            if (currentTime < combatMomentumCapUntilTime)
                capMultiplier = Mathf.Max(capMultiplier, combatMomentumCapMultiplier);
            if (currentTime < wallKickMomentumCapUntilTime)
                capMultiplier = Mathf.Max(capMultiplier, wallKickSpeedCapMultiplier);
            if (currentTime < slideMomentumCapUntilTime)
                capMultiplier = Mathf.Max(capMultiplier, slideSpeedCapMultiplier);

            return effectiveBaseSpeed * capMultiplier;
        }

        public static void ApplyCombatImpulse(
            Rigidbody body,
            Transform fallbackTransform,
            Vector3 direction,
            int killCount,
            float killImpulseSpeed,
            float killImpulseVerticalLift,
            float multiKillImpulseMultiplier,
            float speedCap)
        {
            if (body == null)
                return;

            Vector3 impulseDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (impulseDirection.sqrMagnitude <= Mathf.Epsilon && fallbackTransform != null)
                impulseDirection = fallbackTransform.forward;

            if (impulseDirection.sqrMagnitude <= Mathf.Epsilon)
                return;

            impulseDirection.Normalize();

            float killMultiplier = killCount > 1 ? multiKillImpulseMultiplier : 1f;
            Vector3 velocity = body.linearVelocity;
            Vector3 horizontalVelocity = MovementMotor.GetHorizontalVelocity(velocity);
            horizontalVelocity += impulseDirection * (killImpulseSpeed * killMultiplier);
            horizontalVelocity = MovementMotor.ClampHorizontalSpeed(horizontalVelocity, speedCap);

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            velocity.y = Mathf.Max(velocity.y, killImpulseVerticalLift);
            body.linearVelocity = velocity;
        }
    }
}
