using UnityEngine;

namespace _Code.EntityCompo.Move
{
    internal static class MovementMotor
    {
        public static Vector3 GetHorizontalVelocity(Vector3 velocity)
        {
            return new Vector3(velocity.x, 0f, velocity.z);
        }

        public static Vector3 ApplyFriction(Vector3 horizontalVelocity, float stopSpeed, float friction, float deltaTime)
        {
            float speed = horizontalVelocity.magnitude;
            if (speed <= Mathf.Epsilon)
                return Vector3.zero;

            float control = speed < stopSpeed ? stopSpeed : speed;
            float drop = control * friction * deltaTime;
            float nextSpeed = Mathf.Max(speed - drop, 0f);

            return horizontalVelocity * (nextSpeed / speed);
        }

        public static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float wishSpeed, float acceleration, float deltaTime)
        {
            if (wishDirection.sqrMagnitude <= Mathf.Epsilon || wishSpeed <= 0f)
                return currentVelocity;

            float currentSpeed = Vector3.Dot(currentVelocity, wishDirection);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f)
                return currentVelocity;

            float accelSpeed = Mathf.Min(acceleration * wishSpeed * deltaTime, addSpeed);
            return currentVelocity + wishDirection * accelSpeed;
        }

        public static Vector3 ApplyAirControl(Vector3 horizontalVelocity, Vector3 wishDirection, float responsiveness, float deltaTime, float controlScale = 1f)
        {
            if (wishDirection.sqrMagnitude <= Mathf.Epsilon || horizontalVelocity.sqrMagnitude <= 0.01f || controlScale <= 0f)
                return horizontalVelocity;

            float speed = horizontalVelocity.magnitude;
            float steer = 1f - Mathf.Exp(-responsiveness * controlScale * deltaTime);
            Vector3 blendedDirection = Vector3.Slerp(horizontalVelocity.normalized, wishDirection, steer).normalized;

            return blendedDirection * speed;
        }

        public static Vector3 ClampHorizontalSpeed(Vector3 horizontalVelocity, float maxSpeed)
        {
            if (maxSpeed <= 0f)
                return Vector3.zero;

            if (horizontalVelocity.sqrMagnitude <= maxSpeed * maxSpeed)
                return horizontalVelocity;

            return horizontalVelocity.normalized * maxSpeed;
        }
    }
}
