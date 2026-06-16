using Code.Core._02.Sound;
using Code.Core.GameEvent;
using UnityEngine;

namespace GameEvents
{
    public static class SoundEvents
    {
        public static PlaySFXEvent PlaySFXEvent = new PlaySFXEvent();
        public static PlayBGMEveent PlayBGMEvent = new PlayBGMEveent(); 
        public static StopBGMEvent StopBGMEvent = new StopBGMEvent();
        public static PlayLongSFXEvent PlayLongSFXEvent = new PlayLongSFXEvent();
        public static StopLongSFXEvent StopLongSFXEvent = new StopLongSFXEvent();
    }

    public class PlaySFXEvent : GameEvent
    {
        public Vector3 position;
        public SoundSO soundClip;

        public PlaySFXEvent Initializer(Vector3 position, SoundSO soundClip)
        {
            this.position = position;
            this.soundClip = soundClip;
            return this;
        }
    }
    
    public class PlayBGMEveent : GameEvent
    {
        public SoundSO bgmClip;

        public PlayBGMEveent Initializer(SoundSO soundClip)
        {
            this.bgmClip = soundClip;
            return this;
        }
    }

    public class StopBGMEvent : GameEvent
    {
        
    }
    
    public class PlayLongSFXEvent : GameEvent
    {
        public Vector3 position;
        public SoundSO soundClip;
        public int idxNumber;

        public PlayLongSFXEvent Initializer(Vector3 position, SoundSO soundClip, int idxNumber)
        {
            this.position = position;   
            this.soundClip = soundClip;
            this.idxNumber = idxNumber;
            return this;
        }
    }

    public class StopLongSFXEvent : GameEvent
    {
        public int idxNumber;

        public StopLongSFXEvent Initializer(int idxNumber)
        {
            this.idxNumber = idxNumber;
            return this;
        }
    }
}