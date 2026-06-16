using UnityEngine;

namespace _Code.KDH.EntityCompo.Move
{
    internal static class WallMove
    {
        public static Vector3 GetWallForward(Vector3 wallNormal, Vector3 wishDirection, Transform viewReference)
        {
            Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal).normalized;
            Vector3 referenceDirection = wishDirection.sqrMagnitude > Mathf.Epsilon
                ? wishDirection
                : viewReference.forward;

            if (Vector3.Dot(wallForward, referenceDirection) < 0f)
                wallForward = -wallForward;

            return wallForward;
        }

        public static Vector3 GetTravelReference(Vector3 wallNormal, Vector3 horizontalVelocity, Transform viewReference)
        {
            Vector3 tangentVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
            tangentVelocity.y = 0f;
            if (tangentVelocity.sqrMagnitude > 0.01f)
                return tangentVelocity.normalized;

            Vector3 viewForward = Vector3.ProjectOnPlane(viewReference.forward, wallNormal);
            viewForward.y = 0f;
            return viewForward.sqrMagnitude > Mathf.Epsilon ? viewForward.normalized : Vector3.zero;
        }

        public static Vector3 ApplyRideMovement(
            Vector3 horizontalVelocity,
            Vector3 wishDirection,
            Vector3 wallNormal,
            float contactVelocityTrim,
            float rideMaxSpeed,
            float rideAcceleration,
            float deltaTime)
        {
            wallNormal.Normalize();
            float intoWallSpeed = Vector3.Dot(horizontalVelocity, -wallNormal);
            if (intoWallSpeed > 0f)
                horizontalVelocity += wallNormal * (intoWallSpeed * contactVelocityTrim);

            horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);

            Vector3 wallWishDirection = Vector3.ProjectOnPlane(wishDirection, wallNormal);
            wallWishDirection.y = 0f;
            if (wallWishDirection.sqrMagnitude > Mathf.Epsilon)
            {
                horizontalVelocity = MovementMotor.Accelerate(
                    horizontalVelocity,
                    wallWishDirection.normalized,
                    rideMaxSpeed,
                    rideAcceleration,
                    deltaTime);
            }

            return MovementMotor.ClampHorizontalSpeed(horizontalVelocity, rideMaxSpeed);
        }

        public static void ApplyRideGravity(
            ref float verticalVelocity,
            float gravity,
            float maxFallSpeed,
            float maxRiseSpeed,
            float upwardBrake,
            float deltaTime)
        {
            if (verticalVelocity > maxRiseSpeed)
            {
                verticalVelocity = Mathf.MoveTowards(verticalVelocity, maxRiseSpeed, upwardBrake * deltaTime);
                return;
            }

            verticalVelocity = Mathf.Max(verticalVelocity - gravity * deltaTime, -maxFallSpeed);
        }

        public static Vector3 BuildKickVelocity(
            Vector3 wallNormal,
            Vector3 wallForward,
            Vector3 horizontalVelocity,
            Transform viewReference,
            float horizontalImpulse,
            float forwardImpulse,
            float forwardRetention,
            float viewAssist,
            float minimumExitSpeed)
        {
            wallNormal.Normalize();
            wallForward = wallForward.sqrMagnitude > Mathf.Epsilon
                ? wallForward.normalized
                : GetWallForward(wallNormal, horizontalVelocity, viewReference);

            Vector3 wallTangentVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
            Vector3 wallTangentDirection = wallTangentVelocity.sqrMagnitude > 0.01f
                ? wallTangentVelocity.normalized
                : wallForward;

            if (Vector3.Dot(wallTangentDirection, wallForward) < 0f)
                wallTangentDirection = -wallTangentDirection;

            float retainedForwardSpeed = Mathf.Max(wallTangentVelocity.magnitude * forwardRetention, forwardImpulse);
            Vector3 kickVelocity = wallNormal * horizontalImpulse + wallTangentDirection * retainedForwardSpeed;
            kickVelocity += GetViewAssist(viewReference, wallNormal, wallTangentDirection, forwardImpulse, viewAssist);

            return EnsureMinimumExitSpeed(kickVelocity, minimumExitSpeed);
        }

        private static Vector3 GetViewAssist(
            Transform viewReference,
            Vector3 wallNormal,
            Vector3 wallTangentDirection,
            float forwardImpulse,
            float viewAssist)
        {
            if (viewAssist <= 0f || viewReference == null)
                return Vector3.zero;

            Vector3 viewForward = Vector3.ProjectOnPlane(viewReference.forward, Vector3.up);
            if (viewForward.sqrMagnitude <= Mathf.Epsilon)
                return Vector3.zero;

            Vector3 assistedDirection = Vector3.ProjectOnPlane(viewForward.normalized, wallNormal);
            if (assistedDirection.sqrMagnitude <= Mathf.Epsilon)
                return Vector3.zero;

            assistedDirection.Normalize();
            if (Vector3.Dot(assistedDirection, wallTangentDirection) < 0f)
                return Vector3.zero;

            return assistedDirection * (forwardImpulse * viewAssist);
        }

        private static Vector3 EnsureMinimumExitSpeed(Vector3 horizontalVelocity, float minimumExitSpeed)
        {
            if (minimumExitSpeed <= 0f || horizontalVelocity.sqrMagnitude <= Mathf.Epsilon)
                return horizontalVelocity;

            float currentSpeed = horizontalVelocity.magnitude;
            if (currentSpeed >= minimumExitSpeed)
                return horizontalVelocity;

            return horizontalVelocity * (minimumExitSpeed / currentSpeed);
        }
    }
}
