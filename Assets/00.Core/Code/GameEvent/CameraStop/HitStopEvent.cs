using System;

namespace Code.Core.Events.Bus
{
    public struct HitStopEvent : IEvent
    {
        public float Duration;
        public float TimeScale;
        public bool OverrideCurrent;

        public HitStopEvent(float duration, float timeScale = 0f, bool overrideCurrent = true)
        {
            Duration = duration;
            TimeScale = timeScale;
            OverrideCurrent = overrideCurrent;
        }
    }
}
