namespace Code.Core.Events.Bus
{
    public struct CamShakeEvent : IEvent
    {
        public float force;
        public float duration;

        public CamShakeEvent(float force, float duration = 0f)
        {
            this.force = force;
            this.duration = duration;
        }
    }
}
