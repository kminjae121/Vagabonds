namespace Code.Core.GameEvent
{
    public static class EventBus<T> where T : IEventBus
    {
        public delegate void Event(T evt);

        public static Event OnEvent;
        
        public static void Raise(T evt) => OnEvent?.Invoke(evt);
    }
}